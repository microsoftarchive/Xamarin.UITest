# Working with files

Xamarin.UITest tests may use local files for various test data. When running these tests in App Center Test, these files aren't uploaded with your tests by default. This generally results in your tests failing in App Center Test if they depend on local files.

The files must be included in your test upload for your tests to access during the test run. There are two approaches to including the files with your test upload: _embedded files_ and _included files_.

## Embedded files

One approach is to include the file as an embedded resource in your Xamarin.UITest project. This bundles the file into your Xamarin.UITest project assembly. This works in App Center Test with no additional parameters or arguments for the `appcenter test run ...` command.

Here is some code that shows how to access embedded resources from your C# test at runtime: [EmbeddedResources](https://github.com/xamarin/mobile-samples/tree/master/EmbeddedResources). The main code to use is [SharedLibResourceLoader.cs](https://github.com/xamarin/mobile-samples/blob/master/EmbeddedResources/SharedLib/ResourceLoader.cs). Include that in your Xamarin.UITest project and uncomment the convenience methods.

Include your files as embedded resources to your Xamarin.UITest project by selecting them in Solution Explorer and select **Build Action > Embedded Resource**. On a Mac, control-click or right-click the file to see the options.

Then, at run time, access the file as an embedded resource from your Xamarin.UITest code. These snippets show how to read embedded files at run time using the `SharedLibResourceLoader` (above):

```csharp
// Read the text file as a string
String contents = ResourceLoader
    .GetEmbeddedResourceString("TestFileOne.txt");
```

or

```csharp
// Read the text file line by line
StreamReader stream = new StreamReader(ResourceLoader
    .GetEmbeddedResourceStream("TestFileOne.txt"));

String line1 = stream.ReadLine();
String line2 = stream.ReadLine();
...
```

## Included files

The other approach is to use the command-line option `--include` that uploads your files with your Xamarin.UITest project.

```shell
--include <file-or-directory>
```

To set this up so that it works with the same code and file configuration locally as in App Center Test place the files or folders to use in the top-level folder of your Xamarin.UITest project.

For each file, set the property to `Copy to Output Directory`. On Windows select the file in Solution Explorer then select either **Copy to Output Directory > Copy if newer**, or, **Copy to Output Directory > Copy always**. On a Mac, control-click or right-click the file then select **Quick Properties > Copy to Output Directory**. If including a folder, repeat this for each file in the folder.

Taking this approach, the files are in the build directory at run time when executing locally. The following code snippets access the files at run time:

```csharp
StreamReader s = new StreamReader("TestFileTwo.txt");
```

or

```csharp
// Read DataFileOne.txt in the folder Data
StreamReader s = new StreamReader("./Data/DataFileOne.txt")
```

The file references above are relative to where the Xamarin.UITest code is built. This works both locally and in App Center Test. If you hard-code the full path that works on your local machine but it fails in App Center Test.

To upload the files to App Center Test, use the `--include` option in the `appcenter test run ...` command. The fragments below, as part of a full command, upload TestFileTwo.txt or the folder Data.

```shell
--include TestFileTwo.txt
```

or

```shell
--include Data
```

The files upload with the Xamarin.UITest project and the same Xamarin.UITest code that reads them locally (above) works at run time in App Center Test.

Use the `--include` option multiple times on your command line to include multiple files or multiple directories.

## Getting help

You can contact support in the App Center portal. In the upper right corner of screen, select the Help (?) menu, then choose 'Contact support'. Our dedicated support team will respond to your questions.

If you want help with a test run, navigate to the test run in question and copy the URL from your browser and paste it into the support conversation. A test run URL looks like something like https://appcenter.ms/orgs/OrgName/apps/App-Name/test/runs/77a1c67e-2cfb-4bbd-a75a-eb2b4fd0a747.
