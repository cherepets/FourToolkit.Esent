using FourToolkit.Esent.Extensions;
using Microsoft.Isam.Esent.Interop;
using System;
using System.Collections.Generic;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentInstance contains a handle to the instance of the database to use for calls to the JET Api.
    /// </summary>
    public partial class EsentInstance : IDisposable
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
