namespace NG.CsvData
{
    public sealed class CsvColumn
    {
        private readonly string _name;

        internal CsvColumn(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public int Ordinal { get; internal set; }

        public CsvDbType DbType { get; set; }

        public bool IsVirtual { get; internal set; }

        //public string Format { get; set; }

        //public System.Globalization.CultureInfo CultureInfo { get; set; }
    }
}
