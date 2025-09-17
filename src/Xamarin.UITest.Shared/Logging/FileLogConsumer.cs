using System;
using System.IO;

namespace Xamarin.UITest.Shared.Logging
{
    public class FileLogConsumer : ILogConsumer
    {
        readonly string _path;

        public FileLogConsumer(string directory = null)
        {
            var folder = directory ?? Path.Combine(Path.GetTempPath(), "uitest");
            Directory.CreateDirectory(folder);
            _path = Path.Combine(folder, string.Format("log-{0}.txt", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")));
        }

        public string LogPath
        {
            get { return _path; }
        }

        public void Consume(LogEntry logEntry)
        {
            lock (_path)
            {
                File.AppendAllText(
                    _path,
                    $"{logEntry.LogTimestamp} - {logEntry.Offset} - {logEntry.Message}{Environment.NewLine}");
            }
        }
    }
}