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

        static string address;
        static int portUdp;

        static Task taskListenUDP;
        public static void ConnectTo(string _address, int _port)
        {
            address = _address;
            portUdp = _port;
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
                Debug.Log($"Unexpected exception : {e} || {e.StackTrace} || {e.Source}");
                Disconnect(true);
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
                //DelayedInitCall();
                //SendMessageUdp("init_udp");

                connected = true;

                byte[] data = new byte[1024];
                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, portUdp);

                int bytes;
                while (connected)
                {
                    try
                    {
                        builder = new StringBuilder();
                        do
                        {
                            bytes = udpSocket.ReceiveFrom(data, ref remoteIp);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (udpSocket.Available > 0);

                        string message = builder.ToString();
                        if (message.Equals(""))
                        {
                            Debug.Log("[UDP]: Received empty message from server");
                        }
                        Connection.OnMessageReceived(message, MessageProtocol.UDP);
                    }
                    catch(Exception e)
                    {
                        Debug.Log("ByDesign:" + e);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + " ||| " + ex.StackTrace);
            }
            finally
            {
                Disconnect(false);
            }
        }

        public static void Disconnect(bool forceClose = false)
        {
            if (!connected && !forceClose) return;
            connected = false;
            Debug.Log("[SYSTEM_MESSAGE]: closed udp");
            ConnectionUtil.DisconnectUDP(udpSocket);
        }
    }
}
