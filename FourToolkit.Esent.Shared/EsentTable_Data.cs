using System;
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
        public int Count
        {
            get
            {
                int count;
                var success = Api.TryMove(Database.Session.JetId, JetId, JET_Move.First, MoveGrbit.None);
                if (!success) return 0;
                Api.JetIndexRecordCount(Database.Session.JetId, JetId, out count, int.MaxValue);
                return count;
            }
        }

        #region Insert
        public void Insert(params EsentCell[] columns)
        {
            CheckState();
            using (Database.Session.BeginTransaction())
            {
                Api.JetPrepareUpdate(Database.Session.JetId, JetId, JET_prep.Insert);
                foreach (var column in columns)
                {
                    var encoding = column.Encoding ?? Columns[column.ColumnName].Encoding;
                    var value = ValueProcessor.GetBytes(column.Value, encoding);
                    if (value != null)
                        Api.SetColumn(Database.Session.JetId, JetId, Columns[column.ColumnName].JetId, value);
                }
                Api.JetUpdate(Database.Session.JetId, JetId);
            }
        }

        public void Insert(string columnName, object value)
            => Insert(new EsentCell(columnName, value));
        #endregion

        #region Select
        private EsentRow RetrieveColumns(EsentCell[] columnArray)
        {
            var cells = new EsentRow();
            foreach (var column in columnArray)
            {
                var columnName = column.ColumnName;
                var encoding = column.Encoding ?? Columns[column.ColumnName].Encoding;
                var type = Columns[columnName].ColumnType;
                var bytes = Api.RetrieveColumn(Database.Session.JetId, JetId, Columns[columnName].JetId);
                var value = ValueProcessor.FromBytes(type, bytes, encoding);
                cells.Add(new EsentCell(columnName, value, encoding));
            }
            return cells;
        }

        public EsentRow Select(IEnumerable<EsentCell> columns, object key, Encoding keyEncoding = null)
        {
            CheckState();
            var columnArray = columns as EsentCell[] ?? columns.ToArray();
            var byteKey = ValueProcessor.GetBytes(key, keyEncoding);
            Api.MakeKey(Database.Session.JetId, JetId, byteKey, MakeKeyGrbit.NewKey);
            var success = Api.TrySeek(Database.Session.JetId, JetId, SeekGrbit.SeekEQ);
            if (!success) return null;
            return RetrieveColumns(columnArray);
        }

        public EsentRow SelectRow(object key, Encoding keyEncoding = null)
        {
            CheckState();
            var columns = Columns.Select(c => new EsentCell(c.Value.Name, null, c.Value.Encoding));
            return Select(columns, key, keyEncoding);
        }

        public List<EsentRow> SelectFirst(IEnumerable<EsentCell> columns, int count, int skip = 0)
        {
            CheckState();
            var columnArray = columns as EsentCell[] ?? columns.ToArray();
            if (count < 1) return null;
            var success = Api.TryMove(Database.Session.JetId, JetId, JET_Move.First, MoveGrbit.None);
            if (!success) return null;
            for (var i = 0; i < skip; i++)
            {
                if (!Api.TryMove(Database.Session.JetId, JetId, JET_Move.Next, MoveGrbit.None))
                    return null;
            }
            var rows = new List<EsentRow>();
            for (var i = 0; i < count; i++)
            {
                var cells = RetrieveColumns(columnArray);
                rows.Add(cells);
                if (!Api.TryMove(Database.Session.JetId, JetId, JET_Move.Next, MoveGrbit.None))
                    break;
            }
            return rows;
        }

        public List<EsentCell> SelectFirst(string columnName, int count, int skip = 0) =>
            SelectFirst(new[] { new EsentCell(columnName) }, count, skip)
                ?.Select(r => r.First())
                .ToList();


        public List<EsentRow> SelectFirstRows(int count, int skip = 0)
        {
            CheckState();
            var columns = Columns.Select(c => new EsentCell(c.Value.Name, null, c.Value.Encoding));
            return SelectFirst(columns, count, skip);
        }

        public EsentRow SelectFirstRow() => SelectFirstRows(1)?.FirstOrDefault();

        public List<EsentRow> SelectLast(IEnumerable<EsentCell> columns, int count)
        {
            CheckState();
            var columnArray = columns as EsentCell[] ?? columns.ToArray();
            if (count < 1) return null;
            var success = Api.TryMove(Database.Session.JetId, JetId, JET_Move.Last, MoveGrbit.None);
            if (!success) return null;
            var rows = new List<EsentRow>();
            for (var i = 0; i < count; i++)
            {
                var cells = RetrieveColumns(columnArray);
                rows.Add(cells);
                if (!Api.TryMove(Database.Session.JetId, JetId, JET_Move.Previous, MoveGrbit.None))
                    break;
            }
            return rows;
        }

        public List<EsentCell> SelectLast(string columnName, int count) =>
            SelectLast(new[] { new EsentCell(columnName) }, count)
                ?.Select(r => r.First())
                .ToList();


        public List<EsentRow> SelectLastRows(int count)
        {
            CheckState();
            var columns = Columns.Select(c => new EsentCell(c.Value.Name, null, c.Value.Encoding));
            return SelectLast(columns, count);
        }

        public EsentRow SelectLastRow() => SelectLastRows(1)?.FirstOrDefault();
        #endregion

        #region Update
        public int Update(Func<EsentRow, bool> predicate, Encoding keyEncoding = null, params EsentCell[] columns)
        {
            CheckState();
            var exColumns = Columns.Select(c => new EsentCell(c.Value.Name, null, c.Value.Encoding));
            var columnArray = exColumns as EsentCell[] ?? exColumns.ToArray();
            var success = Api.TryMove(Database.Session.JetId, JetId, JET_Move.First, MoveGrbit.None);
            if (!success) return 0;
            var count = 0;
            while (true)
            {
                var cells = RetrieveColumns(columnArray);
                if (predicate(cells))
                {
                    count++;
                    Api.JetPrepareUpdate(Database.Session.JetId, JetId, JET_prep.Replace);
                    foreach (var column in columns)
                    {
                        var encoding = column.Encoding ?? Columns[column.ColumnName].Encoding;
                        var value = ValueProcessor.GetBytes(column.Value, encoding);
                        if (value != null)
                            Api.SetColumn(Database.Session.JetId, JetId, Columns[column.ColumnName].JetId, value);
                    }
                    Api.JetUpdate(Database.Session.JetId, JetId);
                }
                if (!Api.TryMove(Database.Session.JetId, JetId, JET_Move.Next, MoveGrbit.None))
                    break;
            }
            return count;
        }

        public int Update(Func<EsentRow, bool> predicate, Encoding keyEncoding, string columnName, object value)
            => Update(predicate, keyEncoding, new EsentCell(columnName, value));

        public int Update(Func<EsentRow, bool> predicate, string columnName, object value)
            => Update(predicate, null, columnName, value);

        public bool Update(object key, Encoding keyEncoding = null, params EsentCell[] columns)
        {
            CheckState();
            var byteKey = ValueProcessor.GetBytes(key, keyEncoding);
            Api.MakeKey(Database.Session.JetId, JetId, byteKey, MakeKeyGrbit.NewKey);
            var success = Api.TrySeek(Database.Session.JetId, JetId, SeekGrbit.SeekEQ);
            if (!success) return false;
            Api.JetPrepareUpdate(Database.Session.JetId, JetId, JET_prep.Replace);
            foreach (var column in columns)
            {
                var encoding = column.Encoding ?? Columns[column.ColumnName].Encoding;
                var value = ValueProcessor.GetBytes(column.Value, encoding);
                if (value != null)
                    Api.SetColumn(Database.Session.JetId, JetId, Columns[column.ColumnName].JetId, value);
            }
            Api.JetUpdate(Database.Session.JetId, JetId);
            return true;
        }

        public void UpdateAll(object key, Encoding keyEncoding = null, params EsentCell[] columns)
        {
            while (Update(key, keyEncoding, columns)) { }
        }

        public bool Update(object key, Encoding keyEncoding, string columnName, object value)
            => Update(key, keyEncoding, new EsentCell(columnName, value));

        public void UpdateAll(object key, Encoding keyEncoding, string columnName, object value)
        {
            while (Update(key, keyEncoding, columnName, value)) { }
        }

        public bool Update(object key, string columnName, object value)
            => Update(key, null, columnName, value);
        
        public void UpdateAll(object key, string columnName, object value)
        {
            while (Update(key, columnName, value)) { }
        }
        #endregion

        #region Delete
        public int Delete(Func<EsentRow, bool> predicate, int? max = null)
        {
            CheckState();
            var columns = Columns.Select(c => new EsentCell(c.Value.Name, null, c.Value.Encoding));
            var columnArray = columns as EsentCell[] ?? columns.ToArray();
            var success = Api.TryMove(Database.Session.JetId, JetId, JET_Move.First, MoveGrbit.None);
            if (!success) return 0;
            var count = 0;
            while (true)
            {
                var cells = RetrieveColumns(columnArray);
                if (predicate(cells))
                {
                    count++;
                    Api.JetDelete(Database.Session.JetId, JetId);
                    if (max.HasValue && count >= max)
                        return count;
                }
                if (!Api.TryMove(Database.Session.JetId, JetId, JET_Move.Next, MoveGrbit.None))
                    break;
            }
            return count;
        }

        public bool Delete(object key, Encoding keyEncoding = null)
        {
            CheckState();
            var byteKey = ValueProcessor.GetBytes(key, keyEncoding);
            Api.MakeKey(Database.Session.JetId, JetId, byteKey, MakeKeyGrbit.NewKey);
            var success = Api.TrySeek(Database.Session.JetId, JetId, SeekGrbit.SeekEQ);
            if (!success) return false;
            Api.JetDelete(Database.Session.JetId, JetId);
            return true;
        }

        public void DeleteAll(object key, Encoding keyEncoding = null)
        {
            while (Delete(key, keyEncoding)) { }
        }
        #endregion

        #region Where
        public List<EsentRow> While(Func<EsentRow, bool> predicate)
            => Where(predicate, r => !predicate.Invoke(r));

        public List<EsentRow> Where(Func<EsentRow, bool> predicate, Func<EsentRow, bool> exitCondition = null)
        {
            CheckState();
            var columns = Columns.Select(c => new EsentCell(c.Value.Name, null, c.Value.Encoding));
            var columnArray = columns as EsentCell[] ?? columns.ToArray();
            var success = Api.TryMove(Database.Session.JetId, JetId, JET_Move.First, MoveGrbit.None);
            if (!success) return null;
            var rows = new List<EsentRow>();
            while (true)
            {
                var cells = RetrieveColumns(columnArray);
                if (predicate(cells)) rows.Add(cells);
                if (rows.Any() && (exitCondition?.Invoke(cells) ?? false))
                    break;
                if (!Api.TryMove(Database.Session.JetId, JetId, JET_Move.Next, MoveGrbit.None))
                    break;
            }
            return rows;
        }

        public EsentRow WhereFirstRow(Func<EsentRow, bool> predicate)
        {
            CheckState();
            var columns = Columns.Select(c => new EsentCell(c.Value.Name, null, c.Value.Encoding));
            var columnArray = columns as EsentCell[] ?? columns.ToArray();
            var success = Api.TryMove(Database.Session.JetId, JetId, JET_Move.First, MoveGrbit.None);
            if (!success) return null;
            while (true)
            {
                var cells = RetrieveColumns(columnArray);
                if (predicate(cells)) return cells;
                if (!Api.TryMove(Database.Session.JetId, JetId, JET_Move.Next, MoveGrbit.None))
                    break;
            }
            return null;
        }
        #endregion
    }
}
