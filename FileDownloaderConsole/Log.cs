using System;
using System.IO;
using System.Text;

namespace FileDownloaderConsole
{
    public class Log
    {
        private static object syncObject = new object();
        public static void WriteToLog(string fileId, Exception exception)
        {
            try
            {
                string pathToLog = @".\Log";

                if (!Directory.Exists(pathToLog))
                {
                    Directory.CreateDirectory(pathToLog);
                }

                DateTime time = DateTime.Now;

                string fileName = pathToLog + @"\log_" + time.ToShortDateString() + ".txt";

                string fullText = time + " " + exception.TargetSite.DeclaringType + "." + exception.TargetSite.Name + " file<<" + fileId + ">> " + exception.Message + "\n";

                lock (syncObject)
                {
                    File.AppendAllText(fileName, fullText, Encoding.Default);
                }
            }
            catch
            {

            }
        }        
        public static void WriteToLog(Exception exception)
        {
            try
            {
                string pathToLog = @".\Log";

                if (!Directory.Exists(pathToLog))
                {
                    Directory.CreateDirectory(pathToLog);
                }

                DateTime time = DateTime.Now;

                string fileName = pathToLog + @"\log_" + time.ToShortDateString() + ".txt";

                string fullText = time + " " + exception.TargetSite.DeclaringType + "." + exception.TargetSite.Name + " " + exception.Message + "\n";

                lock (syncObject)
                {
                    File.AppendAllText(fileName, fullText, Encoding.Default);
                }
            }
            catch
            {

            }
        }
    }
}