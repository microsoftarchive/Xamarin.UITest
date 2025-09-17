using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.AssemblyAnalysis;

namespace Xamarin.UITest.Tests.AssemblyAnalysis
{
    [TestFixture]
    public class AnalysisTests
    {
        [SetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
        }

        [TestFixture]
        internal class InternalChunkerTestClass
        {
            [Test]
            public void Test()
            {

            }
        }

        [TestFixture]
        private class PrivateChunkerTestClass
        {
            [Test]
            public void Test()
            {

            }
        }

        public abstract class AbstractChunkerTestClass
        {
            [Test]
            public void Test()
            {
                
            }

            [TestCase(1)]
            public void TestCase(int i)
            {

            }

            [Test, Sequential]
            public void Sequential()
            {

            }

            [Test, Combinatorial]
            public void Combinatorial()
            {

            }

            [Test, Pairwise]
            public void Pairwise()
            {

            }

            [Test, TestCaseSource("MyInt")]
            public void TestCaseSource(int i)
            {
            }

            [TestCaseSource("MyInt")]
            public void TestCaseSourceNoTestAttribute(int i)
            {
            }
        }

        public class ImplementingChunkerTestClass : AbstractChunkerTestClass
        {
            #pragma warning disable 0414 // Disable assigned but not used warning.  This class is analysed at runtime.
            static int[] MyInt = { 7 };
            #pragma warning restore 0414
        }

        public class ImplementingChunkerTestClass2 : ImplementingChunkerTestClass
        {
            #pragma warning disable 0414 // Disable assigned but not used warning.  This class is analysed at runtime.
            static int[] MyInt = { 9 };
            #pragma warning restore 0414
        }

        [Ignore("Ignored on purpose to test the Ignore functionality")]
        public class MyIgnoreSuperClass
        {
            
        }

        public class MyNotIgnoredClass : MyIgnoreSuperClass
        {
            [Test]
            public void NotIgnoredMethod()
            {
                
            }

            [Test]
            public void IgnoredMethod()
            {
                
            }
        }

        [Ignore("Because I want to")]
        public class MyIgnoredClass
        {
            [Test]
            public void NotIgnoredMethod()
            {
                
            }

            [Test, Ignore("Ignored on purpose to test the Ignore functionality")]
            public void IgnoredMethod()
            {
                
            }
        }

        public class FixtureWithOneIgnoredTest
        {
            [Test]
            [Category("A")]
            public void NotIgnoredMethodWithCategoryA()
            {

            }

            [Test, Ignore("Ignored on purpose to test the Ignore functionality")]
            public void IgnoredMethod()
            {

            }
        }

        public class FixtureWithCategoryOnMethods
        {
            [Test]
            [Category("A")]
            public void MethodWithCategoryA()
            {

            }

            [Test]
            [Category("A")]
            [Category("B")]
            public void MethodWithCategoryAB()
            {

            }
        }


        [Category("MySuperClassCategory")]
        public class MyTestSuperClass
        {
            [Test]
            [Category("MySuperMethodCategory")]
            public void SuperTest()
            {

            }
        }

        [Category("MyClassCategory")]
        public class MyCategoryTests : MyTestSuperClass
        {
            [Test, Ignore("Ignored on purpose to test the Ignore functionality")]
            [Category("MyMethodCategory")]
            public void Test()
            {

            }
        }
    }

    [TestFixture("Foo")]
    [TestFixture("Bar")]
    public class ParameterizedTestFixtureString
    {
        #pragma warning disable 0414 // Disable assigned but not used warning.  This class is analysed at runtime.
        string eq1;
        #pragma warning restore 0414

        public ParameterizedTestFixtureString(string eq1)
        {
            this.eq1 = eq1;
        }

        [Test]
        public void TestInParameterizedTestFixtureString()
        {
        }
    }

    public enum EnumForFixture
    {
        Android,
        Ios
    }

    [TestFixture(42, EnumForFixture.Android)]
    [TestFixture(7, EnumForFixture.Ios)]
    public class ParameterizedTestFixtureComplex
    {
        #pragma warning disable 0414 // Disable assigned but not used warning.  This class is analysed at runtime.
        int eq1;
        EnumForFixture eq2;
        #pragma warning restore 0414

        public ParameterizedTestFixtureComplex(int eq1, EnumForFixture eq2)
        {
            this.eq1 = eq1;
            this.eq2 = eq2;
        }

        [Test]
        public void TestInParameterizedTestFixtureComplex()
        {
        }
    }


    [TestFixture (Platform.Android)]
    [TestFixture (Platform.iOS)]
    public abstract class FeatureBase
    {
        protected IApp app;
        protected Platform platform;

        public FeatureBase (Platform platform)
        {
            this.platform = platform;
        }

    }

    public partial class WelcomePageFeature : FeatureBase
    {
        public WelcomePageFeature (Platform platform)
            : base (platform)
        {
        }
    }

