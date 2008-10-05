using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NetHunspell
{
    internal class AnsiMarshaller : IDisposable
    {

        public AnsiMarshaller(string value)
        {
            MarshalledValue = Marshal.StringToHGlobalAnsi(value);
            valor = value;
        }

        private string valor;
        public string Value { get { return valor; } }

        private IntPtr _unmanagedPrt;
        public IntPtr MarshalledValue
        {
            get { return _unmanagedPrt; }
            protected set { _unmanagedPrt = value; }
        }

    
        public static List<string> MarshalFromAnsiArray(IntPtr listAddress, int count)
        {
            List<string> strings = new List<string>();

            for (int i = 0; i < count; i++)
            {
                IntPtr stringAddress = Marshal.ReadIntPtr(listAddress, i * IntPtr.Size);
                string s = Marshal.PtrToStringAnsi(stringAddress);
                strings.Add(s);

            }
            return strings;

        }

        ~AnsiMarshaller()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;
        private void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            if (disposing)
            {
                //dump managed resources
            }

            Marshal.FreeHGlobal(MarshalledValue);
        }
    }
}
