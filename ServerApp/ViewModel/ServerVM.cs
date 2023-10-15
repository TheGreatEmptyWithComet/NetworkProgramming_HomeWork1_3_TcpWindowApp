using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServerApp
{
    public class ServerVM : NotifyPropertyChangeHandler
    {
        #region Properties
        /**********************************************************************************/
        public Server Server { get; set; }
        public ObservableCollection<string> MessagesLog { get; private set; } = new ObservableCollection<string>();
        private string currentPage;
        public string CurrentPage
        {
            get { return currentPage; }
            set
            {
                currentPage = value;
                NotifyPropertyChanged(nameof(CurrentPage));
            }
        }
        private string outputMessage;
        public string OutputMessage
        {
            get { return outputMessage; }
            set
            {
                outputMessage = value;
                NotifyPropertyChanged(nameof(OutputMessage));
            }
        }
        private string serverIP;
        public string ServerIP
        {
            get { return serverIP; }
            set
            {
                serverIP = value;
                NotifyPropertyChanged(nameof(ServerIP));
            }
        }
        private int serverPort;
        public int ServerPort
        {
            get { return serverPort; }
            set
            {
                serverPort = value;
                NotifyPropertyChanged(nameof(ServerPort));
            }
        }

        #endregion


        #region Commands
        /**********************************************************************************/
        public ICommand PageNavigationCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }


        #endregion


        #region Constructor
        /**********************************************************************************/
        public ServerVM()
        {
            Server = new Server();
            Server.OnServerNotify += (message) => { MessagesLog.Add(message); };
            serverPort = Server.Port;
            serverIP = Server.IPAddress.ToString();

            // set the start page
            CurrentPage = "View\\ConsolePageView.xaml";

            InitCommands();
        }

        #endregion


        #region Methods
        /**********************************************************************************/
        private void InitCommands()
        {
            PageNavigationCommand = new RelayCommand<string>(p => CurrentPage = p);
            SendMessageCommand = new RelayCommand(SendMessageToClient);
            SaveSettingsCommand = new RelayCommand(CheckAndSaveSettings);
        }
        private void SendMessageToClient()
        {
            Server.SendMessageToClient(OutputMessage);
            //MessagesLog.Add(Server.ServerName + OutputMessage);
            OutputMessage = string.Empty;
        }
        private void CheckAndSaveSettings()
        {
            // check server ip
            if (IPAddress.TryParse(ServerIP, out IPAddress iPAddress))
            {
                Server.IPAddress = iPAddress;
            }
            else
            {
                MessageBox.Show("Error: Wrong IP adress");
                ServerIP = Server.IPAddress.ToString();
                return;
            }

            // check port
            if (serverPort > 0)
            {
                Server.Port = serverPort;
            }
            else
            {
                MessageBox.Show("Error: port must be greater than 0");
                ServerPort = Server.Port;
                return;
            }

            MessageBox.Show("Changes were saved successfully");
        }

        #endregion

    }
}
