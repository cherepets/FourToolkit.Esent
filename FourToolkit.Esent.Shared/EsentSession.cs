using FourToolkit.Esent.Extensions;
using Microsoft.Isam.Esent.Interop;
using System;
using System.Collections.Generic;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentSession contains a handle to the session to use for calls to the JET Api.
    /// </summary>
    public class EsentSession : IDisposable
    {
        internal JET_SESID JetId;

        internal List<IDisposable> Holding = new List<IDisposable>();

        public EsentInstance Instance { get; private set; }

        public bool Opened { get; private set; }
        internal bool Valid => Instance.Valid;

        internal EsentSession() { }

        internal static EsentSession Begin(EsentInstance instance)
        {
            return new EsentSession
            {
                Instance = instance
            }.Begin();
        }

        public EsentSession Begin()
        {
            if (!Opened)
                Api.JetBeginSession(Instance.JetId, out JetId, null, null);
            Opened = true;
            return this;
        }

        public EsentDatabase CreateDatabase(string path, bool overwrite)
        {
            CheckState();
            var db = EsentDatabase.Create(this, path, overwrite);
            Holding.Add(db);
            return db;
        }

        public EsentDatabase OpenDatabase(string path)
        {
            CheckState();
            var db = EsentDatabase.Open(this, path);
            Holding.Add(db);
            return db;
        }

        public EsentTransaction BeginTransaction()
        {
            CheckState();
            var transaction = EsentTransaction.Begin(this);
            Holding.Add(transaction);
            return transaction;
        }

        private void CheckState()
        {
            if (!Valid) throw new InvalidOperationException("Session is not valid!");
            if (!Opened) throw new InvalidOperationException("Session is closed!");
        }

        public void End()
        {
            if (!Opened || !Valid) return;
            Holding.Reverse();
            Holding.ForEach(d => d?.Dispose());
            Opened = false;
            Api.JetEndSession(JetId, EndSessionGrbit.None);
            JetId = JET_SESID.Nil;
        }

        public void Dispose() => End();
    }
}
