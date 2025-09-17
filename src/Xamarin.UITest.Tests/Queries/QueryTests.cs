using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.Tokens;

namespace Xamarin.UITest.Tests.Queries
{
    [TestFixture]
    public class QueryTests
    {
        [Test]
        public void DefaultQueryIsEverything()
        {
            TestAndroidQuery(
                x => x,
                "*",
                "*");
        }

        [Test]
        public void QueryButton()
        {
            TestQuery(
                x => x.Button(),
                "Button()",
                android: "android.widget.Button",
                ios: "button"
            );
        }

        [Test]
        public void QueryAndroidButtonChained()
        {
            TestQuery(
                x => x.Id("test").Button(),
                "Id(\"test\").Button()",
                android: "* id:'test' android.widget.Button",
                ios: "* id:'test' button"
            );
        }

        [Test]
        public void QueryTextField()
        {
            TestQuery(
                x => x.TextField(),
                "TextField()",
                android: "android.widget.EditText",
                ios: "view:'UITextField'"
            );
        }

        [Test]
        public void QueryTextFieldChained()
        {
            TestQuery(
                x => x.Id("test").TextField(),
                "Id(\"test\").TextField()",
                android: "* id:'test' android.widget.EditText",
                ios: "* id:'test' view:'UITextField'"
            );
        }

        [Test]
        public void QueryTextFieldMarked()
        {
            TestQuery(
                x => x.TextField("foo"),
                "TextField(\"foo\")",
                android: "android.widget.EditText marked:'foo'",
                ios: "view:'UITextField' marked:'foo'"
            );
        }

        [Test]
        public void QueryTextFieldMarkedChained()
        {
            TestQuery(
                x => x.Id("test").TextField("foo"),
                "Id(\"test\").TextField(\"foo\")",
                android: "* id:'test' android.widget.EditText marked:'foo'",
                ios: "* id:'test' view:'UITextField' marked:'foo'"
            );
        }

        [Test]
        public void QuerySwitch()
        {
            TestQuery(
                x => x.Switch(),
                "Switch()",
                android: "android.widget.CompoundButton",
                ios: "view:'UISwitch'"
            );
        }

        [Test]
        public void QuerySwitchChained()
        {
            TestQuery(
                x => x.Id("test").Switch(),
                "Id(\"test\").Switch()",
                android: "* id:'test' android.widget.CompoundButton",
                ios: "* id:'test' view:'UISwitch'"
            );
        }

        [Test]
        public void QuerySwitchMarked()
        {
            TestQuery(
                x => x.Switch("bar"),
                "Switch(\"bar\")",
                android: "android.widget.CompoundButton marked:'bar'",
                ios: "view:'UISwitch' marked:'bar'"
            );
        }

        [Test]
        public void QuerySwitchMarkedChained()
        {
            TestQuery(
                x => x.Id("test").Switch("bar"),
                "Id(\"test\").Switch(\"bar\")",
                android: "* id:'test' android.widget.CompoundButton marked:'bar'",
                ios: "* id:'test' view:'UISwitch' marked:'bar'"
            );
        }

        [Test]
        public void QueryMarked()
        {
            TestQuery(
                x => x.Marked("test"),
                "* marked:'test'",
                "Marked(\"test\")");
        }

        [Test]
        public void QueryMarkedWithSpaces()
        {
            TestQuery(
                x => x.Marked("test  "),
                "* marked:'test  '",
                "Marked(\"test  \")");
        }

        [Test]
        public void QueryMarkedChained()
        {
            TestQuery(
                x => x.Id("bar").Marked("test"),
                "* id:'bar' marked:'test'",
                "Id(\"bar\").Marked(\"test\")");
        }

        [Test]
        public void QueryId()
        {
            TestQuery(
                x => x.Id("test"),
                "* id:'test'",
                "Id(\"test\")");
        }

        [Test]
        public void QueryIdInteger()
        {
            TestQuery(
                x => x.Id(1234),
                "* id:'1234'",
                "Id(1234)");
        }

        [Test]
        public void QueryIdChained()
        {
            TestQuery(
                x => x.Marked("bar").Id("test"),
                "* marked:'bar' id:'test'",
                "Marked(\"bar\").Id(\"test\")");
        }

        [Test]
        public void QueryButtonMarked()
        {
            TestQuery(x =>
                x.Button("test"),
                "Button(\"test\")",
                android: "android.widget.Button marked:'test'",
                ios: "button marked:'test'"
            );
        }

        [Test]
        public void QueryButtonMarkedChained()
        {
            TestQuery(x =>
                x.Id("test").Button("test"),
                "Id(\"test\").Button(\"test\")",
                android: "* id:'test' android.widget.Button marked:'test'",
                ios: "* id:'test' button marked:'test'"
            );
        }

        [Test]
        public void QuerySibling()
        {
            TestQuery(
                x => x.Sibling(),
                "sibling *",
                "Sibling()");
        }

        [Test]
        public void QuerySiblingChained()
        {
            TestQuery(
                x => x.Id("test").Sibling(),
                "* id:'test' sibling *",
                "Id(\"test\").Sibling()");
        }

        [Test]
        public void QuerySiblingClass()
        {
            TestQuery(
                x => x.Sibling("test"),
                "sibling test",
                "Sibling(\"test\")");
        }

