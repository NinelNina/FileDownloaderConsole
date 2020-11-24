using System;

namespace FileDownloaderConsole
{
    class CountDownloadingFiles
    {
       
        private int fileDownloadedCount = 0;
        private int fileUndownloadedCount = 0;
        private double downloadedFiles;
        private double undownloadedFiles;
        private double numberOfFiles = InputData.numberOfFiles;

        public void CountDownloadedFiles(string message)
        {
            fileDownloadedCount++;

            downloadedFiles = (Convert.ToDouble(fileDownloadedCount) / Convert.ToDouble(numberOfFiles)) * 100;
            downloadedFiles = (int)Math.Round(downloadedFiles);

            Console.WriteLine($"Загрузка файла <<{message}>>");

            if (downloadedFiles + undownloadedFiles == 100)
            {
                Console.WriteLine($"Загружено файлов: {fileDownloadedCount}. Загружено с ошибками: {fileUndownloadedCount}");
            }
        }

        public void CountUndownloadedFiles(string message, Exception exception)
        {
            fileUndownloadedCount++;

            Log.WriteToLog(message, exception);

            undownloadedFiles = (Convert.ToDouble(fileUndownloadedCount) / Convert.ToDouble(numberOfFiles)) * 100;
            undownloadedFiles = (int)Math.Round(undownloadedFiles);

            if (downloadedFiles + undownloadedFiles == 100)
            {
                Console.WriteLine($"Загружено файлов: {fileDownloadedCount}. Загружено с ошибками: {fileUndownloadedCount}");
            }
        }
    }
}
