using System;
using System.IO;
using System.Net.Http;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Policy;
using System.Collections.Concurrent;

namespace FileDownloaderConsole
{
    class Program
    {
        static void Main()
        {
            FileDownloader fileDownloader = new FileDownloader();
                        
            InputData inputData = new InputData();
            inputData.PathToOpen = @"I:\Programming\FileDownloaderConsole\FileDownloaderConsole\bin\Debug\url-list.txt";
            inputData.Input();

            inputData.PathToSave = @"E:\Pictures\";

            int i = 1;

            foreach (string url in inputData.fileUrls)
            {
                fileDownloader.AddFileToDownloadingQueue(Convert.ToString(i), url, inputData.PathToSave);
                i++;
            }
        }
    }

    class InputData
    {
        public string PathToOpen { get; set; }
        public string PathToSave { set; get; }
        public List<string> fileUrls;
        public void Input()
        {
            fileUrls = new List<string>(10);

            using (StreamReader reader = new StreamReader(File.Open(PathToOpen, FileMode.Open)))
            {
                int i = 1;
                while (!reader.EndOfStream)
                {
                    fileUrls.Add(reader.ReadLine());
                    i++;
                }
            }
        }
    }

    interface IFileDownloader
    {
        void SetDegreeOfParallelism(int degreeOfParallelism);
        void AddFileToDownloadingQueue(string fileId, string url, string pathToSave);
    }
    
    class FileDownloader : IFileDownloader
    {
        private ConcurrentQueue<FileData> fileDownloadQueue;
        private bool taskState;
        struct FileData
        {
            public string url;
            public string pathToSave;
        }
        public void SetDegreeOfParallelism(int degreeOfParallelism)
        {

        }
        public void AddFileToDownloadingQueue(string fileId, string url, string pathToSave)
        {
            fileDownloadQueue = new ConcurrentQueue<FileData>();

            FileData data = new FileData();
            FileData dataForSaving;

            data.url = url;

            int index = url.LastIndexOf('.');
            string fileExtension = url.Substring(index, url.Length - index);
            data.pathToSave = pathToSave + fileId + fileExtension;

            fileDownloadQueue.Enqueue(data);
            Console.WriteLine(fileId);

            if (!taskState)
            {
                taskState = true;

                Task dequeueTask = Task.Run(async () =>
                {
                    while (fileDownloadQueue.Count != 0)
                    {
                        fileDownloadQueue.TryDequeue(out dataForSaving);
                        Console.WriteLine(dataForSaving.pathToSave);

                        await DownloadFile(dataForSaving.url, dataForSaving.pathToSave);
                    }
                    taskState = false;
                });
                dequeueTask.Wait();
            }      
        }

        private async Task DownloadFile(string url, string pathToSave)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                     if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                     {
                         using (var content = await response.Content.ReadAsStreamAsync())
                         {
                             int fileSize = (int)content.Length;
                             int temp = 1;
                             using (FileStream file = File.Create(pathToSave))
                             {
                                 while (temp != 0)
                                 {
                                     byte[] buf = new byte[8096];

                                     temp = await content.ReadAsync(buf, 0, buf.Length);
                                     await file.WriteAsync(buf, 0, buf.Length);
                                 }
                             }
                         }
                     }
                }
            }
            
        }
    }
}