        [Test]
        public void QuerySiblingClassChained()
        {
            TestQuery(
                x => x.Id("bar").Sibling("test"),
                "* id:'bar' sibling test",
                "Id(\"bar\").Sibling(\"test\")");
        }

        [Test]
        public void QuerySiblingIndex()
        {
            TestQuery(
                x => x.Sibling(42),
                "sibling * index:42",
                "Sibling(42)");
        }

        [Test]
        public void QuerySiblingIndexChained()
        {
            TestQuery(
                x => x.Id("test").Sibling(42),
                "* id:'test' sibling * index:42",
                "Id(\"test\").Sibling(42)");
        }

        [Test]
        public void QueryParent()
        {
            TestQuery(
                x => x.Parent(),
                "parent *",
                "Parent()");
        }

        [Test]
        public void QueryParentChained()
        {
            TestQuery(
                x => x.Id("test").Parent(),
                "* id:'test' parent *",
                "Id(\"test\").Parent()");
        }

        [Test]
        public void QueryParentClass()
        {
            TestQuery(
                x => x.Parent("test"),
                "parent test",
                "Parent(\"test\")");
        }

        [Test]
        public void QueryParentClassChained()
        {
            TestQuery(
                x => x.Id("bar").Parent("test"),
                "* id:'bar' parent test",
                "Id(\"bar\").Parent(\"test\")");
        }

        [Test]
        public void QueryParentIndex()
        {
            TestQuery(
                x => x.Parent(7),
                "parent * index:7",
                "Parent(7)");
        }

        [Test]
        public void QueryParentIndexChained()
        {
            TestQuery(
                x => x.Id("test").Parent(7),
                "* id:'test' parent * index:7",
                "Id(\"test\").Parent(7)");
        }

        [Test]
        public void QueryDescendant()
        {
            TestQuery(
                x => x.Descendant(),
                "descendant *",
                "Descendant()");
        }

        [Test]
        public void QueryDescendantChained()
        {
            TestQuery(
                x => x.Id("test").Descendant(),
                "* id:'test' descendant *",
                "Id(\"test\").Descendant()");
        }

        [Test]
        public void QueryDescendantClass()
        {
            TestQuery(
                x => x.Descendant("test"),
                "descendant test",
                "Descendant(\"test\")");
        }

        [Test]
        public void QueryDescendantClassChained()
        {
            TestQuery(
                x => x.Id("bar").Descendant("test"),
                "* id:'bar' descendant test",
                "Id(\"bar\").Descendant(\"test\")");
        }

        [Test]
        public void QueryDescendantIndex()
        {
            TestQuery(
                x => x.Descendant(45),
                "descendant * index:45",
                "Descendant(45)");
        }

        [Test]
        public void QueryDescendantIndexChained()
        {
            TestQuery(
                x => x.Id("test").Descendant(45),
                "* id:'test' descendant * index:45",
                "Id(\"test\").Descendant(45)");
        }

        [Test]
        public void QueryChild()
        {
            TestQuery(
                x => x.Child(),
                "child *",
                "Child()");
        }

        [Test]
        public void QueryChildChained()
        {
            TestQuery(
                x => x.Id("test").Child(),
                "* id:'test' child *",
                "Id(\"test\").Child()");
        }

        [Test]
        public void QueryChildClass()
        {
            TestQuery(
                x => x.Child("test"),
                "child test",
                "Child(\"test\")");
        }

        [Test]
        public void QueryChildClassChained()
        {
            TestQuery(
                x => x.Id("bar").Child("test"),
                "* id:'bar' child test",
                "Id(\"bar\").Child(\"test\")");
        }

        [Test]
        public void QueryChildIndex()
        {
            TestQuery(
                x => x.Child(9),
                "child * index:9",
                "Child(9)");
        }

        [Test]
        public void QueryChildIndexChained()
        {
            TestQuery(
                x => x.Id("test").Child(9),
                "* id:'test' child * index:9",
                "Id(\"test\").Child(9)");
        }

        [Test]
        public void QueryClassAndText()
        {
            TestQuery(
                x => x.Class("*").Text("Test!"),
                "* text:'Test!'",
                "Class(\"*\").Text(\"Test!\")");
        }

        [Test]
        public void QueryClassAndTextWithSpaces()
        {
            TestQuery(
                x => x.Class("*").Text("Test!  "),
                "* text:'Test!  '",
                "Class(\"*\").Text(\"Test!  \")");
        }

        [Test]
        public void QueryClassAndTextChained()
        {
            TestQuery(
                x => x.Id("test").Class("*").Text("Test!"),
                "* id:'test' * text:'Test!'",
                "Id(\"test\").Class(\"*\").Text(\"Test!\")");
        }

        [Test]
        public void QueryFullClassAndroid()
        {
            TestQuery(
                x => x.ClassFull("<classname>"),
                "ClassFull(\"<classname>\")",
                android: "<classname>",
                ios: "view:'<classname>'"
            );
        }

        [Test]
        public void QueryFullClassChained()
        {
            TestQuery(
                x => x.Id("test").ClassFull("<classname>"),
                "Id(\"test\").ClassFull(\"<classname>\")",
                android: "* id:'test' <classname>",
                ios: "* id:'test' view:'<classname>'"
            );
        }

        [Test]
        public void QueryTextFirst()
        {
            TestQuery(
                x => x.Text("Test!"),
                "* text:'Test!'",
                "Text(\"Test!\")");
        }

