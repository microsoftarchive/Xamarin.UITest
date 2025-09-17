using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xamarin.UITest.Shared.AssemblyAnalysis;

namespace Xamarin.UITest.Tests.NUnit2.AssemblyAnalysis
{
    [TestFixture]
    public class TypeNameConverterTests
    {
        [Test]
        public void ConvertFromClassnameToFullClassName()
        {
            var expected = new[]
            {
                "Full.Class.Name.Already",
                $"Xamarin.UITest.Tests.NUnit2.AssemblyAnalysis.{nameof(ToFindInOneNamespace)}"
            };
            var initial = new[] { "Full.Class.Name.Already" , nameof(ToFindInOneNamespace) };

            var fixtureFinder = new TypeNameConverter(Assembly.GetExecutingAssembly());
            var actual = fixtureFinder.ConvertTypeNamesToFullNames(initial);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ConvertAllOccurencesOfClassnameInAssembly()
        {
            var expected = new[]
            {
                "Full.Class.Name.Already",
                $"Xamarin.UITest.Tests.NUnit2.AssemblyAnalysis.{nameof(ToFindInTwoNamespaces)}",
                $"Xamarin.UITest.Tests.NUnit2.AssemblyAnalysis.Test.{nameof(ToFindInTwoNamespaces)}"
            };
            var initial = new[] { "Full.Class.Name.Already" , nameof(ToFindInTwoNamespaces) };

            var fixtureFinder = new TypeNameConverter(Assembly.GetExecutingAssembly());
            var actual = fixtureFinder.ConvertTypeNamesToFullNames(initial);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void NotConvertedIfClassDoesntExist()
        { 
            var expected = new[]
            {
                "Full.Class.Name.Already",
                $"DoesNotExist"
            };
            var initial = new[] { "Full.Class.Name.Already" , "DoesNotExist" };

            var fixtureFinder = new TypeNameConverter(Assembly.GetExecutingAssembly());
            var actual = fixtureFinder.ConvertTypeNamesToFullNames(initial);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ShouldNotMatchClassnameWithSameEndsWith()
        {
            var expected = $"Xamarin.UITest.Tests.NUnit2.AssemblyAnalysis.{nameof(DoNotMatchThisToFindInOneNamespace)}";

            var initial = new[] { nameof(ToFindInOneNamespace) };

            var fixtureFinder = new TypeNameConverter(Assembly.GetExecutingAssembly());
            var actual = fixtureFinder.ConvertTypeNamesToFullNames(initial);

            Assert.IsTrue(actual.Count() == 1);
            CollectionAssert.DoesNotContain(actual, expected);
        }

        [Test]
        public void NullReturnsNull()
        { 
            var fixtureFinder = new TypeNameConverter(Assembly.GetExecutingAssembly());
            var actual = fixtureFinder.ConvertTypeNamesToFullNames(null);
            Assert.IsNull(actual);
        }

        [Test]
        public void ReturnEmptyListIfPassedAnEmptyList()
        { 
            var fixtureFinder = new TypeNameConverter(Assembly.GetExecutingAssembly());
            var actual = fixtureFinder.ConvertTypeNamesToFullNames(new string[0]);
            CollectionAssert.IsEmpty(actual);
        }
    }

    public class ToFindInOneNamespace
    { 
    
    }

    public class ToFindInTwoNamespaces
    { 
    
    }

    public class DoNotMatchThisToFindInOneNamespace
    { 
    
    }
}

namespace Xamarin.UITest.Tests.NUnit2.AssemblyAnalysis.Test
{ 
    public class ToFindInTwoNamespaces
    {

    }        
}
