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
        public static Task FillAsync(this TcpClient client, byte[] buffer, int length)=> client.GetStream().FillAsync(buffer, length);

        public static async Task FillAsync(this NetworkStream stream, byte[] buffer, int length)
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
