using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
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

        private object lockObject = new object();

        public event Action<string> OnDownloaded;
        public event Action<string, Exception> OnFailed;

        public FileDownloader()
        {
            fileDownloadQueue = new ConcurrentQueue<FileData>();

            threadsCounter = 0;

            client = new HttpClient();

            degreeOfParallelism = 4;
        }

        private void DownloadingSuccess(string fileId)
        {
            OnDownloaded.Invoke(fileId);
        }
        private void DownloadingError(string fileId, Exception exception)
        {
            OnFailed.Invoke(fileId, exception);
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
                throw new Exception("Загрузка файлов уже запущена");
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

            if (!Directory.Exists(pathToSave))
            {
                throw new Exception("Указанный путь не существует");
            }

            data.url = url;
            data.fileId = fileId;
            try
            {
                data.pathToSave = FileExtension.GetFileExtension(url, fileId, pathToSave);
            }
            catch (Exception exception)
            {
                DownloadingError(fileId, exception);
            }
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
                                await DownloadFile(dataForSaving.fileId, dataForSaving.url, dataForSaving.pathToSave);
                            }
                            else
                            {
                                Exception downloadException = new Exception("Ошибка получения объекта из очереди");

                                DownloadingError(fileId, downloadException);
                            }
                        }
                        lock (lockObject)
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
                                try
                                {
                                    while (bytesRead != 0)
                                    {
                                        byte[] buf = new byte[8096];

                                        bytesRead = await content.ReadAsync(buf, 0, buf.Length);
                                        await file.WriteAsync(buf, 0, buf.Length);
                                    }

                                    DownloadingSuccess(fileId);
                                }
                                catch (Exception exception)
                                {
                                    DownloadingError(fileId, exception);
                                
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            DownloadingError(fileId, exception);
                        }
                    }
                }
                else
                {
                    Exception exception = new Exception("Невозможно подключиться к серверу");

                    DownloadingError(fileId, exception);
                }
            }
        }
    }
}