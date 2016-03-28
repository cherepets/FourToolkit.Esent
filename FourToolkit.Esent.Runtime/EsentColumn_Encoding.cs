using System;
using System.Text;
using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent
{
    public partial class EsentColumn
    {
        public Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
            internal set
            {
                if (value != null && !Equals(value, Encoding.UTF8) && !Equals(value, Encoding.Unicode))
                    throw new NotSupportedException("Not supported on WinRT");
                if (value != null) JetDef.cp = JET_CP.Unicode;
            }
        }
    }
}
