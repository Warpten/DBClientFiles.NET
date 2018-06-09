using System;

namespace DBClientFiles.NET.Definitions.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class CommentAttribute : Attribute
    {
        public string Comment { get; set; }

        public CommentAttribute(string comment)
        {
            Comment = comment;
        }
    }
}
