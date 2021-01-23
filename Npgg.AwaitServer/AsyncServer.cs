using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Npgg.Socket
{
    public abstract class AsyncServer<TSESSION>
    {
        public readonly int HeaderSize;

        public abstract int GetPayloadLength(byte[] header);

        public abstract Task OnReceiveMessage(TSESSION session, byte[] header, byte[] message, int length);

        public abstract TSESSION OnSessionOpened(TcpClient client);

        public abstract void OnSessionClosed(TSESSION session);

        public abstract void OnSessionClosed(TSESSION session, Exception ex);

        public AsyncServer(int headerSize)
        {
            HeaderSize = headerSize;
        }

        TcpListener listener = null;

        public async Task Run(IPEndPoint end)
        {
            this.listener = new TcpListener(end);

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);

                _ = StartReceive(client);
            }
        }
        public void Stop()
        {
            this.listener.Stop();
        }

        async Task StartReceive(TcpClient tcpClient)
        {
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            TSESSION session = this.OnSessionOpened(tcpClient);
            try
            {
                var headerBuffer = new byte[HeaderSize];
                var arrayPool = ArrayPool<byte>.Shared;

                using (var stream = tcpClient.GetStream())
                {
                    while (true)
                    {
                        await stream.FillAsync(headerBuffer, HeaderSize).ConfigureAwait(false);

                        var length = this.GetPayloadLength(headerBuffer);

                        var payloadBuffer = arrayPool.Rent(length);

                        await stream.FillAsync(payloadBuffer, length).ConfigureAwait(false);

                        await OnReceiveMessage(session, headerBuffer, payloadBuffer, length);
                        
                        arrayPool.Return(payloadBuffer);
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnSessionClosed(session, ex);
            }
            finally
            {
                tcpClient.Close();
                OnSessionClosed(session);
            }
        }

        
    }
}
