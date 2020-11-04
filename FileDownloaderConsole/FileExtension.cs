namespace FileDownloaderConsole
{
    class FileExtension
    {
        public static string GetFileExtension(string url, string fileId, string pathToSave)
        {
            int index = url.LastIndexOf('.');
            string fileExtension = url.Substring(index, url.Length - index);
            pathToSave = pathToSave + @"\" + fileId + fileExtension;

            return pathToSave;
        }
    }
}
