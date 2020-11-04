using System;

namespace FileDownloaderConsole
{
    class Program
    {
        static void Main()
        {
            FileDownloader fileDownloader = new FileDownloader();
                        
            InputData inputData = new InputData();
            inputData.PathToOpen = "url-list.txt";
            inputData.Input();

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