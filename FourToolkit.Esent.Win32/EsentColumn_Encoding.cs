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
                return JetDef.cp == JET_CP.Unicode
                       ? Encoding.UTF8
                       : JetDef.cp == JET_CP.ASCII
                           ? Encoding.ASCII
                           : Encoding.Default;
            }
            internal set
            {
                if (value != null)
                    JetDef.cp = Equals(value, Encoding.UTF8) ||
                    Equals(value, Encoding.Unicode)
                                            ? JET_CP.Unicode
                                            : (Equals(value, Encoding.ASCII)
                                                ? JET_CP.ASCII
                                                : JET_CP.None);
            }
        }
    }
}
