using System.Linq;
using System.Text;
using Xamarin.UITest.XDB.Exceptions.IDB;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.XDB.Services.OSX.IDB
{
    /// <summary>
    /// Locator for IDB executable.
    /// </summary>
    internal static class IDBLocator
    {
        private static ProcessResult RunWhereisIDBCommand(IProcessService processService)
        {
            return processService.Run(command: "/bin/bash", arguments: "-c \"whereis idb\"");
        }

        public static bool IsIDBInstalled(IProcessService processService)
        {
            return RunWhereisIDBCommand(processService: processService).StandardOutput.Split(' ').Length > 1;
        }

        public static string GetIDBPath(IProcessService processService, IEnvironmentService environmentService, ILoggerService loggerService)
        {
            string IDBPathOverride = environmentService.IDBPathOverride;
            if (!string.IsNullOrWhiteSpace(IDBPathOverride))
            {
                loggerService.LogInfo("IDBService => IDBLocator: Found IDB path override set by user.");
                return IDBPathOverride;
            }
            string whereisIDBCommandoutput = RunWhereisIDBCommand(processService: processService).StandardOutput;
            if (whereisIDBCommandoutput.Split(' ').Length <= 1)
            {
                StringBuilder exceptionMessage = new StringBuilder();
                exceptionMessage.AppendLine("IDB couldn't be found.");
                exceptionMessage.AppendLine("Either it is not installed or it has installation path that is unexpected for Xamarin.UITest.");
                exceptionMessage.AppendLine("If IDB is installed on your system but this exception still being thrown,");
                exceptionMessage.AppendLine("please create symbolic link from actual location to /usr/bin/idb.");
                throw new IDBLocatorException(exceptionMessage.ToString());
            }
            return whereisIDBCommandoutput.Split(' ').ToList().Last();
        }
    }
}