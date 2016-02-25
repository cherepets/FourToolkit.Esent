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
                if (prop != null && column.ColumnType == prop.PropertyType)
                {
                    var value = prop.GetValue(obj);
                    cells.Add(new EsentCell(column.Name, value));
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
                if (prop != null && cell.Value?.GetType() == prop.PropertyType)
                    prop.SetValue(obj, cell.Value);
            }
            return obj;
        }
    }
}
