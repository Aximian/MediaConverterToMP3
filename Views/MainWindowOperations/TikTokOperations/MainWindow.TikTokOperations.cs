using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MediaConverterToMP3.Models;
using MediaConverterToMP3.Views.MainWindowOperations.Utilities;
using Newtonsoft.Json;

namespace MediaConverterToMP3.Views
{
    public partial class MainWindow
    {
        private async Task LoadTikTokVideo(string videoUrl)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Loading TikTok video...";
            });

            // Run the process on a background thread to avoid blocking UI
            await Task.Run(() =>
            {
                // Use yt-dlp to get video info
                string? ytDlpPath = FileUtilities.FindYtDlp();
                if (string.IsNullOrEmpty(ytDlpPath))
                {
                    throw new Exception("yt-dlp not found. Please place yt-dlp.exe in the application directory.");
                }

                // Get video information
                string infoArgs = $"--dump-json --no-playlist --no-warnings --quiet \"{videoUrl}\"";
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = infoArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                System.Diagnostics.Process? process = null;
                try
                {
                    process = System.Diagnostics.Process.Start(processInfo);
                    if (process == null)
                    {
                        throw new Exception("Failed to start yt-dlp process.");
                    }

                    var outputBuilder = new System.Text.StringBuilder();
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);
                        }
                    };

                    process.BeginOutputReadLine();

                    // Wait with timeout (30 seconds)
                    bool exited = process.WaitForExit(30000);
                    if (!exited)
                    {
                        try { process.Kill(); } catch { }
                        throw new Exception("Loading video timed out after 30 seconds.");
                    }

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Failed to get video information. Please check if the URL is valid.");
                    }

                    string jsonOutput = outputBuilder.ToString();
                    if (string.IsNullOrWhiteSpace(jsonOutput))
                    {
                        throw new Exception("Failed to get video information. The video may be unavailable.");
                    }

                    try
                    {
                        var videoInfo = JsonConvert.DeserializeObject<dynamic>(jsonOutput);
                        string title = videoInfo?.title?.ToString() ?? videoInfo?.description?.ToString() ?? "TikTok Video";
                        string uploader = videoInfo?.uploader?.ToString() ?? videoInfo?.uploader_id?.ToString() ?? "TikTok User";
                        string? thumbnail = videoInfo?.thumbnail?.ToString();
                        double? duration = videoInfo?.duration != null ? (double?)videoInfo.duration : null;

                        // Store the original URL in Id for downloading
                        // Extract ID from URL for display purposes
                        string videoId = videoInfo?.id?.ToString() ?? "";
                        if (string.IsNullOrEmpty(videoId))
                        {
                            // Try to extract from URL
                            videoId = UrlParser.ExtractTikTokVideoId(videoUrl) ?? Guid.NewGuid().ToString();
                        }

                        var trackItem = new TrackItem
                        {
                            Id = videoUrl, // Store URL for downloading
                            Title = title,
                            Artist = uploader,
                            Album = "TikTok Videos",
                            AlbumArtist = uploader,
                            Year = "",
                            Genre = "",
                            Duration = duration.HasValue ? TimeSpan.FromSeconds(duration.Value) : TimeSpan.Zero,
                            ImageUrl = thumbnail,
                            CanDownload = true,
                            DownloadButtonText = "Download"
                        };
                        
                        // Check download status
                        CheckAndUpdateTrackDownloadStatus(trackItem);

                        // Update UI on the main thread
                        Dispatcher.Invoke(() =>
                        {
                            _allTracks.Clear();
                            _tracks.Clear();
                            _allTracks.Add(trackItem);
                            _tracks.Add(trackItem);

                            StatusText.Text = $"Loaded video: {title}";
                            FilterTextBox.Visibility = Visibility.Visible;
                            DownloadAllButton.Visibility = Visibility.Collapsed; // No download all for single videos
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to parse video information: {ex.Message}");
                    }
                }
                finally
                {
                    // Ensure process is disposed
                    try
                    {
                        process?.Dispose();
                    }
                    catch { }
                }
            });
        }
    }
}

