using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MediaPlayer.Extensions;
using MediaPlayer.Model;
using MediaPlayer.Storage;
using MediaPlayer.Util;

namespace MediaPlayer.Views
{
    public sealed partial class SidePage
    {
        private ObservableCollection<SavedDirectory> Folders { get; } = new ObservableCollection<SavedDirectory>();

        public SidePage()
        {
            InitializeComponent();
            Background = new SolidColorBrush(StaticColors.SideBackgroundColor);
            AddDirectory.Background = new SolidColorBrush(StaticColors.AddDirectoryButtonBackgroundColor);
            AddSymbol.Foreground = new SolidColorBrush(StaticColors.AddDirectorySymbolColor);

            DataContext = this;
            Folders.CollectionChanged += OnFoldersChanged;
            LoadSavedFolders();
        }

        private async void LoadSavedFolders()
        {
            var contents = await StorageManager.GetFileContents("directories");
            if (contents.Length <= 0)
            {
                return;
            }

            var lineSplit = contents.Split('\n');
            if (lineSplit.Length <= 0)
            {
                return;
            }

            lineSplit.ToList().Where(l => l.Contains(",")).ToList().ForEach(l =>
            {
                var split = l.Split(',');
                if (split.Length <= 0)
                {
                    return;
                }

                Folders.Add(new SavedDirectory(split[0], split[1], split[2]));
            });
        }

        private async void OnFoldersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var builder = new StringBuilder();
            Folders.ToList().ForEach(f => { builder.Append(f.Name).Append(",").Append(f.Path).Append(",").Append(f.Token).Append("\n"); });
            await StorageManager.CreateFile("directories", builder.ToString());
        }

        private async void AddDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                var savedDirectory = new SavedDirectory(folder.Name, folder.Path, "");
                if (Folders.Contains(savedDirectory))
                {
                    await Alert.SendAlert("This directory is already included");
                    return;
                }

                var token = StorageApplicationPermissions.FutureAccessList.Add(folder);
                Folders.Add(new SavedDirectory(folder.Name, folder.Path, token));
            }
            else
            {
                await Alert.SendAlert("No folder selected");
            }
        }

        private void DirectoryListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var savedDirectory = e.ClickedItem as SavedDirectory;
            MainPage.SelectedDirectory.Clear();
            MainPage.SelectedDirectory.Add(savedDirectory);
        }

        private void DirectoryListView_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var selectedDirectory = (e.OriginalSource as FrameworkElement)?.DataContext as SavedDirectory;

            var flyout = new MenuFlyout();
            var deleteItem = new MenuFlyoutItem {Text = "Delete"};
            deleteItem.Click += (s, eventArgs) => { DeleteItemContextMenu_OnClick(selectedDirectory); };

            flyout.Items?.Add(deleteItem);

            var senderElement = e.OriginalSource as FrameworkElement;
            flyout.ShowAt(senderElement);
        }

        private void DeleteItemContextMenu_OnClick(SavedDirectory savedDirectory)
        
            Folders.RemoveAll(s => s.Path.Equals(savedDirectory.Path));
        }
    }
}