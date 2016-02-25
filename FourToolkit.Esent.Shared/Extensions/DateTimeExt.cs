using System;

namespace FourToolkit.Esent.Extensions
{
    public static class DateTimeExt
    {
        #region Const duplicate

        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond*1000;
        private const long TicksPerMinute = TicksPerSecond*60;
        private const long TicksPerHour = TicksPerMinute*60;
        private const long TicksPerDay = TicksPerHour*24;
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond*60;
        private const int MillisPerHour = MillisPerMinute*60;
        private const int MillisPerDay = MillisPerHour*24;
        private const int DaysPerYear = 365;
        private const int DaysPer4Years = DaysPerYear*4 + 1;
        private const int DaysPer100Years = DaysPer4Years*25 - 1;
        private const int DaysPer400Years = DaysPer100Years*4 + 1;
        private const int DaysTo1899 = DaysPer400Years*4 + DaysPer100Years*3 - 367;
        private const int DaysTo10000 = DaysPer400Years*25 - 366;
        internal const long MinTicks = 0;
        internal const long MaxTicks = DaysTo10000*TicksPerDay - 1;
        private const long MaxMillis = (long) DaysTo10000*MillisPerDay;
        private const long DoubleDateOffset = DaysTo1899*TicksPerDay;
        private const long OADateMinAsTicks = (DaysPer100Years - DaysPerYear)*TicksPerDay;
        private const double OADateMinAsDouble = -657435.0;
        private const double OADateMaxAsDouble = 2958466.0;

        public static readonly DateTime MinValue = new DateTime(MinTicks, DateTimeKind.Unspecified);
        public static readonly DateTime MaxValue = new DateTime(MaxTicks, DateTimeKind.Unspecified);

        #endregion

        private static double TicksToOADate(long value)
        {
            if (value == 0) return 0.0;
            if (value < TicksPerDay) value += DoubleDateOffset;
            if (value < OADateMinAsTicks) throw new OverflowException("Max ticks value is" + OADateMinAsTicks);
            var millis = (value - DoubleDateOffset)/TicksPerMillisecond;
            if (millis >= 0) return (double) millis/MillisPerDay;
            var frac = millis%MillisPerDay;
            if (frac != 0) millis -= (MillisPerDay + frac)*2;
            return (double) millis/MillisPerDay;
        }

        private static long DoubleDateToTicks(double value)
        {
            if (value >= OADateMaxAsDouble || value <= OADateMinAsDouble)
                throw new ArgumentException("Value doesn't fall within expected bounds");
            var millis = (long) (value*MillisPerDay + (value >= 0 ? 0.5 : -0.5));
            if (millis < 0) millis -= millis%MillisPerDay*2;
            millis += DoubleDateOffset/TicksPerMillisecond;
            if (millis < 0 || millis >= MaxMillis)
                throw new ArgumentException("Milliseconds doesn't fall within expected bounds");
            return millis*TicksPerMillisecond;
        }
        
        public static double ToOADate(this DateTime datetime) => TicksToOADate(datetime.Ticks);
        
        public static DateTime FromOADate(double d) => new DateTime(DoubleDateToTicks(d), DateTimeKind.Unspecified);
    }
}