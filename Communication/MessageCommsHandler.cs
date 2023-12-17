using System.Net.Sockets;

namespace Communication
{
    public class MessageCommsHandler
    {
        private readonly ConversionHandler _conversionHandler;
        private readonly SocketHelper _socketHelper;

        public MessageCommsHandler(TcpClient tcpClient)
        {
            _conversionHandler = new ConversionHandler();
            _socketHelper = new SocketHelper(tcpClient);
        }

        public async Task SendMessageAsync(string message)
        {
            // ---> Enviar el largo del mensaje
            await _socketHelper.SendAsync(_conversionHandler.ConvertIntToBytes(message.Length));
            // ---> Enviar el mensaje
            await _socketHelper.SendAsync(_conversionHandler.ConvertStringToBytes(message));
        }

        public async Task<string> ReceiveMessageAsync()
        {
            // ---> Recibir el largo del mensaje
            int msgLength = _conversionHandler.ConvertBytesToInt(await _socketHelper.ReceiveAsync(Protocol.FixedDataSize));
            // ---> Recibir el mensaje
            string message = _conversionHandler.ConvertBytesToString(await _socketHelper.ReceiveAsync(msgLength));
            return message;
        }

        public async Task<float> ReceiveNumberAsync()
        {
            // ---> Recibir el largo del mensaje
            int msgLength = _conversionHandler.ConvertBytesToInt(await _socketHelper.ReceiveAsync(Protocol.FixedDataSize));
            // ---> Recibir el valor
            float message = _conversionHandler.ConvertBytesToFloat(await _socketHelper.ReceiveAsync(msgLength));
            return message;

        }
    }
}

