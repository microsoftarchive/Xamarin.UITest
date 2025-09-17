using NUnit.Framework;
using Should;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.PlatformSpecific;

namespace Xamarin.UITest.Tests.Queries
{
    [TestFixture]
    public class iOSMappingTests
    {
        [Test]
        public void iOSResult_LabelSame()
        {
            var result = new iOSResult { Label = "test" };

            var mapped = new AppResult(result);

            mapped.Label.ShouldEqual(result.Label);
        }

        [Test]
        public void iOSResult_DescriptionSame()
        {
            var result = new iOSResult { Description = "desc" };

            var mapped = new AppResult(result);

            mapped.Description.ShouldEqual(result.Description);
        }

        [Test]
        public void iOSResult_EnabledSame()
        {
            var result = new iOSResult { Enabled = true };

            var mapped = new AppResult(result);

            mapped.Enabled.ShouldEqual(result.Enabled);
        }

        [Test]
        public void iOSResult_IdSame()
        {
            var result = new iOSResult { Id = "id" };

            var mapped = new AppResult(result);

            mapped.Id.ShouldEqual(result.Id);
        }

        [Test]
        public void iOSResult_ClassSame()
        {
            var result = new iOSResult { Class = "class" };

            var mapped = new AppResult(result);

            mapped.Class.ShouldEqual(result.Class);
        }

        [Test]
        public void iOSResult_TextSame()
        {
            var result = new iOSResult { Text = "text" };

            var mapped = new AppResult(result);

            mapped.Text.ShouldEqual(result.Text);
        }

        [Test]
        public void iOSResult_RectSame()
        {
            var result = new iOSResult
            {
                Rect = new AppRect()
                {
                    CenterX = 1.1f,
                    CenterY = 1.2f,
                    Height = 5,
                    Width = 10,
                    X = 15,
                    Y = 20,
                }
            };

            var mapped = new AppResult(result);

            mapped.Rect.CenterX.ShouldEqual(result.Rect.CenterX);
            mapped.Rect.CenterY.ShouldEqual(result.Rect.CenterY);
            mapped.Rect.X.ShouldEqual(result.Rect.X);
            mapped.Rect.Y.ShouldEqual(result.Rect.Y);
            mapped.Rect.Height.ShouldEqual(result.Rect.Height);
            mapped.Rect.Width.ShouldEqual(result.Rect.Width);
        }
    }
}