    public partial class WelcomePageFeature
    {
        [Test]
        [Description("App is run")]
        public virtual void AppIsRun()
        {
        }
    }

    [TestFixture(typeof(ArrayList))]
    [TestFixture(typeof(List<int>))]
    public class IList_Tests<TList> where TList : IList, new()
    {
        private IList list;

        [SetUp]
        public void CreateList()
        {
            list = new TList();
        }

        [Test]
        public void CanAddToList()
        {
            list.Add(1); list.Add(2); list.Add(3);
            Assert.AreEqual(3, list.Count);
        }
    }

    [TestFixture(100.0, 42)]
    [TestFixture(typeof(double), typeof(int), 300.0, 142)]
    [TestFixture(100.0, 42, TypeArgs = new[] { typeof(double), typeof(int) })]
    public class TestFixtureGenericArgs<T1, T2>
    {
        #pragma warning disable 0414 // Disable assigned but not used warning.  This class is analysed at runtime.
        T1 t1;
        T2 t2;
        #pragma warning restore 0414
        
        public TestFixtureGenericArgs(T1 t1, T2 t2)
        {
            this.t1 = t1;
            this.t2 = t2;
        }

        [TestCase(5, 7)]
        public void TestMyArgTypes(T1 t1, T2 t2)
        {
            Assert.That(t1, Is.TypeOf<T1>());
            Assert.That(t2, Is.TypeOf<T2>());
        }
    }

    public class NUnitAnnotationsTestClass
    {
        [Test]
        public void TestBasic()
        {
            
        }

        [Test, Ignore("Ignored on purpose to test the Ignore functionality")]
        public void TestIgnore()
        {

        }

        [Test]
        [Category("A")]
        public void TestCategory()
        {

        }

        [Test]
        [Category("TestIgnoredTestCase")]
        public void TestJustBasic()
        {

        }

        [TestCase(1), Ignore("Ignored on purpose to test the Ignore functionality")]
        [Category("TestCaseIgnoredTestCase")]
        public void TestCaseIgnore(int i)
        {

        }

        [TestCase(1)]
        public void TestCase(int i)
        {

        }

        [TestCase("one")]
        [TestCase("two")]
        public void TestCaseString(string s)
        {

        }

        [TestCase("one","oneA")]
        [TestCase("two", "twoA")]
        [TestCase("three", "threeA", TestName="RenamedTestCase")]
        public void TestCaseStrings(string s, string t)
        {

        }

        [TestCase(1, 2.02f, 3.03d, 0x12,'f',new int[]{0,1,2})]
        public void TestCaseSix(int a, float b, double d, byte e, char f, int[] g) 
        {

        }

        [TestCase(1, 2.02f, 3.03d, 0x12,'f',new int[]{0,1,2}, TestName="TestCaseSevenRenamed")]
        public void TestCaseSeven(int a, float b, double d, byte e, char f, int[] g) 
        {

        }

        [TestCase]
        public void TestCaseEmpty()
        {

        }

        [Test, Sequential]
        public void Sequential()
        {

        }

        [Test, Combinatorial]
        public void Combinatorial()
        {

        }

        [Test, Pairwise]
        public void Pairwise()
        {

        }

        #pragma warning disable 0414 // Disable assigned but not used warning.  This class is analysed at runtime.
        static int[] MyInt = { 7 };
        #pragma warning restore 0414
        
        [Test, TestCaseSource("MyInt")]
        public void TestCaseSource(int i)
        {
        }

        [TestCaseSource("MyInt")]
        public void TestCaseSourceNoTestAttribute(int i)
        {
        }

        public static string[] TestValueSourceRunNames = { "Value Source Run A.", "Value Source Run B." };

        [Test]
        public void TestValueSource([ValueSource(nameof(TestValueSourceRunNames))] string runName)
        {
        }

        [Test]
        public void TestValues([Values(1,2,3)] int x)
        {
        }

        [Test,Combinatorial]
        public void TestCombinatorialWithValues([Values(1,2,3)] int x,[Values("A","B")] string s)
        {
        }

        [Test,Sequential]
        public void TestSequentialWithValues([Values(1,2,3)] int x,[Values("A","B")] string s)
        {
        }

        [Test,Pairwise]
        public void TestPairwiseWithValues([Values(1,2,3)] int x,[Values("A","B")] string s)
        {
        }

        [Datapoints]
        public double[] num = { 0.0, 1.0, -1.0, 42.0 };

        [Theory]
        public void TestTheoryWithDataPoints(double num)
        {
        }

        //There appears to be a bug in NUnit3's test discovery which means that
        // When the Range is set to 0.6 - only 0.2 and 0.4 are created as tests.
        // This is echoed in the VS 4 Mac test runner so assuming this is a bonefide bug
        [Test]
        public void TestRange([Range(0.2,0.7,0.2)] double d)
        {
        }
    }

}