using System;
using System.Text;
using FourToolkit.Esent.Helpers;
using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent
{
    public class EsentColumn
    {
        internal JET_COLUMNDEF JetDef = new JET_COLUMNDEF();
        internal JET_COLUMNID JetId;

        public EsentTable Table { get; internal set; }
        public string Name { get; internal set; }
        public object DefaultValue { get; internal set; }

        public Type ColumnType
        {
            get { return JetDef.coltyp.ToClr(); }
            internal set { JetDef.coltyp = value.ToJet(); }
        }

        public Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
            internal set
            {
                if (value != null)
                    JetDef.cp = JET_CP.Unicode;
                if (!Equals(value, Encoding.UTF8) && !Equals(value, Encoding.Unicode))
                    throw new NotSupportedException("Not supported on WinRT");
            }
        }

        public int? Max
        {
            get { return JetDef.cbMax; }
            internal set { if (value.HasValue) JetDef.cbMax = value.Value; }
        }

        public Option Options
        {
            get { return (Option)JetDef.grbit; }
            internal set { JetDef.grbit = (ColumndefGrbit)value; }
        }

        internal EsentColumn() { }

        internal static EsentColumn Create<T>(EsentTable table, string name, Encoding encoding, int? max, object defaultValue, Option option)
        {
            var columnType = typeof (T);
            var column = new EsentColumn
            {
                Table = table,
                Name = name,
                ColumnType = columnType,
                Encoding = encoding,
                Max = max,
                DefaultValue = defaultValue,
                JetDef = {grbit = (ColumndefGrbit) option}
            };
            var defaultValueBytes = ValueProcessor.GetBytes(defaultValue, encoding);
            var defaultValueSize = defaultValueBytes?.Length ?? 0;
            Api.JetAddColumn(table.Database.Session.JetId, table.JetId, name, column.JetDef, defaultValueBytes, defaultValueSize, out column.JetId);
            return column;
        }

        internal static void Drop(EsentTable table, string name)
        {
            Api.JetDeleteColumn(table.Database.Session.JetId, table.JetId, name);
        }

        public void Drop() => Drop(Table, Name);

        [Flags]
        public enum Option
        {
            None = 0,
            Fixed = 1,
            Tagged = 2,
            NotNull = 4,
            Version = 8,
            Autoincrement = 16,
            Updatable = 32,
            MultiValued = 1024,
            EscrowUpdate = 2048,
            Unversioned = 4096,
            MaybeNull = 8192,
            Finalize = 16384,
            UserDefinedDefault = 32768,
            Key = 64,
            Descending = 128
        }
    }
}
