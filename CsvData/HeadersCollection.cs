using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NG.CsvData
{
    public interface ICsvHeadersCollection : IEnumerable<CsvColumn>
    {
        CsvColumn this[int i] { get; }
        CsvColumn this[string columnName] { get; }

        int Count { get; }

        int GetOrdinal(string name);

        bool Contains(string name);
    }

    internal class CsvHeadersCollection : IDisposable, ICsvHeadersCollection
    {
        private Dictionary<string, CsvColumn> _indexses = new Dictionary<string, CsvColumn>(256);
        private List<CsvColumn> _columns = new List<CsvColumn>(256);
        int _index = -1;

        private const int MAX_COLUMN_NAME_SIZE = 120;

        internal void Add(string name = "", bool isVirtual = false)
        {
            
            _index++;

            if (string.IsNullOrWhiteSpace(name))
                name = $"Column{_index + 1}";

            string baseName = name.Substring(0, Math.Min(MAX_COLUMN_NAME_SIZE, name.Length));

            int i = 0;

            while (_indexses.ContainsKey(name))
            {
                i++;
                name = $"{baseName}_{i}";
            }

            CsvColumn column = new CsvColumn(name)
            {
                DbType = CsvDbType.String,
                Ordinal = _index,
                IsVirtual = isVirtual,
            };

            _indexses.Add(name, column);
            _columns.Add(column);
        }

        public int Count => _indexses.Count;

        public CsvColumn this[int index] => _columns[index];
        
        public CsvColumn this[string columnName] => _indexses[columnName];
        
        public int GetOrdinal(string name)
        {
            if (_indexses.ContainsKey(name))
                return _indexses[name].Ordinal;
            else
                return -1;
        }

        public bool Contains(string name)
        {
            return GetOrdinal(name) > -1;
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты).
                    _indexses.Clear();
                    _indexses = null;
                    _columns.Clear();
                    _columns = null;
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                // TODO: задать большим полям значение NULL.

                disposedValue = true;
            }
        }

        // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
        // ~HeadersCollection() {
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
        #endregion IDisposable Support

        #region IEnumerable Support
        public IEnumerator<CsvColumn> GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columns.GetEnumerator();;
        }
        #endregion //IEnumerable Support
    }

}
