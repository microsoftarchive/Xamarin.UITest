using System;

namespace Xamarin.UITest
{
    internal enum ScrollDirection
    {
        Up,
        Down,
        Left, 
        Right
    }

    internal static class ScrollDirectionExtensions
    {
        public static ScrollDirection Opposite(this ScrollDirection direction)
        {
            switch (direction)
            {
                case ScrollDirection.Down:
                    return ScrollDirection.Up;
                case ScrollDirection.Up:
                    return ScrollDirection.Down;
                case ScrollDirection.Left:
                    return ScrollDirection.Right;
                case ScrollDirection.Right:
                    return ScrollDirection.Left;
                default:
                    throw new Exception("Unknown direction: " + direction);
            }
        }
    }
}