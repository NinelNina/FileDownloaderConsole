using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Collections;
using System.Drawing.Imaging;
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

            Task task1 = Task.Run(() =>
            {
                foreach (string url in inputData.fileUrl)
            {
                    fileDownloader.Downloader(url, Convert.ToString(i) + ".jpg");
                    Console.WriteLine(i + ".jpg");
                    i++;
            }
                state = true;
            });                             

            Task.WaitAll(task1);
            if (state)
            {
                Console.WriteLine("Файл загружен.");
                Console.ReadKey();
            }
        }

    }

    class InputData
    {
        public string PathToSave { get; set; }
        public string PathToOpen { get; set; }

        public List<string> fileId;
        public List<string> fileUrl;
        public void Input()
        {
            fileId = new List<string>(10);
            fileUrl = new List<string>(10);
            
            using (StreamReader reader = new StreamReader(File.Open(PathToOpen, FileMode.Open)))
            {
                int i = 1;
                while (!reader.EndOfStream)
                {
                    fileId.Add(Convert.ToString(i));
                    fileUrl.Add(reader.ReadLine());
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
        public Dictionary<string, string> Files { get; set; }

        public void SetDegreeOfParallelism(int degreeOfParallelism)
        {

        }
        public void AddFileToDownloadingQueue(string fileId, string url, string pathToSave)
        {

        }
        public async void Downloader(string url, string pathToSave)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                    {
                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                        {
                            var image = new Bitmap(Image.FromStream(streamToReadFrom));
                            image.Save(pathToSave, ImageFormat.Jpeg);

                        }
                    }
                }
            }
        }
    }

}
