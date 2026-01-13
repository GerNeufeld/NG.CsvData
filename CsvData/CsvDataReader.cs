#nullable disable

using System.Data;
using System.Text;
using System.Globalization;

namespace NG.CsvData
{
    public delegate bool ParseAction(CsvColumn column, string value, out object result);

    public interface ICsvDataReader : IDataReader
    {
        int CsvFieldCount { get; }

        string GetRawRecord();

        string GetHeaderRawRecord();

        bool IsVirtualField(int i);

        int RecordsReaded { get; }

        string Filename { get; }

        ICsvHeadersCollection Headers { get; }

        ParseAction ParseValue { get; set; }
    }


    public sealed class CsvDataReader : ICsvDataReader
    {
        public const string
            COLUMN_NAME = "ColumnName",
            COLUMN_ORDINAL = "ColumnOrdinal",
            DATA_TYPE = "DataType",
            ALLOW_DBNULL = "AllowDBNull",
            IS_VIRTUAL_FIELD = "IsVirtualField";

        private CsvDataReader()
        {
            _readersList = [];
            OnGetVirtualColumnValue = null!;
        }

        private CsvDataReader_private[] _readersList;

        private int _current = -1;

        private void NextReader()
        {
            if (CurrentReader == null)
            {
                _current = 0;
            }
            else
            {
                CurrentReader.OnGetVirtualColumnValue -= CurrentReader_OnGetVirtualColumnValue;
                _current++;
            }

            CurrentReader = _readersList[_current];

            CurrentReader.OnGetVirtualColumnValue += CurrentReader_OnGetVirtualColumnValue;
        }

        public ParseAction ParseValue
        {
            get { return CurrentReader.ParseValue; }
            set { if (CurrentReader.ParseValue != value) CurrentReader.ParseValue = value; }
        }


        public ICsvHeadersCollection Headers => CurrentReader.Headers;

        public event EventHandler<GetVirtualColumnEventArgs> OnGetVirtualColumnValue;

        public object this[int i] => CurrentReader[i];

        public object this[string name] => CurrentReader[name];

        public int Depth => CurrentReader.Depth;

        public bool IsClosed => CurrentReader?.IsClosed ?? true;

        public int RecordsAffected => CurrentReader.RecordsAffected;

        public int FieldCount => CurrentReader.FieldCount;

        public int CsvFieldCount => CurrentReader.CsvFieldCount;

        public int RecordColumnsCount => CurrentReader.RecordColumnsCount;

        public int RecordsReaded => CurrentReader.RecordsReaded;

        public string Filename => CurrentReader.Filename;

        private CsvDataReader_private CurrentReader { get; set; } = null;



        public static CsvDataReader Open(Stream stream, CsvDataReaderOptions options = null, bool leaveOpen = false)
        {
            return Open([stream], options, leaveOpen);
        }

        public static CsvDataReader Open(Stream[] streamList, CsvDataReaderOptions options = null, bool leaveOpen = false)
        {
            CsvDataReader result = new()
            {
                _readersList = [.. streamList.Select(stream => new CsvDataReader_private(stream, options, leaveOpen))],
            };

            result.NextReader();

            result.CurrentReader.Initialize();

            return result;
        }

        private void CurrentReader_OnGetVirtualColumnValue(object sender, GetVirtualColumnEventArgs e)
        {
            var onGetVirtualColumnValue = OnGetVirtualColumnValue;

            if (onGetVirtualColumnValue == null)
                return;
            else
                onGetVirtualColumnValue.Invoke(this, e);

        }

        public static CsvDataReader Open(string filename, CsvDataReaderOptions options = null)
        {
            return Open([filename], options);
        }

        public static CsvDataReader Open(string[] filenames, CsvDataReaderOptions options = null)
        {
            return Open([.. filenames.Select(filename => System.IO.File.OpenRead(filename))], options, false);
        }



        public bool IsVirtualField(int i)
        {
            return CurrentReader.IsVirtualField(i);
        }

        public string GetRawRecord()
        {
            return CurrentReader.GetRawRecord();
        }

        public string GetHeaderRawRecord()
        {
            return CurrentReader.GetHeaderRawRecord();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _readersList.ToList().ForEach(r => r.Dispose());
        }

