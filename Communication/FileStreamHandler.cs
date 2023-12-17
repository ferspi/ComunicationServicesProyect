namespace Communication
{
    public class FileStreamHandler
    {
        private readonly FileHandler _fileHandler;
        public FileStreamHandler()
        {
            _fileHandler = new FileHandler();
        }
        public async Task<byte[]> ReadAsync(string path, long offset, int length)
        {
            if (await _fileHandler.FileExistsAsync(path))
            {
                var data = new byte[length];

                using var fs = new FileStream(path, FileMode.Open) { Position = offset };
                var bytesRead = 0;
                while (bytesRead < length)
                {
                    var read = await fs.ReadAsync(data, bytesRead, length - bytesRead);
                    if (read == 0)
                        throw new Exception("Error reading file");
                    bytesRead += read;
                }

                return data;
            }

            throw new Exception("File does not exist");
        }

        public async Task WriteAsync(string fileName, byte[] data)
        {
            var fileMode = await _fileHandler.FileExistsAsync(fileName) ? FileMode.Append : FileMode.Create;
            using var fs = new FileStream(fileName, fileMode);
            await fs.WriteAsync(data, 0, data.Length);
        }
    }
}
