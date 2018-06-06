using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.Analyzers
{
    public static class Diagnostics
    {
        public static readonly string MissingKeyProperty      = "DBC001";
        public static readonly string KeyPropertyTypeMismatch = "DBC002";
        public static readonly string MissingKeyField         = "DBC003";
        public static readonly string KeyFieldTypeMismatch    = "DBC004";
    }
}
