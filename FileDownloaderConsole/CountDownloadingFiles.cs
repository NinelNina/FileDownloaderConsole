using System;

namespace FileDownloaderConsole
{
    public class CountDownloadingFiles
    {
       
        private int fileDownloadedCount = 0;
        private int fileUndownloadedCount = 0;
        private double downloadedFiles;
        private double numberOfFiles;


        public void CountDownloadedFiles(string message)
        {
            fileDownloadedCount++;

            downloadedFiles = (Convert.ToDouble(fileDownloadedCount) / Convert.ToDouble(numberOfFiles)) * 100;
            downloadedFiles = (int)Math.Round(downloadedFiles);

            Console.WriteLine($"Загрузка файла <<{message}>> {downloadedFiles}%");

            ShowNumberOfDownloadedFiles();
        }

        private void ShowNumberOfDownloadedFiles()
        {
            if (fileDownloadedCount + fileUndownloadedCount == numberOfFiles)
            {
                Console.WriteLine($"Загружено файлов: {fileDownloadedCount}. Загружено с ошибками: {fileUndownloadedCount}");
            }
        }

        public void CountUndownloadedFiles(string message, Exception exception)
        {
            fileUndownloadedCount++;

            Log.WriteToLog(message, exception);

            ShowNumberOfDownloadedFiles();
        }
    }
}
