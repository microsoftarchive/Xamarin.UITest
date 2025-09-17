using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.MacOS.Utilities.Xcrun
{
    internal static class Xcrun
    {
        private static string XcrunPath = "/usr/bin/xcrun";

        public static string RunXcrunToolCommand(IProcessService processService, ILoggerService loggerService, string toolName, string arguments)
        {
            loggerService.LogInfo(message: $"XCRUN: Running {toolName} {arguments}");
            ProcessResult xcrunResult = processService.Run(command: XcrunPath, arguments: string.Join(separator: " ", toolName, arguments ));
            loggerService.LogInfo(message: $"XCRUN: {toolName} exited with code: {xcrunResult.ExitCode}");
            if (xcrunResult.ExitCode != 0)
            {
                loggerService.LogError(message: "XCRUN: Tool execution failed.");
                loggerService.LogError(message: $"Tool name: {toolName}");
                loggerService.LogError(message: $"Arguments: {arguments}");
                loggerService.LogError(message: $"Error: {xcrunResult.StandardError}");
                return null;
            }

            return xcrunResult.StandardOutput;
        }
    }
}