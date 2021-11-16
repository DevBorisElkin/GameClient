using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using static NetworkingMessageAttributes;
using static BorisUnityDev.Networking.Connection;
using UniRx;

namespace BorisUnityDev.Networking
{
    public static class ConnectionUtil
    {
        // #Listen and return message
        static byte[] bytesArray;
        public static string ReadLine(Socket socket)
        {
            bytesArray = new byte[1024];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;

            do
            {
                bytes = socket.Receive(bytesArray, bytesArray.Length, 0);
                builder.Append(Encoding.Unicode.GetString(bytesArray, 0, bytes));
            }
            while (socket.Available > 0);

            return builder.ToString();
        }

        public static void Disconnect(Socket socket)
        {
            if (socket != null)
            {
                if (socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
                else socket.Close();
                socket.Dispose();
                socket = null;
            }
        }
        public static void DisconnectUDP(Socket socket)
        {
            if (socket != null)
            {
                if (socket.Connected)
                    socket.Close();
                socket.Dispose();
                socket = null;
            }
        }

        public static ReactiveProperty<int> LastTcpPing;
        public static ReactiveProperty<int> LastUdpPing;

        public static void InitReactivePingProperties()
        {
            LastTcpPing = new ReactiveProperty<int>();
            LastUdpPing = new ReactiveProperty<int>();
        }

        // returns true if not belongs to check_connected_echo
        public static bool OnCheckConnectedEchoTCP(string message)
        {
            if (!message.Contains(CHECK_CONNECTED_ECHO_TCP)) return true;
            try
            {
                string[] substrings = message.Split('|');
                int echoId = Int32.Parse(substrings[1]);

                if (echoId == TCP_pingCheckId)
                {
                    int delayMs = (int)(DateTime.Now - TCP_pingCheckRecordedTime).TotalMilliseconds;
                    UnityThread.executeInUpdate(() => { LastTcpPing.Value = delayMs; });
                    //Debug.Log($"Ping TCP: {delayMs}");
                }
            }
            catch (Exception e) { Debug.Log(e); }
            return false;
        }
        // returns true if not belongs to check_connected_echo
        public static bool OnCheckConnectedEchoUDP(string message)
        {
            if (!message.Contains(CHECK_CONNECTED_ECHO_UDP)) return true;
            try
            {
                string[] substrings = message.Split('|');
                int echoId = Int32.Parse(substrings[1]);

                if (echoId == UDP_pingCheckId)
                {
                    int delayMs = (int)(DateTime.Now - UDP_pingCheckRecordedTime).TotalMilliseconds;
                    UnityThread.executeInUpdate(() => { LastUdpPing.Value = delayMs; });
                    //Debug.Log($"Ping UDP: {delayMs}");
                }
            }
            catch (Exception e) { Debug.Log(e); }
            return false;
        }
    }
}
