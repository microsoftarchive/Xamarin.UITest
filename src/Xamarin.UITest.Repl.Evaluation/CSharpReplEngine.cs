
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Xamarin.UITest.Repl.Evaluation
{
    /// <summary>
    /// <see cref="CSharpReplEngine"/> class.
    /// </summary>
    public class CSharpReplEngine
    {
        /// <summary>
        /// REPL engine options, such as usings and loaded assemblies.
        /// </summary>
        private ScriptOptions scriptOptions = ScriptOptions.Default;

        /// <summary>
        /// Current script evaluations state.
        /// This property contains state from previous evaluations if there were such.
        /// </summary>
        public ScriptState<object> scriptState = null;

        /// <summary>
        /// Creates <see cref="CSharpReplEngine"/> instance.
        /// </summary>
        public CSharpReplEngine()
        {
            Console.WriteLine("Initializing REPL");
            LoadAssembly(typeof(object).Assembly.Location);
            LoadAssembly(typeof(Uri).Assembly.Location);
            LoadAssembly(typeof(Enumerable).Assembly.Location);
            LoadAssembly(typeof(System.IO.DirectoryInfo).Assembly.Location);
            LoadAssembly(typeof(Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript).Assembly.Location);
            AddUsing("System");
            AddUsing("System.Linq");
            AddUsing("System.Runtime");
            AddUsing("Xamarin.UITest");
        }

        /// <summary>
        /// Loading assembly to the REPL engine.
        /// </summary>
        /// <param name="filePath">File path to the assembly.</param>
        public void LoadAssembly(string filePath)
        {
            Debug.WriteLine("Loading assembly into REPL engine: {0}", filePath);
            scriptOptions = scriptOptions.AddReferences(MetadataReference.CreateFromFile(filePath));
        }

        /// <summary>
        /// Adding "using" directive to the REPL engine.
        /// </summary>
        /// <param name="usingEntry">Namespace to add.</param>
        public void AddUsing(string usingEntry)
        {
            Debug.WriteLine("Adding 'using {0}' to the CSharpReplEngine");
            scriptOptions = scriptOptions.AddImports(usingEntry);
        }

        /// <summary>
        /// Evaluates code.
        /// </summary>
        /// <param name="code">Code to evaluate.</param>
        /// <returns>ReplResult.</returns>
        public ReplResult Evaluate(string code)
        {
            object value;
            bool hasResult;
            var error = string.Empty;

            Debug.WriteLine("Evaluating...");
            Debug.WriteLine(code);
            if (scriptState == null)
            {
                scriptState = CSharpScript.RunAsync(code, scriptOptions).Result;
            }
            else
            {
                scriptState = scriptState.ContinueWithAsync(code).Result;
            }
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
            {
                value = scriptState.ReturnValue;
                hasResult = true;
            }
            else
            {
                value = null;
                hasResult = false;
            }

            return new ReplResult(hasResult, value, error);
        }
    }
}