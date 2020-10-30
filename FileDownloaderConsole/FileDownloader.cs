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
    }

    class FileDownloader : IFileDownloader
    {
        private ConcurrentQueue<FileData> fileDownloadQueue;
        private bool taskState;

        public FileDownloader()
        {
            fileDownloadQueue = new ConcurrentQueue<FileData>();
            taskState = false;
        }

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
            FileData data = new FileData();
            FileData dataForSaving;

            data.url = url;

            int index = url.LastIndexOf('.');
            string fileExtension = url.Substring(index, url.Length - index);
            data.pathToSave = pathToSave + @"\" + fileId + fileExtension;

            fileDownloadQueue.Enqueue(data);

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
