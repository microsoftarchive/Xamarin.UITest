# Using AppDataMode to manage app data between tests

AppDataMode has three options that manage how your app is managed between separate test methods.

## AppDataMode.Auto and AppDataMode.Clear (same behavior)

- Locally the app is never reinstalled.
- Android - the app is installed the first time you call StartApp(). Every time after that the data is cleared from the device. The same is true for iOS simulators.
- iOS - No data is ever cleared. In the cloud your app is reinstalled before each test so the data is cleared. The app can persist data in the iOS keychain between installs, which can sometimes be an issue.

## AppDataMode.DoNotClear

- Locally the app is never reinstalled and the data isn't cleared. It will still get terminated and restarted every time you call StartApp() though.
- In the cloud your app won't get reinstalled and its data won't get cleared (as it works locally).
