﻿using System.Net;
using System.Threading.Tasks;
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

            await Send(Encoding.ASCII.GetBytes("CONNECT"));

            var readBuffer = new byte[100];
            while(true)
            {
                var xx = Console.ReadKey(true);
                var encoded = Encoding.ASCII.GetBytes(new char[] { xx.KeyChar });

                Console.WriteLine($"send : {xx.KeyChar}");

                await Send(encoded);

                var stream = client.GetStream();
                
                //var ack = await stream.ReadAsync(readBuffer, 0, readBuffer.Length);

                //Console.WriteLine("ack :" + ack);
            }
        }
        static TcpClient client = new TcpClient();

        static async Task Send(byte[] payload)
        {
            var length = BitConverter.GetBytes(payload.Length);

            var message = length.Concat(payload).ToArray();

            var stream = client.GetStream();
            Console.WriteLine("send : " + payload.Length);
            await stream.WriteAsync(message, 0, message.Length, CancellationToken.None);
        }
    }


}