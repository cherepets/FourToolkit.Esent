using System.Collections.Generic;
using System.Linq;
using System.Text;
using FourToolkit.Esent.Helpers;
using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentTable contains a handle to the database cursor to use for a call to the JET Api.
    /// A cursor can only be used with the session that was used to open that cursor.
    /// </summary>
    public partial class EsentTable
    {
        public Dictionary<string, EsentColumn> Columns
        {
            get
            {
                if (!_columnsValid) RefreshColumns();
                return _columns;
            }
        }

        private Dictionary<string, EsentColumn> _columns;

        private void RefreshColumns()
        {
            CheckState();
            _columns = Api.GetTableColumns(Database.Session.JetId, JetId)
                .Select(i =>
                    new EsentColumn
                    {
                        Table = this,
                        Name = i.Name,
                        JetId = i.Columnid,
                        JetDef = new JET_COLUMNDEF
                        {
                            coltyp = i.Coltyp,
                            cp = i.Cp,
                            cbMax = i.MaxLength,
                            grbit = i.Grbit
                        },
                        DefaultValue =
                            ValueProcessor.FromBytes(i.Coltyp.ToClr(), i.DefaultValue?.ToArray(), Encoding.UTF8)
                    }
                )
                .ToDictionary(c => c.Name);
            _columnsValid = true;
        }

        public EsentColumn AddColumn<T>(string name, Encoding encoding = null, int? max = null,
            object defaultValue = null, EsentColumn.Option option = EsentColumn.Option.None)
        {
            CheckState();
            _columnsValid = false;
            return EsentColumn.Create<T>(this, name, encoding, max, defaultValue, option);
        }

        public void DropColumn(string name)
        {
            CheckState();
            _columnsValid = false;
            EsentColumn.Drop(this, name);
        }
    }
}