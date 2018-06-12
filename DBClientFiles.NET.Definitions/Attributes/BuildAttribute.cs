using System;

namespace DBClientFiles.NET.Definitions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BuildAttribute : Attribute
    {
        public BuildInfo Build { get; set; }

        public BuildAttribute(int v, int a, int i, int b)
        {
            Build = new BuildInfo()
            {
                Version = v,
                Major = a,
                Minor = i,
                Build = b
            };
        }

        public BuildAttribute(BuildInfo b)
        {
            Build = new BuildInfo()
            {
                Version = b.Version,
                Major = b.Major,
                Minor = b.Minor,
                Build = b.Build
            };
        }

        public BuildAttribute(string inputLine)
        {
            var tokens = inputLine.Split('.');

            Build = new BuildInfo()
            {
                Version = int.Parse(tokens[0]),
                Major   = int.Parse(tokens[1]),
                Minor   = int.Parse(tokens[2]),
                Build   = int.Parse(tokens[3]),
            };
        }
    }
}