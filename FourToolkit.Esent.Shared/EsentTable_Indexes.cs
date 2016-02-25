using System.Collections.Generic;
using System.Linq;
using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentTable contains a handle to the database cursor to use for a call to the JET Api.
    /// A cursor can only be used with the session that was used to open that cursor.
    /// </summary>
    public partial class EsentTable
    {
        public Dictionary<string, EsentIndex> Indexes
        {
            get
            {
                if (!_indexesValid) RefreshIndexes();
                return _indexes;
            }
        }

        private Dictionary<string, EsentIndex> _indexes;

        private void RefreshIndexes()
        {
            CheckState();
            _indexes = Api.GetTableIndexes(Database.Session.JetId, JetId)
                .Select(i =>
                    new EsentIndex
                    {
                        Table = this,
                        Name = i.Name,
                        Primary = i.Grbit == CreateIndexGrbit.IndexPrimary,
                        Unique = i.Grbit == CreateIndexGrbit.IndexUnique
                    }
                )
                .ToDictionary(c => c.Name);
            foreach (var index in _indexes)
                Api.JetGetTableIndexInfo(Database.Session.JetId, JetId, index.Key, out index.Value.JetId, JET_IdxInfo.IndexId);
            _indexesValid = true;
        }
        
        public void CreateIndex(string name, string column, bool ascending, int density = 100, bool primary = false,
            bool unique = false)
        {
            CreateIndex(name, new Dictionary<string, bool>
            {
                {column, ascending}
            }, density, primary, unique);
        }

        public void CreateIndex(string name, Dictionary<string, bool> indexKeys, int density = 100, bool primary = false,
            bool unique = false)
        {
            CheckState();
            _indexesValid = false;
            EsentIndex.Create(this, name, indexKeys, density, primary, unique);
        }

        public void SetCurrentIndex(string name)
        {
            CheckState();
            EsentIndex.SetCurrent(this, name);
        }

        public void DropIndex(string name)
        {
            CheckState();
            _indexesValid = false;
            EsentIndex.Drop(this, name);
        }
    }
}
