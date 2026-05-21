using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MediaConverterToMP3.Models;
using MediaConverterToMP3.Views.MainWindowOperations.Utilities;

namespace MediaConverterToMP3.Views
{
    public partial class MainWindow
    {
        private void UpdateSourceSelector()
        {
            // Reset all selectors to unselected state first
            void SetUnselected(System.Windows.Controls.Border selector)
            {
                selector.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828"));
                selector.Effect = null;
                if (selector.Child is System.Windows.Controls.StackPanel stackPanel && stackPanel.Children.Count > 1)
                {
                    if (stackPanel.Children[1] is System.Windows.Controls.TextBlock textBlock)
                    {
                        textBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B3B3B3"));
                    }
                }
            }

            void SetSelected(System.Windows.Controls.Border selector, System.Windows.Media.Color color, string title, string subtitle, string tooltipText, string emptyStateText)
            {
                selector.Background = new System.Windows.Media.SolidColorBrush(color);
                selector.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = color,
                    Direction = 270,
                    ShadowDepth = 3,
                    BlurRadius = 8,
                    Opacity = 0.5
                };
                if (selector.Child is System.Windows.Controls.StackPanel stackPanel && stackPanel.Children.Count > 1)
                {
                    if (stackPanel.Children[1] is System.Windows.Controls.TextBlock textBlock)
                    {
                        textBlock.Foreground = System.Windows.Media.Brushes.White;
                    }
                }
                TitleText.Text = title;
                SubtitleText.Text = subtitle;
                if (SearchTextBox.ToolTip is ToolTip tooltip)
                {
                    tooltip.Content = tooltipText;
                }
                EmptyStateText.Text = emptyStateText;
            }

