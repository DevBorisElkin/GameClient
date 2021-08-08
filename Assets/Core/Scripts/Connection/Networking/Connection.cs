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
    static class Connection
    {
        public delegate void OnConnectedDelegate(EndPoint endPoint);
        public static event OnConnectedDelegate OnConnectedEvent;

        public delegate void OnDisconnectedDelegate();
        public static event OnDisconnectedDelegate OnDisconnectedEvent;

        public delegate void OnMessageReceivedDelegate(string message, MessageProtocol protocol);
        public static event OnMessageReceivedDelegate OnMessageReceivedEvent;

        #region variables
        public static Socket socket;
        public static bool connected;

        static string ip;
        static int portTcp;
        static int portUdp;

        static double ms_connectedCheck = 3000;
        static DateTime lastConnectedConfirmed;
        #endregion

        // [CONNECT TO SERVER AND START RECEIVING MESSAGES]
        public static void Connect(string _ip, int _portTcp, int _portUdp)
        {
            ip = _ip;
            portTcp = _portTcp;
            portUdp = _portUdp;
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), portTcp);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(remoteEP, new AsyncCallback(ConnectedCallback), socket);
            }
            catch (SocketException se)
            {
                Console.WriteLine($"{se.Message} {se.StackTrace}");
                Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message} {e.StackTrace}");
                Disconnect();
            }
        }
        static void ConnectedCallback(IAsyncResult ar)
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

                UDP.ConnectTo(ip, portUdp);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[CONNECTION_ERROR]: connection attempt failed - timed out");
            }
        }
        // [RECEIVE MESSAGES FROM SERVER]
        static void ReceiveMessages()
        {
            int errorNumber = 0;
            while (connected)
            {
                try
                {
                    string message = ConnectionUtil.ReadLine(socket);
                    if (message.Equals(string.Empty))
                    {
                        Debug.Log("[TCP]: Received empty message from server");
                        errorNumber++;
                        if(errorNumber > 25)
                        {
                            Debug.Log($"[TCP]: {errorNumber} empty messages from server, disconnecting");
                            Disconnect();
                            break;
                        }
                    }
                    else
                    {
                        lastConnectedConfirmed = DateTime.Now;
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
        static Task connectionChecker;
        static void CheckClientConnected()
        {
            while (connected)
            {
                Thread.Sleep(500);

                var msSinceLastConnectionConfirmed = (DateTime.Now - lastConnectedConfirmed).TotalMilliseconds;
                if (msSinceLastConnectionConfirmed > ms_connectedCheck)
                {
                    // Connection timed out, disconnect client
                    Debug.Log($"[CLIENT_MESSAGE]: connection to server [{ip}] timed out [{msSinceLastConnectionConfirmed} > {ms_connectedCheck}]");
                    Disconnect();
                }
                else
                {
                    SendMessage(CHECK_CONNECTED+"<EOF>", MessageProtocol.TCP);
                }
            }
        }
        // [DISCONNECT FROM THE SERVER]
        public static void Disconnect()
        {
            UDP.Disconnect();
            ConnectionUtil.Disconnect(socket);
            connected = false;
            OnDisconnected();
        }

        // [SEND MESSAGE TO SERVER]
        public static void SendMessage(string message, MessageProtocol mp = MessageProtocol.TCP)
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
        public static void SendMessage(byte[] message)
        {
            if (connected)
                socket.Send(message);
        }
        // [CALLBACKS]
        static void OnConnected(EndPoint endPoint) { OnConnectedEvent?.Invoke(endPoint); }
        static void OnDisconnected() { OnDisconnectedEvent?.Invoke(); }
        public static void OnMessageReceived(string message, MessageProtocol mp = MessageProtocol.TCP) { OnMessageReceivedEvent?.Invoke(message, mp); }
    }
}


