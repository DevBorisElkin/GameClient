using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

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
    }
}
