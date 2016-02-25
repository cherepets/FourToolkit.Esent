using FourToolkit.Esent.Extensions;
using Microsoft.Isam.Esent.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentDatabase contains the handle to the database. A database handle is used to manage the
    /// schema of a database. It can also be used to manage the tables inside of that database.
    /// </summary>
    public class EsentDatabase : IDisposable
    {
        internal JET_DBID JetId;

        internal List<IDisposable> Holding = new List<IDisposable>();

        public EsentSession Session { get; internal set; }
        public string FilePath { get; internal set; }
        public string Name => Path.GetFileNameWithoutExtension(FilePath);

        public bool Opened { get; private set; }
        internal bool Valid => Session.Valid;

        private bool _tablesValid;

        internal EsentDatabase() { }

        internal static EsentDatabase Create(EsentSession session, string path, bool overwrite)
        {
            var db = new EsentDatabase
            {
                Session = session,
                FilePath = path
            };
            Api.JetCreateDatabase(session.JetId, path, null, out db.JetId,
                overwrite ? CreateDatabaseGrbit.OverwriteExisting : CreateDatabaseGrbit.None);
            db.Opened = true;
            return db;
        }

        internal static EsentDatabase Open(EsentSession session, string path)
        {
            return new EsentDatabase
            {
                Session = session,
                FilePath = path
            }.Open();
        }

        public EsentDatabase Open()
        {
            _tablesValid = false;
            if (!Opened)
            {
                Api.JetAttachDatabase(Session.JetId, FilePath, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(Session.JetId, FilePath, null, out JetId, OpenDatabaseGrbit.None);
            }
            Opened = true;
            return this;
        }

        public Dictionary<string, EsentTable> Tables
        {
            get
            {
                if (!_tablesValid) RefreshTables();
                return _tables;
            }
        }

        private void RefreshTables()
        {
            CheckState();
            _tables = Api.GetTableNames(Session.JetId, JetId)
                .Select(i =>
                    new EsentTable
                    {
                        Database = this,
                        Name = i
                    }
                )
                .ToDictionary(t => t.Name);
            _tablesValid = true;
        }

        private Dictionary<string, EsentTable> _tables;

        public EsentTable CreateTable(string name, int pages, int density = 100)
        {
            CheckState();
            _tablesValid = false;
            var table = EsentTable.Create(this, name, pages, density);
            Holding.Add(table);
            return table;
        }

        public EsentTable OpenTable(string name)
        {
            CheckState();
            _tablesValid = false;
            var table = EsentTable.Open(this, name);
            Holding.Add(table);
            return table;
        }

        public bool TryOpenTable(string name, out EsentTable table)
        {
            CheckState();
            _tablesValid = false;
            var success = EsentTable.TryOpen(this, name, out table);
            if (success) Holding.Add(table);
            return success;
        }

        public void DropTable(string name)
        {
            CheckState();
            _tablesValid = false;
            EsentTable.Drop(this, name);
        }

        private void CheckState()
        {
            if (!Valid) throw new InvalidOperationException("Database is not valid!");
            if (!Opened) throw new InvalidOperationException("Database is closed!");
        }

        public void Close()
        {
            if (!Opened || !Valid) return;
            Holding.Reverse();
            Holding.ForEach(d => d?.Dispose());
            Opened = false;
            Api.JetCloseDatabase(Session.JetId, JetId, CloseDatabaseGrbit.None);
            Api.JetDetachDatabase(Session.JetId, FilePath);
            JetId = JET_DBID.Nil;
        }

        public void Dispose() => Close();
    }
}
