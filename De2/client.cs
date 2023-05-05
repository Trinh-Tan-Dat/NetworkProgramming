using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace De2
{
    public partial class client : Form
    {
        private UdpClient udpClient;
        IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);

        public client()
        {
            InitializeComponent();
            
            
        }
        public void ReceiveMessage()
        {
            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] receiveMess = udpClient.Receive(ref remoteEP);
                String message = Encoding.UTF8.GetString(receiveMess);
                //richTextBox1.Invoke(new Action(() => { richTextBox1.Text = message; }));
                richTextBox1.Text = String.Concat(richTextBox1.Text, message + "\n");
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            udpClient = new UdpClient();
            Byte[] sendBytes = Encoding.UTF8.GetBytes(textBox1.Text);
            udpClient.Send(sendBytes, sendBytes.Length, serverEndpoint);

            Thread.Sleep(300);
            
            Thread thread = new Thread(new ThreadStart(ReceiveMessage));
            thread.Start();

        }
    }
}
