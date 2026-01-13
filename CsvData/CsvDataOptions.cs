#nullable disable

using System.Text;

namespace NG.CsvData
{
    public interface ICsvDataReaderOptions : ICloneable
    {
        Encoding Encoding { get; }

        string Delimiter { get; }

        char? Quote { get; }

        bool FirstLineIsHeader { get; }

        bool IgnoreBlankLines { get; }

        IReadOnlyList<string> VirtualFields { get; }

        int GetVirtualFieldsOrdinal(string fieldName);
    }

    public interface ICsvDataWriterOptions : ICloneable
    {
        Encoding Encoding { get; }

        string Delimiter { get; }

        char? Quote { get; }

        bool RequireFieldQuotation { get; }

        IReadOnlyList<string> Headers { get; }
    }

    public sealed class CsvDataReaderOptions : ICsvDataReaderOptions
    {
        public CsvDataReaderOptions()
        {
        }

        public Encoding Encoding { get; set; } = Encoding.Default;

        public string Delimiter { get; set; } = ";";

        public char? Quote { get; set; } = '"';

        public bool FirstLineIsHeader { get; set; } = true;

        public bool IgnoreBlankLines { get; set; } = true;

        private IReadOnlyList<string> _virtualFields = null;
        private readonly Dictionary<string, int> _virtualFieldsDict = new(8);

        public IReadOnlyList<string> VirtualFields
        {
            get { return _virtualFields; }
            set
            {
                if (!object.ReferenceEquals(_virtualFields, value))
                {
                    _virtualFieldsDict.Clear();

                    if (value != null)
                    {
                        for (int i = 0; i < value.Count; i++)
                        {
                            _virtualFieldsDict.Add(value[i], i);
                        }
                    }
                    _virtualFields = value!;
                }
            }
        }

        public int GetVirtualFieldsOrdinal(string fieldName)
        {
            if (_virtualFieldsDict.TryGetValue(fieldName, out int ordinal))
                return ordinal;
            return -1;
        }

        public object Clone()
        {
            return new CsvDataReaderOptions()
            {
                Delimiter = this.Delimiter,
                Encoding = this.Encoding,
                FirstLineIsHeader = this.FirstLineIsHeader,
                IgnoreBlankLines = this.IgnoreBlankLines,
                Quote = this.Quote,
                VirtualFields = this.VirtualFields,
            };
        }
    }

    public sealed class CsvDataWriterOptions : ICsvDataWriterOptions
    {
        public Encoding Encoding { get; set; } = Encoding.Default;

        public string Delimiter { get; set; } = ";";

        public char? Quote { get; set; } = '"';

        public bool RequireFieldQuotation { get; set; } = false;

        public IReadOnlyList<string> Headers { get; set; } = null!;

        public object Clone()
        {
            return new CsvDataWriterOptions()
            {
                Delimiter = this.Delimiter,
                Encoding = this.Encoding,
                Quote = this.Quote,
                RequireFieldQuotation = this.RequireFieldQuotation,
                Headers = this.Headers,
            };
        }
    }
}
