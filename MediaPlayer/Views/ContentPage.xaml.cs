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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using MediaPlayer.Model;
using MediaPlayer.Storage;
using MediaPlayer.Util;

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
                var folder = await StorageFolder.GetFolderFromPathAsync(savedDirectory?.Path ?? "");
                var files = await folder.GetFilesAsync();
                foreach (var storageFile in files.Where(f => AllowedExtensions.Contains(f.FileType)))
                {
                    var contents = await StorageManager.GetFileContents(storageFile.Name);
                    if (contents.Length <= 0)
                    {
                        Videos.Add(new SavedVideo(storageFile.Name, storageFile.Path, 0));
                        continue;
                    }

                    Videos.Add(new SavedVideo(storageFile.Name, storageFile.Path, int.Parse(contents)));
                }
            }
        }

        private async void VideoListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            await Task.Delay(200, new CancellationTokenSource().Token);

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
            page.Content = mediaPlayerElement;

            ElementCompositionPreview.SetAppWindowContent(appWindow, page);
            appWindow.Title = storageFile.Path;
            await appWindow.TryShowAsync();

            appWindow.Closed += async delegate
            {
                mediaPlayerElement.MediaPlayer.Pause();

                var time = (int) mediaPlayerElement.MediaPlayer.PlaybackSession.Position.TotalSeconds;

                await StorageManager.CreateFile(storageFile.Name, time.ToString());
                Videos[selectedIndex] = new SavedVideo(selectedVideo?.Name, selectedVideo?.Path, time);

                mediaPlayerElement = null;
                page.Content = null;
                appWindow = null;
            };
        }
    }
}
