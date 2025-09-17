using NUnit.Framework;
using Should;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.PlatformSpecific;

namespace Xamarin.UITest.Tests.Queries
{
    [TestFixture]
    public class AndroidMappingTests
    {
        [Test]
        public void AndroidResult_ContentDescriptionMapsToLabel()
        {
            var result = new AndroidResult { ContentDescription = "test" };

            var mapped = new AppResult(result);

            mapped.Label.ShouldEqual(result.ContentDescription);
        }

        [Test]
        public void AndroidResult_DescriptionSame()
        {
            var result = new AndroidResult { Description = "desc" };

            var mapped = new AppResult(result);

            mapped.Description.ShouldEqual(result.Description);
        }

        [Test]
        public void AndroidResult_EnabledSame()
        {
            var result = new AndroidResult { Enabled = true };

            var mapped = new AppResult(result);

            mapped.Enabled.ShouldEqual(result.Enabled);
        }

        [Test]
        public void AndroidResult_IdSame()
        {
            var result = new AndroidResult { Id = "id" };

            var mapped = new AppResult(result);

            mapped.Id.ShouldEqual(result.Id);
        }

        [Test]
        public void AndroidResult_QualifiedClassMapsToClass()
        {
            var result = new AndroidResult { Class = "class" };

            var mapped = new AppResult(result);

            mapped.Class.ShouldEqual(result.Class);
        }

        [Test]
        public void AndroidResult_TextSame()
        {
            var result = new AndroidResult { Text = "text" };

            var mapped = new AppResult(result);

            mapped.Text.ShouldEqual(result.Text);
        }

        [Test]
        public void AndroidResult_RectSame()
        {
            var result = new AndroidResult
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