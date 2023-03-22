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
using System.Windows.Forms;

namespace SocketServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (clinets.ContainsKey(comboBox1.Text))
            {
                byte[] sendData = Encoding.Default.GetBytes(textBox2.Text);
                clinets[comboBox1.Text].Send(sendData);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Socket testSocket = CreateSocket(txtIP.Text, txtPort.Text);
            CreateThread(testSocket);
            textBox1.AppendText("启动服务成功\r\n");
            button1.Enabled = false;
        }
        //原创来自 http://www.luofenming.com/show.aspx?id=ART2018120700001
        /// <summary>
        /// 创建Scoket 服务端
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="prot"></param>
        /// <returns></returns>
        public Socket CreateSocket(string IP, string prot)
        {
            //定义一个套接字用于监听客户端发来的信息  包含3个参数(IP4寻址协议,流式连接,TCP协议)
            Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //发送信息 需要1个IP地址和端口号
            IPAddress ipaddress = IPAddress.Parse(IP); //获取文本框输入的IP地址
                                                       //将IP地址和端口号绑定到网络节点endpoint上 
            IPEndPoint endpoint = new IPEndPoint(ipaddress, int.Parse(prot)); //获取文本框上输入的端口号
                                                                              //套接字点听绑定网络端点
            socketWatch.Bind(endpoint);
            //将套接字的监听队列长度设置为20
            socketWatch.Listen(20);
            return socketWatch;
        }

        /// <summary>
        /// 创建一个负责监听客户端的线程  并启动
        /// </summary>
        /// <returns></returns>
        public void CreateThread(Socket socket)
        {
            //创建一个负责监听客户端的线程 
            Thread threadWatch = new Thread(new ParameterizedThreadStart(WatchConnecting));
            //将窗体线程设置为与后台同步
            threadWatch.IsBackground = true;
            //启动线程
            threadWatch.Start(socket);
        }
        Dictionary<string, Socket> clinets = new Dictionary<string, Socket>();
        /// <summary>
        /// 持续不断监听客户端发来的请求, 用于不断获取客户端发送过来的连续数据信息
        /// </summary>
        private void WatchConnecting(object socket)
        {
            Socket socketWatch = socket as Socket;
            while (true)
            {
                Socket socConnection = null;
                try
                {
                    socConnection = socketWatch.Accept();
                    string ip = ((IPEndPoint)socConnection.RemoteEndPoint).Address.ToString();//获取客户端IP
                    string port = ((IPEndPoint)socConnection.RemoteEndPoint).Port.ToString();//获取客户端端口
                    this.BeginInvoke(new Action(() => { 
                        comboBox1.Items.Add(ip + ":" + port);
                        textBox1.AppendText("客户端" + ip + ":" + port + "连接成功");
                    }));
                    clinets[ip + ":" + port] = socConnection;
                }
                catch (Exception ex)
                {
                    break;//提示套接字监听异常  ex.Message   
                }
                //创建通信线程 
                Thread thr = new Thread(ServerRecMsg);
                thr.IsBackground = true;
                //启动线程 
                thr.Start(socConnection);
            }
        }

        /// <summary>
        /// 接收客户端发来的信息
        /// </summary>
        private void ServerRecMsg(object socketClientPara)
        {
            Socket socketServer = socketClientPara as Socket;
            int ReceiveBufferSize = 8 * 1024;
            string ip = ((IPEndPoint)socketServer.RemoteEndPoint).Address.ToString();//获取客户端IP
            string port = ((IPEndPoint)socketServer.RemoteEndPoint).Port.ToString();//获取客户端端口
            string client = ip + ":" + port;
            while (true)
            {
                int firstReceived = 0;
                byte[] buffer = new byte[ReceiveBufferSize];
                try
                {
                    //获取接收的数据,并存入内存缓冲区  返回一个字节数组的长度
                    if (socketServer != null) firstReceived = socketServer.Receive(buffer);

                    if (firstReceived > 0) //接受到的长度大于0 说明有信息或文件传来
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            
                            textBox1.AppendText(client + "   "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"  \r\n");
                            textBox1.AppendText(Encoding.Default.GetString(buffer.Skip(0).Take(firstReceived).ToArray()) + "\r\n");
                        })); ;
                        //对接收数据进行逻辑判断 回复客户端需要的数据
                        string msg = "接收成功" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                        byte[] datas = Encoding.Default.GetBytes(msg);
                        socketServer.Send(datas);//小数据可以一次性发送  如果大数据要分端发送 用while 循环
                    }
                    else
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            comboBox1.Items.Remove(client);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        comboBox1.Items.Remove(client);
                    }));
                    break;  //捕获异常信息 ex.Message
                }
            }
        }
    }
}
