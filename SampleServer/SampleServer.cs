﻿using System.Threading.Tasks;

using Npgg.Socket;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace ConsoleApp1
{
    public class SampleSession
    {
        public int SessionId { get; set; }
        public TcpClient Client { get; set; }

        public NetworkStream Stream { get; set; }
    }

    public class SampleServer : AsyncServer<SampleSession>
    {
        public SampleServer() : base(4) { }

        public override int GetPayloadLength(byte[] header) => BitConverter.ToInt32(header);

        public override Task OnReceiveMessage(SampleSession session, byte[] header, byte[] message, int length)
        {
            var text = Encoding.ASCII.GetString(message, 0, length);

            Console.WriteLine($"on recv {text}");

            return Task.CompletedTask;
        }

        public override SampleSession OnSessionOpened(TcpClient client)
        {
            int session = Interlocked.Increment(ref sessionIdSeq);
            Console.WriteLine($"session opened : {session}");
            return new SampleSession()
            {
                SessionId = session,
                Client = client,
                Stream = client.GetStream(),
            };
        }

        public override void OnSessionClosed(SampleSession session)
        {
            Console.WriteLine($"error> {session.SessionId}");
        }

        public override void OnSessionClosed(SampleSession session, Exception ex)
        {
            Console.WriteLine($"error> {session.SessionId}\n {ex}");
        }

        int sessionIdSeq = 0;
    }
}


 

