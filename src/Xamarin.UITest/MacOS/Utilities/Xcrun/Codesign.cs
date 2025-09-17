using System;
using System.IO;
using System.Text;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.MacOS.Utilities.Xcrun
{
    internal static class Codesign
    {
        private static readonly string ToolName = "codesign";

        public static void Sign(IProcessService processService, ILoggerService loggerService, FileSystemInfo fileToSign, CodesignIdentity identity, FileInfo entitlementsPlistFile)
        {
            StringBuilder argumentsBuilder = new();

            argumentsBuilder.Append(value: "-f");
            argumentsBuilder.Append(value: ' ');

            argumentsBuilder.Append(value: "-s");
            argumentsBuilder.Append(value: ' ');
            argumentsBuilder.Append(value: $"\"{identity.Name}\"");
            argumentsBuilder.Append(value: ' ');

            argumentsBuilder.Append(value: "--entitlements");
            argumentsBuilder.Append(value: ' ');
            argumentsBuilder.Append(value: $"\"{entitlementsPlistFile.FullName}\"");
            argumentsBuilder.Append(value: ' ');

            argumentsBuilder.Append(value: fileToSign.FullName);

            Xcrun.RunXcrunToolCommand(processService: processService, loggerService: loggerService, toolName: ToolName, arguments: argumentsBuilder.ToString());
        }

        public static void RemoveSignature(IProcessService processService, ILoggerService loggerService, FileSystemInfo file)
        {
            StringBuilder argumentsBuilder = new();

            argumentsBuilder.Append(value: "--remove-signature");
            argumentsBuilder.Append(value: ' ');
            argumentsBuilder.Append(value: $"\"{file.FullName}\"");

            Xcrun.RunXcrunToolCommand(processService: processService, loggerService: loggerService, toolName: ToolName, arguments: argumentsBuilder.ToString());
        }
    }
}

