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

namespace ClientApp
{
    public class ClientVM : NotifyPropertyChangeHandler
    {
        #region Properties
        /**********************************************************************************/
        public Client Client { get; set; }
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
        public ICommand ConnectToServerCommand { get; private set; }
        #endregion


        #region Constructor
        /**********************************************************************************/
        public ClientVM()
        {
            Client = new Client();
            Client.OnServerNotify += (message) => { MessagesLog.Add(message); };
            serverPort = Client.Port;
            serverIP = Client.IPAddress.ToString();

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
            SendMessageCommand = new RelayCommand(SendMessageToServer);
            SaveSettingsCommand = new RelayCommand(CheckAndSaveSettings);
            ConnectToServerCommand = new RelayCommand(Client.ConnectToServer);
        }
        private void SendMessageToServer()
        {
            Client.SendMessageToServer(OutputMessage);
            //MessagesLog.Add(Client.ClientName + OutputMessage);
            OutputMessage = string.Empty;
        }
        private void CheckAndSaveSettings()
        {
            // check server ip
            if (IPAddress.TryParse(ServerIP, out IPAddress iPAddress))
            {
                Client.IPAddress = iPAddress;
            }
            else
            {
                MessageBox.Show("Error: Wrong IP adress");
                ServerIP = Client.IPAddress.ToString();
                return;
            }

            // check port
            if (serverPort > 0)
            {
                Client.Port = serverPort;
            }
            else
            {
                MessageBox.Show("Error: port must be greater than 0");
                ServerPort = Client.Port;
                return;
            }

            MessageBox.Show("Changes were saved successfully");
        }

        #endregion

    }
}
