using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ServerApp.Server;
using System.IO;
using System.Threading;
using System.Windows;
using System.Net.Http;

namespace ServerApp
{
    public class Server
    {
        #region Delegates & Events
        /**********************************************************************************/
        public delegate void ServerNotify(string message);
        public event ServerNotify OnServerNotify;
        #endregion


        #region Properties
        /**********************************************************************************/
        private const string CloseConnectionWord = "Bye";
        public string ServerName { get; private set; } = "Server: ";
        private string clientName = "Client: ";
        private TcpListener server = null!;
        private TcpClient client;
        private NetworkStream networkStream;
        private List<string> serverAutomaticResponseMessages;
        private Thread serverThread;
        private Random random;

        // Defines whether user or PC answer on client input message
        public bool ResponseModeIsPC { get; set; } = false;
        public int Port { get; set; } = 13000;
        public IPAddress IPAddress { get; set; }
        #endregion


        #region Constructor
        /**********************************************************************************/
        public Server()
        {
            random = new Random(Guid.NewGuid().GetHashCode());
            ResponseModeIsPC = false;
            Port = 13000;
            IPAddress = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(IPAddress, Port);
            // Phrases from Ellochka-Ludoyedka (see "12 Chairs") were used
            serverAutomaticResponseMessages = File.ReadAllLines("ResponseMessages.txt").ToList();

            serverThread = new Thread(ListenToClient);
            serverThread.IsBackground = false;
            serverThread.Start();
        }
        #endregion


        #region Methods
        /**********************************************************************************/
        private void ListenToClient()
        {
            byte[] buffer = new byte[4096];
            string inputMessage = string.Empty;
            try
            {
                // Start server
                server.Start();

                NotifyMessageFromServer(ServerName + "Waiting for connection...");
                // Create Tcp client when it connect to the server
                client = server.AcceptTcpClient();
                NotifyMessageFromServer(ServerName + "Connected");

                // Create network stream from client
                networkStream = client.GetStream();

                // Reading messages from client
                while (true && client.Connected)
                {
                    // read message
                    int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                    inputMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead); // cut off white spaces or whatever
                    Array.Clear(buffer);

                    // notify that server got a message
                    NotifyMessageFromServer(clientName + inputMessage);

                    // check if connection must be closed
                    if (ConnectionMustBeClosed(inputMessage))
                    {
                        CloseConnection();
                    }

                    // Check for automatic response
                    if (ResponseModeIsPC)
                    {
                        // Added some latency for imitation of conversation
                        Thread.Sleep(1000);
                        SendMessageToClient(GenerateServerAutomaticResponse());
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyMessageFromServer(ServerName + $"Error: {ex.Message}");
            }
            finally
            {
                // Stop the server at exit
                server.Stop();
            }
        }
        private string GenerateServerAutomaticResponse()
        {
            if (serverAutomaticResponseMessages.Count > 0)
            {
                int messageIndex = random.Next(serverAutomaticResponseMessages.Count);
                return serverAutomaticResponseMessages[messageIndex];
            }
            return "I have no answer for you";
        }
        public void SendMessageToClient(string message)
        {
            if (networkStream != null && networkStream.CanWrite)
            {
                byte[] outputMessage = Encoding.ASCII.GetBytes(message);
                networkStream.Write(outputMessage, 0, outputMessage.Length);
                NotifyMessageFromServer(ServerName + message);

                // check if connection must be closed
                if (ConnectionMustBeClosed(message))
                {
                    CloseConnection();
                }
            }
            else
            {
                NotifyMessageFromServer(ServerName + "No connection established yet");
            }
        }
        private void NotifyMessageFromServer(string message)
        {
            App.Current?.Dispatcher.Invoke((System.Action)delegate
            {
                OnServerNotify?.Invoke(message);
            });
        }
        private bool ConnectionMustBeClosed(string message)
        {
            bool res = (message.ToUpper() == CloseConnectionWord.ToUpper());
            return res;
        }
        private void CloseConnection()
        {
            networkStream.Close();
            client.Dispose();
            client.Close();
            NotifyMessageFromServer(ServerName + $"Connection closed");
        }
        #endregion
    }
}
