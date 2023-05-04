using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace De7
{
    public partial class Client : Form
    {
        TcpClient client = null;
        byte[] buffer = new byte[1024];

        public Client()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (check)
            {
                try
                {
                    count++;
                    string serverIP = "127.0.0.1";
                    int serverPort = int.Parse("8080");
                    client = new TcpClient(serverIP, serverPort);
                    cancellationTokenSource = new CancellationTokenSource();
                    _ = Task.Run(() => ListenForMessages(cancellationTokenSource.Token));
                    string message = "New client connected from: 127.0.0.1 "  + "\r\n";
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    NetworkStream stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                    textBox1.Select();
                    button1.Text = "Stop";
                    check = false;
                    Task.Delay(7000);
                    StartC();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to connect to server: " + ex.Message + "\n" + "Maybe sever not listen");
                    StopC();
                }
            }
            else
            {
                check = true;
                button1.Text = "Connect";
                cancellationTokenSource.Cancel();
                client?.Close();
                stream?.Close();
            }
        }
        int count = 0;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationTokenSource cancellationTokenSource2;
        private async void StartC()
        {
            cancellationTokenSource2 = new CancellationTokenSource();

            try
            {
                while (!cancellationTokenSource2.IsCancellationRequested)
                {
                    await Task.Delay(3000, cancellationTokenSource2.Token);
                    Connect();
                }
            }
            catch (TaskCanceledException) { }
        }

        private void StopC()
        {
            cancellationTokenSource2.Cancel();
        }
        void Connect()
        {
            try
            {
                string serverIP = "127.0.0.1";
                int serverPort = int.Parse("8080");
                client = new TcpClient(serverIP, serverPort);
            }
            catch
            {
                this.Invoke((MethodInvoker)delegate {
                    button1.PerformClick();
                });
            }
        }


        bool check = true;
        NetworkStream stream;
        private void ListenForMessages(CancellationToken cancellationToken)
        {
            stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    AddMessageToChatWindow(message);
                }
                catch
                {
                    MessageBox.Show("Server has disconnected.");
                    client.Close();
                    break;
                }
            }
        }
        private void AddMessageToChatWindow(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddMessageToChatWindow(message)));
                return;
            }
            richTextBox1.Text = String.Concat(richTextBox1.Text, message + "\r\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string message = textBox1.Text;
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
                textBox1.Text = "";
            }
            catch
            {
                MessageBox.Show("Kết nối trở lại!");
                // button1.PerformClick();
            }
        }
    }
}
