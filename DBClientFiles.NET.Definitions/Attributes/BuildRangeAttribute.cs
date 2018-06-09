using System;

namespace DBClientFiles.NET.Definitions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BuildRangeAttribute : Attribute
    {
        public BuildInfo From { get; set; }
        public BuildInfo To { get; set; }

        public BuildRangeAttribute(string inputLine)
        {
            var splits = inputLine.Split('-');
            var tokens = splits[0].Trim().Split('.');

            From = new BuildInfo
            {
                Version = int.Parse(tokens[0]),
                Major = int.Parse(tokens[1]),
                Minor = int.Parse(tokens[2]),
                Build = int.Parse(tokens[3]),
            };

            tokens = splits[1].Trim().Split('.');

            To = new BuildInfo
            {
                Version = int.Parse(tokens[0]),
                Major = int.Parse(tokens[1]),
                Minor = int.Parse(tokens[2]),
                Build = int.Parse(tokens[3]),
            };
        }
    }
}