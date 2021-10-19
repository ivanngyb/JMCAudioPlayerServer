using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace JMCAudioPlayerServer
{
    class PipeServer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle CreateNamedPipe(
           String pipeName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe,
           IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DisconnectNamedPipe(SafeFileHandle hHandle);

        [StructLayoutAttribute(LayoutKind.Sequential)]
        struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        private const uint SECURITY_DESCRIPTOR_REVISION = 1;

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool InitializeSecurityDescriptor(ref SECURITY_DESCRIPTOR sd, uint dwRevision);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool SetSecurityDescriptorDacl(ref SECURITY_DESCRIPTOR sd, bool daclPresent, IntPtr dacl, bool daclDefaulted);

        //Client class
        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
            public UserInfo userInfo;
            public bool connected = false;
        }

        public class UserInfo
        {
            private string username;
            private string password;
            public string Username { get => username; set => username = value; }
            public string Password { get => password; set => password = value; }
            public bool loggedIn = false;
        }

        //Events for message handling and client connecting/disconnecting
        public delegate void MessageReceivedHandler(byte[] message);
        public event MessageReceivedHandler MessageReceived;
        public delegate void ClientDisconnectedHandler();
        public event ClientDisconnectedHandler ClientDisconnected;
        public delegate void NewUserRegisterHandler();
        public event NewUserRegisterHandler UserRegister;

        public delegate void ClientConnectedHandler();
        public event ClientConnectedHandler ClientConnected;
        const int BUFFER_SIZE = 4096;

        Thread listenThread;
        public readonly List<Client> clients = new List<Client>();
        public List<UserInfo> userInfos = new List<UserInfo>();

        //Get total of clients connected
        public int TotalConnectedClients
        {
            get
            {
                lock (clients)
                {
                    return clients.Count;
                }
            }
        }

        public string PipeName { get; private set; }
        public bool Running { get; private set; }

        //Generating SHA512 for password
        private string GenerateSHA512String(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }

            return result.ToString();
        }

        //Comparing SHA512 to see if i'ts correct
        private bool CompareSHA512(string attemptPass, UserInfo userCheck)
        {

            if (GenerateSHA512String(attemptPass).Equals(GenerateSHA512String(userCheck.Password)))
            {
                return true;
            }
            return false;
        }

        //Starts the server
        public void Start(string pipename)
        {
            try
            {
                PipeName = pipename;
                listenThread = new Thread(ListenForClients)
                {
                    IsBackground = true
                };

                listenThread.Start();
                Running = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        //Thread for listening to clients joining
        void ListenForClients()
        {
            SECURITY_DESCRIPTOR sd = new SECURITY_DESCRIPTOR();

            InitializeSecurityDescriptor(ref sd, SECURITY_DESCRIPTOR_REVISION);
            SetSecurityDescriptorDacl(ref sd, true, IntPtr.Zero, false);

            IntPtr ptrSD = Marshal.AllocCoTaskMem(Marshal.SizeOf(sd));
            Marshal.StructureToPtr(sd, ptrSD, false);

            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES
            {
                nLength = Marshal.SizeOf(sd),
                lpSecurityDescriptor = ptrSD,
                bInheritHandle = 1
            };

            IntPtr ptrSA = Marshal.AllocCoTaskMem(Marshal.SizeOf(sa));
            Marshal.StructureToPtr(sa, ptrSA, false);


            while (true)
            {
                SafeFileHandle clientHandle =
                    CreateNamedPipe(
                        PipeName,
                        0x40000003,
                        0,
                        255,
                        BUFFER_SIZE,
                        BUFFER_SIZE,
                        0,
                        ptrSA);


                if (clientHandle.IsInvalid)
                    continue;

                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);


                if (success == 0)
                {
                    clientHandle.Close();
                    continue;
                }

                Client client = new Client
                {
                    handle = clientHandle
                };

                lock (clients)
                    clients.Add(client);

                Thread readThread = new Thread(Read)
                {
                    IsBackground = true
                };
                readThread.Start(client);

                Console.WriteLine("User connected");

                if (ClientConnected != null)
                    ClientConnected();
            }


            Marshal.FreeCoTaskMem(ptrSD);
            Marshal.FreeCoTaskMem(ptrSA);
        }

        //Reading thread for getting message from users
        void Read(object clientObj)
        {
            Client client = (Client)clientObj;
            client.stream = new FileStream(client.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] buffer = new byte[BUFFER_SIZE];

            ASCIIEncoding encoder = new ASCIIEncoding();
            //When connected users get their login details sent to them
            //byte[] messageBuffer = encoder.GetBytes("Welcome to JMC Login! Your login details are: \r\n" + "Username: " + client.username + "\r\nPassword: " + client.password);

            //this.SendMessageToClient(messageBuffer, client);

            while (true)
            {
                int bytesRead = 0;

                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        int totalSize = client.stream.Read(buffer, 0, 4);

                        if (totalSize == 0)
                            break;

                        totalSize = BitConverter.ToInt32(buffer, 0);

                        do
                        {
                            int numBytes = client.stream.Read(buffer, 0, Math.Min(totalSize - bytesRead, BUFFER_SIZE));

                            ms.Write(buffer, 0, numBytes);

                            bytesRead += numBytes;

                        } while (bytesRead < totalSize);


                    }
                    catch
                    {
                        break;
                    }

                    if (bytesRead == 0)
                        break;

                    if (MessageReceived != null)
                        MessageReceived(ms.ToArray());

                    if (client.connected == false)
                    {
                        string[] data = encoder.GetString(ms.ToArray(), 0, ms.ToArray().Length).Split(' ');

                        if (data[0].Equals("REGISTER"))
                        {
                            UserInfo newUser = new UserInfo();
                            newUser.Username = data[1];
                            newUser.Password = data[2];
                            userInfos.Add(newUser);

                            if (UserRegister != null)
                                UserRegister();
                        }
                        else if (data[0].Equals("LOGIN"))
                        {
                            //Validates their username and password
                            ValidateUser(ms.ToArray(), client);
                        }
                    }

                }
            }


            lock (clients)
            {
                //Disconnects and cleans up stream
                DisconnectNamedPipe(client.handle);
                client.stream.Close();
                client.handle.Close();
                if (client.userInfo != null)
                    client.userInfo.loggedIn = false;

                clients.Remove(client);
                Console.WriteLine("Clients removed");
            }

            if (ClientDisconnected != null)
                ClientDisconnected();
        }

        //Validating user class
        public void ValidateUser(byte[] message, Client client)
        {
            Console.WriteLine("Checking login info");
            ASCIIEncoding encoder = new ASCIIEncoding();
            string[] data = encoder.GetString(message, 0, message.Length).Split(' ');
            bool userFound = false;
            UserInfo userCheck = null;

            

            foreach (UserInfo u in userInfos)
            {
                if (u.Username.Equals(data[1])) 
                {
                    userFound = true;
                    userCheck = u;
                    break;
                }
            }

            if (userFound && userCheck != null)
            {
                Console.WriteLine("Comparing Passwords");
                if (CompareSHA512(data[2], userCheck))
                {
                    if (!userCheck.loggedIn)
                    {
                        Console.WriteLine("Sucess");
                        userCheck.loggedIn = true;
                        client.userInfo = userCheck;
                        byte[] messageBuffer = encoder.GetBytes("LOGIN_SUCCESS");
                        this.SendMessageToClient(messageBuffer, client);
                    }
                    else
                    {
                        userCheck.loggedIn = true;
                        byte[] messageBuffer = encoder.GetBytes("LOGIN_FAILED2");
                        this.SendMessageToClient(messageBuffer, client);
                    }

                }
                else
                {
                    Console.WriteLine("Fail Incorrect Password");
                    byte[] messageBuffer = encoder.GetBytes("LOGIN_FAILED1");
                    this.SendMessageToClient(messageBuffer, client);
                }
            }
            else
            {
                Console.WriteLine("Fail user not found");
                byte[] messageBuffer = encoder.GetBytes("LOGIN_FAILED1");
                this.SendMessageToClient(messageBuffer, client);
            }
        }

        //For sending message to everyone on server
        public void SendMessage(byte[] message)
        {
            lock (clients)
            {
                byte[] messageLength = BitConverter.GetBytes(message.Length);

                foreach (Client client in clients)
                {
                    client.stream.Write(messageLength, 0, 4);
                    client.stream.Write(message, 0, message.Length);
                    client.stream.Flush();
                }
            }
        }

        //For sending message to specific client on server
        public void SendMessageToClient(byte[] message, Client client)
        {
            lock (clients)
            {
                byte[] messageLength = BitConverter.GetBytes(message.Length);


                client.stream.Write(messageLength, 0, 4);
                client.stream.Write(message, 0, message.Length);
                client.stream.Flush();

            }
        }
    }
}
