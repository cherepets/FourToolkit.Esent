using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent
{
    public class EsentIndex
    {
        internal JET_INDEXID JetId;

        public EsentTable Table { get; internal set; }
        public string Name { get; internal set; }
        public Dictionary<string, bool> IndexKeys { get; internal set; }
        public bool Primary { get; internal set; }
        public bool Unique { get; internal set; }

        internal EsentIndex() { }

        internal static EsentIndex Create(EsentTable table, string name, Dictionary<string, bool> indexKeys, int density,
            bool primary, bool unique)
        {
            var index = new EsentIndex
            {
                Table = table,
                Name = name,
                IndexKeys = indexKeys,
                Primary = primary,
                Unique = unique
            };
            var count = indexKeys.Count;
            if (count < 1) throw new InvalidOperationException("Index needs at least one column!");
            var key = string.Empty;
            for (var i = 0; i < count; i++)
            {
                var c = indexKeys.Keys.ToList()[i];
                key += indexKeys[c] ? '+' : '-';
                key += $"{c}\0";
            }
            key += '\0';
            var option =
                (primary ? CreateIndexGrbit.IndexPrimary : CreateIndexGrbit.None) |
                (unique ? CreateIndexGrbit.IndexUnique : CreateIndexGrbit.None);
            Api.JetCreateIndex(table.Database.Session.JetId, table.JetId, name, option, key, key.Length, density);
            return index;
        }

        internal static void SetCurrent(EsentTable table, string name)
        {
            Api.JetSetCurrentIndex(table.Database.Session.JetId, table.JetId, name);
        }

        internal static void Drop(EsentTable table, string name)
        {
            Api.JetDeleteIndex(table.Database.Session.JetId, table.JetId, name);
        }

        public void SetCurrent() => SetCurrent(Table, Name);
        public void Drop() => Drop(Table, Name);
    }
}
