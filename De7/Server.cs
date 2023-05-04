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
using System.IO;
namespace De7
{
    public partial class Server : Form
    {
        TcpListener server = null;
        List<TcpClient> clients = new List<TcpClient>();
        private bool isRunning;
        public Server()
        {
            InitializeComponent();
        }
        IPAddress ipAddress;
        bool check = true;
        private void button1_Click(object sender, EventArgs e)
        {
            if (check)
            {
                ipAddress = IPAddress.Parse("127.0.0.1");
                int port = int.Parse("8080");
                server = new TcpListener(ipAddress, port);
                server.Start();
                richTextBox1.Text = String.Concat(richTextBox1.Text, "Server is running on 127.0.0.1: 8080\r\n");
                Task.Run(() => ListenForClients());
                button1.Text = "Stop";
                check = false;
            }
            else
            {
                button1.Text = "Listen";
                richTextBox1.Text = String.Concat(richTextBox1.Text, "Server has stopped on 127.0.0.1: 8080\r\n");
                Disconnect();
                check = true;
            }
        }
        void Disconnect()
        {
            server?.Stop();
            stream?.Close();
            // client?.Close();
        }
        private async Task ListenForClients()
        {
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                Task.Run(() => HandleClientMessages(client));
            }
        }

        NetworkStream stream;
        private async Task<string> ReadFromStreamAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0)
            {
                return null;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Nếu buffer chứa dữ liệu còn lại, đệ quy đọc tiếp
            if (stream.DataAvailable)
            {
                string remainingData = await ReadFromStreamAsync(stream);
                if (remainingData != null)
                {
                    message += remainingData;
                }
            }

            return message;
        }

        private async void HandleClientMessages(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                try
                {
                    string message = await ReadFromStreamAsync(stream);
                    if (message == null)
                    {
                        break;
                    }

                    if (message.Contains("New client connected from: 127.0.0.1"))
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            richTextBox1.Text = String.Concat(richTextBox1.Text, message);
                        });
                    }
                    else
                    {
                        BroadcastMessage(message, client);

                        this.Invoke((MethodInvoker)delegate {
                            richTextBox1.Text = String.Concat(richTextBox1.Text, message + "\r\n");
                        });
                    }
                }
                catch
                {
                    clients.Remove(client);
                    break;
                }
            }
        }


        private void BroadcastMessage(string message, TcpClient sender)
        {
            String content;   
            if (File.Exists(message))
            {
                content = File.ReadAllText(message);
                // Đọc nội dung của file
            }
            else
            {
                content = "File not found!";
            }
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            foreach (TcpClient client in clients)
            {
                if (client != sender)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            await stream.WriteAsync(buffer, 0, buffer.Length);
                        }
                        catch
                        {
                            clients.Remove(client);
                        }
                    });
                }
            }
        }

    }
}
