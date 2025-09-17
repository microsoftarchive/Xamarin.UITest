using System;

namespace Xamarin.UITest.Events
{
    /// <summary>
    /// Can be used on static methods to register for notifications immediately after an app instance has been started.
    /// 
    /// Valid parameters:
    /// 
    /// - Zero arguments
    /// - One argument of type <see cref="Xamarin.UITest.IApp"/>.
    /// - One argument of type <see cref="Xamarin.UITest.Android.AndroidApp"/>.
    /// - One argument of type <see cref="Xamarin.UITest.iOS.iOSApp"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AfterAppStartedAttribute : Attribute
    {
    }
}