using System;
using NUnit.Framework;
using Should;
using System.Linq;
using Xamarin.UITest.Queries;
using System.Collections.Generic;
using IntegrationTests.Shared;
using Newtonsoft.Json;

public class iOSCalSmokeTests : IOSTestBase
{
    protected override AppInformation _appInformation => TestApps.iOSCalSmoke;

    [Test]
    public void DragAndDropTest1()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }

    [Test]
    public void DragAndDropTest2()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest3()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest4()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest5()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest6()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest7()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest8()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest9()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest10()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest11()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest12()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest13()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest14()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest15()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest16()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest17()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest18()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest19()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest20()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest21()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest22()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest23()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest24()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest25()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest26()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest27()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest28()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest29()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest30()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest31()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest32()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest33()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest34()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest35()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest36()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest37()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest38()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest39()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest40()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest41()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest42()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest43()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest44()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest45()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest46()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest47()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest48()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest49()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest50()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest51()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest52()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest53()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest54()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest55()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest56()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }
    [Test]
    public void DragAndDropTest57()
    {
        _app.Tap(c => c.Class("tabBarButton").Index(3));
        _app.WaitForElement("special page");

        var beforeColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        _app.DragAndDrop("red", "right well");

        var afterColorJson = _app.Query(c => c.Id("right well").Invoke("backgroundColor")).First();

        afterColorJson.ShouldNotEqual(beforeColorJson);
    }


    // [Test]
    // public void SwitchAndPropertyValueTest()
    // {
    //     _app.Screenshot("app started");
    //     var beforeValue = _app.Query(c => c.Switch("switch").Property("isOn").Value<int>()).First();
    //     _app.Tap(c => c.Switch("switch"));
    //     _app.Screenshot("switch switched");
    //     var afterValue = _app.Query(c => c.Switch("switch").Property("isOn").Value<int>()).First();
    //     afterValue.ShouldNotEqual(beforeValue);
    // }

    // [Test]
    // public void EnterAndClearTextTest()
    // {
    //     var textValue = "my test text";

    //     Func<AppQuery, AppQuery> textQuery = c => c.Id("text");

    //     _app.Tap(textQuery);
    //     _app.Query(textQuery).Single().Text.ShouldNotEqual(textValue);
    //     _app.EnterText(textValue);
    //     _app.Screenshot("test text entered");
    //     _app.Query(textQuery).Single().Text.ShouldEqual(textValue);

    //     _app.Query(c => c.Class("UIKBKeyplaneView")).ShouldNotBeEmpty();
    //     _app.PressEnter();
    //     _app.Screenshot("pressed enter");
    //     _app.Query(c => c.Class("UIKBKeyplaneView")).ShouldBeEmpty();

    //     _app.Tap(textQuery);
    //     _app.ClearText();
    //     _app.Screenshot("cleared text");
    //     _app.Query(textQuery).Single().Text.ShouldBeEmpty();

    //     _app.EnterText(textQuery, textValue);
    //     _app.Screenshot("test text entered with query");
    //     _app.Query(textQuery).Single().Text.ShouldEqual(textValue);

    //     _app.ClearText(textQuery);
    //     _app.Screenshot("cleared text with query");
    //     _app.Query(textQuery).Single().Text.ShouldBeEmpty();
    // }

    // [Test]
    // public void EnterTextStringEscapingTest()
    // {
    //     var textValue = "Now it's Stoney's problem";

    //     Func<AppQuery, AppQuery> textQuery = c => c.Id("text");

    //     _app.EnterText(textQuery, textValue);
    //     _app.Screenshot("single quotes text");
    //     _app.Query(textQuery).Single().Text.ShouldEqual(textValue);

    //     _app.ClearText(textQuery);
    //     _app.Query(textQuery).Single().Text.ShouldBeEmpty();

    //     textValue = "Now it's Stoney's \"problem\"";
    //     _app.EnterText(textQuery, textValue);
    //     _app.Screenshot("double quotes text");
    //     _app.Query(textQuery).Single().Text.ShouldEqual(textValue);

    //     _app.ClearText(textQuery);
    //     _app.Query(textQuery).Single().Text.ShouldBeEmpty();

    //     textValue = "Now it's Stoney's \\ problem";
    //     _app.EnterText(textQuery, textValue);
    //     _app.Screenshot("backslash text");
    //     _app.Query(textQuery).Single().Text.ShouldEqual(textValue);
    // }

    // [Test]
    // public void TestClearTextNoArgsNoFocus()
    // {
    //     var ex = Assert.Throws<Exception>(_app.ClearText);
    //     Assert.AreEqual("Error while performing ClearText()", ex.Message);
    // }

    // [Test]
    // public void TestClearTextNonExistant()
    // {
    //     Func<AppQuery, AppQuery> textQuery = c => c.TextField();
    //     _app.EnterText(textQuery, "foo");
    //     _app.Screenshot("I see foo");
    //     try
    //     {
    //         _app.ClearText(c => c.TextField("foo"));
    //     }
    //     catch (Exception e)
    //     {
    //         e.Message.ShouldContain("foo");
    //         return;
    //     }
    //     Assert.Fail("ClearText should have thrown exception");
    // }

    // [Test]
    // public void BackdoorTest()
    // {
    //     const string stringArg = "my string arg";

    //     _app.Invoke("backdoorWithString:", stringArg).ToString().ShouldEqual(stringArg);

    //     _app.Invoke("backdoorString").ToString().ShouldEqual("string");

    //     var result = (IEnumerable<object>)_app.Invoke(
    //         "backdoorWithString:array:",
    //         new object[] { stringArg, new[] { "a", "b", "c" } });

    //     result.Count().ShouldEqual(2);
    //     result.First().ToString().ShouldEqual(stringArg);
    // }

    // [Test]
    // public void FlashTest()
    // {
    //     _app.Tap(c => c.Class("tabBarButton").Index(3));
    //     _app.WaitForElement("special page");

    //     _app.Flash(c => c.Class("UIButton")).Length.ShouldEqual(3);
    //     _app.Flash("doesn't exist").Length.ShouldEqual(0);
    // }

    // [Test]
    // public void InvokeInvalid()
    // {
    //     _app.Tap(c => c.Class("tabBarButton").Index(0));
    //     _app.WaitForElement("controls page");

    //     _app.Query(c => c.Marked("controls page").Invoke("unknownMethod"))
    //         .First().ShouldEqual("*****");

    //     _app.Query(c => c.Marked("controls page").Invoke("stringFromMethodWithSelf", "__self__"))
    //         .First().ShouldEqual("Self reference! Hurray!");

    //     _app.Query(c => c.Marked("controls page").Invoke("takesPointer", "a string"))
    //         .First().ShouldEqual(1L);

    //     _app.Query(c => c.Marked("controls page").Invoke("takesInt", -1))
    //         .First().ShouldEqual(1L);

    //     _app.Query(c => c.Marked("controls page").Invoke("takesUnsignedInt", 1))
    //         .First().ShouldEqual(1L);

    //     _app.Query(c => c.Marked("controls page").Invoke("takesDouble", 0.1))
    //         .First().ShouldEqual(1L);

    //     _app.Query(c => c.Marked("controls page").Invoke("takesLongDouble", Math.PI))
    //         .First().ShouldEqual(1L);

    //     _app.Query(c => c.Marked("controls page").Invoke("takesChar", 'a'))
    //         .First().ShouldEqual(1L);

    //     _app.Query(c => c.Marked("controls page").Invoke("takesBOOL", true))
    //         .First().ShouldEqual(1L);

    //     _app.Query(c => c.Marked("controls page").Invoke("takesPoint", new { x = 5.0, y = 10.2 }))
    //         .First().ShouldEqual(1L);

    //     _app.Query(c => c.Marked("controls page").Invoke("returnsVoid"))
    //         .First().ShouldEqual(null);

    //     _app.Query(c => c.Marked("controls page").Invoke("returnsPointer"))
    //         .First().ShouldEqual("a pointer");

    //     _app.Query(c => c.Marked("controls page").Invoke("returnsChar"))
    //         .First().ShouldEqual(-22L);

    //     _app.Query(c => c.Marked("controls page").Invoke("returnsBool"))
    //         .First().ShouldEqual(1L);

    //     var expectedPoint = JsonToDictonary("{\n  \"X\": 0,\n  \"Y\": 0,\n  \"description\": \"NSPoint: {0, 0}\"\n}");

    //     var queryResult = _app.Query(c => c.Marked("controls page").Invoke("returnsPoint"));
    //     var actualPoint = JsonToDictonary(queryResult.First().ToString());
    //     actualPoint.ShouldEqual(expectedPoint);

    //     _app.Query(c => c.Marked("controls page").Invoke("selectorWithArg", "a", "arg", "b", "arg", "c"))
    //         .First().ToString().ShouldNotEqual("*****");
    // }

    // IDictionary<string, object> JsonToDictonary(string json)
    // {
    //     return JsonConvert.DeserializeObject<IDictionary<string, object>>(json);
    // }

    // [Test]
    // public void TapTests()
    // {
    //     _app.Tap(c => c.Class("tabBarButton").Index(1));
    //     _app.Tap("tapping row");
    //     _app.WaitForElement("tapping page");
    //     _app.DoubleTap("left box");
    //     _app.WaitForElement(c => c.Marked("last gesture").Text("Double tap"));
    // }

    // [Test]
    // public void PinchTest()
    // {
    //     _app.Tap(c => c.Class("tabBarButton").Index(1));
    //     _app.Tap("pinching row");
    //     _app.WaitForElement("pinching page");

    //     var boxWidth = _app.Query(c => c.Id("box")).Single().Rect.Width;

    //     _app.Screenshot("Before any pinch");

    //     _app.PinchToZoomIn(c => c.Id("box"));

    //     var bigBoxWidth = _app.Query(c => c.Id("box")).Single().Rect.Width;
    //     bigBoxWidth.ShouldBeGreaterThan(boxWidth);

    //     _app.Screenshot("After zoom in");

    //     _app.PinchToZoomOut(c => c.Id("box"));

    //     _app.Query(c => c.Id("box")).Single().Rect.Width.ShouldBeLessThan(bigBoxWidth);

    //     _app.Screenshot("After zoom out");
    // }
}
