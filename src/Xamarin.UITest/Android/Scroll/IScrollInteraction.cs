using Xamarin.UITest.Queries;

namespace Xamarin.UITest.Android.Scroll
{
    internal interface IScrollInteraction
    {
        AppQuery Query();
        bool Scroll(ScrollDirection direction);
        void ScrollToStart();
        void ScrollToEnd();
    }
}