        public bool GetBoolean(int i)
        {
            return CurrentReader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return CurrentReader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return CurrentReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return CurrentReader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return CurrentReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return CurrentReader.GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return CurrentReader.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return CurrentReader.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return CurrentReader.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return CurrentReader.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return CurrentReader.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return CurrentReader.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return CurrentReader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return CurrentReader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return CurrentReader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return CurrentReader.GetInt64(i);
        }

        public string GetName(int i)
        {
            return CurrentReader.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            return CurrentReader.GetOrdinal(name);
        }

        public DataTable GetSchemaTable()
        {
            return CurrentReader.GetSchemaTable();
        }

        public string GetString(int i)
        {
            return CurrentReader.GetString(i);
        }

        public object GetValue(int i)
        {
            return CurrentReader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return CurrentReader.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return CurrentReader.IsDBNull(i);
        }

        public bool NextResult()
        {
            if (_current >= _readersList.Length)
            {
                return false;
            }
            else
            {
                NextReader();
                CurrentReader.Initialize();
                return true;
            }
        }

        public bool Read()
        {
            return CurrentReader.Read();
        }

        private class CsvDataReader_private : ICsvDataReader
        {

            private const char
                // BOM = '\uFEFF',
                LF = '\u000A',
                CR = '\u000D',
                NEL = '\u0085',
                LS = '\u2028',
                PS = '\u2029';

            private const int BUFFER_SIZE = 1024;

            public CsvDataReader_private(Stream stream, CsvDataReaderOptions options = null, bool leaveOpen = false)
            {
                if (!stream.CanSeek)
                    throw new NotSupportedException("This stream does not support seeking");

                _readerOptions = (ICsvDataReaderOptions)(options?.Clone() ?? new CsvDataReaderOptions());
                _streamReader = new StreamReader(stream, _readerOptions.Encoding, false, BUFFER_SIZE, leaveOpen);
                ParseValue = null!;
                OnGetVirtualColumnValue = null!;
            }



            #region private fields

            private readonly StreamReader _streamReader;

            private readonly ICsvDataReaderOptions _readerOptions;

            private readonly StringBuilder _rawRecordBuilder = new(1024);

            private string[] _fields = null!;

            private List<string> _firstRecord = null!;

            private readonly CsvHeadersCollection _headers = [];

            public ICsvHeadersCollection Headers => _headers;

            private readonly int _recordsAffected = -1;

            private bool _isClosed = false;

            private string _headersRawRecord = null!;

            private char[]
                _buffer1 = new char[BUFFER_SIZE],
                _buffer2 = new char[BUFFER_SIZE];

            int
                _len1 = -1,
                _len2 = -1,
                _index = -1;

            private int
                _totalFieldCount = 0,
                _csvFieldCount = 0,
                _virtualFieldCount = 0;

            private enum ReadFieldResult
            {
                None = 0,
                EndOfField = 1,
                EndOfRecord = 2,
                EndOfFile = 4,
            }

            #endregion // private fields

            public ParseAction ParseValue { get; set; }

            public string Filename
            {
                get
                {
                    return (_streamReader.BaseStream as FileStream)?.Name ?? string.Empty;
                }
            }

            public event EventHandler<GetVirtualColumnEventArgs> OnGetVirtualColumnValue;

            public int RecordColumnsCount { get; private set; } = 0;

            public bool RecordError => RecordColumnsCount != _csvFieldCount;

            public int RecordsReaded { get; private set; } = 0;

            /// <summary>
            /// Считывает следующий блок
            /// </summary>
            private void ReadBlock()
            {
                if (_len2 < 0)
                {
                    _len1 = _streamReader.ReadBlock(_buffer1, 0, BUFFER_SIZE);
                }
                else
                {
                    _buffer1 = _buffer2;
                    _len1 = _len2;
                }
                _buffer2 = new char[BUFFER_SIZE];
                _len2 = _streamReader.ReadBlock(_buffer2, 0, BUFFER_SIZE);
            }

