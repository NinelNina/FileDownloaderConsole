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
            bool state = false;
            InputData inputData = new InputData();

            inputData.PathToOpen = "url-list.txt";

            inputData.Input();

            FileDownloader fileDownloader = new FileDownloader();

            int i = 1;

            Task task1 = Task.Run(async () =>
            {
                foreach (string url in inputData.fileUrls)
                {
                    await fileDownloader.DownloadFile(url, Convert.ToString(i) + ".jpg");
                    Console.WriteLine(i + ".jpg");
                    i++;
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
        public string PathToSave { get; set; }
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
        Queue fileIdQueue;
        Queue fileUrlQueue;
        Queue filePathQueue;
        public void SetDegreeOfParallelism(int degreeOfParallelism)
        {

        }
        public void AddFileToDownloadingQueue(string fileId, string url, string pathToSave)
        {

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