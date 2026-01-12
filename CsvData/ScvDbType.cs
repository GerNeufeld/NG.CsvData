using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NG.CsvData
{
    [Flags]
    public enum CsvDbType
    {
        String          = 0b0000000000000000,
        Int32           = 0b0000000000000001,
        DateTime        = 0b0000000000000010,
        Boolean         = 0b0000000000000100,
        Byte            = 0b0000000000001000,
        Char            = 0b0000000000010000,
        Decimal         = 0b0000000000100000,
        Double          = 0b0000000001000000,
        Single          = 0b0000000010000000,
        Guid            = 0b0000000100000000,
        Int16           = 0b0000001000000000,
        Int64           = 0b0000010000000000,
        Binary          = 0b0000100000000000,
        Timespan        = 0b0001000000000000,
        DateTimeOffset  = 0b0010000000000000,
    }

    public static class CsvDbTypeHelper
    {
        public static Type GetDbType(this CsvDbType dbType)
        {
            switch (dbType)
            {
                case CsvDbType.String:
                    return typeof(String);
                case CsvDbType.Int32:
                    return typeof(Int32);
                case CsvDbType.DateTime:
                    return typeof(DateTime);
                case CsvDbType.Boolean:
                    return typeof(Boolean);
                case CsvDbType.Byte:
                    return typeof(Byte);
                case CsvDbType.Char:
                    return typeof(char);
                case CsvDbType.Decimal:
                    return typeof(Decimal);
                case CsvDbType.Double:
                    return typeof(Double);
                case CsvDbType.Single:
                    return typeof(Single);
                case CsvDbType.Guid:
                    return typeof(Guid);
                case CsvDbType.Int16:
                    return typeof(Int16);
                case CsvDbType.Int64:
                    return typeof(Int64);
                case CsvDbType.Binary:
                    return typeof(Byte[]);
                case CsvDbType.Timespan:
                    return typeof(TimeSpan);
                case CsvDbType.DateTimeOffset:
                    return typeof(DateTimeOffset);
                default:
                    throw new NotSupportedException();
            }
        }
    }

}
