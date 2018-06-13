using System;

namespace DBClientFiles.NET.Definitions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BuildRangeAttribute : Attribute
    {
        public BuildInfo From;
        public BuildInfo To;

        public BuildRangeAttribute(string inputLine)
        {
            var splits = inputLine.Split('-');
            var tokens = splits[0].Trim().Split('.');

            From = new BuildInfo(tokens[0]);
            To = new BuildInfo(tokens[1]);
        }

        public override string ToString()
        {
            return $"{From}-{To}";
        }
    }
}