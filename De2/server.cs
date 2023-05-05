using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Script.Serialization;
namespace De2
{
    public partial class server : Form
    {
        public server()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        public void serverThread()
        {

            UdpClient udpClient = new UdpClient(8080);
            try
            {
                while (true)
                {
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    string mess = RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString();
                    InfoMessage(mess);
                    //


                    string url = String.Format
                    ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
                     "en", "vi", Uri.EscapeUriString(mess));
                    HttpClient httpClient = new HttpClient();
                    string result = httpClient.GetStringAsync(url).Result;
                    var jsonData = new JavaScriptSerializer().Deserialize<List<dynamic>>(result);
                    var translationItems = jsonData[0];
                    string translation = "";
                    foreach (object item in translationItems)
                    {
                        IEnumerable translationLineObject = item as IEnumerable;
                        IEnumerator translationLineString = translationLineObject.GetEnumerator();
                        translationLineString.MoveNext();
                        translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
                    }
                    if (translation.Length > 1) { translation = translation.Substring(1); };


                    //
                    byte[] responseData = Encoding.UTF8.GetBytes("Server: " + translation);
                    udpClient.Send(responseData, responseData.Length, RemoteIpEndPoint);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }
        }
        public void InfoMessage(String mess)
        {
            richTextBox1.Text = String.Concat(richTextBox1.Text, mess + "\n");
        }

        private void server_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread thdUDPServer = new Thread(new ThreadStart(serverThread));
            thdUDPServer.Start();
        }
    }
}
