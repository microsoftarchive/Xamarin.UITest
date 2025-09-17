using System;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Json;

namespace Xamarin.UITest.Tests.Utils.Json
{
    [TestFixture]
    public class JsonPrettyPrinterTests
    {
        readonly JsonPrettyPrinter _printer = new JsonPrettyPrinter();

        [Test]
        public void SimpleObjectTest()
        {
            var str = _printer.PrintObject(new { test = 42 });

            str.ShouldEqualIgnoringNewLineStyles(
@"{
  test => 42
}");
        }

        [Test]
        public void MultiplePropertyObjectTest()
        {
            var obj = new { a = 1, b = 2 };
            var str = _printer.PrintObject(obj);
            str.ShouldEqualIgnoringNewLineStyles(
@"{
  a => 1,
  b => 2
}");
        }

        [Test]
        public void ArrayOfSimpleObjectsTest()
        {
            var obj = new object[]
            {
                new {a = 1},
                new {b = 2},
                new {c = 3}
            };
            var str = _printer.PrintObject(obj);
            str.ShouldEqualIgnoringNewLineStyles(
@"[
  [0] {
    a => 1
  },
  [1] {
    b => 2
  },
  [2] {
    c => 3
  }
]"
                );
        }

        [Test]
        public void ArrayWithinObjectTest()
        {
            var obj = new
            {
                arr = new object[] { 1, 2, 3 }
            };
            var str = _printer.PrintObject(obj);
            str.ShouldEqualIgnoringNewLineStyles(
@"{
  arr => [
    [0] 1,
    [1] 2,
    [2] 3
  ]
}"
                );
        }

        [Test]
        public void ArrayWithinArrayTest()
        {
            var obj = new object[]
            {
                new { a = new [] { 1, 2 }},
                new { b = 2 }
            };
            var str = _printer.PrintObject(obj);

            const string expected = @"[
  [0] {
    a => [
      [0] 1,
      [1] 2
    ]
  },
  [1] {
    b => 2
  }
]";

            str.ShouldEqualIgnoringNewLineStyles(expected);
        }

        [Test]
        public void LargeObjectWithAllTypes()
        {
            var obj = new object[]
            {
                new {stringy = "1"},
                new {floaty = 33.123f},
                new {booly = false},
                new {datey = new DateTime(1990, 9, 22)},
                new {timey = TimeSpan.FromSeconds(30)},
                new {inty = 0},
                new {bytesy = new [] {new Byte(), new Byte(), new Byte()}},
                new {guidy = new Guid()},
                new {urly = new Uri("http://www.uri-string.com")},
                new {arrayy = new object[] { "a", "b", "c"}}
            };
            var str = _printer.PrintObject(obj);
            str.ShouldEqualIgnoringNewLineStyles(
@"[
  [0] {
    stringy => ""1""
  },
  [1] {
    floaty => 33.123
  },
  [2] {
    booly => false
  },
  [3] {
    datey => 1990-09-22 00:00:00
  },
  [4] {
    timey => ""00:00:30""
  },
  [5] {
    inty => 0
  },
  [6] {
    bytesy => ""AAAA""
  },
  [7] {
    guidy => ""00000000-0000-0000-0000-000000000000""
  },
  [8] {
    urly => ""http://www.uri-string.com""
  },
  [9] {
    arrayy => [
      [0] ""a"",
      [1] ""b"",
      [2] ""c""
    ]
  }
]");
        }

        [Test]
        public void CalabashResultTest()
        {
            var obj = new object[] {
                new {
                    id = "buttonMultiScroll",
                    enabled = false,
                    contentDescription = (object)null,
                    text = "MultiScroll",
                    visible = true,
                    tag = (object)null,
                    description = "android.widget.Button{52872374 VFED..C. ........ 0,1518-990,1652 #7f08005a app:id/buttonMultiScoll}",
                    classs = "android.widget.Button",
                    rect = new {
                        center_y = 1700,
                        center_x = 540,
                        height = 134,
                        y = 1633,
                        width = 990,
                        x = 45
                    }
                },
                new {
                        id = "buttonMultiScroll",
                        enabled = false,
                        contentDescription = (object)null,
                        text = "MultiScroll",
                        visible = true,
                        tag = (object)null,
                        description = "android.widget.Button{52872374 VFED..C. ........ 0,1518-990,1652 #7f08005a app:id/buttonMultiScoll}",
                        classs = "android.widget.Button",
                        rect = new {
                            center_y = 1700,
                            center_x = 540,
                            height = 134,
                            y = 1633,
                            width = 990,
                            x = 45
                    }
                },
                new {
                    id = "buttonMultiScroll",
                    enabled = false,
                    contentDescription = (object)null,
                    text = "MultiScroll",
                    visible = true,
                    tag = (object)null,
                    description = "android.widget.Button{52872374 VFED..C. ........ 0,1518-990,1652 #7f08005a app:id/buttonMultiScoll}",
                    classs = "android.widget.Button",
                    rect = new {
                        center_y = 1700,
                        center_x = 540,
                        height = 134,
                        y = 1633,
                        width = 990,
                        x = 45
                    }
                }
            };

            const string expected = @"[
  [0] {
    id => ""buttonMultiScroll"",
    enabled => false,
    contentDescription => null,
    text => ""MultiScroll"",
    visible => true,
    tag => null,
    description => ""android.widget.Button{52872374 VFED..C. ........ 0,1518-990,1652 #7f08005a app:id/buttonMultiScoll}"",
    classs => ""android.widget.Button"",
    rect => {
      center_y => 1700,
      center_x => 540,
      height => 134,
      y => 1633,
      width => 990,
      x => 45
    }
  },
  [1] {
    id => ""buttonMultiScroll"",
    enabled => false,
    contentDescription => null,
    text => ""MultiScroll"",
    visible => true,
    tag => null,
    description => ""android.widget.Button{52872374 VFED..C. ........ 0,1518-990,1652 #7f08005a app:id/buttonMultiScoll}"",
    classs => ""android.widget.Button"",
    rect => {
      center_y => 1700,
      center_x => 540,
      height => 134,
      y => 1633,
      width => 990,
      x => 45
    }
  },
  [2] {
    id => ""buttonMultiScroll"",
    enabled => false,
    contentDescription => null,
    text => ""MultiScroll"",
    visible => true,
    tag => null,
    description => ""android.widget.Button{52872374 VFED..C. ........ 0,1518-990,1652 #7f08005a app:id/buttonMultiScoll}"",
    classs => ""android.widget.Button"",
    rect => {
      center_y => 1700,
      center_x => 540,
      height => 134,
      y => 1633,
      width => 990,
      x => 45
    }
  }
]";

            var str = _printer.PrintObject(obj);

            str.ShouldEqualIgnoringNewLineStyles(expected);
        }
    }

    public static class StringAssertExtensions
    {
        public static void ShouldEqualIgnoringNewLineStyles(this string str, string expected)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(expected))
            {
                str.ShouldEqual(expected);
            }

            str = str.Replace("\r\n", "\n");
            expected = expected.Replace("\r\n", "\n");

            str.ShouldEqual(expected);
        }
    }
}