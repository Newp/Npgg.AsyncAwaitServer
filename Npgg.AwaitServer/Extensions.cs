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

        public static async Task FillAsync(this NetworkStream stream, byte[] buffer, int length, CancellationToken cancellationToken)
        {
            int offset = 0;
            int rest = length;
            while (rest > 0)
            {
                var readTask = stream.ReadAsync(buffer, offset, rest);

                readTask.Wait(cancellationToken);

                var readLength = readTask.Result;

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
