using System;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Internals
{
    internal class StringMarshaller : ICustomMarshaler
    {
        public void CleanUpManagedData(object ManagedObj)
        {
            throw new NotImplementedException();
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            throw new NotImplementedException();
        }

        public int GetNativeDataSize()
        {
            throw new NotImplementedException();
        }

        [ThreadStatic]
        private string _marshalledObject;

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            if (ManagedObj == null)
                return IntPtr.Zero;

            if (!(ManagedObj is string stringObject))
                throw new MarshalDirectiveException("StringMarshaller cannot operate on non-strings!");

            _marshalledObject = stringObject;
            throw new NotImplementedException();
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            _marshalledObject = null;

            throw new NotImplementedException();
        }
    }
}
