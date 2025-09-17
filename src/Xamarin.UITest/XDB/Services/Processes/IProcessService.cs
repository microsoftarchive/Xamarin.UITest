using System;
using System.Threading.Tasks;

namespace Xamarin.UITest.XDB.Services.Processes
{
    interface IProcessService
    {
        Task<ProcessResult> RunAsync(
            string command, 
            string arguments = null, 
            Action<string> standardOutputCallback = null, 
            Action<string> standardErrorCallback = null);
    
            ProcessResult Run(string command, string arguments = null);
    }
}
