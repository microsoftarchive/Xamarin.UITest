using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.UITest.Helpers
{
    /// <summary>
    /// Helper class for handling Environment Variable Forwarding.
    /// </summary>
    public class EnvironmentVariableForwardingHelper : IEnvironmentVariableForwardingHelper
    {

        private readonly IEnvironmentVariableHelper _envHelper;

        /// <summary>
        /// Helper class for handling Environment Variable Forwarding.
        /// </summary>
        public EnvironmentVariableForwardingHelper()
        {
            this._envHelper = new EnvironmentVariableHelper();
        }

        /// <summary>
        /// Helper class for handling Environment Variable Forwarding.
        /// </summary>
        public EnvironmentVariableForwardingHelper(IEnvironmentVariableHelper envHelper)
        {
            this._envHelper = envHelper;
        }

        /// <summary>
        /// Merges the user specified environment variables with the system defined environment variables for the AUT.
        /// </summary>
        public Dictionary<string, string> MergeAutEnvironmentVariables(Dictionary<string, string> autEnvironmentVars)
        {
            var variables = this._envHelper.GetEnvironmentVariables();


            if (variables == null)
            {
                return autEnvironmentVars;
            }

            var allEnvVars = variables.Concat(autEnvironmentVars)
                                            .GroupBy(x => x.Key)
                                            .Select(x => x.First());

            return allEnvVars.ToDictionary(k => k.Key, k => k.Value);
        }
    }
}
