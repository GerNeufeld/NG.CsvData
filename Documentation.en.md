# CsvData Library Documentation

## Overview
CsvData is a .NET library designed for reading and writing CSV files with advanced functionality. It implements the `IDataReader` interface, making it compatible with data processing workflows that expect this standard interface.

## Features
- Read and write CSV files with customizable delimiters and quotes
- Support for virtual columns that can be computed during reading
- Automatic data type conversion with configurable parsing
- Compatibility with .NET's `IDataReader` interface
- Support for multiple streams and files
- Configurable encoding and formatting options

## Installation
The library is built for .NET 8.0 and can be referenced directly in your project.

## Classes

### CsvDataReader
The main class for reading CSV files. Implements `ICsvDataReader` which extends `IDataReader`.

#### Key Methods:
- `Open(Stream stream, CsvDataReaderOptions options = null, bool leaveOpen = false)` - Opens a CSV stream for reading
- `Open(string filename, CsvDataReaderOptions options = null)` - Opens a CSV file by filename
- `Read()` - Reads the next record
- `Close()` - Closes the reader
- `GetRawRecord()` - Gets the raw record string
- `IsVirtualField(int i)` - Checks if a field is virtual

#### Properties:
- `FieldCount` - Number of fields in the current record
- `RecordsReaded` - Number of records read
- `Headers` - Collection of column headers
- `Filename` - Name of the source file

### CsvDataWriter
The main class for writing CSV files.

#### Key Methods:
- `CsvDataWriter(string path, CsvDataWriterOptions options = null)` - Creates a writer for a file path
- `Write(string[] record)` - Writes a record to the file
- `Write(CsvRow row)` - Writes a CsvRow object
- `NewRow()` - Creates a new CsvRow instance

### CsvColumn
Represents a column in a CSV file with properties for name, ordinal position, data type, and virtual status.

### CsvDataReaderOptions
Configuration options for reading CSV files:
- `Encoding` - Text encoding (default: Encoding.Default)
- `Delimiter` - Field delimiter (default: ";")
- `Quote` - Quote character (default: '"')
- `FirstLineIsHeader` - Whether the first line contains headers (default: true)
- `IgnoreBlankLines` - Whether to ignore blank lines (default: true)
- `VirtualFields` - List of virtual fields to add

### CsvDataWriterOptions
Configuration options for writing CSV files:
- `Encoding` - Text encoding (default: Encoding.Default)
- `Delimiter` - Field delimiter (default: ";")
- `Quote` - Quote character (default: '"')
- `RequireFieldQuotation` - Whether to require field quotation (default: false)
- `Headers` - List of headers to write

## Usage Examples

### Reading a CSV File
```csharp
using NG.CsvData;

var reader = CsvDataReader.Open("data.csv");
while(reader.Read())
{
    string firstName = reader["FirstName"].ToString();
    string lastName = reader["LastName"].ToString();
    // Process the data
}
reader.Dispose();
```

### Writing a CSV File
```csharp
using NG.CsvData;

var options = new CsvDataWriterOptions()
{
    Headers = new List<string> {"Name", "Age", "City"}
};

using(var writer = new CsvDataWriter("output.csv", options))
{
    var row = writer.NewRow();
    row["Name"] = "John Doe";
    row["Age"] = "30";
    row["City"] = "New York";
    writer.Write(row);
}
```

## Data Types
The library supports various data types through the `CsvDbType` enumeration:
- String
- Int32
- DateTime
- Boolean
- Byte
- Char
- Decimal
- Double
- Single
- Guid
- Int16
- Int64
- Binary
- Timespan
- DateTimeOffset

## Virtual Fields
Virtual fields allow you to add computed or derived fields during reading without modifying the original CSV file. You can handle the `OnGetVirtualColumnValue` event to provide values for virtual fields.

## Error Handling
The library throws standard .NET exceptions when encountering malformed data or I/O errors. FormatExceptions are thrown when data type conversion fails.

## Performance Considerations
- The library uses buffered reading for efficient processing of large files
- Memory usage scales with the width of the CSV (number of columns) rather than the length
- Streams are processed incrementally to minimize memory footprint

## License
This library is licensed under the terms specified in the LICENSE file.