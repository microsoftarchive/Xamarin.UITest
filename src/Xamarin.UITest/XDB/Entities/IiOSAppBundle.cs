namespace Xamarin.UITest.XDB.Entities
{
    interface IiOSAppBundle
    {
        string BundleId { get; }
        string DTPlatform { get; }
        string Path { get; }        
    }
}