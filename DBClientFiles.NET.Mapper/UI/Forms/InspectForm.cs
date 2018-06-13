using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Collections;
using DBClientFiles.NET.Collections.Generic;

namespace DBClientFiles.NET.Mapper.UI.Forms
{
    public partial class InspectForm : Form
    {
        public Type RecordType { get; set; }
        public Stream Stream { get; set; }

        public InspectForm()
        {
            InitializeComponent();
        }


        private void OnLoad(object sender, EventArgs e)
        {
            var storageEnumerable = typeof(StorageEnumerable<>).MakeGenericType(RecordType);

            foreach (var propInfo in RecordType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!propInfo.PropertyType.IsArray)
                {
                    var colGen = new OLVColumn
                    {
                        AspectName = propInfo.Name,
                        Text = propInfo.Name
                    };

                    if (propInfo.IsDefined(typeof(IndexAttribute), false))
                        fastObjectListView1.Columns.Insert(0, colGen);
                    else
                        fastObjectListView1.Columns.Add(colGen);
                }
                else
                {
                    var arity = propInfo.GetCustomAttribute<CardinalityAttribute>();
                    for (var i = 0; i < arity.SizeConst; ++i)
                    {
                        OLVColumn colGen = null;
                        colGen = new OLVColumn
                        {
                            AspectGetter = (o) => ((Array) o).GetValue((int)colGen.Tag),
                            Text = $"{propInfo.Name}[{i}]",
                            Tag = i,
                        };

                        fastObjectListView1.Columns.Add(colGen);
                    }
                }
            }

            var options = new StorageOptions()
            {
                CopyToMemory = false,
                LoadMask = LoadMask.Records,
                InternStrings = true,
                MemberType = MemberTypes.Property,
                OverrideSignedChecks = true
            };

            Stream.Position = 0;
            var enumerable = (IEnumerable) Activator.CreateInstance(storageEnumerable, Stream, options);
            fastObjectListView1.Objects = enumerable;
        }
    }
}
