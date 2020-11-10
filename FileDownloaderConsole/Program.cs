using System;
using System.Diagnostics;
using System.Threading;

namespace FileDownloaderConsole
{
    class Program
    {
        static void Main()
        {
            InputData inputData = new InputData();
            inputData.PathToOpen = "url-list.txt";
            inputData.Input();

            FileDownloader fileDownloader = new FileDownloader();

            Console.Write("¬ведите путь дл€ сохранени€ файлов: ");
            inputData.PathToSave = Console.ReadLine();

            int index = 1;

            foreach (string url in inputData.fileUrls)
            {
                fileDownloader.AddFileToDownloadingQueue(Convert.ToString(index), url, inputData.PathToSave);
                index++;
            }

            Console.ReadKey();
        }
    }
}