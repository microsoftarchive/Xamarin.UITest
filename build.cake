#addin nuget:?package=Cake.FileHelpers&version=5.0.0

var target = Argument("target", "CreatePackage");
var configuration = Argument("configuration", "Release");

var version = EnvironmentVariable<string>("BUILD_VERSION", "0.0.0");
var versionNumber = EnvironmentVariable<string>("VERSION_NUMBER", "0.0.0");

var buildDir = "./build";
var compileDir = $"{buildDir}/compile";
var mergeDir = $"{buildDir}/merge";

var coreBuildDir = $"{compileDir}/core";
var coreMergeDir = $"{mergeDir}/core";
var coreZipDir = $"{buildDir}/core-zip";

var cliBuildDir = $"{compileDir}/cli";
var cliMergeDir = $"{mergeDir}/cli";
var cliZipDir = $"{buildDir}/cli-zip";

var nunit3BuildDir = $"{buildDir}/nunit3";
var nunit3MergeDir = $"{mergeDir}/nunit3";

var nunit_console = $"nunit-console";

var replConsoleBuildDir = $"{compileDir}/repl-console";
var replConsoleMergeDir = $"{mergeDir}/repl-console";

var nugetBuildDir = buildDir;

var integrationTestsDir = "./IntegrationTests";

var IntegrationNunit3ProjectPath = "./src/Xamarin.UITest.Integration.NUnit3/Xamarin.UITest.Integration.NUnit3.csproj";
var CLIAssemblyAnalysisFilesPath = "./src/Xamarin.UITest.CLI/AssemblyAnalysis/Files";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories($"./src/**/bin");
    CleanDirectories($"./src/**/obj");
    CleanDirectory(buildDir);
});

Task("Version")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var UITestAssemblyInfoFilePath = "./src/Xamarin.UITest/Properties/AssemblyInfo.cs";
    var CLIAssemblyInfoFilePath = "./src/Xamarin.UITest.CLI/Properties/AssemblyInfo.cs";
    CreateAssemblyInfo(UITestAssemblyInfoFilePath, new AssemblyInfoSettings {
        Title = "Xamarin.UITest",
        Description = "UI test automation for Android and iOS.",
        Guid = "5d5a784f-65a0-45c9-a83d-10bfb7004c70",
        Product = "Xamarin.UITest",
        Version = $"{versionNumber}.0",
        FileVersion = $"{versionNumber}.0",
        InternalsVisibleTo = new [] {
            "Xamarin.UITest.Tests",
            "DynamicProxyGenAssembly2"
        }
    });
    CreateAssemblyInfo(CLIAssemblyInfoFilePath, new AssemblyInfoSettings {
        Title = "Xamarin.UITest.CLI",
        Description = "UI test automation CLI.",
        Guid = "2c8f99bb-4193-442b-bd3b-232d5e9da581",
        Product = "Xamarin.UITest.CLI",
        Version = $"{versionNumber}.0",
        FileVersion = $"{versionNumber}.0"
    });
});

Task("BuildRepl")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(() =>
{
    Information("Building Xamarin.UITest.Repl.Console project for .NET Framework 4.6.2...");
    DotNetBuild("./src/Xamarin.UITest.Repl.Console/Xamarin.UITest.Repl.Console.csproj", new DotNetBuildSettings
    {
        Configuration = configuration,
        OutputDirectory = $"{replConsoleBuildDir}/net462",
        Framework = "net462"
    });

    Information("Building Xamarin.UITest.Repl.Console project for .NET 6...");
    DotNetBuild("./src/Xamarin.UITest.Repl.Console/Xamarin.UITest.Repl.Console.csproj", new DotNetBuildSettings
    {
        Configuration = configuration,
        OutputDirectory = $"{replConsoleBuildDir}/net6.0",
        Framework = "net6.0"
    });

    Information("Building Xamarin.UITest.Repl.Console project for .NET 8...");
    DotNetBuild("./src/Xamarin.UITest.Repl.Console/Xamarin.UITest.Repl.Console.csproj", new DotNetBuildSettings
    {
        Configuration = configuration,
        OutputDirectory = $"{replConsoleBuildDir}/net8.0",
        Framework = "net8.0"
    });
});

