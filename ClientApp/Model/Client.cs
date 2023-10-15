using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ClientApp.Client;
using System.IO;
using System.Threading;
using System.Windows;

namespace ClientApp
{
    public class Client
    {
        #region Delegates & Events
        /**********************************************************************************/
        public delegate void ServerNotify(string message);
        public event ServerNotify OnServerNotify;
        #endregion


        #region Properties
        /**********************************************************************************/
        private string serverName = "Server: ";
        public string ClientName { get; private set; } = "Client: ";
        private TcpListener server = null!;
        private TcpClient client;
        private NetworkStream networkStream;
        private List<string> clientAutomaticResponseMessages;
        private Thread serverThread;
        private Random random;

        // Defines whether user or PC answer on client input message
        public bool ResponseModeIsPC { get; set; } = false;
        public int Port { get; set; } = 13000;
        public IPAddress IPAddress { get; set; }
        #endregion


        #region Constructor
        /**********************************************************************************/
        public Client()
        {
            random = new Random(Guid.NewGuid().GetHashCode());
            ResponseModeIsPC = false;
            Port = 13000;
            IPAddress = IPAddress.Parse("127.0.0.1");
            // Phrases from Ellochka-Ludoyedka (see "12 Chairs") were used
            clientAutomaticResponseMessages = File.ReadAllLines("ResponseMessages.txt").ToList();
        }
        #endregion


        #region Methods
        /**********************************************************************************/
        private void ListenToServer()
        {
            byte[] buffer = new byte[4096];
            string inputMessage = string.Empty;
            try
            {
                // Create Tcp client when it connect to the server
                client = new TcpClient(IPAddress.ToString(), Port);

                // Create network stream from client
                networkStream = client.GetStream();
                NotifyMessageFromServer(serverName + "Connected");

                // Reading messages from server && client.Client.Poll(0, SelectMode.SelectRead)
                while (true && client.Connected)
                {
                    // read message
                    networkStream.Read(buffer, 0, buffer.Length);
                    Array.Clear(buffer);

                    // notify that client got a message
                    NotifyMessageFromServer(serverName + inputMessage);

                    // Check for automatic response
                    if (ResponseModeIsPC)
                    {
                        // Added some latency for imitation of conversation
                        Thread.Sleep(1000);
                        SendMessageToServer(GenerateServerAutomaticResponse());
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                NotifyMessageFromServer(ClientName + $"Error: {ex.Message}");
            }
        }
        private string GenerateServerAutomaticResponse()
        {
            if (clientAutomaticResponseMessages.Count > 0)
            {
                int messageIndex = random.Next(clientAutomaticResponseMessages.Count);
                return clientAutomaticResponseMessages[messageIndex];
            }
            return "I have no answer for you";
        }
        public void SendMessageToServer(string message)
        {
            if (networkStream != null && client.Connected)
            {
                byte[] outputMessage = Encoding.ASCII.GetBytes(message);
                networkStream.Write(outputMessage, 0, outputMessage.Length);
                NotifyMessageFromServer(ClientName + message);
            }
            else
            {
                NotifyMessageFromServer(serverName + "No connection established yet");
            }
        }
        public void ConnectToServer()
        {
            serverThread = new Thread(ListenToServer);
            serverThread.IsBackground = false;
            serverThread.Start();
        }
        private void NotifyMessageFromServer(string message)
        {
            App.Current?.Dispatcher.Invoke((System.Action)delegate
            {
                OnServerNotify?.Invoke(message);
            });
        }
        #endregion
    }
}
