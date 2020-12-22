using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileDownloaderConsole
{
    public interface IFileDownloader
    {
        void SetDegreeOfParallelism(int degreeOfParallelism);
        void AddFileToDownloadingQueue(string fileId, string url, string pathToSave);
        event Action<string> OnDownloaded;
        event Action<string, Exception> OnFailed;
    }

    public class FileDownloader : IFileDownloader
    {
        private ConcurrentQueue<FileData> fileDownloadQueue;
        private int threadsCounter;
        private HttpClient client;
        private int degreeOfParallelism;

        private object lockObject = new object();

        public event Action<string> OnDownloaded;
        public event Action<string, Exception> OnFailed;
        public event Action<string, int, int> OnFileProgress;
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
        private void FileDownloadingProgress(string id, int totalBytes, int downloadedBytes)
        {
            OnFileProgress.Invoke(id, totalBytes, downloadedBytes);
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
                if (degreeOfParallel != degreeOfParallelism)
                {
                    degreeOfParallelism = degreeOfParallel;
                }
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
                data.pathToSave = FileExtension.GetFileExtension(url, pathToSave);
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
                try
                {
                    if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                    {
                        var fileSizeAsString = response.Content.Headers.SingleOrDefault(h => h.Key.Equals("Content-Length")).Value.First();

                        int totalValue = 0;
                        int downloadedBytes = 1;

                        if (fileSizeAsString != null)
                        {
                            totalValue = Convert.ToInt32(fileSizeAsString);
                            downloadedBytes = 0;
                        }

                        FileDownloadingProgress(fileId, totalValue, downloadedBytes);

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

                                            if (totalValue != 0)
                                            {
                                                downloadedBytes += bytesRead;
                                                FileDownloadingProgress(fileId, totalValue, downloadedBytes);
                                            }

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
                }
                catch(Exception exception)
                {
                    DownloadingError(fileId, exception);
                }
            }
        }
    }
}