using System;
using System.Diagnostics;

namespace Xamarin.UITest.Repl
{
    public static class ClipboardHelper
    {
        public static void SetClipboardText(string text)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var process = Process.Start(new ProcessStartInfo("pbcopy")
                {
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                });

                process.StandardInput.Write(text);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
            }
            else
            {
                var textWithEscapedSymbols = text.Replace("\"", "\\\"");
                var process = Process.Start(new ProcessStartInfo("cmd")
                {
                    Arguments = $"/c \"{textWithEscapedSymbols}\"",
                    RedirectStandardInput = true,
                    UseShellExecute = true
                });
                process.WaitForExit();
            }
        }
    }
}