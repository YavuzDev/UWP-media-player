using System.Collections.ObjectModel;
using MediaPlayer.Model;

namespace MediaPlayer
{
    public sealed partial class MainPage
    {
        public static ObservableCollection<SavedDirectory> SelectedDirectory { get; } =
            new ObservableCollection<SavedDirectory>();

        public MainPage()
        {
            InitializeComponent();
        }
    }
}