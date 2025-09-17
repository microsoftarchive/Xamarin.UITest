using NUnit.Framework;
using Should;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.PlatformSpecific;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.Utils;

namespace Xamarin.UITest.Tests.Queries
{
    [TestFixture]
    public class iOSRectTransformerTests
    {
        iOSRectTransformer _transformer;

        [SetUp]
        public void BeforeEachTest()
        {
            _transformer = new iOSRectTransformer(new iOSResolution(768, 1024), new VersionNumber("7.0"));
        }

        [Test]
        public void DoesNotChangeRectWhenOrientationDown()
        {
            var inputRect = new AppRect { CenterX = 100, CenterY = 200, X = 75, Width = 50, Y = 150, Height = 100 };
            
            var rect = _transformer.TransformRect(inputRect, ScrollDirection.Down);

            RectIsSane(rect);

            rect.X.ShouldEqual(75);
            rect.Y.ShouldEqual(150);
            rect.Height.ShouldEqual(100);
            rect.Width.ShouldEqual(50);
            rect.CenterX.ShouldEqual(100);
            rect.CenterY.ShouldEqual(200);
        }

        [Test]
        public void FlipsRectWhenOrientationUp()
        {
            var inputRect = new AppRect { CenterX = 100, CenterY = 200, X = 75, Width = 50, Y = 150, Height = 100 };
            
            var rect = _transformer.TransformRect(inputRect, ScrollDirection.Up);

            RectIsSane(rect);

            rect.X.ShouldEqual(643);
            rect.Y.ShouldEqual(774);
            rect.Height.ShouldEqual(100);
            rect.Width.ShouldEqual(50);
            rect.CenterX.ShouldEqual(668);
            rect.CenterY.ShouldEqual(824);
        }

        [Test]
        public void FlipsYAndSwapsXYWhenOrientationRight()
        {
            var inputRect = new AppRect { CenterX = 100, CenterY = 200, X = 75, Width = 50, Y = 150, Height = 100 };

            var rect = _transformer.TransformRect(inputRect, ScrollDirection.Right);

            RectIsSane(rect);

            rect.X.ShouldEqual(150);
            rect.Y.ShouldEqual(643);
            rect.Height.ShouldEqual(50);
            rect.Width.ShouldEqual(100);
            rect.CenterX.ShouldEqual(200);
            rect.CenterY.ShouldEqual(668);
        }

        [Test]
        public void FlipsXAndSwapsXYWhenOrientationLeft()
        {
            var inputRect = new AppRect { CenterX = 100, CenterY = 200, X = 75, Width = 50, Y = 150, Height = 100 };

            var rect = _transformer.TransformRect(inputRect, ScrollDirection.Left);

            RectIsSane(rect);

            rect.X.ShouldEqual(774);
            rect.Y.ShouldEqual(75);
            rect.Height.ShouldEqual(50);
            rect.Width.ShouldEqual(100);
            rect.CenterX.ShouldEqual(824);
            rect.CenterY.ShouldEqual(100);
        }

        [Test]
        public void TransformsRectsOnResults()
        {
            var inputRect = new AppRect { CenterX = 100, CenterY = 200, X = 75, Width = 50, Y = 150, Height = 100 };
            var result = new iOSResult { Rect = inputRect };

            var rect = _transformer.TransformRect(result, ScrollDirection.Left).Rect;

            rect.CenterX.ShouldEqual(824);
            rect.CenterY.ShouldEqual(100);
        }

        void RectIsSane(AppRect rect)
        {
            rect.CenterX.ShouldEqual(rect.X + (rect.Width / 2), "CenterX is off.");
            rect.CenterY.ShouldEqual(rect.Y + (rect.Height / 2), "CenterY is off.");
        }
    }
}