using FourToolkit.Esent.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FourToolkit.Esent
{
    /// <summary>
    /// EsentSerializer is a static class that serializes any class to Esent data or vice versa
    /// </summary>
    public static class EsentSerializer
    {
        public static EsentCell[] Serialize(object obj, EsentTable table)
        {
            var type = obj.GetType();
            var props = type.GetRuntimeProperties();
            var cells = new List<EsentCell>();
            foreach (var column in table.Columns.Values)
            {
                var prop = props.FirstOrDefault(p => p.Name == column.Name);
                if (column.Options.HasFlag(EsentColumn.Option.Autoincrement) || prop == null)
                    continue;
                if (column.ColumnType == prop.PropertyType)
                {
                    var value = prop.GetValue(obj);
                    cells.Add(new EsentCell(column.Name, value, column.Encoding));
                }
                if (column.ColumnType == typeof(byte[]) && prop.PropertyType != typeof(byte[]))
                {
                    var value = ValueProcessor.GetBytes(prop.GetValue(obj) as byte[]);
                    cells.Add(new EsentCell(column.Name, value, column.Encoding));
                }
            }
            return cells.ToArray();
        }

        public static T Deserialize<T>(EsentRow r) where T : new()
        {
            var obj = new T();
            var type = typeof(T);
            var props = type.GetRuntimeProperties();
            foreach (var cell in r)
            {
                var prop = props.FirstOrDefault(p => p.Name == cell.ColumnName);
                if (prop == null) continue;
                if (cell.Value?.GetType() == prop.PropertyType)
                    prop.SetValue(obj, cell.Value);
                if (cell.Value?.GetType() == typeof(byte[]) && prop.PropertyType != typeof(byte[]))
                    prop.SetValue(obj, ValueProcessor.FromBytes(prop.PropertyType, cell.AsByteArray, cell.Encoding));
            }
            return obj;
        }
    }
}
