using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NetworkingMessageAttributes;

namespace BorisUnityDev.Networking
{
    public enum MessageProtocol { TCP, UDP }
    class Connection
    {
        public delegate void OnConnectedDelegate(EndPoint endPoint);
        public event OnConnectedDelegate OnConnectedEvent;

        public delegate void OnDisconnectedDelegate();
        public event OnDisconnectedDelegate OnDisconnectedEvent;

        public delegate void OnMessageReceivedDelegate(string message, MessageProtocol protocol);
        public event OnMessageReceivedDelegate OnMessageReceivedEvent;

        #region variables
        public Socket socket;
        public bool connected;

        string ip;
        int port;

        double ms_connectedCheck = 2000;
        DateTime lastConnectedConfirmed;
        #endregion

        // [CONNECT TO SERVER AND START RECEIVING MESSAGES]
        public void Connect(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(remoteEP, new AsyncCallback(ConnectedCallback), socket);
            }
            catch (SocketException se)
            {
                Disconnect();
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }
        void ConnectedCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                lastConnectedConfirmed = DateTime.Now;
                OnConnected(client.RemoteEndPoint);
                connected = true;

                Task listenToIncomingMessages = new Task(ReceiveMessages);
                listenToIncomingMessages.Start();
                connectionChecker = new Task(CheckClientConnected);
                connectionChecker.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[CONNECTION_ERROR]: connection attempt failed - timed out");
            }
        }
        // [RECEIVE MESSAGES FROM SERVER]
        void ReceiveMessages()
        {
            int errorNumber = 0;
            while (connected)
            {
                try
                {
                    string message = ConnectionUtil.ReadLine(socket);
                    if (message.Equals(""))
                    {
                        errorNumber++;
                        if(errorNumber > 25)
                        {
                            Disconnect();
                            break;
                        }
                    }
                    else
                    {
                        lastConnectedConfirmed = DateTime.Now;
                        if(!message.Equals(CHECK_CONNECTED))
                            OnMessageReceived(message);
                    }
                }
                catch (Exception e)
                {
                    Disconnect();
                    break;
                }
            }
        }
        Task connectionChecker;
        void CheckClientConnected()
        {
            while (connected)
            {
                Thread.Sleep(500);

                var msSinceLastConnectionConfirmed = (DateTime.Now - lastConnectedConfirmed).TotalMilliseconds;
                if (msSinceLastConnectionConfirmed > ms_connectedCheck)
                {
                    // Connection timed out, disconnect client
                    Debug.Log($"[CLIENT_MESSAGE]: connection to server [{ip}] timed out");
                    Disconnect();
                }
                else
                {
                    SendMessage(CHECK_CONNECTED, MessageProtocol.TCP);
                }
            }
        }
        // [DISCONNECT FROM THE SERVER]
        public void Disconnect()
        {
            UDP.Disconnect();
            ConnectionUtil.Disconnect(socket);
            connected = false;
            OnDisconnected();
        }

        // [SEND MESSAGE TO SERVER]
        public void SendMessage(string message, MessageProtocol mp = MessageProtocol.TCP)
        {
            //Debug.Log($"Sending Message in Connection:{message}");
            if (mp.Equals(MessageProtocol.TCP))
                if (connected)
                {
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    socket.Send(data);
                    return;
                }

            if (mp.Equals(MessageProtocol.UDP))
            {
                if(UDP.connected)
                    UDP.SendMessageUdp(message);
            }
        }
        public void SendMessage(byte[] message)
        {
            if (connected)
                socket.Send(message);
        }
        // [CALLBACKS]
        void OnConnected(EndPoint endPoint) { OnConnectedEvent?.Invoke(endPoint); }
        void OnDisconnected() { OnDisconnectedEvent?.Invoke(); }
        public void OnMessageReceived(string message, MessageProtocol mp = MessageProtocol.TCP) { OnMessageReceivedEvent?.Invoke(message, mp); }
    }
}


