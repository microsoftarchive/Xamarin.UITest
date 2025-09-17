# Automate iOS native apps with Xamarin.UITest

To follow the guide, you need access to the app source code.

## Add an automation backend to your app

Part of the Xamarin.UITest toolchain is the backend that runs alongside the app. The Xamarin.UITest test script communicates with the backend, which executes the commands on the simulator or device.

Because Xamarin.UITest shares backend with Calabash, first step is to [link the calabash.framework, using one of the three methods from the Calabash doc](https://github.com/calabash/calabash-ios#step-1-link-calabashframework).

You should see that the server is running in Xcode, after following the Calabash guide.

You build the `.app` file from the terminal like below:

> [!NOTE]
> .app files only work simulators

```
 xcrun xcodebuild \
  -project ProjectFile.xcodeproj \
  -configuration ConfiguratonWithCalabashLinked \
  -sdk iphonesimulator \
  ARCHS="x86_64" \
  VALID_ARCHS="x86_64" \
  ONLY_ACTIVE_ARCH=NO \
  -scheme SchemeWithCalabashLinked \
  -derivedDataPath DerivedData \
  build
```

The following switches must match your project:

- `-project`
- `-configuration`
- `-scheme`

## Run your Xamarin.UITest on a simulator

With the Calabash backend setup, the next step is to configure your Xamarin.UITest so that it opens your app on a simulator. [See the Working with Simulators page on how identify simulators](simulator.md).

The `.AppBundle()` and `.DeviceIdentifier()` must be configured. You can find the AppBundle in the DerivedData if your app was built using the command from Add an automation backend to your app.

## Run a test

Start your tests from Visual Studio for Mac by selecting Unit Tests in the right side.
