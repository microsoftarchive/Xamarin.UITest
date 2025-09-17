# Automating Xamarin.Forms DatePickers & TimePickers with Xamarin.UITest

Selecting values from these pickers can be tricky in UITest. This issue is because the DatePicker control is an OS construct outside of your app’s UI interface.

## DatePicker Example

```csharp
public void SetDatePicker(DateTime date)
{
    // needed to tap & thus select the items
    string month = DateTimeFormatInfo.CurrentInfo.GetMonthName(date.Month);
    string day = date.Day.ToString();
    string year = date.Year.ToString();

    if (platform == Platform.iOS)
    {
        //Invoke the native method selectRow()
        app.Query(x => x.Class("UIPickerView").Invoke("selectRow", date.Month, "inComponent", 0, "animated", true)); // brings month in scope
        var rect = app.Query(x => x.Class("UIPickerTableView").Index(0).Child()).First().Rect;
        app.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterX, rect.CenterY + 20);  // nudging control to trigger the label to update

        app.Query(x => x.Class("UIPickerView").Invoke("selectRow", date.Day, "inComponent", 1, "animated", true));
        rect = app.Query(x => x.Class("UIPickerTableView").Index(3).Child()).First().Rect;
        app.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterX, rect.CenterY + 20);

        app.Query(x => x.Class("UIPickerView").Invoke("selectRow", date.Year, "inComponent", 2, "animated", true));
        rect = app.Query(x => x.Class("UIPickerTableView").Index(6).Child()).First().Rect;
        app.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterX, rect.CenterY + 20);

        app.Tap(c => c.Text("Done"));
    } else // Android
    {
        app.Query(x => x.Class("DatePicker").Invoke("updateDate", date.Year, date.Month, date.Day));
        app.Tap("OK");
    }
}
```

## TimePicker Example

```csharp
public void SetTimePicker(int hour, int minute, bool am)
{
    string hourString = hour.ToString();
    string minuteString = minute.ToString();
    DateTime date = DateTime.Now;

    DateTime time = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
    int ampm;

    if (am)
    {
        ampm = 0;
    }
    else ampm = 1;

    if (platform == Platform.iOS)
    {
        app.Query(x => x.Class("UIPickerView").Invoke("selectRow", time.Hour, "inComponent", 0, "animated", true)); //if time.Hour == 0, than hour is '1'. if time.Hour == 11, than hour is '12'
        app.Query(x => x.Class("UIPickerView").Invoke("selectRow", time.Minute, "inComponent", 1, "animated", true)); //if time.Minute == 0, than minutes is '1'. if time.Minute == 59, than minutes is '59'
        app.Query(x => x.Class("UIPickerView").Invoke("selectRow", ampm, "inComponent", 2, "animated", true)); //0 == AM and 1 == PM

        // nudging each control very slightly to trigger the label to update
        var rect = app.Query(x => x.Class("UIPickerTableView").Index(0).Child()).First().Rect;
        app.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterX, rect.CenterY + 20);

        rect = app.Query(x => x.Class("UIPickerTableView").Index(3).Child()).First().Rect;
        app.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterX, rect.CenterY + 20);

        rect = app.Query(x => x.Class("UIPickerTableView").Index(6).Child()).First().Rect;
        app.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterX, rect.CenterY + 20);

        app.Tap(c => c.Text("Done"));
    } else // Android
    {
        // switch to Text entry for time
        app.Tap("toggle_mode");

        app.ClearText();
        app.EnterText(hourString);

        app.ClearText("input_minute");
        app.EnterText(minuteString);

        if (!am)
        {
            app.Tap("AM"); // opens spinner
            app.Tap("PM"); // changes to PM
        }
        app.Tap("OK"); // finalize time
    }
}
```
