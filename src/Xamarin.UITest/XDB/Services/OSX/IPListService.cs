namespace Xamarin.UITest.XDB.Services.OSX
{
    interface IPListService
    {
        string ReadPListAsXml(string plistPath);

        string ReadPListValueFromFile(string plistPath, string key);

        string ReadPListValueFromString(string plistContents, string key);

        void SetOrAddPListValueInFile(string plistPath, string key, string type, string value);
    }
}