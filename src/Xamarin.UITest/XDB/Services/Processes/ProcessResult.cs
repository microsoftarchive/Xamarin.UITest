namespace Xamarin.UITest.XDB.Services.Processes
{
    /// <summary>
    /// External process execution results.
    /// </summary>
    class ProcessResult
    {
        public ProcessResult(int exitCode, string standardOutput, string standardError, string combinedOutput)
        {
            this.ExitCode = exitCode;
            this.StandardOutput = standardOutput ?? string.Empty;
            this.StandardError = standardError ?? string.Empty;
            this.CombinedOutput = combinedOutput ?? string.Empty;
        }

        /// <summary>
        /// Process exit code.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Process standard output.
        /// </summary>
        public string StandardOutput { get; }

        /// <summary>
        /// Process standard error.
        /// </summary>
        public string StandardError { get; }

        /// <summary>
        /// Process standard output and standard error
        /// </summary>
        public string CombinedOutput { get; }
    }
}