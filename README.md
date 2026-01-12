# CsvData Library

CsvData is a .NET library for reading and writing CSV files with advanced functionality. It implements the `IDataReader` interface, making it compatible with data processing workflows that expect this standard interface.

## Features
- Read and write CSV files with customizable delimiters and quotes
- Support for virtual columns that can be computed during reading
- Automatic data type conversion with configurable parsing
- Compatibility with .NET's `IDataReader` interface
- Support for multiple streams and files
- Configurable encoding and formatting options

## Installation
The library is built for .NET 8.0 and can be referenced directly in your project.

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

## Documentation
For detailed documentation, see:
- [English Documentation](Documentation.en.md)
- [Russian Documentation](Documentation.ru.md)

## License
This project is licensed under the terms specified in the [LICENSE](LICENSE) file.