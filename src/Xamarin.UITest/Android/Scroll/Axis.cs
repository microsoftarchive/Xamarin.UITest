using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.UITest.Android.Scroll
{
    enum Axis
    {
        Horizontal,
        Vertical,
        Both
    }

    internal static class AxisExtensions
    {
        internal static bool OnAxis(this Axis axis, ScrollDirection direction)
        {
            switch (axis)
            {
                case Axis.Horizontal:
                    return direction == ScrollDirection.Left || direction == ScrollDirection.Right;
                case Axis.Vertical:
                    return direction == ScrollDirection.Up || direction == ScrollDirection.Down;
                case Axis.Both:
                default:
                    return true;
            }
        }

        internal static ScrollDirection Start(this Axis axis)
        {
            switch (axis)
            {
                case Axis.Horizontal:
                    return ScrollDirection.Left;
                case Axis.Vertical:
                    return ScrollDirection.Up;
                case Axis.Both:
                default:
                    throw new Exception("Not supported");
            }
        }

        internal static ScrollDirection End(this Axis axis)
        {
            switch (axis)
            {
                case Axis.Horizontal:
                    return ScrollDirection.Right;
                case Axis.Vertical:
                    return ScrollDirection.Down;
                case Axis.Both:
                default:
                    throw new Exception("Not supported");
            }

        }
    }
}
