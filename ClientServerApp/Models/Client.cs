using ServerApp.Common;
using ServerApp.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace ServerApp.Models
{
    public class Client
    {
        private TcpClient _tcpClient = new TcpClient();
        public const int BUFSIZE = 1024;
        byte[] _buffer = new byte[BUFSIZE];
        private ClientViewModel _vm;

        private Timer _connectionTimer = new Timer();
       

        public Client(ClientViewModel vm)
        {
            _vm = vm;
            _connectionTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _connectionTimer.Interval = 1000;
        }

        

        public bool Connect (string ipAddress, string port)
        {
            bool connected = false;

            if (_tcpClient == null) _tcpClient = new TcpClient();

            Console.WriteLine("Connecting.....");
            IPAddress ip;
            if (IPAddress.TryParse(ipAddress, out ip))
            {
                IPEndPoint localEndPoint = new IPEndPoint(ip, Convert.ToInt16(port));
                _tcpClient.Connect(ipAddress, localEndPoint.Port);
                // use the ipaddress as in the server program

                Console.WriteLine("Connected");
                _tcpClient.Client.BeginReceive(_buffer, 0, BUFSIZE, 0, new AsyncCallback(ReadCallback), this);
                _connectionTimer.Start();

            }
            return connected;
        }

        public void Disconnect()
        {
            _connectionTimer.Stop();
            try
            {
                _tcpClient.Client.Disconnect(false);
                _tcpClient.Close();
                _tcpClient = null;
            }
            catch (SocketException) { }
            
            _vm.Disconnected = true;
        }

        private void ReadCallback(IAsyncResult ar)
        {
            if (_tcpClient == null) return;
            try
            {
                int read = _tcpClient.Client.EndReceive(ar);
                if (read > 0)
                {
                    string buffer = Encoding.UTF8.GetString(_buffer, 0, read);
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        _vm.MessageList.Add(new ChatMessage(buffer, false));
                    }));

                    Console.WriteLine("Client Socket read " + read + " bytes: " + buffer);
                    _tcpClient.Client.BeginReceive(_buffer, 0, BUFSIZE, 0, new AsyncCallback(ReadCallback), this);

                }
                else
                {

                    Console.WriteLine(" zero byte read closure");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(" socket exception closure: " + e);
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(" disposed exception closure" + e);
            }
        }

        public void SendMessage(string msg)
        {
            Stream stm = _tcpClient.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(msg);
            Console.WriteLine("Transmitting.....");
            _tcpClient.Client.BeginSend(ba, 0, ba.Length, SocketFlags.None, EndSend, null);
            _vm.MessageList.Add(new ChatMessage(msg));

        }


        private void EndSend(IAsyncResult ar)
        {
            try
            {
                _tcpClient.Client.EndSend(ar);

            }
            catch
            {
                MessageBox.Show("Cannot send message", "Sending message ...");
                Disconnect();

            }
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (!Utils.IsSocketConnected(_tcpClient.Client))
            {
                Disconnect();
            }
        }
    }
}
