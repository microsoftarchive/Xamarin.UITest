using System.IO;

namespace Xamarin.UITest.Shared.Http
{
    public class FormFile
    {
        public FormFile(FileInfo fileInfo, string contentType = "application/octet-stream")
        {
            Name = fileInfo.Name;
            ContentType = contentType;
            FilePath = fileInfo.FullName;
        }

        public string Name { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }
    }
}