using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SocketClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //原创来自 http://www.luofenming.com/show.aspx?id=ART2018120700001
        /// <summary>
        /// 创建Scoket 客户端
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="prot"></param>
        /// <returns></returns>
        public Socket CreateSocket(string IP, string prot)
        {
            //定义一个套接字用于监听客户端发来的信息  包含3个参数(IP4寻址协议,流式连接,TCP协议)
            Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //发送信息 需要1个IP地址和端口号
            IPAddress ipaddress = IPAddress.Parse(IP); //获取文本框输入的IP地址
                                                       //将IP地址和端口号绑定到网络节点endpoint上 
            IPEndPoint endpoint = new IPEndPoint(ipaddress, int.Parse(prot)); //获取文本框上输入的端口号
                                                                              //向指定的ip和端口号的服务端发送连接请求 用的方法是Connect 不是Bind
            socketClient.Connect(endpoint);
            return socketClient;
        }
        /// <summary>
        /// 创建一个接收服务端发来信息的  线程并启动
        /// </summary>
        /// <param name="socket"></param>
        private void CreateThread(Socket socket)
        {
            //创建一个新线程 用于监听服务端发来的信息
            Thread threadClient = new Thread(RecMsg);
            //将窗体线程设置为与后台同步
            threadClient.IsBackground = true;
            //启动线程
            threadClient.Start(socket);
        }

        /// <summary>
        /// 接受服务端发来信息的方法
        /// </summary>
        private void RecMsg(object socket)
        {
            Socket socketClient = socket as Socket;
            int SendBufferSize = 2 * 1024;
            while (true) //持续监听服务端发来的消息
            {
                int length = 0;
                byte[] buffer = new byte[SendBufferSize];
                try
                {
                    length = socketClient.Receive(buffer);
                }
                catch (Exception ex)
                {
                    break;
                }
                //将套接字获取到的字节数组转换为人可以看懂的字符串
                string strRecMsg = Encoding.Default.GetString(buffer, 0, length);
                this.BeginInvoke(new Action(() =>
                {
                    textBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH::mm:ss") + "\r\n");
                    textBox1.AppendText(strRecMsg + "\r\n");
                }));
            }
        }
        Socket socket;

        private void button1_Click(object sender, EventArgs e)
        {
            //textBox1.Text 为服务端IP  textBox2.Text 为服务端端口
            socket = CreateSocket(txtIP.Text, txtPort.Text);
            CreateThread(socket);
            button1.Enabled = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] sendData = Encoding.Default.GetBytes(textBox2.Text);
            socket.Send(sendData);
        }
    }
}
