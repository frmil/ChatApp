using ServerApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.Models
{
    public class ClientInformation
    {
        public const int BUFSIZE = 1024;
        byte[] _buffer = new byte[BUFSIZE];

        private Server _server;
        private Socket ClientSocket { get; set; }
        public ClientInformation(Socket clientSocket, int id, Server server)
        {
            ClientSocket = clientSocket;
            Id = id;
            _server = server;
        }

        public int Id { get; set; }

        public void BeginReceive()
        {
            ClientSocket.BeginReceive(_buffer, 0, BUFSIZE, 0, new AsyncCallback(ReadCallback), this);
        }

        public void SendMessage (byte[] buffer, int length)
        {
            ClientSocket.BeginSend(buffer, 0, length, SocketFlags.None, EndSendMessage, null);
        }

        public void Disconnect()
        {
            _server.AddLogMsg($"Disconnecting client{Id}");
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            _server.AddLogMsg($"Client{Id} disconnected");
        }

        public bool IsConnected()
        {
            return Utils.IsSocketConnected(ClientSocket);
        }

        private void EndSendMessage(IAsyncResult ar)
        {
            try { ClientSocket.EndSend(ar); }
            catch { }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                int read = ClientSocket.EndReceive(ar);
                if (read > 0)
                {
                    string buffer = Encoding.UTF8.GetString(_buffer, 0, read);
                    Console.WriteLine("ID: " + Id + " Socket read " + read + " bytes: " + buffer);
                    ClientSocket.BeginReceive(_buffer, 0, BUFSIZE, 0, new AsyncCallback(ReadCallback), this);
                    _server.AddLogMsg($"Sending message '{buffer.ToString()}' from client {Id}");
                    _server.BroadcastMessage(_buffer, Id, read);
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
    }
}
