using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Xamarin.UITest.Helpers;

namespace Xamarin.UITest.Tests
{
    [TestFixture]
    public class EnvironmentVariableForwardingHelperTests
    {
        [Test]
        public void MergeAutEnvironmentVariables_DoesNotAddNonForwardValue()
        {
            var envHelper = Substitute.For<IEnvironmentVariableHelper>();
            var testDict = new Dictionary<string, string>();
            testDict.Add("TEST_VAR1", "TEST_VAL1");
            envHelper.GetEnvironmentVariables().Returns(testDict);

            var autDict = new Dictionary<string, string>();
            autDict.Add("TEST_VAR2", "TEST_VAL2");

            var envForwardHelper = new EnvironmentVariableForwardingHelper(envHelper);
            var output = envForwardHelper.MergeAutEnvironmentVariables(autDict);

            Assert.AreEqual(1, output.Count);
            Assert.IsFalse(output.ContainsKey("TEST_VAR1"));
            Assert.IsTrue(output.ContainsKey("TEST_VAR2"));
            Assert.AreEqual("TEST_VAL2", output["TEST_VAR2"]);
        }

        [Test]
        public void MergeAutEnvironmentVariables_HandlesNullEnvValue()
        {
            var envHelper = Substitute.For<IEnvironmentVariableHelper>();
            envHelper.GetEnvironmentVariables().Returns((Dictionary<string, string>)null);

            var autDict = new Dictionary<string, string>();
            autDict.Add("TEST_VAR1", "TEST_VAL1");

            var envForwardHelper = new EnvironmentVariableForwardingHelper(envHelper);
            var output = envForwardHelper.MergeAutEnvironmentVariables(autDict);

            Assert.AreEqual(1, output.Count);
            Assert.True(output.ContainsKey("TEST_VAR1"));
            Assert.AreEqual("TEST_VAL1", output["TEST_VAR1"]);
        }
    }
}
