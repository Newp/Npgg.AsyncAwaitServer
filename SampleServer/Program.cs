using System.Net;
using System.Threading.Tasks;

using Npgg.Socket;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new SampleServer();

            int port = 3939;
            _ = server.Run(new IPEndPoint(IPAddress.Any, port));
            //Task.WaitAll());

            await Task.Delay(100);

            await client.ConnectAsync("127.0.0.1", port);

            client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

            await Send(new byte[] { 1, 2, 3, 4 });

            Console.WriteLine("go");
            while(true)
            {
                var xx = Console.ReadKey(true);
                var encoded = Encoding.ASCII.GetBytes(new char[] { xx.KeyChar });
                await Send(encoded);
            }
        }
        static TcpClient client = new TcpClient();

        static async Task Send(byte[] payload)
        {
            var length = BitConverter.GetBytes(payload.Length);

            var message = length.Concat(payload).ToArray();

            var stream = client.GetStream();
            await stream.WriteAsync(message, 0, message.Length, CancellationToken.None);
        }
    }


    public class SampleSession
    {
        public int SessionId { get; set; }
        public TcpClient Client { get; set; }
    }

    public class SampleServer : AsyncServer<SampleSession>
    {
        public SampleServer() : base(4) { }

        public override int GetPayloadLength(byte[] header) => BitConverter.ToInt32(header);

        public override async Task OnReceiveMessage(SampleSession session, byte[] header, byte[] message, int length)
        {
            var cts = new CancellationTokenSource();
            session.Client.ReceiveTimeout = 3000;
            try
            {
                var stream = session.Client.GetStream();
                Console.WriteLine("ready");
                while (true)
                {
                    cts.CancelAfter(3000);
                    var buffer = new byte[5];
                    await stream.FillAsync(buffer, buffer.Length, cts.Token);
                    
                    Console.WriteLine($"{BitConverter.ToInt32(buffer, 0)} // {buffer[4]}");
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Connected : {session.Client.Connected}");
                Console.WriteLine("passed");
            }
            
        }

        public override SampleSession OnSessionOpened(TcpClient client)
        {
            int session = Interlocked.Increment(ref sessionIdSeq);
            Console.WriteLine($"session opened : {session}");
            return new SampleSession()
            {
                SessionId = session,
                Client = client,
            };
        }

        public override void OnSessionClosed(SampleSession session)
        {
        }

        public override void OnSessionClosed(SampleSession session, Exception ex)
        {
            Console.WriteLine($"error> {session.SessionId}\n {ex.ToString()}");
        }

        int sessionIdSeq = 0;
    }
}


 

