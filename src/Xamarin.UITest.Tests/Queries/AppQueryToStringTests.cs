using NUnit.Framework;
using Xamarin.UITest.Queries;

namespace Xamarin.UITest.Tests.Queries
{
    public class AppQueryToStringTests
    {
        [Test]
        public void SingleAsterixIn_SingleAsterixOut()
        {
            var helper = new AppQuery(QueryPlatform.Android);
            var input = new[] { "*" };

            var output = helper.RemoveConsecutiveDuplicatesOf(input);
            var joinedOutput = string.Join(" ", output);

            Assert.AreEqual("*", joinedOutput);
        }

        [Test]
        public void ThreeAsterixIn_SingleAsterixOut()
        {
            var helper = new AppQuery(QueryPlatform.Android);
            var input = new[] { "*", "*", "*" };

            var output = helper.RemoveConsecutiveDuplicatesOf(input);
            var joinedOutput = string.Join(" ", output);

            Assert.AreEqual("*", joinedOutput);
        }

        [Test]
        public void ExtraSpacesIn_SingleAsterixOut()
        {
            var helper = new AppQuery(QueryPlatform.Android);
            var input = new[] { "*", "* ", "*   " };

            var output = helper.RemoveConsecutiveDuplicatesOf(input);
            var joinedOutput = string.Join(" ", output);

            Assert.AreEqual("*", joinedOutput);
        }

        [Test]
        public void ThreeAsterixInbetweenCommandsIn_SingleAsterixSplitByCommandOut()
        {
            var helper = new AppQuery(QueryPlatform.Android);
            var input = new[] { "all *", "*", "* : 'Test'" };

            var output = helper.RemoveConsecutiveDuplicatesOf(input);
            var joinedOutput = string.Join(" ", output);

            Assert.AreEqual("all * : 'Test'", joinedOutput);
        }

        [Test]
        public void MultipleAsterixSplitByCommandIn_SingleAsterixSplitByCommandOut()
        {
            var helper = new AppQuery(QueryPlatform.Android);
            var input = new[] { "all * ", "*", "*", " : * 'Test'" };

            var output = helper.RemoveConsecutiveDuplicatesOf(input);
            var joinedOutput = string.Join(" ", output);

            Assert.AreEqual("all * : * 'Test'", joinedOutput);
        }
    }
}