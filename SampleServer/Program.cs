using System.Net;
using System.Threading.Tasks;

using Npgg.Socket;
using System;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SampleServer();

            Task.WaitAll(server.Run(new IPEndPoint(IPAddress.Any, 3939)));
        }
    }

    public class SampleServer : AsyncServer<int>
    {
        public SampleServer() : base(4) { }

        public override int GetPayloadLength(byte[] header) => BitConverter.ToInt32(header);
        int sessionIdSeq = 0;
        public override int OnSessionOpened(TcpClient client)
        {
            int session = Interlocked.Increment(ref sessionIdSeq);
            Console.WriteLine($"session opened : {session}");
            return session;
        }
        public override void OnSessionClosed(int session) => Console.WriteLine($"session closed : {session}");

        public override async Task OnReceiveMessage(int session, byte[] message)
        {
            //do something
            await Task.CompletedTask;
        }

        public override void OnSessionClosed(int session, Exception ex) => Console.WriteLine($"error> {session}\n {ex.ToString()}");
    }
}


 

