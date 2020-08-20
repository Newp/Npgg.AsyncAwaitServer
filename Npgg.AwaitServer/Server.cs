using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Npgg.AwaitServer
{
    public abstract class Server<TSESSION>
    {
        public readonly int HeaderSize;

        public abstract int GetBodySize(byte[] header);
        public abstract Task OnReceiveMessage(TSESSION session, byte[] message);

        public abstract TSESSION CreateSession(TcpClient client);

        public abstract TSESSION OnSessionClosed(TSESSION session);

        public abstract TSESSION OnCatchException(TSESSION session, Exception ex);

        public Server(int headerSize)
        {
            HeaderSize = headerSize;
        }

        public async Task Run(IPEndPoint end)
        {
            var listener = new TcpListener(end);
            
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);

                _ = StartReceive( client);
            }
        }

        async Task StartReceive(TcpClient tcpClient)
        {
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            TSESSION session = this.CreateSession(tcpClient);
            try
            {
                var headerBuffer = new byte[HeaderSize];
                var arrayPool = ArrayPool<byte>.Shared;

                using (var stream = tcpClient.GetStream())
                {
                    while (true)
                    {
                        await fill(stream, headerBuffer, HeaderSize).ConfigureAwait(false);

                        var length = this.GetBodySize(headerBuffer);
                        if (length == 0)
                        {
                            return;
                        }

                        var payloadBuffer = arrayPool.Rent(length);
                        await fill(stream, payloadBuffer, length).ConfigureAwait(false);

                        await OnReceiveMessage(session, payloadBuffer);
                        
                        arrayPool.Return(payloadBuffer);
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnCatchException(session, ex);
            }
            finally
            {
                tcpClient.Close();
                OnSessionClosed(session);
            }
        }

        async Task fill(NetworkStream stream, byte[] buffer, int rest)
        {
            int offset = 0;
            while (rest > 0)
            {
                var length = await stream.ReadAsync(buffer, offset, rest, CancellationToken.None).ConfigureAwait(false);
                
                if (length == 0)
                {
                    throw new Exception("recv 0");
                }
                rest -= length;
                offset += length;
            }

        }
    }
}
