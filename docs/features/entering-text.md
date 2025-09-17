# Working with text entry

A common task for a test is to enter text into views. Xamarin.UITest provides two APIs to address this need:

- `IApp.EnterText`
- `IApp.ClearText`

Each of these APIs will be discussed in more detail below.

The `IApp.EnterText` method allows a test to enter text into a view. The behavior of this method is slightly different between iOS and Android:

- **iOS** - Xamarin.UITest will display the soft keyboard, and then simulate presses on the keyboard.
- **Android** - Xamarin.UITest doesn't use the soft keyboard. Instead, it injects itself between the soft keyboard and the application and simulates key presses.

The following snippet shows an example of entering text:

```csharp
app.EnterText(c=>c.Marked("creditCardTextField"), new string('9', 15));
```

## Disable the Hardware Keyboard

`IApp.EnterText` will fail if the iOS simulator is configured to use the hardware keyboard. Before running your Xamarin.UITests locally, ensure that the simulator is configured to use the soft keyboard.

![Screenshot of the Keyboard menu in the iOS Simulator](./images/entering-text-01-sml.png)]

## Clearing Text

The `IApp.ClearText` method will clear the text from the view, for example:

```csharp
app.ClearText(c=>c.Marked("creditCardTextField"));
```
