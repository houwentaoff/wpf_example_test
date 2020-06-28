using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dmx512OUT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        string buf = "";
        public string Buffer
        {
            get
            {
                return buf;
            }
            set
            {
                buf = value;
                Notify("Buffer");
            }
        }
        public void appendData(byte[] data)
        {
            string s = "";
            foreach (byte b in data)
            {
                s += b.ToString("x2");
                s += " ";
            }
            Buffer += s;
            //textBox.AppendText(s);
        }
        public MainWindow()
        {
            InitializeComponent();
            buffer.DataContext = this;

            IPEndPoint serverIP = new IPEndPoint(0, 8089);
            Socket udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpServer.Bind(serverIP);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)ipep;

            Task.Factory.StartNew(() =>
            {
                byte[] pBuf = new byte[512];
                for (uint i = 0; i < 512; i++)
                {
                    pBuf[i] = 0;
                }
                while (true)
                {
                    int len = udpServer.ReceiveFrom(pBuf, 512, SocketFlags.None, ref Remote);
                    if (len != 512) continue;
                    Buffer = "";
                    appendData(pBuf);
                }
            });
        }
    }
}
