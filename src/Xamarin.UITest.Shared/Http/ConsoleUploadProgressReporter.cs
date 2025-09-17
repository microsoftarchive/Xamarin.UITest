using System;

namespace Xamarin.UITest.Shared.Http
{
    public class ConsoleUploadProgressReporter : IUploadProgressReporter
    {
        long _lastReportedPercentage;

        public void UploadStart(string fileName)
        {
            _lastReportedPercentage = 0;

            if (Console.IsOutputRedirected)
            {
                Console.WriteLine();
                return;
            }

            Console.Write(GetMessage(fileName, 0));
        }

        public void UploadProgress(string fileName, long current, long total)
        {
            var percentage = (current * 100) / total;

            if (Console.IsOutputRedirected)
            {
                if (percentage >= _lastReportedPercentage + 10)
                {
                    Console.WriteLine(GetMessage(fileName, percentage));
                    _lastReportedPercentage = percentage;
                }
                return;
            }

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(GetMessage(fileName, percentage));
        }

        public void UploadError(string fileName)
        {
            if (Console.IsOutputRedirected)
            {
                Console.WriteLine(string.Format("Error uploading {0}...", fileName));
                return;
            }

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine(string.Format("Error uploading {0}...", fileName));
        }

        public void UploadComplete(string fileName)
        {
            if (Console.IsOutputRedirected)
            {
                Console.WriteLine(GetMessage(fileName, 100));
                return;
            }
            
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine(GetMessage(fileName, 100));
        }

        string GetMessage(string fileName, long percentage)
        {
            if (percentage <= 0)
            {
                return string.Format("Uploading {0}... ", fileName);
            }

            return string.Format("Uploading {0}... {1}%", fileName, percentage);
        }
    }
}