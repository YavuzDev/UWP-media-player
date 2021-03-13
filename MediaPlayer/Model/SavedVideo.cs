namespace MediaPlayer.Model
{
    public class SavedVideo
    {
        public SavedVideo(string name, string path, int time, double volume)
        {
            Name = name;
            Path = path;
            Time = time;
            Volume = volume;
        }

        public string Name { get; }

        public string Path { get; }

        public int Time { get; }

        public double Volume { get; }
    }
}