            if (_selectedSource == "Spotify")
            {
                SetSelected(SpotifySelector, System.Windows.Media.Color.FromRgb(29, 185, 84),
                    "🎵 Spotify to MP3",
                    "Convert your favorite Spotify tracks to MP3",
                    "Enter a song name, artist, or paste a Spotify track/playlist URL. Plain text will search Spotify for similar songs.",
                    "🎵 Search for tracks, or paste a Spotify track/playlist URL to get started");
                SetUnselected(YouTubeSelector);
                SetUnselected(InstagramSelector);
                SetUnselected(TikTokSelector);
                FormatSelectorPanel.Visibility = Visibility.Collapsed;
            }
            else if (_selectedSource == "YouTube")
            {
                // YouTube selected - blue and red gradient
                var gradientBrush = new System.Windows.Media.LinearGradientBrush();
                gradientBrush.StartPoint = new System.Windows.Point(0, 0);
                gradientBrush.EndPoint = new System.Windows.Point(1, 0);
                gradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(
                    System.Windows.Media.Color.FromRgb(255, 0, 0), 0.0)); // Red
                gradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(
                    System.Windows.Media.Color.FromRgb(0, 0, 255), 1.0)); // Blue
                YouTubeSelector.Background = gradientBrush;
                YouTubeSelector.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromRgb(0, 0, 255),
                    Direction = 270,
                    ShadowDepth = 3,
                    BlurRadius = 8,
                    Opacity = 0.5
                };
                if (YouTubeSelector.Child is System.Windows.Controls.StackPanel ytStackPanel && ytStackPanel.Children.Count > 1)
                {
                    if (ytStackPanel.Children[1] is System.Windows.Controls.TextBlock ytTextBlock)
                    {
                        ytTextBlock.Foreground = System.Windows.Media.Brushes.White;
                    }
                }
                SetUnselected(SpotifySelector);
                SetUnselected(InstagramSelector);
                SetUnselected(TikTokSelector);
                FormatSelectorPanel.Visibility = Visibility.Visible;
                UpdateFormatSelector();
                string formatText = _selectedFormat == "MP4" ? "MP4" : "MP3";
                TitleText.Text = $"▶️ YouTube to {formatText}";
                SubtitleText.Text = $"Convert your favorite YouTube videos to {formatText}";
                if (SearchTextBox.ToolTip is ToolTip tooltip)
                {
                    tooltip.Content = "Enter a song name or paste a YouTube video URL. Plain text will search YouTube for similar videos.";
                }
                EmptyStateText.Text = "▶️ Search for videos, or paste a YouTube video URL to get started";
            }
            else if (_selectedSource == "Instagram")
            {
                // Instagram gradient: purple to pink
                var instagramGradient = new System.Windows.Media.LinearGradientBrush();
                instagramGradient.StartPoint = new System.Windows.Point(0, 0);
                instagramGradient.EndPoint = new System.Windows.Point(1, 0);
                instagramGradient.GradientStops.Add(new System.Windows.Media.GradientStop(
                    System.Windows.Media.Color.FromRgb(225, 48, 108), 0.0)); // Pink
                instagramGradient.GradientStops.Add(new System.Windows.Media.GradientStop(
                    System.Windows.Media.Color.FromRgb(131, 58, 180), 1.0)); // Purple
                InstagramSelector.Background = instagramGradient;
                InstagramSelector.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromRgb(131, 58, 180),
                    Direction = 270,
                    ShadowDepth = 3,
                    BlurRadius = 8,
                    Opacity = 0.5
                };
                if (InstagramSelector.Child is System.Windows.Controls.StackPanel igStackPanel && igStackPanel.Children.Count > 1)
                {
                    if (igStackPanel.Children[1] is System.Windows.Controls.TextBlock igTextBlock)
                    {
                        igTextBlock.Foreground = System.Windows.Media.Brushes.White;
                    }
                }
                SetUnselected(SpotifySelector);
                SetUnselected(YouTubeSelector);
                SetUnselected(TikTokSelector);
                FormatSelectorPanel.Visibility = Visibility.Collapsed;
                TitleText.Text = "📷 Instagram Reels to MP4";
                SubtitleText.Text = "Download Instagram Reels as MP4";
                if (SearchTextBox.ToolTip is ToolTip tooltip)
                {
                    tooltip.Content = "Paste an Instagram Reel URL to download as MP4.";
                }
                EmptyStateText.Text = "📷 Paste an Instagram Reel URL to get started";
            }
            else if (_selectedSource == "TikTok")
            {
                // TikTok: cyan gradient (TikTok brand color)
                var tiktokGradient = new System.Windows.Media.LinearGradientBrush();
                tiktokGradient.StartPoint = new System.Windows.Point(0, 0);
                tiktokGradient.EndPoint = new System.Windows.Point(1, 0);
                tiktokGradient.GradientStops.Add(new System.Windows.Media.GradientStop(
                    System.Windows.Media.Color.FromRgb(0, 242, 234), 0.0)); // Cyan
                tiktokGradient.GradientStops.Add(new System.Windows.Media.GradientStop(
                    System.Windows.Media.Color.FromRgb(255, 0, 80), 1.0)); // Pink/Red
                TikTokSelector.Background = tiktokGradient;
                TikTokSelector.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromRgb(0, 242, 234),
                    Direction = 270,
                    ShadowDepth = 3,
                    BlurRadius = 8,
                    Opacity = 0.5
                };
                if (TikTokSelector.Child is System.Windows.Controls.StackPanel tiktokStackPanel && tiktokStackPanel.Children.Count > 1)
                {
                    if (tiktokStackPanel.Children[1] is System.Windows.Controls.TextBlock tiktokTextBlock)
                    {
                        tiktokTextBlock.Foreground = System.Windows.Media.Brushes.White;
                    }
                }
                SetUnselected(SpotifySelector);
                SetUnselected(YouTubeSelector);
                SetUnselected(InstagramSelector);
                FormatSelectorPanel.Visibility = Visibility.Collapsed;
                TitleText.Text = "🎵 TikTok to MP4";
                SubtitleText.Text = "Download TikTok videos as MP4";
                if (SearchTextBox.ToolTip is ToolTip tooltip)
                {
                    tooltip.Content = "Paste a TikTok video URL to download as MP4.";
                }
                EmptyStateText.Text = "🎵 Paste a TikTok video URL to get started";
            }
        }

        private void UpdateFormatSelector()
        {
            if (_selectedFormat == "MP3")
            {
                // MP3 selected - green
                MP3FormatSelector.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1DB954"));
                MP3FormatSelector.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromRgb(29, 185, 84),
                    Direction = 270,
                    ShadowDepth = 2,
                    BlurRadius = 6,
                    Opacity = 0.4
                };
                ((System.Windows.Controls.TextBlock)MP3FormatSelector.Child).Foreground = System.Windows.Media.Brushes.White;

                // MP4 unselected - dark
                MP4FormatSelector.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828"));
                MP4FormatSelector.Effect = null;
                ((System.Windows.Controls.TextBlock)MP4FormatSelector.Child).Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B3B3B3"));
            }
            else // MP4
            {
                // MP4 selected - green
                MP4FormatSelector.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1DB954"));
                MP4FormatSelector.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Color.FromRgb(29, 185, 84),
                    Direction = 270,
                    ShadowDepth = 2,
                    BlurRadius = 6,
                    Opacity = 0.4
                };
                ((System.Windows.Controls.TextBlock)MP4FormatSelector.Child).Foreground = System.Windows.Media.Brushes.White;

                // MP3 unselected - dark
                MP3FormatSelector.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#282828"));
                MP3FormatSelector.Effect = null;
                ((System.Windows.Controls.TextBlock)MP3FormatSelector.Child).Foreground =
                    new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B3B3B3"));
            }

            // Refresh track status when format changes
            RefreshTrackDownloadStatus();
        }

        private void ShowErrorDialog(string title, string message)
        {
            var errorDialog = new ErrorDialog(title, message)
            {
                Owner = this
            };
            errorDialog.ShowDialog();
        }

        private void RefreshTrackDownloadStatus()
        {
            // Refresh tracks for both YouTube and Spotify sources
            foreach (var track in ResultsList.Items.OfType<TrackItem>())
            {
                // Skip tracks that are currently downloading or just downloaded in this session
                if (track.IsDownloading || track.DownloadButtonText == "Downloaded ✓")
                    continue;
                
                // Update status for all other tracks (including "Already Downloaded ✓" ones)
                CheckAndUpdateTrackDownloadStatus(track);
            }
        }
        
        private void CheckAndUpdateTrackDownloadStatus(TrackItem track)
        {
            string fileNameTitle = string.IsNullOrWhiteSpace(track.Title) ? "Unknown Title" : track.Title;
            string fileNameArtist = string.IsNullOrWhiteSpace(track.Artist) ? "Unknown Artist" : track.Artist;
            string baseFileName = $"{FileUtilities.SanitizeFileName(fileNameTitle)} - {FileUtilities.SanitizeFileName(fileNameArtist)}";
            
            // Check if file already exists first (before checking cache)
            string extension = ((_selectedSource == "YouTube" && _selectedFormat == "MP4") || _selectedSource == "Instagram" || _selectedSource == "TikTok") ? ".mp4" : ".mp3";
            string outputPath = Path.Combine(_downloadPath, $"{baseFileName}{extension}");
            bool fileExists = File.Exists(outputPath);
            
            // Check cache for partial downloads only if file doesn't exist
            if (!fileExists)
            {
                var cache = Models.DownloadCache.Load();
                var cacheEntry = cache.GetEntry(track.Id);
                // Determine expected format: Instagram and TikTok default to MP4
                string expectedFormat = _selectedFormat;
                if (_selectedSource == "Instagram" || _selectedSource == "TikTok")
                {
                    expectedFormat = "MP4";
                }
                if (cacheEntry != null && cacheEntry.Source == _selectedSource && cacheEntry.Format == expectedFormat)
                {
                    // Check if temp file still exists
                    bool tempFileExists = false;
                    if (!string.IsNullOrEmpty(cacheEntry.TempFilePattern))
                    {
                        string? dir = Path.GetDirectoryName(cacheEntry.TempFilePattern);
                        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                        {
                            string pattern = Path.GetFileName(cacheEntry.TempFilePattern) + ".*";
                            var files = Directory.GetFiles(dir, pattern);
                            tempFileExists = files.Length > 0;
                        }
                    }

                    if (tempFileExists)
                    {
                        // Only show individual Continue/Clear buttons for non-playlist downloads
                        // For playlists, Continue/Clear should be handled by Download All buttons
                        if (!_isSpotifyPlaylist)
                        {
                            // Show Continue/Clear buttons for stopped download (individual track)
                            track.HasStoppedDownload = true;
                            track.ShowContinueButton = true;
                            track.ShowClearButton = true;
                            track.DownloadButtonText = "";
                            track.CanDownload = false;
                            track.DownloadProgress = cacheEntry.Progress;
                            track.ShowProgress = true;
                        }
                        else
                        {
                            // Playlist download - don't show individual Continue/Clear buttons
                            // Show as queued/stopped, handled by Download All buttons
                            track.HasStoppedDownload = false;
                            track.ShowContinueButton = false;
                            track.ShowClearButton = false;
                            track.DownloadButtonText = "Queued...";
                            track.CanDownload = false;
                            track.DownloadProgress = cacheEntry.Progress;
                            track.ShowProgress = true;
                        }
                        
                        // Continue with other checks (Spotify button, etc.) but skip download status check
                        // Hide Spotify button for Spotify source (no need to add Spotify tracks to Spotify Local)
                        if (_selectedSource == "Spotify")
                        {
                            track.SpotifyButtonText = "";
                        }
                        // Hide Spotify button when MP4 format is selected (MP4 cannot be added to Spotify)
                        else if (_selectedSource == "YouTube" && _selectedFormat == "MP4")
                        {
                            track.SpotifyButtonText = "";
                        }
                        // Hide Spotify button for Instagram and TikTok (MP4 videos cannot be added to Spotify)
                        else if (_selectedSource == "Instagram" || _selectedSource == "TikTok")
                        {
                            track.SpotifyButtonText = "";
                        }
                        // Check Spotify button status for YouTube MP3 tracks
                        else if (!string.IsNullOrEmpty(_spotifyLocalFilesPath))
                        {
                            string spotifyFilePath = Path.Combine(_spotifyLocalFilesPath, $"{baseFileName}.mp3");
                            if (File.Exists(spotifyFilePath))
                            {
                                track.SpotifyButtonText = "Already in Spotify ✓";
                            }
                            else
                            {
                                track.SpotifyButtonText = "Add to Spotify Local";
                            }
                        }
                        else
                        {
                            track.SpotifyButtonText = "Add to Spotify Local";
                        }
                        return; // Exit early, don't check download status
                    }
                    else
                    {
                        // Temp file doesn't exist, clear cache entry
                        cache.ClearEntry(track.Id);
                    }
                }
            }
            
            // Hide Spotify button for Spotify source (no need to add Spotify tracks to Spotify Local)
            if (_selectedSource == "Spotify")
            {
                track.SpotifyButtonText = "";
            }
            // Hide Spotify button when MP4 format is selected (MP4 cannot be added to Spotify)
            else if (_selectedSource == "YouTube" && _selectedFormat == "MP4")
            {
                track.SpotifyButtonText = "";
            }
            // Hide Spotify button for Instagram and TikTok (MP4 videos cannot be added to Spotify)
            else if (_selectedSource == "Instagram" || _selectedSource == "TikTok")
            {
                track.SpotifyButtonText = "";
            }
            // Check Spotify button status for YouTube MP3 tracks
            else if (!string.IsNullOrEmpty(_spotifyLocalFilesPath))
            {
                string spotifyFilePath = Path.Combine(_spotifyLocalFilesPath, $"{baseFileName}.mp3");
                if (File.Exists(spotifyFilePath))
                {
                    track.SpotifyButtonText = "Already in Spotify ✓";
                }
                else
                {
                    // Check if MP3 exists in download folder (can be added to Spotify)
                    string mp3Path = Path.Combine(_downloadPath, $"{baseFileName}.mp3");
                    if (File.Exists(mp3Path))
                    {
                        track.SpotifyButtonText = "Add to Spotify Local";
                    }
                    else
                    {
                        track.SpotifyButtonText = "Add to Spotify Local";
                    }
                }
            }
            else
            {
                track.SpotifyButtonText = "Add to Spotify Local";
            }
            
            if (_selectedSource == "YouTube")
            {
                // For YouTube: Check both MP3 and MP4
                string mp3Path = Path.Combine(_downloadPath, $"{baseFileName}.mp3");
                string mp4Path = Path.Combine(_downloadPath, $"{baseFileName}.mp4");
                bool mp3Exists = File.Exists(mp3Path);
                bool mp4Exists = File.Exists(mp4Path);
                
                // Check if the currently selected format exists
                bool selectedFormatExists = (_selectedFormat == "MP4") ? mp4Exists : mp3Exists;
                
                // Allow download if the selected format doesn't exist (even if the other format exists)
                track.CanDownload = !selectedFormatExists;
                
                // Update button text to show which formats are available
                if (mp3Exists && mp4Exists)
                {
                    track.DownloadButtonText = "Already Downloaded ✓ (MP3+MP4)";
                }
                else if (mp3Exists)
                {
                    track.DownloadButtonText = _selectedFormat == "MP3" ? "Already Downloaded ✓ (MP3)" : "Download (MP3 exists)";
                }
                else if (mp4Exists)
                {
                    track.DownloadButtonText = _selectedFormat == "MP4" ? "Already Downloaded ✓ (MP4)" : "Download (MP4 exists)";
                }
                else
                {
                    track.DownloadButtonText = "Download";
                }
            }
            else
            {
                // For Spotify: Only check MP3
                string mp3Path = Path.Combine(_downloadPath, $"{baseFileName}.mp3");
                bool mp3Exists = File.Exists(mp3Path);
                
                track.CanDownload = !mp3Exists;
                track.DownloadButtonText = mp3Exists ? "Already Downloaded ✓" : "Download";
            }
        }

        private async Task CheckAndUpdateYtDlpAsync()
        {
            try
            {
                var settings = AppSettings.Load();
                if (settings.LastYtDlpUpdateCheck.HasValue &&
                    (DateTime.Now - settings.LastYtDlpUpdateCheck.Value).TotalHours < 24)
                    return;

                string? ytDlpPath = FileUtilities.FindYtDlp();
                if (string.IsNullOrEmpty(ytDlpPath))
                    return;

                Dispatcher.Invoke(() => StatusText.Text = "Checking for yt-dlp updates...");

                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = "-U",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process == null) return;

                var output = new System.Text.StringBuilder();
                process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                process.BeginOutputReadLine();

                await Task.Run(() => process.WaitForExit());

                settings.LastYtDlpUpdateCheck = DateTime.Now;
                settings.Save();

                string result = output.ToString();
                bool updated = result.Contains("Updated yt-dlp") || result.Contains("Updating to");
                Dispatcher.Invoke(() =>
                    StatusText.Text = updated ? "yt-dlp updated successfully." : "yt-dlp is up to date.");

                await Task.Delay(3000);
                Dispatcher.Invoke(() =>
                {
                    if (StatusText.Text == "yt-dlp updated successfully." || StatusText.Text == "yt-dlp is up to date.")
                        StatusText.Text = "Ready";
                });
            }
            catch { }
        }

        private void LoadFilterIcon()
        {
            try
            {
                if (FilterIcon == null) return;

                // Try multiple paths
                string[] possiblePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "filter.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filter.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "filter.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "filter.png")
                };

                string? filterPath = null;
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        filterPath = path;
                        break;
                    }
                }

                if (filterPath != null)
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filterPath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = 24;
                    bitmap.DecodePixelHeight = 24;
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    Dispatcher.Invoke(() =>
                    {
                        FilterIcon.Source = bitmap;
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load filter icon: {ex.Message}");
            }
        }
    }
}

