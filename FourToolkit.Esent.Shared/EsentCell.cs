using System;
using System.Text;

namespace FourToolkit.Esent
{
    public struct EsentCell
    {
        public string ColumnName { get; internal set; }
        public object Value { get; internal set; }
        public Encoding Encoding { get; internal set; }

        public EsentCell(string columnName, object value = null, Encoding encoding = null)
        {
            ColumnName = columnName;
            Value = value;
            Encoding = encoding;
        }

        public T GetValue<T>()
        {
            if (Value == null) return default(T);
            var targetType = typeof (T);
            if (Value.GetType() != targetType)
                throw new InvalidCastException($"Value '{Value} is not a valid {targetType.Name}");
            return (T) Value;
        }

        public bool AsBool => GetValue<bool>();
        public byte AsByte => GetValue<byte>();
        public short AsShort => GetValue<short>();
        public int AsInt => GetValue<int>();
        public long AsLong => GetValue<long>();
        public float AsFloat => GetValue<float>();
        public double AsDouble => GetValue<double>();
        public DateTime AsDateTime => GetValue<DateTime>();
        public string AsString => GetValue<string>();
        public byte[] AsByteArray => GetValue<byte[]>();
    }
}