        [Test]
        public void QueryTextFirstChained()
        {
            TestQuery(
                x => x.Id("test").Text("Test!"),
                "* id:'test' text:'Test!'",
                "Id(\"test\").Text(\"Test!\")");
        }

        [Test]
        public void QueryComplex1()
        {
            TestQuery(
                x => x.Button("login").Sibling().Marked("switch"),
                "Button(\"login\").Sibling().Marked(\"switch\")",
                android: "android.widget.Button marked:'login' sibling * marked:'switch'",
                ios: "button marked:'login' sibling * marked:'switch'"
            );
        }

        [Test]
        public void QueryPropertyEquals()
        {
            TestQuery(
                x => x.Property("text", "test"),
                "* text:'test'",
                "Property(\"text\", \"test\")");
        }

        [Test]
        public void QueryPropertyEqualsChained()
        {
            TestQuery(
                x => x.Id("test").Property("text", "test"),
                "* id:'test' text:'test'",
                "Id(\"test\").Property(\"text\", \"test\")");
        }

        [Test]
        public void QueryCss()
        {
            TestQuery(
                x => x.Css("cssSelector"),
                "Css(\"cssSelector\")",
                android: "android.webkit.WebView css:'cssSelector'",
                ios: "UIWebView css:'cssSelector'"
            );
        }

        [Test]
        public void QueryCssChained()
        {
            TestQuery(
                x => x.Id("test").Css("cssSelector"),
                "* id:'test' css:'cssSelector'",
                "Id(\"test\").Css(\"cssSelector\")"
            );
        }

        [Test]
        public void QueryCssChainedWithAll()
        {
            TestQuery(
                x => x.Id("test").All().Css("cssSelector"),
                "Id(\"test\").All().Css(\"cssSelector\")",
                android: "* id:'test' all android.webkit.WebView css:'cssSelector'",
                ios: "* id:'test' all UIWebView css:'cssSelector'"
            );
        }

        [Test]
        public void QueryAllCss()
        {
            TestQuery(
                x => x.All().Css("cssSelector"),
                "All().Css(\"cssSelector\")",
                android: "all android.webkit.WebView css:'cssSelector'",
                ios: "all UIWebView css:'cssSelector'"
            );
        }

        [Test]
        public void QueryCssWithIndex()
        {
            TestQuery(
                x => x.Css("cssSelector").Index(1),
                "Css(\"cssSelector\").Index(1)",
                android: "android.webkit.WebView css:'cssSelector' index:1",
                ios: "UIWebView css:'cssSelector' index:1"
            );
        }

        [Test]
        public void QueryCssWithIndexChained()
        {
            TestQuery(
                x => x.Id("test").Css("cssSelector").Index(1),
                "* id:'test' css:'cssSelector' index:1",
                "Id(\"test\").Css(\"cssSelector\").Index(1)"
            );
        }

        [Test]
        public void QueryXpath()
        {
            TestQuery(
                x => x.XPath("//foo"),
                "XPath(\"//foo\")",
                android: "android.webkit.WebView xpath:'//foo'",
                ios: "UIWebView xpath:'//foo'"
            );
        }

        [Test]
        public void QueryXpathChained()
        {
            TestQuery(
                x => x.Id("test").XPath("//foo"),
                "* id:'test' xpath:'//foo'",
                "Id(\"test\").XPath(\"//foo\")"
            );
        }

        [Test]
        public void QueryXpathChainedWithAll()
        {
            TestQuery(
                x => x.Id("test").All().XPath("//foo"),
                "Id(\"test\").All().XPath(\"//foo\")",
                android: "* id:'test' all android.webkit.WebView xpath:'//foo'",
                ios: "* id:'test' all UIWebView xpath:'//foo'"
            );
        }

        [Test]
        public void QueryXpathAll()
        {
            TestQuery(
                x => x.All().XPath("//foo"),
                "All().XPath(\"//foo\")",
                android: "all android.webkit.WebView xpath:'//foo'",
                ios: "all UIWebView xpath:'//foo'"
            );
        }

        [Test]
        public void QueryXpathWithIndex()
        {
            TestQuery(
                x => x.XPath("//foo").Index(1),
                "XPath(\"//foo\").Index(1)",
                android: "android.webkit.WebView xpath:'//foo' index:1",
                ios: "UIWebView xpath:'//foo' index:1"
            );
        }

        [Test]
        public void QueryXpathWithIndexChained()
        {
            TestQuery(
                x => x.Id("test").XPath("//foo").Index(1),
                "* id:'test' xpath:'//foo' index:1",
                "Id(\"test\").XPath(\"//foo\").Index(1)"
            );
        }

        [Test]
        public void QueryFrameWebViewCss()
        {
            TestQuery(
                x => x.WebView().Frame("myiframe").Css("cssSelector"),
                "WebView().Css(\"myiframe\").Css(\"cssSelector\")",
                android: "android.webkit.WebView css:'myiframe' css:'cssSelector'",
                ios: "view:'UIWebView' css:'myiframe' css:'cssSelector'");
        }

        [Test]
        public void QueryFrameCss()
        {
            TestQuery(
                x => x.Frame("myiframe").Css("cssSelector"),
                "Css(\"myiframe\").Css(\"cssSelector\")",
                android: "android.webkit.WebView css:'myiframe' css:'cssSelector'",
                ios: "UIWebView css:'myiframe' css:'cssSelector'"
            );
        }

