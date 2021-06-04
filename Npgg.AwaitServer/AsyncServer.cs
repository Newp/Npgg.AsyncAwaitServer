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
            this.listener.Start();
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
            var socket = tcpClient.Client;
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            TSESSION session = this.OnSessionOpened(tcpClient);
            try
            {
                var headerBuffer = new byte[HeaderSize];
                var arrayPool = ArrayPool<byte>.Shared;

                
                while (true)
                {
                    Console.WriteLine($"connected : {socket.Connected}");
                    using var cts = new CancellationTokenSource();
                    try
                    {
                        cts.CancelAfter(3000); //헤더를 3초안에 받지 못하면 다음 루프로 = OperationCanceledException
                        await socket.FillAsync(headerBuffer, HeaderSize, cts.Token).ConfigureAwait(false);

                        var length = this.GetPayloadLength(headerBuffer);

                        var payloadBuffer = arrayPool.Rent(length);

                        await socket.FillAsync(payloadBuffer, length, CancellationToken.None).ConfigureAwait(false);

                        await OnReceiveMessage(session, headerBuffer, payloadBuffer, length);

                        arrayPool.Return(payloadBuffer);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"OperationCanceledException => connected :  {socket.Connected}");
                        continue;
                    }
                    catch (Exception)
                    {

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
