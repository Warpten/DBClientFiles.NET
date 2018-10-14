using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Types
{
    /// <summary>
    /// This interface needs to decorate your records types if you wish to use <see cref="StorageDictionary{TValue}"/>.
    /// </summary>
    public interface IKeyType
    {
        uint ID { get; set; }
    }
}
