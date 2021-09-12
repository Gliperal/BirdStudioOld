﻿using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using TasBird.Link;

namespace BirdStudio
{
    class Message
    {
        private Dictionary<string, string[]> messageArgTypes = new Dictionary<string, string[]>
        {
            { "SaveReplay", new string[]{"string", "string", "int"} },
            { "Frame", new string[]{"int", "float", "float", "float", "float"} }
        };
        private string _type;
        public string type { get => _type; }
        private object[] _args;
        public object[] args { get => _args; }

        public Message(NetworkStream stream)
        {
            _type = Util.ReadString(stream);
            if (!messageArgTypes.ContainsKey(_type))
                throw new FormatException("Unrecognized message type: '" + _type + "'.");
            string[] argTypes = messageArgTypes[_type];
            _args = new object[argTypes.Length];
            for (int i = 0; i < argTypes.Length; i++)
                switch (argTypes[i])
                {
                    case "float":
                        _args[i] = Util.ReadFloat(stream);
                        break;
                    case "string":
                        _args[i] = Util.ReadString(stream);
                        break;
                    case "int":
                        _args[i] = Util.ReadInt(stream);
                        break;
                    default:
                        throw new Exception("programmer dumb");
                }
        }
    }

    class TcpManager
    {
        private static TcpClient tcp;
        private static NetworkStream stream;

        private TcpManager() {}

        public static void connect()
        {
            if (tcp != null && tcp.Connected)
                return;
            tcp = new TcpClient();
            while (true)
            {
                try
                {
                    tcp.Connect("localhost", 13337);
                    stream = tcp.GetStream();
                    return;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public static bool isConnected()
        {
            return tcp.Connected;
        }

        public static Message listenForMessage()
        {
            connect();
            try
            {
                return new Message(stream);
            }
            catch(Exception e) when (e is SocketException || e is FormatException)
            {
                // FormatException: Unable to process the next stream data, but
                // bytes were still consumed. Any attempts to continue with the
                // current stream will likely produce further errors, so force
                // a reconnect.
                // SocketException: Lost connection to TCP server, likely
                // because the game was closed, so disconnect.
                tcp = null;
                return null;
            }
        }

        public static void sendLoadReplayCommand(string levelName, string replayBuffer, int breakpoint)
        {
            if (!tcp.Connected)
                return;
            Util.WriteString(stream, "LoadReplay");
            Util.WriteString(stream, levelName);
            Util.WriteString(stream, replayBuffer);
            Util.WriteInt(stream, breakpoint);
        }
    }
}
