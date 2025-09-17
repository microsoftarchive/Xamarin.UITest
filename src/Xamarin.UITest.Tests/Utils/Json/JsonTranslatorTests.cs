using System;
using System.Linq;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Queries.Tokens;
using Xamarin.UITest.Shared.Json;

namespace Xamarin.UITest.Tests.Utils.Json
{
    [TestFixture]
    public class JsonTranslatorTests
    {
        readonly JsonTranslator _translator = new JsonTranslator();

        [Test]
        public void CanDeserializePascalCase()
        {
            var obj = _translator.Deserialize<JsonTestClass1>("{ companyName: 'xamarin' }");

            obj.CompanyName.ShouldEqual("xamarin");
        }        
        
        [Test]
        public void CanDeserializeCamelCaseToPascalCase()
        {
            var obj = _translator.Deserialize<JsonTestClass1>("{ companyName: 'xamarin' }");

            obj.CompanyName.ShouldEqual("xamarin");
        }

        [Test]
        public void CanDeserializeSnakeCaseToPascalCase()
        {
            var obj = _translator.Deserialize<JsonTestClass1>("{ company_name: 'xamarin' }");

            obj.CompanyName.ShouldEqual("xamarin");
        }

		[Test]
		public void CanDeserializeSnakeCaseToPascalCaseForNestedObjects()
		{
			var obj = _translator.Deserialize<JsonTestClass1>("{ company_name: 'xamarin', child: { test_array: [ 'test' ] } }");

			obj.Child.TestArray.Count().ShouldEqual(1);
		}

        [Test]
        public void CanDeserializeArrays()
        {
            var obj = _translator.Deserialize<JsonTestClass2>("{ test_array: [ 'xamarin', 'microsoft' ] }");

            obj.TestArray.Count().ShouldEqual(2);
        }

        [Test]
        public void CanDeserializeArrayOfboolean()
        {
            var obj = _translator.DeserializeArray<bool>("[ true, false ]");

            obj.ShouldEqual<bool[]>(new bool[] { true, false });
        }

        [Test]
        public void CannotDeserializeArrayOf01Asboolean()
        {
            Assert.Throws<InvalidOperationException>(delegate
            {
                _translator.DeserializeArray<bool>("[ 1, 0 ]");
            });
        }

        [Test]
        public void CanDeserializeArrayOfIntsToInt()
        {
            var obj = _translator.DeserializeArray<int>("[ 45, 42 ]");

            obj.ShouldEqual(new int[] {45,42});
        }

        [Test]
        public void CanDeserializeArrayOfIntsToDouble()
        {
            var obj = _translator.DeserializeArray<double>("[ 45, 42 ]");

            obj.ShouldEqual(new double[] { 45, 42 });
        }

        [Test]
        public void CanDeserializeArrayOfStringsToStrings()
        {
            var obj = _translator.DeserializeArray<string>("[ 'foo', 'bar' ]");

            obj.ShouldEqual(new string[] {"foo", "bar" });
        }

        [Test]
        public void CanDeserializeMixedObjectArrays()
        {
            var obj = _translator.DeserializeArray<object>("[ 'foo', 'bar', 42 ]");

            obj.Length.ShouldEqual(3);

            obj[0].ShouldEqual("foo");
            obj[1].ShouldEqual("bar");
            obj[2].ShouldEqual(42L);
        }

        protected class JsonTestClass1
        {
            public string CompanyName { get; set; }
			public JsonTestClass2 Child { get; set; }
        }

        protected class JsonTestClass2
        {
            public string[] TestArray { get; set; } 
        }
    }
}