using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace MediaPlayer.Storage
{
    public static class StorageManager
    {
        public static async Task CreateFile(string fileName, string contents)
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
            if (file != null)
            {
                using (var stream = await ((IStorageFile) file).OpenStreamForWriteAsync())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteAsync(contents);
                    }
                }
            }
            else
            {
                var newFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
                using (var stream = await newFile.OpenStreamForWriteAsync())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteAsync(contents);
                    }
                }
            }
        }

        public static async Task<string> GetFileContents(string fileName)
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
            if (file == null)
            {
                return "";
            }

            using (var stream = await ((IStorageFile) file).OpenStreamForReadAsync())
            {
                using (var reader = new StreamReader(stream))
                {
                    var contents = await reader.ReadToEndAsync();
                    return contents;
                }
            }
        }
    }
}
