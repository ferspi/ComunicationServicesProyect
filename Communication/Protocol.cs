using System.Text;

namespace Communication
{
    public static class Protocol
    {
        public static readonly int FixedDataSize = 4; // data length

        public const int FixedFileSize = 8;
        public const int MaxPacketSize = 32768; //32KB tamaño maximo de los paquetes que vamos a enviar
        public const string NoImage = "SIN_IMAGEN";

        public static async Task<long> CalculateFilePartsAsync(long fileSize)
        {
            var fileParts = fileSize / MaxPacketSize;
            return await Task.Run(() => fileParts * MaxPacketSize == fileSize ? fileParts : fileParts + 1);
        }
    }
}
