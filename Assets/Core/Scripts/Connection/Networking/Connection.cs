using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                OnConnected(client.RemoteEndPoint);
                connected = true;

                Task listenToIncomingMessages = new Task(ReceiveMessages);
                listenToIncomingMessages.Start();
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
        // [DISCONNECT FROM THE SERVER]
        public void Disconnect()
        {
            ConnectionUtil.Disconnect(socket);
            connected = false;
            OnDisconnected();
        }

        // [SEND MESSAGE TO SERVER]
        public void SendMessage(string message, MessageProtocol mp = MessageProtocol.TCP)
        {
            Debug.Log($"Sending Message in Connection:{message}");
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


