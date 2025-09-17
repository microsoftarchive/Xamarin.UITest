namespace Xamarin.UITest.Shared.Processes
{
    internal class ProcessInfo
    {
        public readonly int PID;
        public readonly string CommandLine;
        public readonly string User;

        internal ProcessInfo(int pid, string commandLine, string user)
        {
            PID = pid;
            CommandLine = commandLine;
            User = user;
        }

        internal ProcessInfo(int pid, string commandLine) : this(pid, commandLine, "unknown")
        {
        }
    }
}