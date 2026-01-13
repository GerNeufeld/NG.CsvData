// See https://aka.ms/new-console-template for more information
using NG.CsvData;
using System.Text;

string csvFilename =  @".\Sample.csv";
Console.WriteLine(csvFilename);

if (!File.Exists(csvFilename))
    throw new FileNotFoundException();

 
/* Opening the reader to read from the Samples.csv file with parameters:
 *  - column delimiter: ";"
 *  - file encoding: UTF-8
 *  - first line is header: true
 *  - quote character: '"'
 *  - virtual fields: "Id" */
using CsvDataReader reader = CsvDataReader.Open(csvFilename,
        options: new()
        {
            Delimiter = ";",
            Encoding = Encoding.UTF8,
            FirstLineIsHeader = true,
            Quote = '"',
            VirtualFields = ["Id"],
        });

// sequence value for determining the virtual field Id
int sequence = 0;

// Define the event for getting the virtual field value
reader.OnGetVirtualColumnValue += reader_OnGetVirtualColumnValue;

void reader_OnGetVirtualColumnValue(object? sender, GetVirtualColumnEventArgs e)
{
    if (e.VirtualColumn.Name == "Id")
    {
        e.Value = sequence;
    }
}

// Print the header to console
Console.WriteLine(new string('-', 45));
Console.WriteLine($"|{reader.Headers["Id"].Name,-5}|{reader.Headers["Name"].Name,-15}|{reader.Headers[1].Name,-5}|{reader.Headers["City"].Name,-15}|");
Console.WriteLine(new string('-', 45));

// Read the file line by line
while (reader.Read())
{
    sequence++;
    Console.WriteLine($"|{reader["Id"],5}|{reader.GetString(0),-15}|{reader.GetString(1),5}|{reader.GetString(2),-15}|");
}

Console.WriteLine(new string('-', 45));




