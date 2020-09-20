namespace MediaPlayer.Model
{
    public class SavedVideo
    {
        public SavedVideo(string name, string path, int time)
        {
            Name = name;
            Path = path;
            Time = time;
        }

        public string Name { get; }

        public string Path { get; }

        public int Time { get; }
    }
}