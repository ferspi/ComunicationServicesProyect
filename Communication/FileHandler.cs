
namespace Communication
{
    public class FileHandler
    {
        public async Task<bool> FileExistsAsync(string path)
        {
            return await Task.Run(() => File.Exists(path));
        }

        public async Task<string> GetFileNameAsync(string path)
        {
            //if (await FileExistsAsync(path))
            //{
            //    return new FileInfo(path).Name;
            //}

            //throw new Exception("File does not exist");
            return new FileInfo(path).Name;
        }


        public async Task<long> GetFileSizeAsync(string path)
        {
            if (await FileExistsAsync(path))
            {
                return new FileInfo(path).Length;
            }

            throw new Exception("File does not exist");
        }

        public async Task DeleteFileAsync(string path)
        {
            if (await FileExistsAsync(path))
            {
                File.Delete(path);
            }
            else
            {
                throw new Exception("File does not exist");
            }
        }
    }
}