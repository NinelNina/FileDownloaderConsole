using System;

namespace FileDownloaderConsole
{
    class FileExtension
    {
        public static string GetFileExtension(string url, string fileId, string pathToSave)
        {
            try
            {
                int index = url.LastIndexOf('.');
                string fileExtension = url.Substring(index, url.Length - index);
                pathToSave = pathToSave + @"\" + fileId + fileExtension;

            } 
            catch(Exception exception)
            {
                throw new Exception("Неверный формат ссылки", exception);

            }

            return pathToSave;
        }
    }
}
