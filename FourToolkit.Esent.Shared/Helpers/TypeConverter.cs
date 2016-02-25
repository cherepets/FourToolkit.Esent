using System;
using Microsoft.Isam.Esent.Interop;

namespace FourToolkit.Esent.Helpers
{
    internal static class TypeConverter
    {
        public static JET_coltyp ToJet(this Type type)
        {
            if (type == null) return JET_coltyp.Nil;
            if (type == typeof (bool))
                return JET_coltyp.Bit;
            if (type == typeof (byte))
                return JET_coltyp.UnsignedByte;
            if (type == typeof (short))
                return JET_coltyp.Short;
            if (type == typeof (int))
                return JET_coltyp.Long;
            if (type == typeof(long))
                return JET_coltyp.Currency;
            if (type == typeof(float))
                return JET_coltyp.IEEESingle;
            if (type == typeof(double))
                return JET_coltyp.IEEEDouble;
            if (type == typeof(DateTime))
                return JET_coltyp.DateTime;
            if (type == typeof(string))
                return JET_coltyp.LongText;
            return JET_coltyp.LongBinary;
        }

        public static Type ToClr(this JET_coltyp coltyp)
        {
            switch (coltyp)
            {
                case JET_coltyp.Bit:
                    return typeof (bool);
                case JET_coltyp.UnsignedByte:
                    return typeof (byte);
                case JET_coltyp.Short:
                    return typeof (short);
                case JET_coltyp.Long:
                    return typeof (int);
                case JET_coltyp.Currency:
                    return typeof (long);
                case JET_coltyp.IEEESingle:
                    return typeof (float);
                case JET_coltyp.IEEEDouble:
                    return typeof (double);
                case JET_coltyp.DateTime:
                    return typeof (DateTime);
                case JET_coltyp.LongText:
                case JET_coltyp.Text:
                    return typeof (string);
                default:
                    return typeof (byte[]);
            }
        }
    }
}