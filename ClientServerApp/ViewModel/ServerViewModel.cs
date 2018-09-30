using ServerApp.Common;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServerApp.ViewModel
{
    public class ServerViewModel: BindableBase
    {
        private Server _server;
        private string _logMsg;
        private bool _connected;

        public ServerViewModel()
        {
            Connected = false;
            _server = new Server(this);
            StartServerCommand = new DelegateCommand(StartServer);
            StopServerCommand = new DelegateCommand(StopServer);
        }

        public ICommand StartServerCommand { get; }
        public ICommand StopServerCommand { get; }

        public bool Connected
        {
            get { return _connected; }
            set
            {
                if (SetProperty(ref _connected, value))
                {
                    OnPropertyChanged(nameof(Disconnected));
                }
            }
        }

        public bool Disconnected
        {
            get { return !_connected; }
            set
            {
                SetProperty(ref _connected, !value);
            }
        }

        public string LogMsg
        {
            get { return _logMsg; }
            set
            {
                string str = value + "\n" + _logMsg;
                SetProperty(ref _logMsg, str);
            }
        }

        private void StartServer(object obj)
        {
            String strHostName = string.Empty;
            // Getting Ip address of local machine...
            // First get the host name of local machine.
            strHostName = Dns.GetHostName();
            LogMsg = "Local Machine's Host Name: " + strHostName;
            LogMsg = Utils.GetLocalIPAddress() + ":8000";

            LogMsg = "Starting server ...";
            _server.StartServer();
            LogMsg = "Server started ...";
            Connected = true;
        }

        private void StopServer(object obj)
        {
            _server.StopServer();
            Connected = false;
        }




    }
}
