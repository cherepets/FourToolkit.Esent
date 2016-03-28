using Microsoft.Isam.Esent.Interop;
using System.IO;

namespace FourToolkit.Esent
{
    public partial class EsentInstance
    {
        public EsentInstance Open()
        {
            if (!Opened)
            {
                var folder = Path.GetDirectoryName(Name);
                Api.JetCreateInstance(out JetId, Name);
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.CreatePathIfNotExist, 1, null);
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.TempPath, 0, Path.Combine(folder, $"temp_{Name}"));
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.SystemPath, 0, Path.Combine(folder, $"system_{Name}"));
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.TempPath, 0, Path.Combine(folder, $"logs_{Name}"));
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.Recovery, 1, null);
                Api.JetSetSystemParameter(JetId, JET_SESID.Nil, JET_param.CircularLog, 1, null);
                Api.JetInit(ref JetId);
            }
            Opened = true;
            return this;
        }
    }
}
