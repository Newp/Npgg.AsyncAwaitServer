using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Npgg.Socket
{
    public static class Extensions
    {

        public static async Task FillAsync(this System.Net.Sockets.Socket socket, byte[] buffer, int length, CancellationToken cancellationToken)
        {
            int offset = 0;
            int rest = length;
            var memory = new Memory<byte>(buffer, 0, length);
      
            while (rest > 0)
            {
                var readLength = await socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);

                if (readLength == 0)
                    throw new Exception("recv 0");

                rest -= readLength;
                offset += readLength;
            }

        }
    }
}
