using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace FileDownloaderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            FileDownloader fileDownloader = new FileDownloader();
            fileDownloader.SetDegreeOfParallelism(4);
            
            
            bool state = false;
            InputData inputData = new InputData();

            inputData.PathToOpen = "url-list.txt";

            inputData.Input();

            int i = 1;

            fileDownloader.filePathQueue = new Queue(fileDownloader.degree);
            fileDownloader.fileUrlQueue = new Queue(fileDownloader.degree);

            Task task1 = Task.Run(async () =>
            {
                for (int j = inputData.fileUrls.Count; j != 0; j--)
                {
                    if (fileDownloader.fileUrlQueue.Count < fileDownloader.degree)
                    {
                        fileDownloader.AddFileToDownloadingQueue(Convert.ToString(i), inputData.fileUrls[i - 1], inputData.fileUrls[i - 1]);
                        i++;
                    }

                    if (fileDownloader.fileUrlQueue.Count == fileDownloader.degree || (j < fileDownloader.degree) && (fileDownloader.fileUrlQueue.Count < 4) && (Convert.ToDouble(inputData.fileUrls.Count) / 4 != 0))
                    {
                        while (fileDownloader.fileUrlQueue.Count != 0)
                        {
                            string url;
                            string path;

                            url = Convert.ToString(fileDownloader.fileUrlQueue.Dequeue());
                            path = Convert.ToString(fileDownloader.filePathQueue.Dequeue());

                            await Task.Run(async () =>
                            {
                                await fileDownloader.DownloadFile(url, path);
                                Console.WriteLine(path);
                            });
                        }
                    }
                }

              state = true;  
            });

            task1.Wait();

            if (state)
            {
                Console.WriteLine("Файлы загружены.");
                Console.ReadKey();
            }
        }

    }

    class InputData
    {
        public string PathToOpen { get; set; }

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
        public Queue fileUrlQueue;
        public Queue filePathQueue;
        public int degree;
        public void SetDegreeOfParallelism(int degreeOfParallelism)
        {
            degree = degreeOfParallelism;
        }
        public void AddFileToDownloadingQueue(string fileId, string url, string pathToSave)
        {
            fileUrlQueue.Enqueue(url);

            int index = pathToSave.LastIndexOf('.');
            string fileExtension = pathToSave.Substring(index, pathToSave.Length - index);
            pathToSave = fileId + fileExtension;

            filePathQueue.Enqueue(pathToSave);

        }
        public async Task DownloadFile(string url, string pathToSave)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                    {
                        byte[] content = await response.Content.ReadAsByteArrayAsync();

                        using (FileStream file = File.Create(pathToSave))
                        {
                            file.Write(content, 0, content.Length);
                        }
                    }
                }
            }
        }
    }

}