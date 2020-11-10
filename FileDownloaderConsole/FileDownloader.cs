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
        event Action<string> OnDownloaded;
        event Action<string, Exception> OnFailed;
    }

    class FileDownloader : IFileDownloader
    {
        private ConcurrentQueue<FileData> fileDownloadQueue;
        private int threadsCounter;
        private HttpClient client;
        private int degreeOfParallelism;

        private object lockObject1 = new object();
        private object lockObject2 = new object();
        private object lockObject3 = new object();

        private int fileDownloadedCount;
        private int fileUndownloadedCount;
        private int numberOfFiles = InputData.numberOfFiles;
        private double downloadedFiles;

        public event Action<string> OnDownloaded;
        public event Action<string, Exception> OnFailed;

        public FileDownloader()
        {
            fileDownloadQueue = new ConcurrentQueue<FileData>();

            threadsCounter = 0;

            client = new HttpClient();

            degreeOfParallelism = 4;
            fileDownloadedCount = 0;
            fileUndownloadedCount = 0;

            OnDownloaded += Message;
            OnFailed += Message;

        }

        private void Downloading(string fileId)
        {
            fileDownloadedCount++;

            downloadedFiles = (Convert.ToDouble(fileDownloadedCount) / Convert.ToDouble(numberOfFiles)) * 100;
            downloadedFiles = (int)Math.Round(downloadedFiles);

            OnDownloaded.Invoke($"Файл <<{fileId}>> загружен\nЗагружено {downloadedFiles}%");

            if (downloadedFiles == 100)
            {
                OnDownloaded.Invoke($"Загружено: {fileDownloadedCount}. С ошибками: {fileUndownloadedCount}.");
            }
        }
        private void DownloadError(string fileId, Exception exception)
        {
            fileUndownloadedCount++;
        }
        private static void Message(string message)
        {
            Console.WriteLine(message);
        }        
        private static void Message(string message, Exception exception)
        {
            //Console.WriteLine(message);
            //Console.WriteLine(exception);
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

            lock (lockObject1)
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
                                await DownloadFile(dataForSaving.fileId, dataForSaving.url, dataForSaving.pathToSave);

                                lock (lockObject2)
                                {
                                    Downloading(dataForSaving.fileId);
                                }
                            }
                            else
                            {
                                Exception downloadException = new Exception("Error getting an object from the queue");
                                
                                lock (lockObject3)
                                {
                                    DownloadError(fileId, downloadException);
                                }
                            }
                        }
                        lock (lockObject1)
                        {
                            threadsCounter--;
                        }
                    });
                }
            }
        }
        private async Task DownloadFile(string fileId, string url, string pathToSave)
        {
            using (HttpResponseMessage response = await client.GetAsync(url))
            {
                if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                {
                    using (var content = await response.Content.ReadAsStreamAsync())
                    {
                        int fileSize = (int)content.Length;
                        int bytesRead = 1;

                        try
                        {
                            using (FileStream file = File.Create(pathToSave))
                            {
                                while (bytesRead != 0)
                                {
                                    byte[] buf = new byte[8096];
                                    try 
                                    {
                                        bytesRead = await content.ReadAsync(buf, 0, buf.Length);
                                        await file.WriteAsync(buf, 0, buf.Length);
                                    }
                                    catch (Exception e)
                                    {
                                        lock (lockObject3)
                                        {
                                            DownloadError(fileId, e);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            lock(lockObject3)
                            {
                                DownloadError(fileId, e); 
                            }
                        }
                    }
                }
            }
        }
    }
}