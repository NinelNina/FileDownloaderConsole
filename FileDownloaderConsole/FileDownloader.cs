using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FileDownloaderConsole
{
    interface IFileDownloader
    {
        void SetDegreeOfParallelism(int degreeOfParallelism);
        void AddFileToDownloadingQueue(string fileId, string url, string pathToSave);
    }

    class FileDownloader : IFileDownloader
    {
        private ConcurrentQueue<FileData> fileDownloadQueue;
        private int threadsCounter;
        private HttpClient client;
        private int degreeOfParallelism;
        private object lockObject = new object();
        public FileDownloader()
        {
            fileDownloadQueue = new ConcurrentQueue<FileData>();
            threadsCounter = 0;
            client = new HttpClient();
            degreeOfParallelism = 4;
        }
        struct FileData
        {
            public string fileId;
            public string url;
            public string pathToSave;
        }
        public void SetDegreeOfParallelism(int degreeOfParallel)
        {
            if (threadsCounter != 0)
            {
                throw new Exception("Загрузка файлов уже запущена.");
            }
            else
            {
                degreeOfParallelism = degreeOfParallel;
            }
        }
        public void AddFileToDownloadingQueue(string fileId, string url, string pathToSave)
        {
            FileData data = new FileData();
            FileData dataForSaving;

            data.url = url;
            data.fileId = fileId;
            data.pathToSave = FileExtension.GetFileExtension(url, fileId, pathToSave);

            fileDownloadQueue.Enqueue(data);

            lock (lockObject)
            {
                if (threadsCounter < degreeOfParallelism)
                {
                    threadsCounter++;

                    Task dequeueTask = Task.Run(async () =>
                    {
                        while (fileDownloadQueue.Count != 0)
                        {
                            var isAddedInQueue = fileDownloadQueue.TryDequeue(out dataForSaving);

                            if (isAddedInQueue)
                            {
                                Console.WriteLine(dataForSaving.pathToSave);

                                await DownloadFile(dataForSaving.url, dataForSaving.pathToSave);
                            }
                        }
                        threadsCounter--;
                    });
                }
            }
        }
        private async Task DownloadFile(string url, string pathToSave)
        {
            using (HttpResponseMessage response = await client.GetAsync(url))
            {
                if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                {
                    using (var content = await response.Content.ReadAsStreamAsync())
                    {
                        int fileSize = (int)content.Length;
                        int bytesRead = 1;
                        using (FileStream file = File.Create(pathToSave))
                        {
                            while (bytesRead != 0)
                            {
                                byte[] buf = new byte[8096];

                                bytesRead = await content.ReadAsync(buf, 0, buf.Length);
                                await file.WriteAsync(buf, 0, buf.Length);
                            }
                        }
                    }
                }
            }
        }
    }
}