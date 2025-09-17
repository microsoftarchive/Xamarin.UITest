using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xamarin.UITest.Repl.Repl;
using Mono.Cecil;
using Xamarin.UITest.Shared.Android;

namespace Xamarin.UITest.Repl
{
    public static class Program
    {
        static string _assemblyPath;

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                Console.Title = "Xamarin.UITest REPL";
                Console.WriteLine();

                Console.Clear();

                if (args.Length < 5)
                {
                    throw new ArgumentException("Syntax: <android/ios> <ui-test-path> <device-uri> <device-identifier> (<use-xdb-ios-only> OR <android-sdk-path>)");
                }

                var deviceTypeStr = args[0];
                var uiTestPath = args[1];
                var deviceUriStr = args[2];
                var deviceIdentifier = args[3];
                bool useXDB = false;

                if (deviceTypeStr.Equals("android"))
                {
                    UITestReplSharedSdkLocation.SetSharedSdkPath(args[4]);
                    WriteLine($"Android SDK Path: {args[4]}");
                }
                else
                {
                    useXDB = bool.Parse(args[4]);
                }

                _assemblyPath = Path.GetDirectoryName(uiTestPath);

                var replFacade = new ReplFacade();
                replFacade.LoadAssembly(uiTestPath);
                LoadAssembliesAndAddUsingsForExtensionMethods(_assemblyPath, uiTestPath, replFacade);

                var deviceUri = new Uri(deviceUriStr);
                var deviceType = getDeviceType(deviceTypeStr);

                var initCode = GetInitCode(deviceUri, deviceType, deviceIdentifier, useXDB);
                var result = replFacade.RunCode(initCode);

                PrintInitialMessage();

                var promptHandler = new PromptHandler(new ConsoleString(">>> ", ConsoleColor.Cyan), replFacade);
                promptHandler.Start();

                while (true)
                {
                    promptHandler.HandleInput(Console.ReadKey());
                }
            }
            catch (Exception ex)
            {
                WriteLine("Execution failed with exception: " + ex);
                WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }

        static void LoadAssembliesAndAddUsingsForExtensionMethods(string assemblyPath, string uiTestPath, ReplFacade replFacade)
        {
            Debug.WriteLine($"Trying to load assemblies from: {assemblyPath}");
            try
            {
                var files = Directory.GetFiles(assemblyPath, "*.dll").Where(s => !string.Equals(s, uiTestPath, StringComparison.InvariantCultureIgnoreCase));

                var extensionMethods = new List<UITestExtensionMethod>();

                foreach (var file in files)
                {
                    Debug.WriteLine($"Parsing methods from: {file}");
                    try
                    {
                        var assembly = AssemblyDefinition.ReadAssembly(file);

                        var foundMethods = (from module in assembly.Modules
                            from type in module.Types
                            from method in type.Methods
                            where method.IsStatic
                            && method.IsPublic
                            && method.CustomAttributes.Any(a => a.AttributeType.Name == "ExtensionAttribute")
                            && method.Parameters.Any()
                            && method.Parameters.First().ParameterType.Namespace.StartsWith("Xamarin.UITest", StringComparison.InvariantCulture)
                            select new UITestExtensionMethod(file, type.Namespace, type.Name, method.Name));

                        extensionMethods.AddRange(foundMethods);
                    }
                    catch (Exception ex)
                    {
                        WriteLine("Skipping assembly: " + file + " - Scan failed with exception: " + ex);
                    }
                }

                foreach (var file in extensionMethods.Select(x => x.AssemblyFile).Distinct())
                {
                    replFacade.LoadAssembly(file);
                }

                foreach (var ns in extensionMethods.Select(x => x.TypeNamespace).Distinct())
                {
                    replFacade.AddUsing(ns);
                }
            }
            catch (Exception ex)
            {
                WriteLine("Loading assemblies failed with: " + ex.Message, ConsoleColor.Red);
            }
        }

        static void PrintInitialMessage()
        {
            WriteLine();
            Write("App has been initialized to the '");
            Write("app", ConsoleColor.Magenta);
            WriteLine("' variable.");
            Write("Exit REPL with ctrl-c or see ");
            Write("help", ConsoleColor.Yellow);
            WriteLine(" for more commands.");
            WriteLine();
        }

        static void Write(string str = null)
        {
            Console.Write(str);
        }

        static void Write(string str, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(str);
            Console.ResetColor();
        }

        static void WriteLine(string str = null)
        {
            Console.WriteLine(str);
        }

        static void WriteLine(string str, ConsoleColor color)
        {
            Write(str + Environment.NewLine, color);
        }

        static DeviceType getDeviceType(string deviceType)
        {
            switch (deviceType)
            {
                case "android":
                    return DeviceType.Android;
                case "ios":
                    return DeviceType.iOS;
                default:
                    throw new Exception("Unknown device type: " + deviceType);
            }
        }

        static string GetInitCode(Uri deviceUri, DeviceType deviceType, string deviceIdentifier, bool useXDB = false)
        {
            switch (deviceType)
            {
                case DeviceType.Android:
                    return string.Format("var app = ConfigureApp.Android.DeviceIp(\"{0}\").DevicePort({1}).DeviceSerial(\"{2}\").WaitTimes(new Xamarin.UITest.Utils.ReplWaitTimes()).ConnectToApp();", deviceUri.Host, deviceUri.Port, deviceIdentifier);
                case DeviceType.iOS:
                    return ConfigureIOSInitCode(deviceUri, deviceType, deviceIdentifier, useXDB);
                default:
                    throw new Exception(string.Format("Unknown device type: {0}", deviceType));
            }
        }

        static string ConfigureIOSInitCode(Uri deviceUri, DeviceType deviceType, string deviceIdentifier, bool useXDB)
        {
            string result = $"var app = ConfigureApp.iOS.DeviceIp(\"{deviceUri.Host}\")" +
                $".DevicePort({deviceUri.Port})" +
                $".DeviceIdentifier(\"{deviceIdentifier}\")" +
                ".WaitTimes(new Xamarin.UITest.Utils.ReplWaitTimes())";
            // TODO: re-enable this once .PreferDeviceAgnet() is available in release builds
            // (we can't use #if DEBUG as repl is always in debug config)
            // return result += useXDB ? ".PreferDeviceAgent().ConnectToApp();" : ".ConnectToApp();";
            return result += ".ConnectToApp();";
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var match = Regex.Match(args.Name, "(?<name>.*), Version=.*, Culture=.*, PublicKeyToken=.*");

                if (match.Success)
                {
                    var assemblyName = match.Groups["name"].Value;
                    var assemblyPath = Path.Combine(_assemblyPath, string.Format("{0}.dll", assemblyName));

                    if (File.Exists(assemblyPath))
                    {
                        return Assembly.LoadFile(assemblyPath);
                    }
                }
            } 
            catch(Exception ex)
            {
                WriteLine("Assembly resolve of " + args.Name + " failed with exception: " + ex);
                WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            return null;
        }
    }
}
