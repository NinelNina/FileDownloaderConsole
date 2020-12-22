using System;

namespace FileDownloaderConsole
{
    public class FileExtension
    {
        public static string GetFileExtension(string url, string pathToSave)
        {
            try
            {

                int extensionIndex = url.LastIndexOf('.');
                string extension = url.Substring(extensionIndex, url.Length - extensionIndex);

                string fileName = GetFileName(url);

                pathToSave = pathToSave + @"\" + fileName;

            } 
            catch(Exception exception)
            {
                throw new Exception("Неверный формат ссылки", exception);

            }

            return pathToSave;
        }
        public static string GetFileName (string url)
        {
            string fileName = "null";
            try
            {
                int fileNameIndex = url.LastIndexOf('/');
                fileName = url.Substring(fileNameIndex + 1, url.Length - fileNameIndex - 1);

            }
            catch (Exception exception)
            {
                throw new Exception("Неверный формат ссылки. " + exception.Message, exception);
            }

            return fileName;
        }
    }
}
