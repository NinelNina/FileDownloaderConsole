using System.Collections.Generic;
using System.IO;


namespace FileDownloaderConsole
{
    public class InputData
    {
        public string PathToOpen { get; set; }
        public string PathToSave { set; get; }
        public List<string> fileUrls;
        public static int numberOfFiles;
        
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
        public string GetFolderName(string folderName)
        {
            try
            {
                int index = folderName.LastIndexOf(@"\");
                folderName = folderName.Substring(0, index);

                return folderName;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message, exception);
            }
        }
    }
}
