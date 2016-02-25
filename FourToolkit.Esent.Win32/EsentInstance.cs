using System;
using System.Collections.Generic;
using Microsoft.Isam.Esent.Interop;

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
                Api.JetCreateInstance(out JetId, Name);
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
            Sessions.ForEach(d => d?.Dispose());
            Opened = false;
            Api.JetTerm(JetId);
            JetId = JET_INSTANCE.Nil;
        }

        public void Dispose() => Close();
    }
}
