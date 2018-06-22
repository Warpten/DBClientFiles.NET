using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Internals.Generators
{
    /// <summary>
    /// This is a helper class that is to be used in conjunction with <see cref="T"/> when decorated when <see cref="NoIndexAttribute"/>.
    /// It provides a pseudo-indexing method for <see cref="StorageDictionary{TKey,TValue}"/> and affiliates.
    /// </summary>
    /// <typeparam name="T">The record type.</typeparam>
    /// <remarks>Not yet implemented.</remarks>
    internal sealed class InstanceProxy<T>
    {
        /// <summary>
        /// A pseudo-key corresponding to the index of the entry in the file.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The actual entry object.
        /// </summary>
        public T Instance { get; set; }
    }
}
