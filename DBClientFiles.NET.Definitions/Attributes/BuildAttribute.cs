using System;

namespace DBClientFiles.NET.Definitions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BuildAttribute : Attribute
    {
        public BuildInfo Build { get; set; } = new BuildInfo();

        public int Version
        {
            get => Build.Version;
            set => Build.Version = value;
        }

        public int Major
        {
            get => Build.Major;
            set => Build.Major = value;
        }

        public int Minor
        {
            get => Build.Minor;
            set => Build.Minor = value;
        }

        public int ClientBuild
        {
            get => Build.Build;
            set => Build.Build = value;
        }

        public BuildAttribute(int v, int a, int i, int b)
        {
            Build.Version = v;
            Build.Major = a;
            Build.Minor = i;
            Build.Build = b;
        }

        public BuildAttribute(BuildInfo b)
        {
            Build.Version = b.Version;
            Build.Major = b.Major;
            Build.Minor = b.Minor;
            Build.Build = b.Build;
        }

        public BuildAttribute(string inputLine)
        {
            var tokens = inputLine.Split('.');

            Build.Version = int.Parse(tokens[0]);
            Build.Major = int.Parse(tokens[1]);
            Build.Minor = int.Parse(tokens[2]);
            Build.Build = int.Parse(tokens[3]);
        }
    }
}