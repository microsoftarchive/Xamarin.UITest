using System.IO;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.MacOS.Utilities
{
    internal static class PlistBuddy
    {
        private const string ExecutablePath = "/usr/libexec/PlistBuddy";
        private const string PrintEntitlementsCommand = "Print :Entitlements";
        private const string PrintDeveloperCertificatesCommand = "Print :DeveloperCertificates";
        private const string SetCommand = "Set";

        private static string ExecuteCommand(string arguments)
        {
            IProcessService processService = XdbServices.GetRequiredService<IProcessService>();
            ILoggerService loggerService = XdbServices.GetRequiredService<ILoggerService>();

            ProcessResult processResult = processService.Run(command: ExecutablePath, arguments: arguments);
            if (processResult.ExitCode != 0)
            {
                loggerService.LogError(message: "PlistBuddy tool execution failed.");
                loggerService.LogError(message: $"PlistBuddy executable path: {ExecutablePath}");
                loggerService.LogError(message: $"Arguments: {arguments}");
                loggerService.LogError(message: $"Error: {processResult.StandardOutput}");

                return null;
            }

            return processResult.StandardOutput;
        }

        public static string PrintEntitlements(FileInfo provisioningProfileDecodedPlist)
        {
            string arguments = $"-x -c \"{PrintEntitlementsCommand}\" \"{provisioningProfileDecodedPlist.FullName}\"";
            return ExecuteCommand(arguments: arguments);
        }

        public static string PrintDeveloperCertificates(FileInfo provisioningProfileDecodedPlist)
        {
            string arguments = $"-x -c \"{PrintDeveloperCertificatesCommand}\" \"{provisioningProfileDecodedPlist.FullName}\"";
            return ExecuteCommand(arguments: arguments);
        }

        public static string SetValueForKey(string plistPath, string keyPath, string value)
        {
            string arguments = $"-x -c \"{SetCommand} {keyPath} {value}\" \"{plistPath}\"";
            return ExecuteCommand(arguments: arguments);
        }
    }
}