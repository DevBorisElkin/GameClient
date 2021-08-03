using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BorisUnityDev.Networking
{
    static class UDP
    {
        static IPEndPoint remoteEpUdp;
        static Socket udpSocket;

        static Connection connection;
        static string address;
        static int portUdp;

        static Task taskListenUDP;
        public static void ConnectTo(string _address, int _port, Connection _connection)
        {
            address = _address;
            portUdp = _port;
            connection = _connection;
            try
            {
                remoteEpUdp = new IPEndPoint(IPAddress.Any, portUdp);
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpSocket.Bind(remoteEpUdp);

                taskListenUDP = new Task(ListenUDP);
                taskListenUDP.Start();
            }
            catch (Exception e)
            {
                Debug.Log($"Unexpected exception : {e}");
            }
        }

        public static void SendMessageUdp(string message)
        {
            EndPoint remotePoint = new IPEndPoint(IPAddress.Parse(address), portUdp);
            byte[] data = Encoding.Unicode.GetBytes(message);
            udpSocket.SendTo(data, remotePoint);
        }
        static StringBuilder builder;
        public static bool connected;
        private static void ListenUDP()
        {
            try
            {
                DelayedInitCall();

                connected = true;

                byte[] data = new byte[1024];
                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, portUdp);

                int bytes;
                while (connected)
                {
                    builder = new StringBuilder();
                    do
                    {
                        bytes = udpSocket.ReceiveFrom(data, ref remoteIp);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (udpSocket.Available > 0);

                    connection.OnMessageReceived(builder.ToString(), MessageProtocol.UDP);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + " ||| " + ex.StackTrace);
            }
            finally
            {
                Disconnect();
            }
        }

        private static void DelayedInitCall()
        {
            Thread.Sleep(1000);
            SendMessageUdp("init_udp");
        }
        public static void Disconnect()
        {
            Debug.Log("[SYSTEM_MESSAGE]: closed udp");
            connected = false;
            if (udpSocket != null)
            {
                udpSocket.Shutdown(SocketShutdown.Both);
                udpSocket.Close();
                udpSocket = null;
            }
            //taskListenUDP.Dispose();
        }
    }
}
