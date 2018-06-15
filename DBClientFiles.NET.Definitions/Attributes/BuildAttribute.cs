using System;

namespace DBClientFiles.NET.Definitions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BuildAttribute : Attribute
    {
        private BuildInfo _build;

        public int Version
        {
            get => _build.Version;
            set => _build.Version = value;
        }

        public int Major
        {
            get => _build.Major;
            set => _build.Major = value;
        }

        public int Minor
        {
            get => _build.Minor;
            set => _build.Minor = value;
        }

        public int ClientBuild
        {
            get => _build.Build;
            set => _build.Build = value;
        }
        public BuildAttribute(string inputLine)
        {
            _build = new BuildInfo(inputLine);
        }

        public override string ToString()
        {
            return _build.ToString();
        }
    }
}