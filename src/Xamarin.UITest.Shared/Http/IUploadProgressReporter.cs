namespace Xamarin.UITest.Shared.Http
{
    public interface IUploadProgressReporter
    {
        void UploadStart(string fileName);
        void UploadComplete(string fileName);
        void UploadProgress(string fileName, long current, long total);
        void UploadError(string fileName);
    }
}