using System;
using System.IO;
using System.Linq;
using MediaConverterToMP3.Models;

namespace MediaConverterToMP3.Views.MainWindowOperations.Utilities
{
    internal static class YouTubeOptions
    {
        public static readonly string[] Browsers = { "None", "firefox", "chrome", "edge", "brave", "opera", "vivaldi" };

        public static string GetArguments(string ytDlpPath)
        {
            string arguments = "";
            var directories = new[] { AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(ytDlpPath) ?? "" }
                .Concat((Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator));
            foreach (string runtime in new[] { "deno", "node" })
            {
                string? executable = directories.Where(d => !string.IsNullOrWhiteSpace(d))
                    .Select(d => Path.Combine(d.Trim().Trim('"'), runtime + ".exe"))
                    .FirstOrDefault(File.Exists);
                if (executable != null)
                {
                    arguments += $"--js-runtimes \"{runtime}:{executable}\" ";
                    break;
                }
            }

            string? browser = AppSettings.Load().YouTubeCookiesBrowser;
            if (browser != null && browser != "None" && Browsers.Contains(browser))
                arguments += $"--cookies-from-browser {browser} ";
            return arguments;
        }

        public static string DescribeError(string error)
        {
            string help = "";
            if (error.Contains("confirm your age", StringComparison.OrdinalIgnoreCase) ||
                error.Contains("age-restricted", StringComparison.OrdinalIgnoreCase))
                help += "YouTube requires age verification for this video. Sign in with an eligible account in your browser, then select that browser under Settings > YouTube sign-in and retry.\n";
            if (error.Contains("No supported JavaScript runtime", StringComparison.OrdinalIgnoreCase))
                help += "Install a supported Deno or Node.js runtime, or place deno.exe beside yt-dlp.exe, then restart the app.\n";
            return help.Length == 0 ? error : help + "\nDetails:\n" + error;
        }
    }
}
