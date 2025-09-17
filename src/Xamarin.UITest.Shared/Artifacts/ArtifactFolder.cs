using System;
using System.IO;
using System.Linq;
using Xamarin.UITest.Shared.Hashes;
using Xamarin.UITest.Shared.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.UITest.Shared.Resources;
using System.Reflection;

namespace Xamarin.UITest.Shared.Artifacts
{
    public class ArtifactFolder
    {
        readonly string _fullPath;
        readonly DateTime _createdUtc = DateTime.UtcNow;
        readonly HashHelper _hashHelper = new HashHelper();

        public ArtifactFolder(params object[] dependencies)
        {
            var identifier = CalculateFolderIdentifier(dependencies);
            _fullPath = CreateArtifactFolder(identifier);

            Log.Debug("Artifact folder: " + _fullPath);
        }

        string CalculateFolderIdentifier(object[] dependencies)
        {
            dependencies = dependencies
                .Concat(GetRelevantLoadedAssemblies())
                .ToArray();

            return _hashHelper.GetCombinedSha1Hash(dependencies) ?? _hashHelper.GetSha256Hash(Guid.NewGuid().ToString());
        }

        FileInfo[] GetRelevantLoadedAssemblies()
        {
            var relevantNames = new HashSet<string>(new [] { "Xamarin.UITest.dll", "Xamarin.UITest.Shared.dll" });

            var relevantAssembliesQuery = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    where !assembly.IsDynamic
                    where !string.IsNullOrEmpty(assembly.Location)
                    let codeBaseUri = new Uri(assembly.Location)
                    where codeBaseUri.IsFile
                    where File.Exists(codeBaseUri.LocalPath)
                    let assemblyName = Path.GetFileName(codeBaseUri.LocalPath)
                    where relevantNames.Contains(assemblyName)
                    select new FileInfo(codeBaseUri.LocalPath));

            return relevantAssembliesQuery.ToArray();
        }

        public DirectoryInfo GetOutputFolder()
        {
            var directoryName = string.Format("Output-{0:yyyyMMdd-HHmmss-ffff}", _createdUtc);
            var outputFolder = new DirectoryInfo(Path.Combine(_fullPath, directoryName));
            outputFolder.Create();
            return outputFolder;
        }

        string CreateArtifactFolder(string identifier)
        {
            var artifactFolderName = string.Format("a-{0}", identifier);
            var fullPath = Path.Combine(RootArtifactFolder, artifactFolderName);

            Directory.CreateDirectory(fullPath);
            Directory.SetLastAccessTime(fullPath, DateTime.Now);

            return fullPath;
        }

        public string FullPath
        {
            get { return _fullPath; }
        }

        public string CreateArtifact(string artifactName, Action<string> createAction)
        {
            var artifactPath = GetArtifactPath(artifactName);

            if (!artifactPath.StartsWith(_fullPath))
            {
                throw new ArgumentException("Artifacts are only allowed within the artifact folder.");
            }

            if (HasArtifact(artifactName))
            {
                Log.Debug("Using cached artifact: " + artifactName);
                return artifactPath;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));

            var workPath = GetArtifactWorkPath(artifactName);
            Directory.CreateDirectory(Path.GetDirectoryName(workPath));

            try
            {
                createAction(workPath);
                if(!HasArtifact(artifactName)) 
                {
                    if (File.Exists(workPath))
                    {
                        File.Move(workPath, artifactPath);
                    }

                    // In mono 6 - File.Move fails on Directories (seems intermittent)
                    if (Directory.Exists(workPath))
                    {
                        Directory.Move(workPath, artifactPath);
                    }
                    
                }
            }
            catch (Exception)
            {
                if (File.Exists(workPath))
                {
                    File.Delete(workPath);
                }

                if (Directory.Exists(workPath))
                {
                    Directory.Delete(workPath);
                }

                throw;
            }

            return artifactPath;
        }

        public bool HasArtifact(string artifactName)
        {
            var artifactPath = GetArtifactPath(artifactName);

            return File.Exists(artifactPath) || Directory.Exists(artifactPath);
        }

        public string GetArtifactPath(string artifactName)
        {
            var artifactPath = Path.Combine(_fullPath, artifactName);
            return new FileInfo(artifactPath).FullName;
        }

        string GetArtifactWorkPath(string artifactName)
        {
            var artifactWorkPath = Path.Combine(_fullPath, Process.GetCurrentProcess().Id.ToString(), artifactName);
            return new FileInfo(artifactWorkPath).FullName;
        }


        string RootArtifactFolder
        {
            get
            {
                var folder = Path.Combine(Path.GetTempPath(), "uitest");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public string CreateArtifactFolder(string artifactFolderName, Action<string> createAction, params string[] requiredFiles)
        {
            var artifactPath = GetArtifactPath(artifactFolderName);

            if (!artifactPath.StartsWith(_fullPath))
            {
                throw new ArgumentException("Artifacts are only allowed within the artifact folder.");
            }

            if (HasArtifact(artifactFolderName))
            {
                if (HasRequiredFiles (artifactPath, requiredFiles)) {
                    Log.Debug ("Using cached artifact folder: " + artifactFolderName);
                    return artifactPath;
                } else {
                    DeleteIncompleteArtifactFolder (artifactPath);
                    Log.Debug ("Recreating artifact folder: " + artifactFolderName);
                }
            }

            var workPath = GetArtifactWorkPath(artifactFolderName);
            Directory.CreateDirectory(workPath);

            try
            {
                createAction(workPath);
                if(!HasArtifact(artifactFolderName)) 
                {
                    Directory.Move(workPath, artifactPath);
                }
            }
            catch (Exception)
            {
                if (File.Exists(workPath))
                {
                    File.Delete(workPath);
                }

                if (Directory.Exists(workPath))
                {
                    Directory.Delete(workPath, true);
                }

                throw;
            }
            return artifactPath;
        }

        bool HasRequiredFiles(string artifactPath, params string[] requiredFiles)
        {
            foreach (var requiredFile in requiredFiles) {
                var fullName = Path.Combine (artifactPath, requiredFile);
                if (!File.Exists(fullName)) {
                    Log.Debug (string.Format("Artifact folder {0} is missing required file {1} ", artifactPath, requiredFile));
                    return false;
                }
            }
            return true;
        }

        void DeleteIncompleteArtifactFolder(string artifactPath)
        {
            Log.Debug ("Deleting incomplete artifact folder: " + artifactPath);
            Directory.Delete (artifactPath, true);
            if (Directory.Exists(artifactPath))
            {
                throw new Exception ("Unable to delete incomplete artifact folder: " + artifactPath);
            }
        }
    }
}
