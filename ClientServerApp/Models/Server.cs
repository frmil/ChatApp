using ServerApp.Common;
using ServerApp.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace ServerApp.Models
{
    public class Server
    {
        private Socket _socket;
        private Hashtable _clients = new Hashtable();
        private int _nextId = 0;
        private ServerViewModel _vm;
        private Timer _connectionTimer = new Timer();

        public Server(ServerViewModel vm)
        {
            _connectionTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _connectionTimer.Interval = 100;
            _vm = vm;
        }

        public void AddLogMsg (string msg)
        {
            _vm.LogMsg = msg;
        }

        public void StartServer()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 8000);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(localEndPoint);
            _socket.Listen(10);
            // Start the accept process. When a connection is accepted, the callback
            // must do this again to accept another connection
            _socket.BeginAccept(new AsyncCallback(AcceptCallback), _socket);
            _connectionTimer.Start();
        }

        public void StopServer()
        {
            _connectionTimer.Stop();
            _vm.LogMsg = "Stopping server";

            List<int> ids = new List<int>();
            foreach (ClientInformation cl in _clients.Values)
            {
                cl.Disconnect();
                ids.Add(cl.Id);
            }
            foreach (int id in ids)
            {
                _clients.Remove(id);
            }

            _vm.LogMsg = "Server disconnected";

            //_socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket = null;

        }

        public void BroadcastMessage(byte[] buffer, int fromClientId, int read)
        {
           
            foreach (ClientInformation cl in _clients.Values)
            {
                if (cl.Id != fromClientId)
                {
                    _vm.LogMsg = $"Send message to Client{cl.Id}";
                    cl.SendMessage(buffer, read);
                }
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket server = (Socket)ar.AsyncState;
                Socket clientSock = server.EndAccept(ar);

                // Start the thing listening again
                server.BeginAccept(new AsyncCallback(AcceptCallback), server);

                ClientInformation client = new ClientInformation(clientSock, ++_nextId, this);
                client.BeginReceive();
                //add to collection
                _clients.Add(client.Id, client);
                _vm.LogMsg = $"Client{client.Id} connected";
            }
            catch (SocketException) { }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured " + e);
            }
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            List<int> ids = new List<int>();
            foreach (ClientInformation cl in _clients.Values)
            {
                if (!cl.IsConnected())
                {
                    _vm.LogMsg = $"Client{cl.Id} disconnected";
                    cl.Disconnect();
                    ids.Add(cl.Id);
                }
            }
            foreach (int id in ids)
            {
                _clients.Remove(id);
            }
                
        }


    }
}
