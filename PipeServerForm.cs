using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Student ID: 30031552
//Student Name: Yang Beng Ng(Ivan)
//Date: 25/10/2021
//Description: An advance audio player with login capabilities and song saving

namespace JMCAudioPlayerServer
{
    public partial class PipeServerForm : Form
    {

        private PipeServer pipeServer = new PipeServer();

        public PipeServerForm()
        {
            InitializeComponent();
            //Start listening to events
            pipeServer.MessageReceived += PipeServer_MessageReceived;
            pipeServer.ClientDisconnected += PipeServer_ClientDisconnected;
            pipeServer.ClientConnected += PipeServer_ClientConnected;
            pipeServer.UserRegister += PipeServer_UserRegister;
            TextBoxPipeName.Text = "jmcaudio";
        }

        void PipeServer_UserRegister()
        {
            Invoke(new PipeServer.NewUserRegisterHandler(UserRegister));
        }

        void PipeServer_ClientDisconnected()
        {
            Invoke(new PipeServer.ClientDisconnectedHandler(ClientDisconnected));
        }

        void PipeServer_ClientConnected()
        {
            Invoke(new PipeServer.ClientConnectedHandler(ClientConnected));
        }

        void ClientDisconnected()
        {
            RichTextBoxConsole.Text += "\r\nClient disconnected";
        }

        void ClientConnected()
        {
            RichTextBoxConsole.Text += "\r\nClient connected!";
        }

        void UserRegister()
        {
            ListBoxUsers.Items.Add(pipeServer.userInfos[pipeServer.userInfos.Count - 1].Username + " " + pipeServer.userInfos[pipeServer.userInfos.Count - 1].Password);
        }

        void PipeServer_MessageReceived(byte[] message)
        {
            Invoke(new PipeServer.MessageReceivedHandler(DisplayMessageReceived),
                new object[] { message });
        }

        void DisplayMessageReceived(byte[] message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            string str = encoder.GetString(message, 0, message.Length);

            RichTextBoxConsole.Text += "\r\n" + str;
        }

        //Starts server and checks ot see if a user file is found. If found loads users
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            if (File.Exists("users.csv"))
            {
                Console.WriteLine("File found");
                List<PipeServer.UserInfo> records;
                using (var reader = new StreamReader("users.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    records = csv.GetRecords<PipeServer.UserInfo>().ToList();
                }
                for (int i = 0; i < records.Count(); i++)
                {
                    PipeServer.UserInfo newUser = new PipeServer.UserInfo();
                    newUser.Username = records[i].Username;
                    newUser.Password = records[i].Password;
                    pipeServer.userInfos.Add(newUser);
                    ListBoxUsers.Items.Add(newUser.Username + " " + newUser.Password);
                }
                
            }
            if (!pipeServer.Running)
            {
                pipeServer.Start("\\\\.\\pipe\\" + TextBoxPipeName.Text);
                RichTextBoxConsole.Text = "Server Started...";
                ButtonStart.Enabled = false;
            }
            else
                MessageBox.Show("Server already running.");
        }

        //When form closes writes records into a CSV for all users info
        private void PipeServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (var writer = new StreamWriter("users.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(pipeServer.userInfos);
            }
        }
    }
}
