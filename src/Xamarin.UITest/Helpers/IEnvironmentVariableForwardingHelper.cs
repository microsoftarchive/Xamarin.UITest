using System;
using System.Collections.Generic;

namespace Xamarin.UITest.Helpers
{
    /// <summary>
    /// Helper class for handling Environment Variable Forwarding.
    /// </summary>
    public interface IEnvironmentVariableForwardingHelper
    {
        /// <summary>
        /// Merges the user specified environment variables with the system defined environment variables for the AUT.
        /// </summary>
        Dictionary<string, string> MergeAutEnvironmentVariables(Dictionary<string, string> autEnvironmentVars);
    }
}
