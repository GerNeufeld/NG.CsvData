#nullable disable

using System.Collections;

namespace NG.CsvData
{
    public sealed class CsvDataWriter : IDisposable
    {
        public CsvDataWriter(string path, CsvDataWriterOptions options = null)
            : this(new FileStream(path, FileMode.Create, FileAccess.Write), options, false)
        {

        }

        public CsvDataWriter(Stream stream, CsvDataWriterOptions options = null, bool leaveOpen = false)
        {
            _writerOptions = (CsvDataWriterOptions)(options?.Clone() as CsvDataWriterOptions) ?? new CsvDataWriterOptions();

            _streamWriter = new StreamWriter(stream, _writerOptions.Encoding, 256, leaveOpen);

            _quoteStr = _writerOptions.Quote;

            if ((_writerOptions.Headers?.Count ?? 0) > 0)
            {
                _headers = [];
                _fieldsCount = _writerOptions.Headers.Count;
                string[] headersStr = new string[_fieldsCount];

                for (int i = 0; i < _fieldsCount; i++)
                {
                    _headers.Add(_writerOptions.Headers[i]);
                    headersStr[i] = _headers[i].Name;
                }
                WriteRecord(headersStr);
            }

        }

        private readonly ICsvDataWriterOptions _writerOptions = null!;

        private int _fieldsCount = -1;

        private readonly char? _quoteStr;

        private readonly StreamWriter _streamWriter;

        private readonly CsvHeadersCollection _headers = null!;

        public ICsvDataWriterOptions Options => _writerOptions;

        public CsvRow NewRow()
        {
            return new CsvRow(Options.Headers);
        }

        public void Write(CsvRow row)
        {
            Write([.. row.Values]);
        }

        public void Write(string[] record)
        {
            _streamWriter.Write(Environment.NewLine);
            WriteRecord(record);
        }

        private void WriteRecord(string[] record)
        {

            if (_fieldsCount < 0)
            {
                _fieldsCount = record.Length;
            }

            int j = _fieldsCount < record.Length ? _fieldsCount : record.Length;

            for (int i = 0; i < j; i++)
            {
                bool q = _writerOptions.RequireFieldQuotation ||
                    (record[i] ?? string.Empty).Contains(_writerOptions.Delimiter) ||
                    (record[i] ?? string.Empty).Contains('\r') ||
                    (record[i] ?? string.Empty).Contains('\n') ||
                    (_quoteStr.HasValue && (record[i] ?? string.Empty).Contains(_quoteStr.Value));

                if (q)
                {
                    _streamWriter.Write(_quoteStr);
                }

                if (!string.IsNullOrEmpty(record[i]))
                {
                    if (_quoteStr.HasValue)
                    {
                        for (int k = 0; k < record[i].Length; k++)
                        {
                            if (record[i][k] == _quoteStr.Value)
                                _streamWriter.Write(_quoteStr);
                            _streamWriter.Write(record[i][k]);
                        }
                    }
                    else
                    {
                        _streamWriter.Write(record[i]);
                    }

                }

                if (q)
                {
                    _streamWriter.Write(_quoteStr);
                }

                if (i != j - 1)
                {
                    _streamWriter.Write(_writerOptions.Delimiter);
                }
            }

            for (int i = j; i < _fieldsCount; i++)
            {
                _streamWriter.Write(_writerOptions.Delimiter);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _streamWriter.Flush();
                    _streamWriter.Dispose();
                }


                disposedValue = true;
            }
        }

        // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
        // ~CsvDataWriter() {
        //   // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
        //   Dispose(false);
        // }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            Dispose(true);
            // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
        }
        #endregion

        public class CsvRow : IDisposable, IReadOnlyDictionary<string, string>
        {
            private readonly Dictionary<string, string> _row;
            private bool disposedValue;

            public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, string>)_row).Keys;

            public IEnumerable<string> Values => ((IReadOnlyDictionary<string, string>)_row).Values;

            public int Count => ((IReadOnlyCollection<KeyValuePair<string, string>>)_row).Count;

            public string this[string key]
            {
                get { return ((IReadOnlyDictionary<string, string>)_row)[key]; }
                set { _row[key] = value; }
            }

            internal CsvRow(IEnumerable<string> headers)
            {
                _row = new Dictionary<string, string>(headers.Count());

                foreach(string key in headers)
                {
                    _row.Add(key, null!);
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _row.Clear();
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public bool ContainsKey(string key)
            {
                return ((IReadOnlyDictionary<string, string>)_row).ContainsKey(key);
            }

            public bool TryGetValue(string key, out string value)
            {
                return ((IReadOnlyDictionary<string, string>)_row).TryGetValue(key, out value);
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<string, string>>)_row).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_row).GetEnumerator();
            }
        }
    }
    
}
