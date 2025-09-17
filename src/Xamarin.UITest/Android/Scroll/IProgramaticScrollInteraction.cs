namespace Xamarin.UITest.Android.Scroll
{
    internal interface IProgramaticScrollInteraction : IScrollInteraction
    {
        bool Supports(ScrollDirection direction);
    }
}