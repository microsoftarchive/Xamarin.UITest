using System;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.PlatformSpecific;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.iOS
{
    internal class iOSRectTransformer
    {
        readonly iOSResolution _resolution;
        readonly VersionNumber _osVersion;

        public iOSRectTransformer(iOSResolution resolution, VersionNumber osVersion)
        {
            if (resolution == null)
            {
                throw new ArgumentNullException("resolution");
            }

            _resolution = resolution;
            _osVersion = osVersion;
        }

        public AppRect TransformRect(AppRect rect, ScrollDirection homeButtonOrientation)
        {
            if (rect == null)
            {
                return null;
            }

            return rect;
        }

        public iOSResult TransformRect(iOSResult result, ScrollDirection homeButtonOrientation)
        {
            result.Rect = TransformRect(result.Rect, homeButtonOrientation);
            return result;
        }
    }
}