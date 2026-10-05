using System;
using System.IO;
using System.Net.Http;

namespace MediaConverterToMP3.Views.MainWindowOperations.Utilities
{
    public static class FileUtilities
    {
        private static readonly string[] YtDlpSearchNames = { "yt-dlp.exe", "yt-dlp" };
        private static readonly string[] FfmpegSearchNames = { "ffmpeg.exe", "ffmpeg" };

        public static string? FindFfmpeg()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var name in FfmpegSearchNames)
            {
                string path = Path.Combine(currentDir, name);
                if (File.Exists(path))
                    return path;
            }

            try
            {
                string? binDir1 = Directory.GetParent(currentDir)?.Parent?.FullName;
                if (!string.IsNullOrEmpty(binDir1))
                {
                    foreach (var name in FfmpegSearchNames)
                    {
                        string path = Path.Combine(binDir1, name);
                        if (File.Exists(path))
                            return path;
                    }
                }

                DirectoryInfo? current = new DirectoryInfo(currentDir);
                for (int i = 0; i < 5 && current != null; i++)
                {
                    current = current.Parent;
                    if (current != null && current.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var name in FfmpegSearchNames)
                        {
                            string path = Path.Combine(current.FullName, name);
                            if (File.Exists(path))
                                return path;
                        }
                        break;
                    }
                }
            }
            catch { }

            try
            {
                string? projectDir = Directory.GetParent(currentDir)?.Parent?.Parent?.FullName;
                if (!string.IsNullOrEmpty(projectDir))
                {
                    foreach (var name in FfmpegSearchNames)
                    {
                        string path = Path.Combine(projectDir, name);
                        if (File.Exists(path))
                            return path;
                    }
                }
            }
            catch { }

            string[] commonDirs =
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ffmpeg", "bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), "ffmpeg", "bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "ffmpeg", "bin"),
                Path.Combine("C:\\", "ffmpeg", "bin"),
                Path.Combine("C:\\", "Program Files", "FFmpeg", "bin"),
                Path.Combine("C:\\", "Program Files (x86)", "FFmpeg", "bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ffmpeg", "bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "ffmpeg", "bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "ffmpeg", "bin")
            };

            foreach (var dir in commonDirs)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                foreach (var name in FfmpegSearchNames)
                {
                    string path = Path.Combine(dir, name);
                    if (File.Exists(path))
                        return path;
                }
            }

            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var dir in pathEnv.Split(Path.PathSeparator))
                {
                    if (string.IsNullOrEmpty(dir)) continue;
                    foreach (var name in FfmpegSearchNames)
                    {
                        string path = Path.Combine(dir, name);
                        if (File.Exists(path))
                            return path;
                    }
                }
            }

            string targetPath = Path.Combine(currentDir, "ffmpeg.exe");
            if (TryDownloadFfmpeg(targetPath))
                return targetPath;

            return null;
        }

        private static bool TryDownloadFfmpeg(string targetPath)
        {
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string tempDir = Path.Combine(Path.GetTempPath(), "MediaConverterToMP3", "ffmpeg");
                Directory.CreateDirectory(tempDir);

                string zipPath = Path.Combine(tempDir, "ffmpeg.zip");
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync("https://github.com/BtbN/FFmpeg-Builds/releases/latest/download/ffmpeg-master-latest-win64-gpl.zip").GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    using var fileStream = File.Create(zipPath);
                    response.Content.CopyToAsync(fileStream).GetAwaiter().GetResult();
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, tempDir, true);

                string[] candidates = Directory.GetFiles(tempDir, "ffmpeg.exe", SearchOption.AllDirectories);
                if (candidates.Length == 0)
                    return false;

                string source = candidates[0];
                string sourceDir = Path.GetDirectoryName(source) ?? tempDir;
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? appDir);
                File.Copy(source, targetPath, overwrite: true);

                string? ffprobePath = Path.Combine(sourceDir, "ffprobe.exe");
                if (File.Exists(ffprobePath))
                {
                    File.Copy(ffprobePath, Path.Combine(Path.GetDirectoryName(targetPath) ?? appDir, "ffprobe.exe"), overwrite: true);
                }

                return File.Exists(targetPath) && new FileInfo(targetPath).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryDownloadYtDlp(string targetPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? AppDomain.CurrentDomain.BaseDirectory);

                using var client = new HttpClient();
                using var response = client.GetAsync("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe").GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                using var fileStream = File.Create(targetPath);
                response.Content.CopyToAsync(fileStream).GetAwaiter().GetResult();

                return File.Exists(targetPath) && new FileInfo(targetPath).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public static string? FindYtDlp()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var name in YtDlpSearchNames)
            {
                string path = Path.Combine(currentDir, name);
                if (File.Exists(path))
                    return path;
            }

            try
            {
                string? binDir1 = Directory.GetParent(currentDir)?.Parent?.FullName;
                if (!string.IsNullOrEmpty(binDir1))
                {
                    foreach (var name in YtDlpSearchNames)
                    {
                        string path = Path.Combine(binDir1, name);
                        if (File.Exists(path))
                            return path;
                    }
                }

                DirectoryInfo? current = new DirectoryInfo(currentDir);
                for (int i = 0; i < 5 && current != null; i++)
                {
                    current = current.Parent;
                    if (current != null && current.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var name in YtDlpSearchNames)
                        {
                            string path = Path.Combine(current.FullName, name);
                            if (File.Exists(path))
                                return path;
                        }
                        break;
                    }
                }
            }
            catch { }

            try
            {
                string? projectDir = Directory.GetParent(currentDir)?.Parent?.Parent?.FullName;
                if (!string.IsNullOrEmpty(projectDir))
                {
                    foreach (var name in YtDlpSearchNames)
                    {
                        string path = Path.Combine(projectDir, name);
                        if (File.Exists(path))
                            return path;
                    }
                }
            }
            catch { }

            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var dir in pathEnv.Split(Path.PathSeparator))
                {
                    if (string.IsNullOrEmpty(dir)) continue;
                    foreach (var name in YtDlpSearchNames)
                    {
                        string path = Path.Combine(dir, name);
                        if (File.Exists(path))
                            return path;
                    }
                }
            }

            string targetPath = Path.Combine(currentDir, "yt-dlp.exe");
            if (TryDownloadYtDlp(targetPath))
                return targetPath;

            return null;
        }

        public static string EscapeForArg(string value)
        {
            // Escape for Windows CreateProcess argument rules (CommandLineToArgvW):
            // backslashes preceding a quote must be doubled, all quotes escaped as \"
            var sb = new System.Text.StringBuilder();
            int slashes = 0;
            foreach (char c in value)
            {
                if (c == '\\') { slashes++; }
                else if (c == '"') { sb.Append('\\', slashes * 2 + 1); sb.Append('"'); slashes = 0; }
                else { sb.Append('\\', slashes); sb.Append(c); slashes = 0; }
            }
            sb.Append('\\', slashes * 2); // double trailing backslashes
            return sb.ToString();
        }

        public static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public static string ExtractYearFromReleaseDate(string? releaseDate, string? precision)
        {
            if (string.IsNullOrWhiteSpace(releaseDate))
                return "";

            // Spotify release_date can be in formats: "2023", "2023-06", "2023-06-15"
            // precision can be "year", "month", "day"
            if (precision == "year" || releaseDate.Length == 4)
            {
                return releaseDate.Substring(0, 4);
            }
            else if (releaseDate.Length >= 4)
            {
                return releaseDate.Substring(0, 4);
            }
            return "";
        }
    }
}

