using System;
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
                await FileIO.WriteTextAsync((IStorageFile) file, contents);
            }
            else
            {
                var newFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
                await FileIO.WriteTextAsync(newFile, contents);
            }
        }

        public static async Task<string> GetFileContents(string fileName)
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
            if (file == null)
            {
                return "";
            }

            return await FileIO.ReadTextAsync((IStorageFile) file);
        }
    }
}
