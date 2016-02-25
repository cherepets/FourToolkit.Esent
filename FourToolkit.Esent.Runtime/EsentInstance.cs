using FourToolkit.Esent.Extensions;
using Microsoft.Isam.Esent.Interop;
using System;
using System.Collections.Generic;
using System.IO;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentInstance contains a handle to the instance of the database to use for calls to the JET Api.
    /// </summary>
    public class EsentInstance : IDisposable
    {
        internal JET_INSTANCE JetId;

        internal List<EsentSession> Sessions = new List<EsentSession>();

        public string Name { get; }

        public bool Opened { get; private set; }
        internal bool Valid => Opened;

        public EsentInstance(string name)
        {
            Name = name;
            Open();
        }

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

        public EsentSession BeginSession()
        {
            var session = EsentSession.Begin(this);
            Sessions.Add(session);
            return session;
        }

        public void Close()
        {
            if (!Opened || !Valid) return;
            Sessions.Reverse();
            Sessions.ForEach(s => s?.Dispose());
            Opened = false;
            Api.JetTerm(JetId);
            JetId = JET_INSTANCE.Nil;
        }

        public void Dispose() => Close();
    }
}
