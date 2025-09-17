using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Scripting;
using System.Threading.Tasks;

namespace Xamarin.UITest.Repl.Repl
{
    /// <summary>
    /// CompletionEngine class.
    /// </summary>
    public class CompletionEngine
    {
        private static MefHostServices mefHostServices = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        private List<MetadataReference> references = new();
        private List<string> usings = new();

        /// <summary>
        /// Initializes instance of <see cref="CompletionEngine"/>.
        /// </summary>
        public CompletionEngine()
        {
            LoadAssembly(typeof(object).Assembly.Location);
            LoadAssembly(typeof(Uri).Assembly.Location);
            LoadAssembly(typeof(Enumerable).Assembly.Location);
            LoadAssembly(typeof(System.IO.DirectoryInfo).Assembly.Location);
            AddUsing("System");
            AddUsing("System.Core");
            AddUsing("System.Runtime");
            AddUsing("Xamarin.UITest");
        }

        /// <summary>
        /// Loads assembly to <see cref="CompletionEngine" />.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly.</param>
        public void LoadAssembly(string assemblyPath)
        {
            Debug.WriteLine("Loading assembly into REPL completion engine: {0}", assemblyPath);
            references.Add(MetadataReference.CreateFromFile(assemblyPath));
        }

        /// <summary>
        /// Adds using directive to <see cref="CompletionEngine"/>
        /// </summary>
        /// <param name="usingEntry">Name of the namespace to add.</param>
        public void AddUsing(string usingEntry)
        {
            Debug.WriteLine("Adding 'using {0}' to the CSharpReplEngine");
            usings.Add(usingEntry);
        }

        /// <summary>
        /// Evaluates input for getting completions.
        /// </summary>
        /// <param name="input">Input code to get completions for.</param>
        /// <param name="scriptState"><see cref="ScriptState" /> from <see cref="Xamarin.UITest.Repl.Evaluation.CSharpReplEngine" /> with current context to evaluate code for.</param>
        /// <returns><see cref="CompletionResult" /> with completions for current input.</returns>
        public CompletionResult Evaluate(string input, ScriptState scriptState)
        {
            // Setting up workspace.
            var workspace = new AdhocWorkspace(mefHostServices);
            var cSharpCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: usings);
            var projectInfo = ProjectInfo.Create(
                id: ProjectId.CreateNewId(),
                version: VersionStamp.Create(),
                "Project",
                "Project",
                LanguageNames.CSharp,
                isSubmission: true)
                .WithMetadataReferences(references)
                .WithCompilationOptions(cSharpCompilationOptions);
            var project = workspace.AddProject(projectInfo);
            var previouslyExecutedCode = scriptState.Script.Code;
            var inputWithPrefix = previouslyExecutedCode + input;
            var documentInfo = DocumentInfo.Create(
                id: DocumentId.CreateNewId(project.Id),
                name: "REPL",
                sourceCodeKind: SourceCodeKind.Script,
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(inputWithPrefix), VersionStamp.Create())));
            var document = workspace.AddDocument(documentInfo);

            var cursorPosition = inputWithPrefix.Length - 1;
            var completionService = CompletionService.GetService(document);
            var completionTask = Task<CompletionList>.Run(() =>
            {
                return completionService.GetCompletionsAsync(document, cursorPosition);
            });

            // Completion service returns completions for text after last dot.
            // So we need to set filter condition here.
            // If there was a dot in input we should compare only text after this dot.
            var filterSubstring = input.Split('.').Last();

            // If produced substring is empty. Then we got nothing to return.
            if (filterSubstring == "")
            {
                return new CompletionResult(new CompletionItem[0], input);
            }

            // Filtering and returning completions.
            var completions = completionTask.Result.ItemsList.Where(x => x.DisplayText.StartsWith(filterSubstring, StringComparison.InvariantCultureIgnoreCase)).OrderBy(x => x.DisplayText).ToArray();
            return new CompletionResult(completions, input);
        }
    }
}