using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xamarin.UITest.XDB.Exceptions;
using Xamarin.UITest.XDB.Exceptions.PList;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.XDB.Services.OSX
{
    class PListService : IPListService
    {
        IProcessService _processService;

        public PListService(IProcessService processService)
        {
            _processService = processService;
        }

        public string ReadPListAsXml(string plistPath)
        {
            var plistReadResult = _processService.Run("plutil", $"-convert xml1 -o - \"{plistPath}\"");

            if (plistReadResult.ExitCode != 0)
            {
                throw new ExternalProcessException(
                    $"plutil failed to process {plistPath}: {plistReadResult.CombinedOutput}");
            }

            return plistReadResult.StandardOutput;
        }

        public string ReadPListValueFromFile(string plistPath, string key)
        {
            var plistContents = ReadPListAsXml(plistPath);
            return ReadPListValueFromString(plistContents, key);
        }

        public string ReadPListValueFromString(string plistContents, string key)
        {
            var doc = XDocument.Parse(plistContents);

            var keyElements = doc.Descendants("key").Where(x => x.Value == key).ToArray();

            if (!keyElements.Any())
            {
                throw new PListMissingKeyException("Could not find key: " + key);
            }

            if (keyElements.Count() > 1)
            {
                throw new PListDuplicateKeyException("Found multiple instances of key: " + key);
            }

            return keyElements.Single().ElementsAfterSelf().First().Value;
        }

        public void SetOrAddPListValueInFile(string plistPath, string key, string type, string value)
        {
            new FileInfo(plistPath).Directory.Create();

            string cmd = $"Add :{key} {type} {value}";

            if (File.Exists(plistPath))
            {
                var plistReadResult = _processService.Run("plutil", $"-convert xml1 -o - \"{plistPath}\"");
                
                if (plistReadResult.ExitCode != 0)
                {
                    throw new ExternalProcessException(
                        $"plutil failed to process {plistPath}: {plistReadResult.CombinedOutput}");
                }

                var doc = XDocument.Parse(plistReadResult.StandardOutput);

                if (doc.Descendants("key").Where(x => x.Value == key).Any())
                {
                    cmd = $"Set :{key} {value}";
                }
            }

            var plistUpdateResult = _processService.Run("/usr/libexec/PlistBuddy", $"-c \"{cmd}\" {plistPath}");

            if (plistUpdateResult.ExitCode != 0)
            {
                throw new ExternalProcessException(
                    $"PlistBuddy failed to update {plistPath}: {plistUpdateResult.CombinedOutput}");
            }
        }
    }
}