            /// <summary>
            /// Проверяет, является ли последовательность символов разделителем
            /// </summary>
            /// <param name="nextChar">если true - проверка последовательности со следующего символа. Иначе - с текущего</param>
            /// <returns></returns>
            private bool IsDelimiter(bool nextChar = false)
            {
                int idx = nextChar ? _index + 1 : _index;

                for (int i = 0; i < _readerOptions.Delimiter.Length; i++)
                {
                    if (idx + i < _len1)
                    {
                        if (_buffer1[idx + i] != _readerOptions.Delimiter[i])
                            return false;
                    }
                    else if (idx - _len1 + i < _len2)
                    {
                        if (_buffer2[idx - _len1 + i] != _readerOptions.Delimiter[i])
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            private int CountLineFeed(bool nextChar = false)
            {
                int idx = nextChar ? _index + 1 : _index,
                    result = -1;

                char chr;

                if (idx < _len1)
                    chr = _buffer1[idx];
                else if (idx - _len1 < _len2)
                    chr = _buffer2[idx - _len1];
                else
                    return 0;


                if (chr == LF || chr == CR || chr == NEL || chr == LS || chr == PS)
                    result = 1;

                if (chr == CR)
                {
                    if ((idx + 1 < _len1 && _buffer1[idx + 1] == LF) ||
                            (idx - _len1 + 1 < _len2 && _buffer2[idx - _len1 + 1] == LF))
                        result = 2;
                }

                return result;
            }

            private void ReadField(FieldBuilder fieldBuilder)
            {
                if (fieldBuilder == null)
                    fieldBuilder = new FieldBuilder(1024);
                else
                    fieldBuilder.Clear();

                if (_len1 < 0)
                {
                    ReadBlock();
                    _index = 0;
                }

                if (_readerOptions.Quote.HasValue)
                    readFieldWithQuotes(ref fieldBuilder);
                else
                    readFieldWithoutQuotes(ref fieldBuilder);

                void readFieldWithQuotes(ref FieldBuilder fb)
                {


                    bool
                        isBeginField = true, //признак - текущий символ - начало поля
                        isBeginQuote = false, //признак - поле начинается с символа квотирования
                        isFirstQuote = false; //


                    do
                    {
                        while (_index < _len1)
                        {
                            char chr = _buffer1[_index];

                            if (!((chr == LF || chr == CR || chr == NEL || chr == LS || chr == PS) && !isBeginQuote))
                                _rawRecordBuilder.Append(chr);

                            if (chr == _readerOptions.Quote.Value)
                            {
                                if (isBeginField)
                                {
                                    isBeginQuote = true;
                                }
                                else if (isBeginQuote)
                                {
                                    if (isFirstQuote)
                                    {
                                        isFirstQuote = false;
                                        fb.Append(chr);
                                    }
                                    else
                                    {
                                        if ((IsDelimiter(true) || CountLineFeed(true) >= 0))
                                        {
                                            isBeginQuote = false;
                                        }
                                        else
                                            isFirstQuote = true;
                                    }
                                }
                            }
                            else if (!isBeginQuote)
                            {
                                int lineFeedsCount = CountLineFeed();

                                if (lineFeedsCount >= 0)
                                {
                                    if (_index + lineFeedsCount < _len1)
                                    {
                                        _index += lineFeedsCount;
                                    }
                                    else
                                    {
                                        _index += (lineFeedsCount - _len1);
                                        ReadBlock();
                                    }


                                    fb.ReadResult = lineFeedsCount == 0 ? ReadFieldResult.EndOfFile : ReadFieldResult.EndOfRecord;

                                    return;
                                }
                                else if (IsDelimiter())
                                {
                                    if (_index + _readerOptions.Delimiter.Length < _len1)
                                    {
                                        _index += _readerOptions.Delimiter.Length;
                                    }
                                    else
                                    {
                                        _index += (_readerOptions.Delimiter.Length - _len1);
                                        ReadBlock();
                                    }

                                    fb.ReadResult = ReadFieldResult.EndOfField;

                                    return;
                                }
                                else
                                    fb.Append(chr);

                            }
                            else
                            {
                                fb.Append(chr);
                            }

                            _index++;
                            isBeginField = false;
                        }


                        ReadBlock();
                        _index = 0;
                    } while (_len1 > 0);

                    fb.ReadResult = ReadFieldResult.EndOfFile;

                    return;
                }


                /// <summary>
                /// Считывает значение поля без квотирования
                /// </summary>
                /// <param name="fb">данные поля</param>
                /// <returns></returns>
                void readFieldWithoutQuotes(ref FieldBuilder fb)
                {

                    do
                    {
                        while (_index < _len1)
                        {
                            char chr = _buffer1[_index];

                            if (!((chr == LF || chr == CR || chr == NEL || chr == LS || chr == PS)))
                                _rawRecordBuilder.Append(chr);

                            int lineFeedsCount = CountLineFeed();

                            if (lineFeedsCount >= 0)
                            {
                                if (_index + lineFeedsCount < _len1)
                                {
                                    _index += lineFeedsCount;
                                }
                                else
                                {
                                    _index += (lineFeedsCount - _len1);
                                    ReadBlock();
                                }


                                fb.ReadResult = lineFeedsCount == 0 ? ReadFieldResult.EndOfFile : ReadFieldResult.EndOfRecord;
                                return;
                            }
                            else if (IsDelimiter())
                            {
                                if (_index + _readerOptions.Delimiter.Length < _len1)
                                {
                                    _index += _readerOptions.Delimiter.Length;
                                }
                                else
                                {
                                    _index += (_readerOptions.Delimiter.Length - _len1);
                                    ReadBlock();
                                }


                                fb.ReadResult = ReadFieldResult.EndOfField;
                                return;
                            }
                            else
                                fb.Append(chr);

                            _index++;
                        }


                        ReadBlock();
                        _index = 0;
                    } while (_len1 > 0);

                    fb.ReadResult = ReadFieldResult.EndOfFile;

                    return;
                }

            }

            private sealed class FieldBuilder : IDisposable
            {
                private readonly StringBuilder _stringBuilder;

                public FieldBuilder(int capacity = 0) => _stringBuilder = capacity > 0 ? new StringBuilder(capacity) : new StringBuilder();

                public ReadFieldResult ReadResult { get; set; } = ReadFieldResult.None;

                public int Length => _stringBuilder.Length;

                public void Clear()
                {
                    _stringBuilder.Clear();
                    ReadResult = ReadFieldResult.None;
                }

                public void Append(char value) => _stringBuilder.Append(value);

                public override string ToString() => _stringBuilder.ToString();

                public void Dispose()
                {
                    _stringBuilder.Clear();
                }
            }


            public void Initialize()
            {
                //ReadFieldResult result;

                using FieldBuilder fieldBuilder = new(1024);
                if (!_readerOptions.FirstLineIsHeader)
                    _firstRecord = new List<string>(256);

                do
                {
                    ReadField(fieldBuilder);
                }
                while (fieldBuilder.Length == 0 && fieldBuilder.ReadResult == ReadFieldResult.EndOfRecord);


                if (fieldBuilder.Length > 0)
                {
                    _headers.Add(_readerOptions.FirstLineIsHeader ? fieldBuilder.ToString() : string.Empty);

                    if (!_readerOptions.FirstLineIsHeader)
                        _firstRecord.Add(fieldBuilder.ToString());
                }

                while (fieldBuilder.ReadResult == ReadFieldResult.EndOfField)
                {
                    ReadField(fieldBuilder);
                    _headers.Add(_readerOptions.FirstLineIsHeader ? fieldBuilder.ToString() : string.Empty);

                    if (!_readerOptions.FirstLineIsHeader)
                        _firstRecord.Add(fieldBuilder.ToString());
                }


                _csvFieldCount = _headers.Count;

                if (_readerOptions.VirtualFields != null)
                {
                    _virtualFieldCount = _readerOptions.VirtualFields.Count;
                    for (int i = 0; i < _virtualFieldCount; i++)
                    {
                        _headers.Add(_readerOptions.VirtualFields[i], true);
                    }
                }

                _totalFieldCount = _headers.Count;


                _fields = new string[_headers.Count];

                if (_readerOptions.FirstLineIsHeader)
                    _headersRawRecord = _rawRecordBuilder.ToString();
            }


            public ICsvDataReaderOptions Options => _readerOptions;

            private bool _isEOF = false;

            private int ReadRecord()
            {
                if (_isEOF)
                    return -1;

                if (!_readerOptions.FirstLineIsHeader && RecordsReaded == 0)
                {
                    _firstRecord.CopyTo(_fields);

                    return _firstRecord.Count;
                }


                int fieldCount = 0;

                //ReadFieldResult result;

                using (FieldBuilder fieldBuilder = new(1024))
                {
                    ClearValues();

                    _rawRecordBuilder.Clear();

                    do
                    {
                        ReadField(fieldBuilder);

                        if (fieldCount < _fields.Length)
                        {
                            _fields[fieldCount] = fieldBuilder.ToString();
                        }
                        if (fieldBuilder.ReadResult == ReadFieldResult.EndOfField || fieldCount != 0 || fieldBuilder.Length != 0)
                            fieldCount++;
                    }
                    while (fieldBuilder.ReadResult == ReadFieldResult.EndOfField);

                    for (int i = fieldCount; i < _fields.Length; i++)
                    {
                        _fields[i] = string.Empty;
                    }


                    if (fieldBuilder.ReadResult == ReadFieldResult.EndOfFile && fieldCount == 0)
                    {
                        _isEOF = true;
                        return -1;
                    }
                }

                return fieldCount;
            }

            public string GetRawRecord()
            {
                return _rawRecordBuilder.ToString();
            }

            public string GetHeaderRawRecord()
            {
                return _headersRawRecord;
            }

            private void ClearValues()
            {
                for (int i = 0; i < _fields.Length; i++)
                {
                    _fields[i] = null!;
                }
            }


            private object GetFieldValue(int i)
            {

                if (!IsVirtualField(i))
                {
                    if (ParseValue != null)
                    {
                        if (ParseValue.Invoke(_headers[i], _fields[i], out object result))
                            return result;
                    }

                    switch (_headers[i].DbType)
                    {
                        case CsvDbType.String:
                            return _fields[i];
                        case CsvDbType.DateTime:
                            if (DateTime.TryParse(_fields[i], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTimeResult))
                                return dateTimeResult;
                            break;
                        case CsvDbType.Boolean:
                            if (bool.TryParse(_fields[i], out bool boolResult))
                            {
                                return boolResult;
                            }
                            break;
                        case CsvDbType.Byte:
                            if (byte.TryParse(_fields[i], out byte byteResult))
                            {
                                return byteResult;
                            }
                            break;
                        case CsvDbType.Char:
                            if (char.TryParse(_fields[i], out char charResult))
                            {
                                return charResult;
                            }
                            break;
                        case CsvDbType.Decimal:
                            if (decimal.TryParse(_fields[i], out decimal decimalResult))
                            {
                                return decimalResult;
                            }
                            break;
                        case CsvDbType.Double:
                            if (double.TryParse(_fields[i], out double doubleResult))
                            {
                                return doubleResult;
                            }
                            break;
                        case CsvDbType.Single:
                            if (float.TryParse(_fields[i], out float floatResult))
                            {
                                return floatResult;
                            }
                            break;
                        case CsvDbType.Guid:
                            if (Guid.TryParse(_fields[i], out Guid guidResult))
                            {
                                return guidResult;
                            }
                            break;
                        case CsvDbType.Int16:
                            if (short.TryParse(_fields[i], out short shortResult))
                            {
                                return shortResult;
                            }
                            break;
                        case CsvDbType.Int32:
                            if (int.TryParse(_fields[i], out int intResult))
                            {
                                return intResult;
                            }
                            break;
                        case CsvDbType.Int64:
                            if (long.TryParse(_fields[i], out long longResult))
                            {
                                return longResult;
                            }
                            break;
                        case CsvDbType.Binary:
                            try
                            {
                                return Convert.FromBase64String(_fields[i]);
                            }
                            catch { }
                            break;
                    }

                    throw new FormatException($"Ошибка преобразования значения столбца {_headers[i].Name} [{_fields[i]}] в тип {_headers[i].DbType.GetDbType().Name}");
                }
                else
                {
                    var onGetVirtualColumnValue = OnGetVirtualColumnValue;

                    if (onGetVirtualColumnValue != null)
                    {
                        var e = new GetVirtualColumnEventArgs()
                        {
                            VirtualColumn = _headers[i],
                        };
                        onGetVirtualColumnValue.Invoke(this, e);
                        return e.Value;
                    }
                    else
                        return null!;
                }
            }

            public bool IsVirtualField(int i)
            {
                return Headers[i].IsVirtual;
            }

            public int VirtualFieldsCount => _virtualFieldCount;

            public int CsvFieldCount => _csvFieldCount;


            #region IDataReader Support

            public object this[int i] => GetFieldValue(i);


            public object this[string name] => GetFieldValue(_headers.GetOrdinal(name));

            public int Depth => 0;

            public bool IsClosed => _isClosed;

            public int RecordsAffected => _recordsAffected;

            public int FieldCount => _totalFieldCount;

            public void Close()
            {
                _streamReader.Close();
                _isClosed = true;
            }

            public bool GetBoolean(int i)
            {
                return (bool)GetValue(i);
            }

            public byte GetByte(int i)
            {
                return (byte)GetValue(i);
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotSupportedException();
            }

            public char GetChar(int i)
            {
                return (char)GetValue(i);
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotSupportedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotSupportedException();
            }

            public string GetDataTypeName(int i)
            {
                return _headers[i].DbType.GetDbType().Name;
            }

            public DateTime GetDateTime(int i)
            {
                return (DateTime)GetValue(i);
            }

            public decimal GetDecimal(int i)
            {
                return (decimal)GetValue(i);
            }

            public double GetDouble(int i)
            {
                return (double)GetValue(i);
            }

            public Type GetFieldType(int i)
            {
                return _headers[i].DbType.GetDbType();
            }

            public float GetFloat(int i)
            {
                return (float)GetValue(i);
            }

            public Guid GetGuid(int i)
            {
                return (Guid)GetValue(i);
            }

            public short GetInt16(int i)
            {
                return (short)GetValue(i);
            }

            public int GetInt32(int i)
            {
                return (int)GetValue(i);
            }

            public long GetInt64(int i)
            {
                return (long)GetValue(i);
            }

            public string GetName(int i)
            {
                if (i < _headers.Count)
                    return _headers[i].Name;
                else
                    if (i >= _headers.Count && _readerOptions.VirtualFields != null && i - _headers.Count < _readerOptions.VirtualFields.Count)
                        return _readerOptions.VirtualFields[i - _headers.Count];
                    else
                        throw new IndexOutOfRangeException($"Index {i} is out of range for getting field name.");
            }

            public int GetOrdinal(string name)
            {
                int result = _headers.GetOrdinal(name);

                if (result < 0)
                    if (_readerOptions.VirtualFields != null)
                        result = _readerOptions.GetVirtualFieldsOrdinal(name);

                return result;
            }

            public DataTable GetSchemaTable()
            {
                DataTable schemaTable = new("SchemaTable");

                schemaTable.Columns.AddRange(
                    [
                        new DataColumn(COLUMN_NAME, typeof(string)),
                        new DataColumn(COLUMN_ORDINAL, typeof(Int32)),
                        new DataColumn(DATA_TYPE, typeof(Type)),
                        new DataColumn(ALLOW_DBNULL, typeof(bool)),
                        new DataColumn(IS_VIRTUAL_FIELD, typeof(bool)),
                    ]);


                for (int i = 0; i < _headers.Count; i++)
                {
                    schemaTable.Rows.Add(_headers[i].Name, _headers[i].Ordinal, _headers[i].DbType.GetDbType(), true, IsVirtualField(i));
                }

                return schemaTable;
            }

            public string GetString(int i)
            {
                return _headers[i].DbType == CsvDbType.String ? _fields[i] : GetFieldValue(i).ToString();
            }

            public object GetValue(int i)
            {
                return IsDBNull(i) ? (object)DBNull.Value : GetFieldValue(i)!;
            }

            public int GetValues(object[] values)
            {
                int result;
                for (result = 0; result < _fields.Length; result++)
                {
                    if (result < values.Length)
                        values[result] = IsDBNull(result) ? (object)DBNull.Value : GetFieldValue(result);
                    else
                        break;
                }
                return result;
            }

            public bool IsDBNull(int i)
            {
                if (IsVirtualField(i))
                    return GetFieldValue(i) == null;
                else
                    return string.IsNullOrEmpty(_fields[i]);
            }

            public bool NextResult()
            {
                return false;
            }

            public bool Read()
            {
                int fieldsCount; // = -1;
                do
                {
                    fieldsCount = ReadRecord();
                    if (fieldsCount < 0)
                    {
                        return false;
                    }
                } while (_readerOptions.IgnoreBlankLines && fieldsCount == 0);

                RecordColumnsCount = fieldsCount;

                RecordsReaded++;

                return true;
            }

            #endregion // IDataReader support

            #region IDisposable Support

            private bool disposedValue = false; // Для определения избыточных вызовов

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _streamReader.Close();

                        _streamReader.Dispose();

                        _headers.Dispose();

                        //ClearValues();

                        _fields = null;
                    }

                    disposedValue = true;
                }

            }

            // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
            // ~CsvDataReader() {
            //   // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            //   Dispose(false);
            // }

            // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
            public void Dispose()
            {
                // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
                Dispose(true);
                // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
                GC.SuppressFinalize(this);
            }
            #endregion


        }
    }

    public class GetVirtualColumnEventArgs : EventArgs
    {
        internal GetVirtualColumnEventArgs()
        {
            VirtualColumn = null!;
            Value = null!;
        }

        public CsvColumn VirtualColumn { get; internal set; }

        public object Value { get; set; }
    }

}
