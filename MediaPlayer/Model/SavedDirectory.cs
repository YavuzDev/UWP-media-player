using System.Collections.Generic;

namespace MediaPlayer.Model
{
    public class SavedDirectory
    {
        public SavedDirectory(string name, string path, string token)
        {
            Name = name;
            Path = path;
            Token = token;
        }

        public string Name { get; }

        public string Path { get; }

        public string Token { get; }

        public override bool Equals(object obj)
        {
            return obj is SavedDirectory directory &&
                   Name == directory.Name &&
                   Path == directory.Path &&
                   Token == directory.Token;
        }

        public override int GetHashCode()
        {
            int hashCode = 1346707966;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Token);
            return hashCode;
        }
    }
}