        [Test]
        public void QueryFrameCssChained()
        {
            TestQuery(
                x => x.Id("test").Frame("myiframe").Css("cssSelector"),
                "Id(\"test\").Css(\"myiframe\").Css(\"cssSelector\")",
                android: "* id:'test' css:'myiframe' css:'cssSelector'",
                ios: "* id:'test' css:'myiframe' css:'cssSelector'"
            );
        }

        [Test]
        public void QueryFrameCssChainedWithAll()
        {
            TestQuery(
                x => x.Id("test").All().Frame("myiframe").Css("cssSelector"),
                "Id(\"test\").All().Css(\"myiframe\").Css(\"cssSelector\")",
                android: "* id:'test' all android.webkit.WebView css:'myiframe' css:'cssSelector'",
                ios: "* id:'test' all UIWebView css:'myiframe' css:'cssSelector'"
            );
        }

        [Test]
        public void QueryFrameAllCss()
        {
            TestQuery(
                x => x.All().Frame("myiframe").Css("cssSelector"),
                "All().Css(\"myiframe\").Css(\"cssSelector\")",
                android: "all android.webkit.WebView css:'myiframe' css:'cssSelector'",
                ios: "all UIWebView css:'myiframe' css:'cssSelector'"
            );
        }

        [Test]
        public void QueryFrameCssWithIndex()
        {
            TestQuery(
                x => x.Frame("myiframe").Css("cssSelector").Index(1),
                "Css(\"myiframe\").Css(\"cssSelector\").Index(1)",
                android: "android.webkit.WebView css:'myiframe' css:'cssSelector' index:1",
                ios: "UIWebView css:'myiframe' css:'cssSelector' index:1"
            );
        }

        [Test]
        public void QueryFrameCssWithIndexChained()
        {
            TestQuery(
                x => x.Id("test").Frame("myiframe").Css("cssSelector").Index(1),
                "Id(\"test\").Css(\"myiframe\").Css(\"cssSelector\").Index(1)",
                android: "* id:'test' css:'myiframe' css:'cssSelector' index:1",
                ios: "* id:'test' css:'myiframe' css:'cssSelector' index:1"
            );
        }

        [Test]
        public void QueryFrameXpath()
        {
            TestQuery(
                x => x.Frame("myiframe").XPath("//foo"),
                "Css(\"myiframe\").XPath(\"//foo\")",
                android: "android.webkit.WebView css:'myiframe' xpath:'//foo'",
                ios: "UIWebView css:'myiframe' xpath:'//foo'"
            );
        }

        [Test]
        public void QueryFrameXpathChained()
        {
            TestQuery(
                x => x.Id("test").Frame("myiframe").XPath("//foo"),
                "* id:'test' css:'myiframe' xpath:'//foo'",
                "Id(\"test\").Css(\"myiframe\").XPath(\"//foo\")"
            );
        }

        [Test]
        public void QueryFrameXpathChainedWithAll()
        {
            TestQuery(
                x => x.Id("test").All().Frame("myiframe").XPath("//foo"),
                "Id(\"test\").All().Css(\"myiframe\").XPath(\"//foo\")",
                android: "* id:'test' all android.webkit.WebView css:'myiframe' xpath:'//foo'",
                ios: "* id:'test' all UIWebView css:'myiframe' xpath:'//foo'"
            );
        }

        [Test]
        public void QueryFrameXpathAll()
        {
            TestQuery(
                x => x.All().Frame("myiframe").XPath("//foo"),
                "All().Css(\"myiframe\").XPath(\"//foo\")",
                android: "all android.webkit.WebView css:'myiframe' xpath:'//foo'",
                ios: "all UIWebView css:'myiframe' xpath:'//foo'"
            );
        }

        [Test]
        public void QueryFrameXpathWithIndex()
        {
            TestQuery(
                x => x.Frame("myiframe").XPath("//foo").Index(1),
                "Css(\"myiframe\").XPath(\"//foo\").Index(1)",
                android: "android.webkit.WebView css:'myiframe' xpath:'//foo' index:1",
                ios: "UIWebView css:'myiframe' xpath:'//foo' index:1"
            );
        }

        [Test]
        public void QueryFrameXpathWithIndexChained()
        {
            TestQuery(
                x => x.Id("test").Frame("myiframe").XPath("//foo").Index(1),
                "* id:'test' css:'myiframe' xpath:'//foo' index:1",
                "Id(\"test\").Css(\"myiframe\").XPath(\"//foo\").Index(1)"
            );
        }

        [Test]
        public void QueryPropertyInt()
        {
            TestQuery(
                x => x.Property("size", 42),
                "* size:42",
                "Property(\"size\", 42)");
        }

        [Test]
        public void QueryPropertyIntChained()
        {
            TestQuery(
                x => x.Id("test").Property("size", 42),
                "* id:'test' size:42",
                "Id(\"test\").Property(\"size\", 42)");
        }

        [Test]
        public void QueryPropertyValue()
        {
            TestQuery(
                x => x.Property("Id").Value<String>(),
                "*",
                new object[] { "Id" },
                "Property(\"Id\").Value<String>()");
        }

