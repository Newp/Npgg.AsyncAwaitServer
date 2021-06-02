using System.Threading.Tasks;

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
    }

    public class SampleServer : AsyncServer<SampleSession>
    {
        public SampleServer() : base(4) { }

        public override int GetPayloadLength(byte[] header) => BitConverter.ToInt32(header);

        public override async Task OnReceiveMessage(SampleSession session, byte[] header, byte[] message, int length)
        {
            var text = Encoding.ASCII.GetString(message, 0, length);

            Console.WriteLine($"on recv {text}");
            var cts = new CancellationTokenSource();

            var stream = session.Client.GetStream();
            try
            {
                Console.WriteLine("ready");
                while (true)
                {
                    cts.CancelAfter(3000);
                    var buffer = new byte[5];
                    await stream.FillAsync(buffer, buffer.Length, cts.Token);

                    var payload = buffer[4..];
                    var readText = Encoding.ASCII.GetString(payload);

                    Console.WriteLine($"recv : {readText} ({BitConverter.ToInt32(buffer, 0)})");
                }
            }
            catch (Exception)
            {
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


 

