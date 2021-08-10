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

        public delegate void OnFailedToConnectDelegate();
        public static event OnFailedToConnectDelegate OnFailedToConnectEvent;

        #region variables
        public static Socket socket_connect;
        public static Socket socket_client;
        public static bool connected;

        static string ip;
        static int portTcp;
        static int portUdp;

        static double ms_connectedCheck = 4000;
        static DateTime lastConnectedConfirmed;

        public const int connectionTimeoutMs = 10000;
        #endregion

        public static void SetConnectionValues(string _ip, int _portTcp, int _portUdp)
        {
            ip = _ip;
            portTcp = _portTcp;
            portUdp = _portUdp;
        }
        // [CONNECT TO SERVER AND START RECEIVING MESSAGES]
        public static void Connect()
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), portTcp);
                socket_connect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult asyncResult = socket_connect.BeginConnect(remoteEP, null, socket_connect);

                bool success = false;
                try
                {
                    success = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(connectionTimeoutMs));
                }
                catch(Exception e)
                {
                    Debug.Log(e.ToString());
                }

                if (success)
                {
                    socket_client = (Socket)asyncResult.AsyncState;
                    socket_client.EndConnect(asyncResult);
                    lastConnectedConfirmed = DateTime.Now;
                    OnConnected(socket_client.RemoteEndPoint);
                    connected = true;

                    Task listenToIncomingMessages = new Task(ReceiveMessages);
                    listenToIncomingMessages.Start();
                    connectionChecker = new Task(CheckClientConnected);
                    connectionChecker.Start();

                    UDP.ConnectTo(ip, portUdp);
                }
                else
                {
                    Disconnect(false);
                    OnFailedToConnect();
                }
            }
            catch (Exception e)
            {
                if(e.Message.Equals("Network is unreachable"))
                {
                    Debug.Log($"Can't connect to the server because [{e.Message}]");
                }else Debug.LogError($"{e}");
                Thread.Sleep(2000); // cooldown
                Disconnect();
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
                    string message = ConnectionUtil.ReadLine(socket_connect);
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
                    Debug.Log(e);
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
        public static void Disconnect(bool notifyDisconnect = true, bool forceUDPclose = false)
        {
            UDP.Disconnect(forceUDPclose);
            ConnectionUtil.Disconnect(socket_connect);
            ConnectionUtil.Disconnect(socket_client);
            connected = false;

            if(notifyDisconnect) OnDisconnected();
        }

        // [SEND MESSAGE TO SERVER]
        public static void SendMessage(string message, MessageProtocol mp = MessageProtocol.TCP)
        {
            //Debug.Log($"Sending Message in Connection:{message}");
            if (mp.Equals(MessageProtocol.TCP))
                if (connected)
                {
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    socket_connect.Send(data);
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
                socket_connect.Send(message);
        }
        // [CALLBACKS]
        static void OnConnected(EndPoint endPoint) { OnConnectedEvent?.Invoke(endPoint); }
        static void OnDisconnected() { OnDisconnectedEvent?.Invoke(); }
        public static void OnMessageReceived(string message, MessageProtocol mp = MessageProtocol.TCP) { OnMessageReceivedEvent?.Invoke(message, mp); }

        static void OnFailedToConnect() { OnFailedToConnectEvent?.Invoke(); }
    }
}


