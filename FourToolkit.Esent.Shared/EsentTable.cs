using System;
using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentTable contains a handle to the database cursor to use for a call to the JET Api.
    /// A cursor can only be used with the session that was used to open that cursor.
    /// </summary>
    public partial class EsentTable : IDisposable
    {
        internal JET_TABLEID JetId;

        public EsentDatabase Database { get; internal set; }
        public string Name { get; internal set; }

        public bool Opened { get; private set; }
        internal bool Valid => Database.Valid;

        private bool _columnsValid;
        private bool _indexesValid;

        internal EsentTable()
        {
        }

        internal static EsentTable Create(EsentDatabase database, string name, int pages, int density)
        {
            var table = new EsentTable
            {
                Database = database,
                Name = name
            };
            Api.JetCreateTable(database.Session.JetId, database.JetId, name, pages, density, out table.JetId);
            table.Opened = true;
            return table;
        }

        internal static void Drop(EsentDatabase database, string name)
        {
            Api.JetDeleteTable(database.Session.JetId, database.JetId, name);
        }

        public void Drop() => Drop(Database, Name);

        internal static EsentTable Open(EsentDatabase database, string name)
        {
            return new EsentTable
            {
                Database = database,
                Name = name
            }.Open();
        }

        internal static bool TryOpen(EsentDatabase database, string name, out EsentTable table)
        {
            table = new EsentTable
            {
                Database = database,
                Name = name
            };
            return table.TryOpen(out table);
        }

        public EsentTable Open()
        {
            _columnsValid = false;
            _indexesValid = false;
            if (!Opened)
                Api.JetOpenTable(Database.Session.JetId, Database.JetId, Name, null, 0, OpenTableGrbit.None, out JetId);
            Opened = true;
            return this;
        }

        public bool TryOpen(out EsentTable table)
        {
            _columnsValid = false;
            _indexesValid = false;
            if (!Opened)
                Opened = Api.TryOpenTable(Database.Session.JetId, Database.JetId, Name, OpenTableGrbit.None, out JetId);
            table = Opened ? this : null;
            return Opened;
        }

        private void CheckState()
        {
            if (!Valid) throw new InvalidOperationException($"Table '{Name}' is not valid!");
            if (!Opened) throw new InvalidOperationException($"Table '{Name}' is closed!");
        }

        public void Close()
        {
            if (!Opened || !Valid) return;
            Opened = false;
            Api.JetCloseTable(Database.Session.JetId, JetId);
            JetId = JET_TABLEID.Nil;
        }

        public void Dispose() => Close();
    }
}
