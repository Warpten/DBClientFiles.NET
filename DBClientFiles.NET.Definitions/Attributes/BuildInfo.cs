namespace DBClientFiles.NET.Definitions.Attributes
{
    public struct BuildInfo
    {
        public int Version;
        public int Major;
        public int Minor;
        public int Build;

        public override string ToString()
        {
            return $"{Version}.{Major}.{Minor}.{Build}";
        }

        public BuildInfo(string str)
        {
            var tokens = str.Split('.');

            Version = int.Parse(tokens[0]);
            Major = int.Parse(tokens[1]);
            Minor = int.Parse(tokens[2]);
            Build = int.Parse(tokens[3]);
        }
    }
}