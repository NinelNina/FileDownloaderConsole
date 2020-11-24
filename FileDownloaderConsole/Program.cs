using System;

namespace FileDownloaderConsole
{
    class Program
    {
        static void Main()
        {
            InputData inputData = new InputData();

            Console.Write("������� ���� � �����: ");

            inputData.PathToOpen = Console.ReadLine();

            try
            {
                inputData.Input();
            }
            catch (Exception exception)
            {
                Log.WriteToLog(exception);
                Console.WriteLine("������!" + exception.Message);

                Console.ReadKey();
                return;
            }

            Console.Write("������� ���� ��� ����������: ");
            inputData.PathToSave = Console.ReadLine();

            FileDownloader fileDownloader = new FileDownloader();

            CountDownloadingFiles downloadingFiles = new CountDownloadingFiles();

            fileDownloader.OnDownloaded += downloadingFiles.CountDownloadedFiles;
            fileDownloader.OnFailed += downloadingFiles.CountUndownloadedFiles;

            int index = 1;

            try
            {
                foreach (string url in inputData.fileUrls)
                {

                    fileDownloader.AddFileToDownloadingQueue(Convert.ToString(index), url, inputData.PathToSave);
                    index++;
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(exception);
                Console.WriteLine("������!" + exception.Message);

                Console.ReadKey();
                return;
            }

            Console.ReadKey();
        }
    }
}