Task("BuildAndZipRepl")
    .IsDependentOn("Clean")
    .IsDependentOn("BuildRepl")
    .Does(() =>
{
    Information("Zipping Xamarin.UITest.Repl.Console outputs...");
    Information("Zipping for .NET Framework 4.6.2...");
    EnsureDirectoryExists($"{replConsoleMergeDir}/net462");
    using(VerboseVerbosity()) Zip($"{replConsoleBuildDir}/net462", $"{replConsoleMergeDir}/net462/xut-repl.zip", $"{replConsoleBuildDir}/net462/*");

    Information("Zipping for .NET 6...");
    EnsureDirectoryExists($"{replConsoleMergeDir}/net6.0");
    using(VerboseVerbosity()) Zip($"{replConsoleBuildDir}/net6.0", $"{replConsoleMergeDir}/net6.0/xut-repl.zip", $"{replConsoleBuildDir}/net6.0/*");

    Information("Zipping for .NET 8...");
    EnsureDirectoryExists($"{replConsoleMergeDir}/net8.0");
    using(VerboseVerbosity()) Zip($"{replConsoleBuildDir}/net8.0", $"{replConsoleMergeDir}/net8.0/xut-repl.zip", $"{replConsoleBuildDir}/net8.0/*");

    Information("Copying Xamarin.UITest.Repl.Console zipped outputs to Xamarin.UITest.Repl Files directory...");
    CopyFile($"{replConsoleMergeDir}/net462/xut-repl.zip", "./src/Xamarin.UITest.Repl/Files/net462/xut-repl.zip");
    CopyFile($"{replConsoleMergeDir}/net6.0/xut-repl.zip", "./src/Xamarin.UITest.Repl/Files/net6.0/xut-repl.zip");
    CopyFile($"{replConsoleMergeDir}/net8.0/xut-repl.zip", "./src/Xamarin.UITest.Repl/Files/net8.0/xut-repl.zip");
});

Task("BuildUITest")
    .IsDependentOn("Clean")
    .IsDependentOn("BuildAndZipRepl")
    .Does(() =>
{
    Information("Building Xamarin.UITest and related projects");

    Information("Building Xamarin.UITest project");
    Information("Building for .NET Framework 4.6.2...");
    DotNetBuild("./src/Xamarin.UITest/Xamarin.UITest.csproj", new DotNetBuildSettings
    {
        Configuration = configuration,
        OutputDirectory = $"{coreBuildDir}/net462",
        Framework = "net462"
    });
    Information("Building for .NET 6...");
    DotNetBuild("./src/Xamarin.UITest/Xamarin.UITest.csproj", new DotNetBuildSettings
    {
        Configuration = configuration,
        OutputDirectory = $"{coreBuildDir}/net6.0",
        Framework = "net6.0"
    });
    Information("Building for .NET 8...");
    DotNetBuild("./src/Xamarin.UITest/Xamarin.UITest.csproj", new DotNetBuildSettings
    {
        Configuration = configuration,
        OutputDirectory = $"{coreBuildDir}/net8.0",
        Framework = "net8.0"
    });
});

Task("BuildAndZipUITest")
    .IsDependentOn("Clean")
    .IsDependentOn("BuildUITest")
    .Does(() =>
{
    Information("Zipping Xamarin.UITest outputs");
    EnsureDirectoryExists(coreZipDir);
    EnsureDirectoryExists($"{coreZipDir}/net462");
    CopyFile($"{coreBuildDir}/net462/Xamarin.UITest.xml", $"{coreZipDir}/net462/Xamarin.UITest.xml");
    CopyFile($"{coreBuildDir}/net462/Xamarin.UITest.dll", $"{coreZipDir}/net462/Xamarin.UITest.dll");
    CopyFile($"{coreBuildDir}/net462/Xamarin.UITest.Shared.dll", $"{coreZipDir}/net462/Xamarin.UITest.Shared.dll");
    CopyFile($"{coreBuildDir}/net462/Xamarin.UITest.Repl.dll", $"{coreZipDir}/net462/Xamarin.UITest.Repl.dll");

    EnsureDirectoryExists($"{coreZipDir}/net6.0");
    CopyFile($"{coreBuildDir}/net6.0/Xamarin.UITest.xml", $"{coreZipDir}/net6.0/Xamarin.UITest.xml");
    CopyFile($"{coreBuildDir}/net6.0/Xamarin.UITest.dll", $"{coreZipDir}/net6.0/Xamarin.UITest.dll");
    CopyFile($"{coreBuildDir}/net6.0/Xamarin.UITest.Shared.dll", $"{coreZipDir}/net6.0/Xamarin.UITest.Shared.dll");
    CopyFile($"{coreBuildDir}/net6.0/Xamarin.UITest.Repl.dll", $"{coreZipDir}/net6.0/Xamarin.UITest.Repl.dll");

    EnsureDirectoryExists($"{coreZipDir}/net8.0");
    CopyFile($"{coreBuildDir}/net8.0/Xamarin.UITest.xml", $"{coreZipDir}/net8.0/Xamarin.UITest.xml");
    CopyFile($"{coreBuildDir}/net8.0/Xamarin.UITest.dll", $"{coreZipDir}/net8.0/Xamarin.UITest.dll");
    CopyFile($"{coreBuildDir}/net8.0/Xamarin.UITest.Shared.dll", $"{coreZipDir}/net8.0/Xamarin.UITest.Shared.dll");
    CopyFile($"{coreBuildDir}/net8.0/Xamarin.UITest.Repl.dll", $"{coreZipDir}/net8.0/Xamarin.UITest.Repl.dll");

    using(VerboseVerbosity()) Zip(coreZipDir, $"{buildDir}/Xamarin.UITest-{version}.zip", $"{coreZipDir}/*");
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);