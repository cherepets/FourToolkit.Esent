using System;
using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentTransaction causes a session to enter a transaction or create a new save point in an existing
    /// transaction.
    /// </summary>
    public class EsentTransaction : IDisposable
    {
        public bool Rollback { get; set; } = false;
        public EsentSession Session { get; private set; }

        public bool Opened { get; private set; }
        internal bool Valid => Session.Valid;

        internal EsentTransaction() { }

        internal static EsentTransaction Begin(EsentSession session)
        {
            return new EsentTransaction
            {
                Session = session
            }.Begin();
        }

        public EsentTransaction Begin()
        {
            if (!Opened)
                Api.JetBeginTransaction(Session.JetId);
            Opened = true;
            return this;
        }

        public void End()
        {
            if (!Opened || !Valid) return;
            Opened = false;
            if (Rollback)
                Api.JetRollback(Session.JetId, RollbackTransactionGrbit.None);
            else
                Api.JetCommitTransaction(Session.JetId, CommitTransactionGrbit.LazyFlush);
        }

        public void Dispose() => End();
    }
}
