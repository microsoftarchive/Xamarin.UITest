## Building the project

### Prerequisites
- macOS version 13.x or above
- Xcode version 14.2 or above
- .NET installed

### Building

Use CAKE build script to build and pack Xamarin.UITest
```
dotnet cake --verbosity=diagnostic
```
Or use Visual Studio Code with C# DevKit Extension installed to open and build src/Xamarin.UITest.sln solution.

---

## Running tests

Test application bundles zip are placed under `binaries` directory.

Run the tests directly from VSCode. To test on physical device signed bundle of DeviceAgent should be provided with iOSConfiguration.

To control the device or simulator that the tests are run on, create a
file `src/IntegrationTests/test-config.json` based on the example file
in that directory.

## Documentation

[Xamarin.UITest](docs/index.md)
