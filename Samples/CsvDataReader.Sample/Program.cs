// See https://aka.ms/new-console-template for more information
using NG.CsvData;
using System.Text;

string csvFilename =  @".\Sample.csv";
Console.WriteLine(csvFilename);

if (!File.Exists(csvFilename))
    throw new FileNotFoundException();

 
/* Открываем ридер для чтения из файла Samples.csv с параметрами:
 *  - разделитель столбцов: ";"
 *  - кодировка файла: UTF-8
 *  - первая строка - заголовок: true
 *  - ограничитель строк: '"'
 *  - виртуальные поля: "Id" */
using CsvDataReader reader = CsvDataReader.Open(csvFilename,
        options: new()
        {
            Delimiter = ";",
            Encoding = Encoding.UTF8,
            FirstLineIsHeader = true,
            Quote = '"',
            VirtualFields = ["Id"],
        });

// значение последовательности для определения виртуального поля Id
int sequence = 0;

// Определить событие получения значения виртуального поля
reader.OnGetVirtualColumnValue += reader_OnGetVirtualColumnValue;

void reader_OnGetVirtualColumnValue(object? sender, GetVirtualColumnEventArgs e)
{
    if (e.VirtualColumn.Name == "Id")
    {
        e.Value = sequence;
    }
}

// Вывести в консоль заголовок
Console.WriteLine(new string('-', 45));
Console.WriteLine($"|{reader.Headers["Id"].Name,-5}|{reader.Headers["Name"].Name,-15}|{reader.Headers[1].Name,-5}|{reader.Headers["City"].Name,-15}|");
Console.WriteLine(new string('-', 45));

// Прочитать построчно файл
while (reader.Read())
{
    sequence++;
    Console.WriteLine($"|{reader["Id"],5}|{reader.GetString(0),-15}|{reader.GetString(1),5}|{reader.GetString(2),-15}|");
}

Console.WriteLine(new string('-', 45));




