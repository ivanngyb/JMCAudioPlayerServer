using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            TextBoxPipeName.Text = "jmcaudio";
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

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            if (!pipeServer.Running)
            {
                pipeServer.Start("\\\\.\\pipe\\" + TextBoxPipeName.Text);
                RichTextBoxConsole.Text = "Server Started...";
                ButtonStart.Enabled = false;
            }
            else
                MessageBox.Show("Server already running.");
        }
    }
}
