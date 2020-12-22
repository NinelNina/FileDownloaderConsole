using System;
using System.Collections.Generic;
using System.IO;

namespace FileDownloaderConsole
{
    public class InputData
    {
        public string PathToOpen { get; set; }
        public string PathToSave { set; get; }
        public List<string> fileUrls;
        public int NumberOfFiles { set; get; }
        
        public void Input()
        {
            fileUrls = new List<string>(10);

            StreamReader reader;

            try
            {
                reader = new StreamReader(File.Open(PathToOpen, FileMode.Open));

                using (reader)
                {
                    int i = 0;
                    while (!reader.EndOfStream)
                    {
                        fileUrls.Add(reader.ReadLine());
                        i++;
                    }
                    NumberOfFiles = i;
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message, exception);
            }
        }
    }
}
