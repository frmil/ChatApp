using ServerApp.Common;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServerApp.ViewModel
{
    public class ClientViewModel : BindableBase
    {
        private Client _client;
        private string _newMessage;
        private bool _disconnected;
        private string _buttonContent;

        public ObservableCollection<ChatMessage> MessageList { get; } = new ObservableCollection<ChatMessage>();
        public ClientViewModel()
        {
            _client = new Client(this);
            IPAddress = Utils.GetLocalIPAddress();
            NewMessage = "";
            Port = "8000";
            Disconnected = true;
            StartClientCommand = new DelegateCommand(StartClient);
            SendMessageCommand = new DelegateCommand(SendMessage);
            MessageList.Add(new ChatMessage("Hello"));
            //MessageList.Add(0, new ChatMessage("message from server", false));
        }



        public string NewMessage
        { get { return _newMessage; }
            set
            {
                SetProperty(ref _newMessage, value);
            }
        }

        public string IPAddress { get; set; }
        public string Port { get; set; }

        public bool Disconnected
        {
            get { return _disconnected; }
            set
            {
                if (SetProperty(ref _disconnected, value))
                {
                    if (_disconnected) ButtonContent = "Connect"; else ButtonContent = "Disconnect";
                    OnPropertyChanged(nameof(Connected));
                    OnPropertyChanged(nameof(ButtonContent));
                }
            }
        }

        public bool Connected
        {
            get { return !_disconnected; }
            set { SetProperty(ref _disconnected, !value);
} 
        }

        public string ButtonContent
        {
            get { return _buttonContent; }
            set { SetProperty(ref _buttonContent, value); }
        }

        public ICommand StartClientCommand { get; }
        public ICommand SendMessageCommand { get; }
        

        

        private void StartClient(object obj)
        {
            try
            {
                if (Disconnected)
                {
                    _client.Connect(IPAddress, Port);
                    Disconnected = false;
                } else
                {
                    _client.Disconnect();
                    Disconnected = true;
                }
            }
            catch
            {
                MessageBox.Show($"Cannot connect to {IPAddress}:{Port}");
                Disconnected = true;
            }
        }

        private void SendMessage(object obj)
        {
            if (NewMessage.Length > 0 && Connected)
            {
                _client.SendMessage(NewMessage);
                NewMessage = string.Empty;
            }
        }

    }
}