        [Test]
        public void QueryPropertyValueChained()
        {
            TestQuery(
                x => x.Id("test").Property("Id").Value<String>(),
                "* id:'test'",
                new object[] { "Id" },
                "Id(\"test\").Property(\"Id\").Value<String>()");
        }

        [Test]
        public void QueryMarkedStringContainingQuote()
        {
            TestQuery(
                c => c.Marked("Men's t-shirt"),
                @"* marked:'Men\'s t-shirt'",
                "Marked(\"Men's t-shirt\")");
        }

        [Test]
        public void QueryButtonStringContainingQuote()
        {
            TestQuery(
                c => c.Button("Men's t-shirt"),
                "Button(\"Men's t-shirt\")",
                android: @"android.widget.Button marked:'Men\'s t-shirt'",
                ios: @"button marked:'Men\'s t-shirt'"
            );
        }

        [Test]
        public void QueryTextStringContainingQuote()
        {
            TestQuery(
                c => c.Text("Men's t-shirt"),
                @"* text:'Men\'s t-shirt'",
                "Text(\"Men's t-shirt\")");
        }

        [Test]
        public void QueryTextStringContainingSpaces()
        {
            TestQuery(
                c => c.Text("Men's t-shirt  "),
                @"* text:'Men\'s t-shirt  '",
                "Text(\"Men's t-shirt  \")");
        }

        [Test]
        public void QueryPropertyStringContainingQuote()
        {
            TestQuery(
                c => c.Property("text", "Men's t-shirt"),
                @"* text:'Men\'s t-shirt'",
                "Property(\"text\", \"Men's t-shirt\")");
        }


        [Test]
        public void QueryPropertyFilterStringContainingQuote()
        {
            TestQuery(
                c => c.Property("name").Contains("Men's t-shirt"),
                @"* {name CONTAINS 'Men\'s t-shirt'}",
                "Property(\"name\").Contains(\"Men's t-shirt\")");
        }

        [Test]
        public void QueryPropertyFilterStringContainingQuoteChained()
        {
            TestQuery(
                c => c.Id("test").Property("name").Contains("Men's t-shirt"),
                @"* id:'test' {name CONTAINS 'Men\'s t-shirt'}",
                "Id(\"test\").Property(\"name\").Contains(\"Men's t-shirt\")");
        }

        [Test]
        public void QueryPropertyBool()
        {
            TestQuery(
                x => x.Property("huge", true),
                "Property(\"huge\", true)",
                android: "* huge:true",
                ios: "* huge:1"
            );
        }

        [Test]
        public void QueryPropertyBoolChained()
        {
            TestQuery(
                x => x.Id("test").Property("huge", true),
                "Id(\"test\").Property(\"huge\", true)",
                android: "* id:'test' huge:true",
                ios: "* id:'test' huge:1"
            );
        }

        [Test]
        public void QueryPropertyStartsWith()
        {
            TestQuery(
                x => x.Property("text").StartsWith("x"),
                "* {text BEGINSWITH 'x'}",
                "Property(\"text\").StartsWith(\"x\")");
        }

        [Test]
        public void QueryPropertyStartsWithChained()
        {
            TestQuery(
                x => x.Id("test").Property("text").StartsWith("x"),
                "* id:'test' {text BEGINSWITH 'x'}",
                "Id(\"test\").Property(\"text\").StartsWith(\"x\")");
        }

        [Test]
        public void QueryRawWithArgs()
        {
            TestQuery(
                x => x.Raw("*", "test"),
                "*",
                new object[] { "test" },
                "Raw(\"*\", \"test\")");
        }

        [Test]
        public void QueryRawWithArgsChained()
        {
            TestQuery(
                x => x.Id("test").Raw("*", "test"),
                "* id:'test' *",
                new object[] { "test" },
                "Id(\"test\").Raw(\"*\", \"test\")");
        }

        [Test]
        public void QueryRawWithAdvancedArgs()
        {
            TestQuery(
                x => x.Raw("*", new { test = 42, hopsa = 53 }),
                "*",
                new object[] { new { test = 42, hopsa = 53 } },
                "Raw(\"*\", { test: 42, hopsa: 53 })");
        }

        [Test]
        public void QueryRawWithAdvancedArgsChained()
        {
            TestQuery(
                x => x.Id("test").Raw("*", new { test = 42, hopsa = 53 }),
                "* id:'test' *",
                new object[] { new { test = 42, hopsa = 53 } },
                "Id(\"test\").Raw(\"*\", { test: 42, hopsa: 53 })");
        }

        [Test]
        public void QueryRawWith1Args()
        {
            TestQuery(
                x => x.Raw("*", 1),
                "*",
                new object[] { 1 },
                "Raw(\"*\", 1)");
        }

        [Test]
        public void QueryRawWith1ArgsChained()
        {
            TestQuery(
                x => x.Id("test").Raw("*", 1),
                "* id:'test' *",
                new object[] { 1 },
                "Id(\"test\").Raw(\"*\", 1)");
        }

        [Test]
        public void QueryRawWith2Args()
        {
            TestQuery(
                x => x.Raw("*", 1, 2),
                "*",
                new object[] { 1, 2 },
                "Raw(\"*\", 1, 2)");
        }

        [Test]
        public void QueryRawWith2ArgsChained()
        {
            TestQuery(
                x => x.Id("test").Raw("*", 1, 2),
                "* id:'test' *",
                new object[] { 1, 2 },
                "Id(\"test\").Raw(\"*\", 1, 2)");
        }

