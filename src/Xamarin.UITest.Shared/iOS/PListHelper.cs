using System;
using System.Linq;
using System.Xml.Linq;
using Xamarin.UITest.Shared.Processes;
using System.IO;

namespace Xamarin.UITest.Shared.iOS
{
    public static class PListHelper
    {
        public static string ReadPListValueFromFile(string plistPath, string key)
        {
            if (!File.Exists(plistPath))
            {
                throw new Exception("file does not exist: {plistPath}");
            }
            var runner = new ProcessRunner();

            var isOSX = Platform.Instance.IsOSX;

            var cmd = isOSX ? "plutil" : "/usr/bin/plistutil";
            var args = isOSX ? $"-convert xml1 -o - \"{plistPath}\"" : $"-i \"{plistPath}\"";

            var plistContents = runner.RunCommand(cmd, args, CheckExitCode.AllowAnything).Output;

            try
            {
                return ReadPListValueFromString(plistContents, key);
            }
            catch (ArgumentException ex)
            {
                if (ex.ParamName == nameof(plistContents))
                {
                    throw new Exception($"Unable to parse contents of {plistPath}");
                }
                throw;
            }
        }

        public static string ReadPListValueFromString(string plistContents, string key)
        {
            XDocument doc = null;

            try
            {
                doc = XDocument.Parse(plistContents);
            }
            catch
            {
                throw new ArgumentException("Invalid XML", nameof(plistContents));
            }

            var keyElements = doc.Descendants("key").Where(x => x.Value == key).ToArray();

            if (!keyElements.Any())
            {
                throw new Exception("Could not find key: " + key);
            }

            if (keyElements.Count() > 1)
            {
                throw new Exception("Found multiple instances of key: " + key);
            }

            return keyElements.Single().ElementsAfterSelf().First().Value;
        }

        public static void SetOrAddPListValueInFile(string plistPath, string key, string type, string value)
        {
            var processRunner = new ProcessRunner();

            new FileInfo(plistPath).Directory.Create();

            string cmd = $"Add :{key} {type} {value}";

            if (File.Exists(plistPath))
            {
                string plistContents = null;

                if (Platform.Instance.IsOSX)
                {
                    plistContents = processRunner.RunCommand(
                       "plutil", string.Format("-convert xml1 -o - \"{0}\"", plistPath)).Output;
                }
                else
                {
                    plistContents = processRunner.RunCommand(
                        "/usr/bin/plistutil", string.Format($"-i {plistPath}")).Output;
                }

                var doc = XDocument.Parse(plistContents);

                if (doc.Descendants("key").Where(x => x.Value == key).Any())
                {
                    cmd = $"Set :{key} {value}";
                }
            }

            processRunner.RunCommand("/usr/libexec/PlistBuddy", $"-c \"{cmd}\" {plistPath}");
        }
    }
}