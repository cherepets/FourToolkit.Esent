using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent
{
    public partial class EsentInstance
    {
        public EsentInstance Open()
        {
            if (!Opened)
            {
                Api.JetCreateInstance(out JetId, Name);
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.CircularLog, 1, null);
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.Recovery, 1, null);
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.CircularLog, 1, null);
                Api.JetInit(ref JetId);
            }
            Opened = true;
            return this;
        }
    }
}
