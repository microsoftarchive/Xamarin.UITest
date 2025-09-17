using System;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Processes;

namespace Xamarin.UITest.Shared.Processes
{
	public class UnixProcessKiller
	{
        IProcessRunner _processRunner;

        internal UnixProcessKiller(IProcessRunner processRunner)
        {
            _processRunner = processRunner;
        }

        public bool Kill(int pid, string signal) 
        {
            Log.Debug(string.Format("Sending signal {0} to pid {1}", signal, pid));
            KillInternal(pid, signal);
            return WaitForProcessToDisappear(pid);
        }

        bool KillInternal(int pid, string signal) {
            var result = _processRunner.RunCommand("/bin/kill", string.Format("-s {0} {1}", signal, pid), CheckExitCode.AllowAnything);
            if (result.ExitCode != 0)
            {
                Log.Debug(string.Format("Unable to send signal {0} to {1}", signal, pid));
                return false;
            }
            return true;
        }

        bool WaitForProcessToDisappear(int pid)
        {
            var waiter = new WaitForHelper(TimeSpan.FromSeconds(5));
            try 
            {
                waiter.WaitFor(() => !KillInternal(pid, 0.ToString()), retryFrequency: TimeSpan.FromMilliseconds(500));
                return true;
            }
            catch (TimeoutException)
            {
                Log.Debug(string.Format("process with pid {0} did not disapper", pid));
                return false;
            }
        }
	}

}
