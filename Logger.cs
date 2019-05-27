using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.IO.File;

namespace EcommerceImport
{
    public class Logger
    {
        private  DateTime logDate = DateTime.Now;
        private  string logText = "";
        private  string errorLogText = "";
        private string filePath;
        private string importName;
        private List<Notification> notifications;

        public Logger(string importName, string filePath, ref List<Notification> notifications)
        {
            this.importName = importName;
            this.filePath = filePath;
            this.notifications = notifications;
        }

        public  void LogMessage(string message)
        {
            Console.WriteLine(message);
            
            logText += message + Environment.NewLine;
        }

        public  void LogMessage(Notification ecommerceError)
        {
            LogMessage(ecommerceError.message);

            errorLogText += ecommerceError.message + Environment.NewLine;
        }

        public  void LogNotification(Notification notification)
        {
            notifications.Add(notification);
        }

        public  void WriteLogFiles()
        {
            WriteMainLogToFile();
            WriteErrorLogToFile();
        }

        private  void WriteMainLogToFile()
        {
            AppendAllText(filePath + @"\" + GetLogFileName(), logText);
        }

        private  void WriteErrorLogToFile()
        {
            if (errorLogText.Length > 0)
            {
             AppendAllText(filePath + @"\" + GetErrorLogFileName(), errorLogText);
            }
        }

        public  string GetLogFileName()
        {
            return $"{importName}{logDate.ToString("MMddyyyyHHmmss")}.log";
        }
        private  string GetErrorLogFileName()
        {
            return $"{importName}{logDate.ToString("MMddyyyyHHmmss")}Error.log";
        }
    }
}