        [Test]
        public void QueryRawWith3Args()
        {
            TestQuery(
                x => x.Raw("*", 1, 2, 3),
                "*",
                new object[] { 1, 2, 3 },
                "Raw(\"*\", 1, 2, 3)");
        }

        [Test]
        public void QueryRawWith3ArgsChained()
        {
            TestQuery(
                x => x.Id("test").Raw("*", 1, 2, 3),
                "* id:'test' *",
                new object[] { 1, 2, 3 },
                "Id(\"test\").Raw(\"*\", 1, 2, 3)");
        }

        [Test]
        public void QueryRawWith4Args()
        {
            TestQuery(
                x => x.Raw("*", 1, 2, 3, 4),
                "*",
                new object[] { 1, 2, 3, 4 },
                "Raw(\"*\", 1, 2, 3, 4)");
        }

        [Test]
        public void QueryRawWith4ArgsChained()
        {
            TestQuery(
                x => x.Id("test").Raw("*", 1, 2, 3, 4),
                "* id:'test' *",
                new object[] { 1, 2, 3, 4 },
                "Id(\"test\").Raw(\"*\", 1, 2, 3, 4)");
        }

        [Test]
        public void QueryRawWith5Args()
        {
            TestQuery(
                x => x.Raw("*", 1, 2, 3, 4, 5),
                "*",
                new object[] { 1, 2, 3, 4, 5 },
                "Raw(\"*\", 1, 2, 3, 4, 5)");
        }

        [Test]
        public void QueryRawWith5ArgsChained()
        {
            TestQuery(
                x => x.Id("test").Raw("*", 1, 2, 3, 4, 5),
                "* id:'test' *",
                new object[] { 1, 2, 3, 4, 5 },
                "Id(\"test\").Raw(\"*\", 1, 2, 3, 4, 5)");
        }

        [Test]
        public void QueryRawWith6Args()
        {
            TestQuery(
                x => x.Raw("*", 1, 2, 3, 4, 5, 6),
                "*",
                new object[] { 1, 2, 3, 4, 5, 6 },
                "Raw(\"*\", 1, 2, 3, 4, 5, 6)");
        }

        [Test]
        public void QueryRawWith6ArgsChained()
        {
            TestQuery(
                x => x.Id("test").Raw("*", 1, 2, 3, 4, 5, 6),
                "* id:'test' *",
                new object[] { 1, 2, 3, 4, 5, 6 },
                "Id(\"test\").Raw(\"*\", 1, 2, 3, 4, 5, 6)");
        }

        [Test]
        public void QueryRawAndroidMethodCall()
        {
            TestAndroidQuery(
                x => x.Raw("*", new { test = 42 }),
                "*",
                new object[] { new { method_name = "test", arguments = new object[] { 42 } } },
                "Raw(\"*\", { test: 42 })");
        }

        [Test]
        public void QueryRawAndroidMethodCallUnwrapArray()
        {
            TestAndroidQuery(
                x => x.Raw("*", new { test = new[] { 1, 2, 3 } }),
                "*",
                new object[] { new { method_name = "test", arguments = new object[] { 1, 2, 3 } } },
                "Raw(\"*\", { test: [ 1, 2, 3 ] })");
        }

        [Test]
        public void QueryRawAndroidMethodCallNoMapping()
        {
            TestAndroidQuery(
                x => x.Raw("*", new { method_name = "test", arguments = new object[] { 42 } }),
                "*",
                new object[] { new { method_name = "test", arguments = new object[] { 42 } } },
                "Raw(\"*\", { method_name: \"test\", arguments: [ 42 ] })");
        }

        [Test]
        public void QueryAndroidCallSingleMethodNoArgs()
        {
            TestAndroidQuery(
                x => x.Invoke("myMethod"),
                "*",
                new object[] { new { method_name = "myMethod", arguments = new object[0] } },
                "Invoke(\"myMethod\")");
        }

        [Test]
        public void QueryAndroidCallTwoMethodsNoArgs()
        {
            TestAndroidQuery(
                x => x.Invoke("myMethod").Invoke("mySecondMethod"),
                "*",
                new object[] {
                    new { method_name = "myMethod", arguments = new object[0] },
                    new { method_name = "mySecondMethod", arguments = new object[0] }
                },
                "Invoke(\"myMethod\").Invoke(\"mySecondMethod\")");
        }

        [Test]
        public void QueryAndroidCallThreeMethodsNoArgs()
        {
            TestAndroidQuery(
                x => x.Invoke("myMethod").Invoke("mySecondMethod").Invoke("myThirdMethod"),
                "*",
                new object[] {
                    new { method_name = "myMethod", arguments = new object[0] },
                    new { method_name = "mySecondMethod", arguments = new object[0] },
                    new { method_name = "myThirdMethod", arguments = new object[0] }
                },
                "Invoke(\"myMethod\").Invoke(\"mySecondMethod\").Invoke(\"myThirdMethod\")");
        }

        [Test]
        public void QueryiOSCallSingleMethodNoArgs()
        {
            TestiOSQuery(
                x => x.Invoke("myMethod"),
                "*",
                new object[] { "myMethod" },
                "Invoke(\"myMethod\")");
        }

