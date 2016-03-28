using System;
using System.Reflection;
using System.Text;
using FourToolkit.Esent.Extensions;

namespace FourToolkit.Esent.Helpers
{
    internal static class ValueProcessor
    {
        public static byte[] GetBytes(object data, Encoding encoding = null)
        {
            if (data == null) return null;
            var dataType = data.GetType();
            if (dataType == typeof(byte[]))
            {
                var input = (byte[])data;
                return input;
            }
            if (dataType == typeof(bool))
            {
                var input = (bool)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(byte))
            {
                var input = (byte)data;
                return new[] {input};
            }
            if (dataType == typeof(DateTime))
            {
                var input = ((DateTime)data).ToOADate();
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(double))
            {
                var input = (double)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(float))
            {
                var input = (float)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(Guid))
            {
                var input = (Guid)data;
                return input.ToByteArray();
            }
            if (dataType == typeof(int))
            {
                var input = (int)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(long))
            {
                var input = (long)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(short))
            {
                var input = (short)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(uint))
            {
                var input = (uint)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(ulong))
            {
                var input = (ulong)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(ushort))
            {
                var input = (ushort)data;
                return BitConverter.GetBytes(input);
            }
            if (dataType == typeof(string))
            {
                var input = (string)data;
                return input.Length == 0 ? null : encoding?.GetBytes(input);
            }
            return new byte[0];
        }

        public static object FromBytes(Type dataType, byte[] bytes, Encoding encoding = null)
        {
            if (bytes == null || bytes.Length == 0) return GetDefault(dataType);
            if (dataType == typeof(byte[]))
            {
                return bytes;
            }
            if (dataType == typeof(bool))
            {
                return BitConverter.ToBoolean(bytes, 0);
            }
            if (dataType == typeof(byte))
            {
                return bytes[0];
            }
            if (dataType == typeof(DateTime))
            {
                var oad = BitConverter.ToDouble(bytes, 0);
                return DateTimeExt.FromOADate(oad);
            }
            if (dataType == typeof(double))
            {
                return BitConverter.ToDouble(bytes, 0);
            }
            if (dataType == typeof(float))
            {
                return BitConverter.ToSingle(bytes, 0);
            }
            if (dataType == typeof(Guid))
            {
                return new Guid(bytes);
            }
            if (dataType == typeof(int))
            {
                return BitConverter.ToInt32(bytes, 0);
            }
            if (dataType == typeof(long))
            {
                return BitConverter.ToInt64(bytes, 0);
            }
            if (dataType == typeof(short))
            {
                return BitConverter.ToInt16(bytes, 0);
            }
            if (dataType == typeof(uint))
            {
                return BitConverter.ToUInt32(bytes, 0);
            }
            if (dataType == typeof(ulong))
            {
                return BitConverter.ToUInt64(bytes, 0);
            }
            if (dataType == typeof(ushort))
            {
                return BitConverter.ToUInt16(bytes, 0);
            }
            if (dataType == typeof(string))
            {
                return encoding.GetString(bytes, 0, bytes.Length);
            }
            return GetDefault(dataType);
        }

        public static T FromBytes<T>(byte[] bytes, Encoding encoding)
        {
            var dataType = typeof(T);
            return (T)FromBytes(dataType, bytes, encoding);
        }

        private static object GetDefault(Type type)
        {
            return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
