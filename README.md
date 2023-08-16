# WillSoss.Csv
A lightweight, efficient, and complete implementation that will correctly deal with escaped qualifiers and line breaks in fields.

## Reading Files

```
using var reader = new CsvReader("c:\my-data.csv");

string[] record = null;
while ((record = reader.Read()) != null)
{
    Console.WriteLine($"First field: {record[0]}");
    Console.WriteLine($"Second field: {record[1]}");
}
```

### Qualifiers and Delimiters
`CsvReader` can read files with non-standard qualifiers (usually quotes: "This is qualified") and delimiters (usually commas: 1,2,3) by specifying the character used for each in the constructor:

```
// Reads a file with data like: ~field 1~;~field 2~
new CsvReader(filename, '~', ';');
```
## Writing Files

```
using var writer = new CsvWriter("c:\my-data.csv");

writer.Write(new string[] { "first", "second", "third" });

await writer.WriteAsync(new int[] { 1, 2, 3 });
```