        [Test]
        public void QueryiOSCallTwoMethodsNoArgs()
        {
            TestiOSQuery(
                x => x.Invoke("myMethod").Invoke("mySecondMethod"),
                "*",
                new object[] { "myMethod", "mySecondMethod" },
                "Invoke(\"myMethod\").Invoke(\"mySecondMethod\")");
        }

        [Test]
        public void QueryiOSCallThreeMethodsNoArgs()
        {
            TestiOSQuery(
                x => x.Invoke("myMethod").Invoke("mySecondMethod").Invoke("myThirdMethod"),
                "*",
                new object[] { "myMethod", "mySecondMethod", "myThirdMethod" },
                "Invoke(\"myMethod\").Invoke(\"mySecondMethod\").Invoke(\"myThirdMethod\")");
        }

        [Test]
        public void QueryiOSCallSingleMethodOneArg()
        {
            TestiOSQuery(
                x => x.Invoke("myMethod", 42),
                "*",
                new object[] { new object[] { new { myMethod = 42 } } },
                "Invoke(\"myMethod\", 42)");
        }

        [Test]
        public void QueryiOSCallSingleMethodOneArgChained()
        {
            TestiOSQuery(
                x => x.Id("test").Invoke("myMethod", 42),
                "* id:'test'",
                new object[] { new object[] { new { myMethod = 42 } } },
                "Id(\"test\").Invoke(\"myMethod\", 42)");
        }

        [Test]
        public void QueryiOSCallSingleMethodTwoArgs()
        {
            try
            {
                TestiOSQuery(
                    x => x.Invoke("myMethod", 42, "forTest"),
                    null,
                    null,
                    null);

                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Invoking an iOS selector requires either 0 or an uneven number of arguments (they have to match up pairwise including method name).", ex.Message);
            }
        }

        [Test]
        public void QueryiOSCallSingleMethodThreeArgs()
        {
            TestiOSQuery(
                x => x.Invoke("myMethod", 42, "forTest", "test"),
                "*",
                new object[] { new object[] { new { myMethod = 42 }, new { forTest = "test" } } },
                "Invoke(\"myMethod\", 42, \"forTest\", \"test\")");
        }

        [Test]
        public void QueryiOSCallSingleMethodThreeArgsChained()
        {
            TestiOSQuery(
                x => x.Id("test").Invoke("myMethod", 42, "forTest", "test"),
                "* id:'test'",
                new object[] { new object[] { new { myMethod = 42 }, new { forTest = "test" } } },
                "Id(\"test\").Invoke(\"myMethod\", 42, \"forTest\", \"test\")");
        }

        [Test]
        public void QueryiOSCallSingleMethodFourArgs()
        {
            try
            {
                TestiOSQuery(
                    x => x.Invoke("myMethod", 42, "forTest", 41, "forHopsa"),
                    null,
                    null,
                    null);

                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Invoking an iOS selector requires either 0 or an uneven number of arguments (they have to match up pairwise including method name).", ex.Message);
            }
        }

        [Test]
        public void QueryiOSCallSingleMethodFiveArgs()
        {
            TestiOSQuery(
                x => x.Invoke("myMethod", 42, "forTest", "test", "hopsa", 41),
                "*",
                new object[] { new object[] { new { myMethod = 42 }, new { forTest = "test" }, new { hopsa = 41 } } },
                "Invoke(\"myMethod\", 42, \"forTest\", \"test\", \"hopsa\", 41)");
        }

        [Test]
        public void QueryiOSCallSingleMethodFiveArgsChained()
        {
            TestiOSQuery(
                x => x.Id("test").Invoke("myMethod", 42, "forTest", "test", "hopsa", 41),
                "* id:'test'",
                new object[] { new object[] { new { myMethod = 42 }, new { forTest = "test" }, new { hopsa = 41 } } },
                "Id(\"test\").Invoke(\"myMethod\", 42, \"forTest\", \"test\", \"hopsa\", 41)");
        }

        [Test]
        public void QueryWebView()
        {
            TestQuery(
                x => x.WebView(),
                "WebView()",
                android: @"android.webkit.WebView",
                ios: @"view:'UIWebView'"
            );
        }

        [Test]
        public void QueryiOSWebViewChained()
        {
            TestQuery(
                x => x.Id("test").WebView(),
                "Id(\"test\").WebView()",
                android: @"* id:'test' android.webkit.WebView",
                ios: @"* id:'test' view:'UIWebView'"
            );
        }

        [Test]
        public void QueryWebViewIndex()
        {
            TestQuery(
                x => x.WebView(5),
                "WebView(5)",
                android: @"android.webkit.WebView index:5",
                ios: @"view:'UIWebView' index:5"
            );
        }

        [Test]
        public void QueryWebViewIndexChained()
        {
            TestQuery(
                x => x.Id("test").WebView(5),
                "Id(\"test\").WebView(5)",
                android: @"* id:'test' android.webkit.WebView index:5",
                ios: @"* id:'test' view:'UIWebView' index:5"
            );
        }

        [Test]
        public void QueryInvokeJs()
        {
            Func<AppQuery, IInvokeJSAppQuery> q = x => x.InvokeJS("foo");
            TestInvokeJSQuery(
                q,
                "*",
                "foo",
                "InvokeJS(\"foo\")"
            );
        }

