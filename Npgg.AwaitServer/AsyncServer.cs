﻿using System;
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

        public abstract Task OnReceiveMessage(TSESSION session, byte[] message);

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
                        await Read(stream, headerBuffer, HeaderSize).ConfigureAwait(false);

                        var length = this.GetPayloadLength(headerBuffer);

                        var payloadBuffer = arrayPool.Rent(length);

                        await Read(stream, payloadBuffer, length).ConfigureAwait(false);

                        await OnReceiveMessage(session, payloadBuffer);
                        
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

        async Task Read(NetworkStream stream, byte[] buffer, int length)
        {
            int offset = 0;
            int rest = length;
            while (rest > 0)
            {
                var readLength = await stream.ReadAsync(buffer, offset, rest, CancellationToken.None).ConfigureAwait(false);
                
                if (readLength == 0)
                {
                    throw new Exception("recv 0");
                }
                rest -= readLength;
                offset += readLength;
            }

        }
    }
}
