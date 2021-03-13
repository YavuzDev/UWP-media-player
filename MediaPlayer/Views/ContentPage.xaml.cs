using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MediaPlayer.Extensions;
using MediaPlayer.Model;
using MediaPlayer.Storage;
using MediaPlayer.Util;
using Windows.Storage.AccessCache;

namespace MediaPlayer.Views
{
    public sealed partial class ContentPage
    {
        private static readonly ImmutableList<string> AllowedExtensions =
            ImmutableList.Create(".mp3", ".mp4", ".mkv");

        private ObservableCollection<SavedVideo> Videos { get; } = new ObservableCollection<SavedVideo>();

        public ContentPage()
        {
            InitializeComponent();
            Background = new SolidColorBrush(StaticColors.ContentBackgroundColor);
            MainPage.SelectedDirectory.CollectionChanged += ListenToSelectedDirectory;
            DataContext = this;
        }

        private async void ListenToSelectedDirectory(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Videos.Clear();
                var savedDirectory = e.NewItems[0] as SavedDirectory;
                var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(savedDirectory?.Token ?? "");
                var files = await folder.GetFilesAsync();
                foreach (var storageFile in files.Where(f => AllowedExtensions.Contains(f.FileType)))
                {
                    var contents = await StorageManager.GetFileContents(storageFile.Name);
                    if (contents.Length <= 0)
                    {
                        Videos.Add(new SavedVideo(storageFile.Name, storageFile.Path, 0, 100));
                        continue;
                    }

                    var split = contents.Split(',');
                    Videos.Add(new SavedVideo(storageFile.Name, storageFile.Path, int.Parse(split[0]),
                        double.Parse(split[1])));
                }
            }
        }

        private async void VideoListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            await Task.Delay(100, new CancellationTokenSource().Token);

            var selectedIndex = VideoListView.SelectedIndex;
            var selectedVideo = Videos[selectedIndex];
            var storageFile = await StorageFile.GetFileFromPathAsync(selectedVideo?.Path ?? "");

            var appWindow = await AppWindow.TryCreateAsync();
            var page = new Page();
            var mediaPlayerElement = new MediaPlayerElement
            {
                AutoPlay = true, AreTransportControlsEnabled = true,
                Source = MediaSource.CreateFromStorageFile(storageFile)
            };

            mediaPlayerElement.MediaPlayer.PlaybackSession.Position = new TimeSpan(0, 0, selectedVideo?.Time ?? 0);
            mediaPlayerElement.MediaPlayer.Volume = selectedVideo?.Volume ?? 100;
            page.Content = mediaPlayerElement;

            ElementCompositionPreview.SetAppWindowContent(appWindow, page);
            appWindow.Title = storageFile.Path;
            await appWindow.TryShowAsync();

            appWindow.Closed += async delegate
            {
                mediaPlayerElement.MediaPlayer.Pause();

                var time = (int) mediaPlayerElement.MediaPlayer.PlaybackSession.Position.TotalSeconds;
                var volume = mediaPlayerElement.MediaPlayer.Volume;

                await StorageManager.CreateFile(storageFile.Name, time + "," + volume);
                Videos[selectedIndex] = new SavedVideo(selectedVideo?.Name, selectedVideo?.Path, time, volume);

                mediaPlayerElement = null;
                page.Content = null;
                appWindow = null;
            };
        }

        private void VideoListView_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var selectedVideo = (e.OriginalSource as FrameworkElement)?.DataContext as SavedVideo;

            var flyout = new MenuFlyout();
            var deleteItem = new MenuFlyoutItem {Text = "Delete"};
            deleteItem.Click += async (s, eventArgs) => { await DeleteItemContextMenu_OnClick(selectedVideo); };

            flyout.Items?.Add(deleteItem);

            var senderElement = e.OriginalSource as FrameworkElement;
            flyout.ShowAt(senderElement);
        }

        private async Task DeleteItemContextMenu_OnClick(SavedVideo savedVideo)
        {
            var storageFile = await StorageFile.GetFileFromPathAsync(savedVideo.Path);
            await storageFile.DeleteAsync();
            Videos.RemoveAll(s => s.Path == savedVideo.Path);
        }
    }
}