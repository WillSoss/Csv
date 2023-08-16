namespace WillSoss.Csv.Tests
{
    public class CsvReaderTests
    {
        [Fact]
        public void ShouldReadCsv()
        {
            using (var r = new CsvReader(@"Test Files\Csv1.csv"))
            {
                var data = r.Read()!;

                Assert.Equal(8, data.Length);
                Assert.Equal("\"", data[0]);
                Assert.Equal(",", data[1]);
                Assert.Equal("hello", data[2]);
                Assert.Equal("world", data[3]);
                Assert.Equal("hello,world", data[4]);
                Assert.Equal(@"line
break", data[5]);
                Assert.Equal("space in field", data[6]);
                Assert.Equal("\"space,outside\"", data[7]);
            }
        }

        [Fact]
        public void ShouldReadCsvWithQualifiedFieldBeforeLineBreak()
        {
            using (var r = new CsvReader(@"Test Files\Csv2.csv"))
            {
                var data = r.Read()!;

                Assert.Equal(3, data.Length);
                Assert.Equal("Tax Authority", data[0]);
                Assert.Equal("Account Number", data[1]);
                Assert.Equal("Property Details Link", data[2]);
            }
        }
    }
}
