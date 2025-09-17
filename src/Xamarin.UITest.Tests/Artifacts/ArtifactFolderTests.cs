using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Artifacts;

namespace Xamarin.UITest.Tests.Artifacts
{
    [TestFixture]
    public class ArtifactFolderTests
    {
        [Test]
        public void OnlyAllowsArtifactsWithinFolder()
        {
            var folder = new ArtifactFolder();
            Assert.Throws<ArgumentException>(() => folder.CreateArtifact("../text.txt", x => File.WriteAllText(x, "test")));
        }

        [Test]
        public void CanCreateDirectories()
        {
            var folder = new ArtifactFolder();

            var folderName = "folder-" + Guid.NewGuid();
            var fileName = "file-" + Guid.NewGuid();

            var artifactName = Path.Combine(folderName, fileName);


            folder.CreateArtifact(artifactName, x => File.WriteAllText(x, "test"));

            Directory.Exists(folder.GetArtifactPath(folderName)).ShouldBeTrue();
            File.Exists(folder.GetArtifactPath(artifactName)).ShouldBeTrue();
        }

        [Test]
        public void IsAtomicForFiles()
        {
            var folder = new ArtifactFolder();
            var artifactName = "file-" + Guid.NewGuid();

            try
            {
                folder.CreateArtifact(artifactName, x =>
                {
                    File.WriteAllText(x, "test");
                    throw new Exception();
                });

                Assert.Fail("Expected exception");
            }
            catch (Exception)
            {
                File.Exists(folder.GetArtifactPath(artifactName)).ShouldBeFalse();
            }
        }

        [Test]
        public void IsAtomicForDirectory()
        {
            var folder = new ArtifactFolder();
            var artifactName = "file-" + Guid.NewGuid();

            try
            {
                folder.CreateArtifact(artifactName, x =>
                {
                    Directory.CreateDirectory(x);
                    throw new Exception();
                });

                Assert.Fail("Expected exception");
            }
            catch (Exception)
            {
                Directory.Exists(folder.GetArtifactPath(artifactName)).ShouldBeFalse();
            }
        }

        [Test]
        public void ArtifactFolderCreate()
        {
            const string ArtifactFolderName = "ArtifactFolderCreate";
            var artifactFolder = new ArtifactFolder(ArtifactFolderName);
            Directory.Exists (artifactFolder.FullPath).ShouldBeTrue ();

            var artifactPath = artifactFolder.CreateArtifactFolder (ArtifactFolderName, 
                dir=>CreateArtifactFolderTestFiles(dir, "test1", "test2"));

            var pathTest1 = Path.Combine (artifactPath, "test1");
            var pathTest2 = Path.Combine (artifactPath, "test2");
            File.Exists (pathTest1).ShouldBeTrue ();
            File.Exists (pathTest2).ShouldBeTrue ();

            Directory.Delete (artifactFolder.FullPath, true);
        }

        [Test]
        public void ArtifactFolderReuse()
        {
            const string ArtifactFolderName = "ArtifactFolderReuse";
            var artifactFolder = new ArtifactFolder(ArtifactFolderName);
            Directory.Exists (artifactFolder.FullPath).ShouldBeTrue ();

            var artifactPath = artifactFolder.CreateArtifactFolder (ArtifactFolderName, 
                dir=>CreateArtifactFolderTestFiles(dir, "test1", "test2"));
 
            var pathTest1 = Path.Combine (artifactPath, "test1");
            var pathTest2 = Path.Combine (artifactPath, "test2");
            var pathTest3 = Path.Combine (artifactPath, "test3");
            File.Exists (pathTest1).ShouldBeTrue ();
            File.Exists (pathTest2).ShouldBeTrue ();
            File.Exists (pathTest3).ShouldBeFalse ();

            artifactFolder.CreateArtifactFolder (ArtifactFolderName, 
                dir=>CreateArtifactFolderTestFiles(dir, "test1", "test2", "test3"));
            
            File.Exists (pathTest1).ShouldBeTrue ();
            File.Exists (pathTest2).ShouldBeTrue ();
            File.Exists (pathTest3).ShouldBeFalse (); // second CreateArtifactFolder action did not execute

            Directory.Delete (artifactFolder.FullPath, true);
        }

        [Test]
        public void ArtifactFolderRebuild()
        {
            const string ArtifactFolderName = "ArtifactFolderRebuild";
            var artifactFolder = new ArtifactFolder(ArtifactFolderName);
            Directory.Exists (artifactFolder.FullPath).ShouldBeTrue ();

            var artifactPath = artifactFolder.CreateArtifactFolder (ArtifactFolderName, 
                dir=>CreateArtifactFolderTestFiles(dir, "test1", "test2"));

            var pathTest1 = Path.Combine (artifactPath, "test1");
            var pathTest2 = Path.Combine (artifactPath, "test2");
            var pathTest3 = Path.Combine (artifactPath, "test3");
            File.Exists (pathTest1).ShouldBeTrue ();
            File.Exists (pathTest2).ShouldBeTrue ();
            File.Exists (pathTest3).ShouldBeFalse ();

            File.Delete (pathTest1);
            File.Exists (pathTest1).ShouldBeFalse ();

            artifactFolder.CreateArtifactFolder (ArtifactFolderName, 
                dir=>CreateArtifactFolderTestFiles(dir, "test1", "test2", "test3"), 
                "test1");

            File.Exists (pathTest1).ShouldBeTrue ();
            File.Exists (pathTest2).ShouldBeTrue ();
            File.Exists (pathTest3).ShouldBeTrue (); // // second CreateArtifactFolder action did not execute

            Directory.Delete (artifactFolder.FullPath, true);
        }

        void CreateArtifactFolderTestFiles(string dir, params string[] fileNames)
        {
            foreach (var fileName in fileNames) 
            {
                File.WriteAllText (Path.Combine (dir, fileName), "text");
            }
        }

    }
}