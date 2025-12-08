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
        private async Task LoadInstagramReel(string reelUrl)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Loading Instagram Reel...";
            });

            // Run the process on a background thread to avoid blocking UI
            await Task.Run(() =>
            {
                // Use yt-dlp to get reel info
                string? ytDlpPath = FileUtilities.FindYtDlp();
                if (string.IsNullOrEmpty(ytDlpPath))
                {
                    throw new Exception("yt-dlp not found. Please place yt-dlp.exe in the application directory.");
                }

                // Get reel information
                string infoArgs = $"--dump-json --no-playlist --no-warnings --quiet \"{reelUrl}\"";
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
                        throw new Exception("Loading reel timed out after 30 seconds.");
                    }

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Failed to get reel information. Please check if the URL is valid.");
                    }

                    string jsonOutput = outputBuilder.ToString();
                    if (string.IsNullOrWhiteSpace(jsonOutput))
                    {
                        throw new Exception("Failed to get reel information. The reel may be unavailable.");
                    }

                    try
                    {
                        var reelInfo = JsonConvert.DeserializeObject<dynamic>(jsonOutput);
                        string title = reelInfo?.title?.ToString() ?? reelInfo?.description?.ToString() ?? "Instagram Reel";
                        string uploader = reelInfo?.uploader?.ToString() ?? reelInfo?.uploader_id?.ToString() ?? "Instagram User";
                        string? thumbnail = reelInfo?.thumbnail?.ToString();
                        double? duration = reelInfo?.duration != null ? (double?)reelInfo.duration : null;

                        // Store the original URL in Id for downloading
                        // Extract ID from URL for display purposes
                        string reelId = reelInfo?.id?.ToString() ?? "";
                        if (string.IsNullOrEmpty(reelId))
                        {
                            // Try to extract from URL
                            reelId = UrlParser.ExtractInstagramReelId(reelUrl) ?? Guid.NewGuid().ToString();
                        }

                        var trackItem = new TrackItem
                        {
                            Id = reelUrl, // Store URL for downloading
                            Title = title,
                            Artist = uploader,
                            Album = "Instagram Reels",
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

                            StatusText.Text = $"Loaded reel: {title}";
                            FilterTextBox.Visibility = Visibility.Visible;
                            DownloadAllButton.Visibility = Visibility.Collapsed; // No download all for single reels
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to parse reel information: {ex.Message}");
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