        [Test]
        public void QueryInvokeJsOnWebView()
        {
            Func<AppQuery, IInvokeJSAppQuery> q = x => x.WebView(3).InvokeJS("foo");
            TestInvokeJSQuery(
                q,
                "foo",
                "WebView(3).InvokeJS(\"foo\")",
                android: "android.webkit.WebView index:3",
                ios: "view:'UIWebView' index:3"
            );
        }

        void TestQuery(Func<AppQuery, ITokenContainer> typedQuery, string expectedCodeString, string android = null, string ios = null)
        {
            TestiOSQuery(typedQuery, ios, expectedCodeString);
            TestAndroidQuery(typedQuery, android, expectedCodeString);
        }

        void TestQuery<T>(Func<AppQuery, AppTypedSelector<T>> typedQuery, string expectedQuery, object[] expectedOptions, string expectedCodeString)
        {
            TestAndroidQuery(typedQuery, expectedQuery, expectedOptions, expectedCodeString);
            TestiOSQuery(typedQuery, expectedQuery, expectedOptions, expectedCodeString);
        }

        void TestAndroidQuery<T>(Func<AppQuery, AppTypedSelector<T>> typedQuery, string expectedQuery, object[] expectedOptions, string expectedCodeString)
        {
            var androidQuery = typedQuery(new AppQuery(QueryPlatform.Android));
            TestQuery(androidQuery, expectedQuery, expectedCodeString);

            var serializedJson = JsonConvert.SerializeObject(((IAppTypedSelector)androidQuery).QueryParams);
            var expectedJson = JsonConvert.SerializeObject(expectedOptions);

            serializedJson.ShouldEqual(expectedJson);
        }

        void TestiOSQuery<T>(Func<AppQuery, AppTypedSelector<T>> typedQuery, string expectedQuery, object[] expectedOptions, string expectedCodeString)
        {
            var iosQuery = typedQuery(new AppQuery(QueryPlatform.iOS));
            TestQuery(iosQuery, expectedQuery, expectedCodeString);

            var serializedJson = JsonConvert.SerializeObject(((IAppTypedSelector)iosQuery).QueryParams);
            var expectedJson = JsonConvert.SerializeObject(expectedOptions);

            serializedJson.ShouldEqual(expectedJson);
        }

        void TestInvokeJSQuery(Func<AppQuery, IInvokeJSAppQuery> queryFunc, string expectedQuery, string expectedJavascript, string expectedCodeString)
        {
            TestAndroidInvokeJSQuery(queryFunc, expectedQuery, expectedJavascript, expectedCodeString);
            TestiOSInvokeJSQuery(queryFunc, expectedQuery, expectedJavascript, expectedCodeString);
        }

        void TestInvokeJSQuery(Func<AppQuery, IInvokeJSAppQuery> queryFunc, string expectedJavascript, string expectedCodeString, string android = null, string ios = null)
        {
            TestAndroidInvokeJSQuery(queryFunc, android, expectedJavascript, expectedCodeString);
            TestiOSInvokeJSQuery(queryFunc, ios, expectedJavascript, expectedCodeString);
        }

        void TestiOSInvokeJSQuery(Func<AppQuery, IInvokeJSAppQuery> queryFunc, string expectedQuery, string expectedJavascript, string expectedCodeString)
        {
            var query = queryFunc(new AppQuery(QueryPlatform.iOS));

            query.AppQuery.ToString().ShouldEqual(expectedQuery);
            TestCodeString(query, expectedCodeString);
            query.Javascript.ShouldEqual(expectedJavascript);
        }

        void TestAndroidInvokeJSQuery(Func<AppQuery, IInvokeJSAppQuery> queryFunc, string expectedQuery, string expectedJavascript, string expectedCodeString)
        {
            var query = queryFunc(new AppQuery(QueryPlatform.Android));

            query.AppQuery.ToString().ShouldEqual(expectedQuery);
            TestCodeString(query, expectedCodeString);
            query.Javascript.ShouldEqual(expectedJavascript);
        }

        void TestQuery(Func<AppQuery, ITokenContainer> queryFunc, string expectedQuery, string expectedCodeString)
        {
            TestAndroidQuery(queryFunc, expectedQuery, expectedCodeString);
            TestiOSQuery(queryFunc, expectedQuery, expectedCodeString);
        }

        void TestiOSQuery(Func<AppQuery, ITokenContainer> queryFunc, string expectedQuery, string expectedCodeString)
        {
            var appQuery = queryFunc(new AppQuery(QueryPlatform.iOS));

            TestQuery(appQuery, expectedQuery, expectedCodeString);
        }

        void TestAndroidQuery(Func<AppQuery, ITokenContainer> queryFunc, string expectedQuery, string expectedCodeString)
        {
            var appQuery = queryFunc(new AppQuery(QueryPlatform.Android));

            TestQuery(appQuery, expectedQuery, expectedCodeString);
        }

        void TestQuery(ITokenContainer query, string expectedQuery, string expectedCodeString)
        {
            query.ToString().ShouldEqual(expectedQuery);

            TestCodeString(query, expectedCodeString);
        }

        static void TestCodeString(ITokenContainer container, string expectedCodeString)
        {
            if (!string.IsNullOrWhiteSpace(expectedCodeString))
            {
                TokenCodePrinter.ToCodeString(container).ShouldEqual(expectedCodeString);
            }
        }
    }
}