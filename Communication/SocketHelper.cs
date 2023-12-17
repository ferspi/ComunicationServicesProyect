using System.Net.Sockets;

namespace Communication
{
    public class SocketHelper
    {
        private readonly TcpClient _tcpClient;

        public SocketHelper(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public async Task SendAsync(byte[] data)
        {
            int offset = 0;
            int size = data.Length;

            // Necesitamos pedir el stream para enviar
            var networkStream = _tcpClient.GetStream();

            await networkStream.WriteAsync(data, offset, size);
        }

        public async Task<byte[]> ReceiveAsync(int length)
        {
            int offset = 0;
            var data = new byte[length];
            var networkStream = _tcpClient.GetStream();

            try
            {
                while (offset < length)
                {

                    int received = await networkStream.ReadAsync(data, offset, length - offset);

                    offset += received;

                }
                return data;
            }
            catch
            {
                throw new Exception("Cliente desconectado");

            }
        }
    }
}