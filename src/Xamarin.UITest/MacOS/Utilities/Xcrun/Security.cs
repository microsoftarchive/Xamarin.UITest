using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.MacOS.Utilities.Xcrun
{
    internal static class Security
    {
        private static readonly string ToolName = "security";

        public static List<string> FindIdentity(IProcessService processService, ILoggerService loggerService, string policy, bool showOnlyValidIdentities = true)
        {
            StringBuilder argumentsBuilder = new();

            argumentsBuilder.Append(value: "find-identity");
            argumentsBuilder.Append(value: ' ');

            if (showOnlyValidIdentities)
            {
                argumentsBuilder.Append(value: "-v");
                argumentsBuilder.Append(value: ' ');
            }

            argumentsBuilder.Append(value: "-p");
            argumentsBuilder.Append(value: ' ');
            argumentsBuilder.Append(value: policy);

            string processOutput = Xcrun.RunXcrunToolCommand(processService: processService, loggerService: loggerService, toolName: ToolName, arguments: argumentsBuilder.ToString());
            if (processOutput == null)
            {
                loggerService.LogError(message: "xcrun security find-identity: Command returned no output.");
                return null;
            }
            else if (processOutput == string.Empty)
            {
                loggerService.LogError(message: "xcrun security find-identity: No valid codesign identities found.");
                return null;
            }
            else
            {
                return processOutput.Split(separator: '\n').ToList();
            }
        }

        public static FileInfo DecodeProvisioningProfileWithCMS(IProcessService processService, ILoggerService loggerService, FileInfo inputFile)
        {
            StringBuilder argumentsBuilder = new();

            argumentsBuilder.Append(value: "cms");
            argumentsBuilder.Append(value: ' ');

            argumentsBuilder.Append(value: "-D");
            argumentsBuilder.Append(value: ' ');

            argumentsBuilder.Append(value: "-i");
            argumentsBuilder.Append(value: ' ');
            argumentsBuilder.Append(value: $"\"{inputFile.FullName}\"");
            argumentsBuilder.Append(value: ' ');

            string outputFilePath = Path.Combine(path1: inputFile.DirectoryName, path2: $"decoded_{inputFile.Name}.plist");
            argumentsBuilder.Append(value: "-o");
            argumentsBuilder.Append(value: ' ');
            argumentsBuilder.Append(value: $"\"{outputFilePath}\"");

            if (Xcrun.RunXcrunToolCommand(processService: processService, loggerService: loggerService, toolName: ToolName, arguments: argumentsBuilder.ToString()) == null)
            {
                loggerService.LogError(message: "xcrun security cms: Command execution failed.");
                return null;
            }
            else if (File.Exists(path: outputFilePath))
            {
                return new FileInfo(fileName: outputFilePath);
            }
            else
            {
                loggerService.LogError(message: "xcrun security cms: Command executed successfully, but output file couldn't be found.");
                return null;
            }
        }
    }
}