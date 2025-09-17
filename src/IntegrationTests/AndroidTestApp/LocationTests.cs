using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Should;

public class LocationTests : AndroidTestAppBase
{
    [Test]
    [Category("Location")]
    [Ignore("This test is ignored because it might fail due to mismatching '.' and ',' in floating numbers.")]
    public void SetLocation()
    {
        var unreliableDevices = new List<string>
        {
            "Amazon Kindle Fire HD 8.9",
            "Amazon Kindle Fire (2nd Gen)",
            "Amazon Fire Phone",
            "ZTE Nubia Z7 Max",
            "Meizu MX3",
            "Lenovo S660",
            "Motorolla Moto X"
        };

        var everestLatitude = 27.988257;
        var everestLongitude = 86.925145;

        _app.Device.SetLocation(everestLatitude, everestLongitude);

        const int maxRetries = 20;
        for (var i = 1; i <= maxRetries; i++)
        {
            SelectTestActivity(c => c.Button("View Data"));

            if (i == maxRetries)
            {
                _app.Query(c => c.Id("textViewLatitude")).First().Text.ShouldEqual(everestLatitude.ToString());
                _app.Query(c => c.Id("textViewLongitude")).First().Text.ShouldEqual(everestLongitude.ToString());
            }
            else
            {
                if (_app.Query(c => c.Id("textViewLatitude")).First().Text == everestLatitude.ToString()
                    && _app.Query(c => c.Id("textViewLongitude")).First().Text == everestLongitude.ToString())
                {
                    break;
                }

                _app.Back();
                Thread.Sleep(1000);
            }
        }

        _app.Back();

        var challengerDeepLatitude = 11.373333;
        var challengerDeepLongitude = 142.591667;

        _app.Device.SetLocation(challengerDeepLatitude, challengerDeepLongitude);

        for (var j = 1; j < maxRetries; j++)
        {
            SelectTestActivity(c => c.Button("View Data"));

            if (j == maxRetries)
            {
                _app.Query(c => c.Id("textViewLatitude"))
                    .First()
                    .Text.ShouldEqual(challengerDeepLatitude.ToString());
                _app.Query(c => c.Id("textViewLongitude"))
                    .First()
                    .Text.ShouldEqual(challengerDeepLongitude.ToString());
            }
            else
            {
                if (_app.Query(c => c.Id("textViewLatitude")).First().Text == challengerDeepLatitude.ToString()
                    && _app.Query(c => c.Id("textViewLongitude")).First().Text == challengerDeepLongitude.ToString())
                {
                    break;
                }

                _app.Back();
                Thread.Sleep(1000);
            }
        }
    }
}
