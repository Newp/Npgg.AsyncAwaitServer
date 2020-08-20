using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            for(int i =0;i<1;i++)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync("rtu.eims.co.kr", 7000);

                var bytes = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                socket.Send(bytes);
            }

        }
    }
}
