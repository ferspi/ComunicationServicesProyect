using System.Net.Sockets;

namespace Communication
{
    public class FileCommsHandler
    {
        private readonly ConversionHandler _conversionHandler;
        private readonly FileHandler _fileHandler;
        private readonly FileStreamHandler _fileStreamHandler;
        private readonly SocketHelper _socketHelper;

        public FileCommsHandler(TcpClient tcpClient)
        {
            _conversionHandler = new ConversionHandler();
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();
            _socketHelper = new SocketHelper(tcpClient);
        }

        public async Task SendFileAsync(string path)
        {
            if (await _fileHandler.FileExistsAsync(path))
            {
                var fileName = await _fileHandler.GetFileNameAsync(path);
                // ---> Enviar el largo del nombre del archivo
                await _socketHelper.SendAsync(_conversionHandler.ConvertIntToBytes(fileName.Length));
                // ---> Enviar el nombre del archivo
                await _socketHelper.SendAsync(_conversionHandler.ConvertStringToBytes(fileName));

                // ---> Obtener el tamaño del archivo
                long fileSize = await _fileHandler.GetFileSizeAsync(path);
                // ---> Enviar el tamaño del archivo
                var convertedFileSize = _conversionHandler.ConvertLongToBytes(fileSize);
                await _socketHelper.SendAsync(convertedFileSize);
                // ---> Enviar el archivo (pero con file stream)
                await SendFileWithStreamAsync(fileSize, path);
            }
            else
            {
                throw new Exception("File does not exist");
            }
        }

        public async Task<string> ReceiveFileAsync(string path)
        {
            // ---> Recibir el largo del nombre del archivo
            int fileNameSize = _conversionHandler.ConvertBytesToInt(await _socketHelper.ReceiveAsync(Protocol.FixedDataSize));
            // ---> Recibir el nombre del archivo
            string fileName = _conversionHandler.ConvertBytesToString(await _socketHelper.ReceiveAsync(fileNameSize));
            // ---> Recibir el largo del archivo
            long fileSize = _conversionHandler.ConvertBytesToLong(await _socketHelper.ReceiveAsync(Protocol.FixedFileSize));
            // ---> Recibir el archivo
            await ReceiveFileWithStreamsAsync(fileSize, path+fileName);
            return fileName;
        }

        private async Task SendFileWithStreamAsync(long fileSize, string path)
        {
            long fileParts = await Protocol.CalculateFilePartsAsync(fileSize);
            long offset = 0;
            long currentPart = 1;  
         
            //Mientras tengo un segmento a enviar
            while (fileSize > offset)
            {
                byte[] data;
                //Es el ultimo segmento?
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    //1- Leo de disco el ultimo segmento
                    //2- Guardo el ultimo segmento en un buffer
                    data = await _fileStreamHandler.ReadAsync(path, offset, lastPartSize); //Puntos 1 y 2
                    offset += lastPartSize;
                }
                else
                {
                    //1- Leo de disco el segmento
                    //2- Guardo ese segmento en un buffer
                    data = await _fileStreamHandler.ReadAsync(path, offset, Protocol.MaxPacketSize);
                    offset += Protocol.MaxPacketSize;
                }

                await _socketHelper.SendAsync(data); //3- Envio ese segmento a travez de la red
                currentPart++;
            }
        }

        private async Task ReceiveFileWithStreamsAsync(long fileSize, string fileName)
        {
            long fileParts = await Protocol.CalculateFilePartsAsync(fileSize);
            long offset = 0;
            long currentPart = 1;

            //Mientras tengo partes para recibir
            while (fileSize > offset)
            {
                byte[] data;
                //1- Me fijo si es la ultima parte
                if (currentPart == fileParts)
                {
                    //1.1 - Si es, recibo la ultima parte
                    var lastPartSize = (int)(fileSize - offset);
                    data = await _socketHelper.ReceiveAsync(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    //2.2- Si no, recibo una parte cualquiera
                    data = await _socketHelper.ReceiveAsync(Protocol.MaxPacketSize);
                    offset += Protocol.MaxPacketSize;
                }
                //3- Escribo esa parte del archivo a disco
                await _fileStreamHandler.WriteAsync(fileName, data);
                currentPart++;
            }
        }
    }
}

