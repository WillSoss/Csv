using System.Collections;
using System.Text;

namespace WillSoss.Csv
{
    /// <summary>
    /// Writes comma separated value (CSV) files.
    /// </summary>
    public class CsvWriter : IDisposable, IAsyncDisposable
    {
        public const char DefaultQualifier = '"';
        public const char DefaultDelimiter = ',';
        public static readonly string DefaultNewLine = Environment.NewLine;

        private readonly StreamWriter _writer;
        private bool _disposed = false;

        private char _qualifier;
        private char _delimiter;
        private string _newLine;
        private string _escapedQualifier;
        private char[] _qualifierNeeded = new char[] 
        {
            DefaultQualifier, DefaultDelimiter,
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', 
            '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F',
            '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017',
            '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F', '\u007F'
        };

        /// <summary>
        /// Initializes a new <see cref="CsvWriter"/> to write to the file at <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The file to write to.</param>
        /// <param name="mode">The <see cref="FileMode"/> with which to open the file for writing.</param>
		/// <param name="qualifier">The character that surrounds a single field. Quotation marks ("), also known as double quotes, by default.</param>
		/// <param name="delimiter">The character that separates one field from the next. Comma (,) by default.</param>
        /// <param name="newLine">The character(s) to insert between records, typically a OS-specific new line. <see cref="Environment.NewLine"/> by default.</param>
		/// <param name="encoding">The character encoding to use. If null, <see cref="Encoding.UTF8"/> will be used.</param>
        public CsvWriter(string filePath, FileMode mode = FileMode.CreateNew, char qualifier = DefaultQualifier, char delimiter = DefaultDelimiter, string? newLine = null, Encoding? encoding = null)
            : this(File.Open(filePath, mode), qualifier, delimiter, newLine, encoding) { }

        /// <summary>
        /// Initializes a new <see cref="CsvWriter"/> to write to the file at <paramref name="filePath"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="qualifier">The character that surrounds a single field. Quotation marks ("), also known as double quotes, by default.</param>
        /// <param name="delimiter">The character that separates one field from the next. Comma (,) by default.</param>
        /// <param name="newLine">The character(s) to insert between records, typically a OS-specific new line. <see cref="Environment.NewLine"/> by default.</param>
        /// <param name="encoding">The character encoding to use. If null, <see cref="Encoding.UTF8"/> will be used.</param>
        public CsvWriter(Stream stream, char qualifier, char delimiter, string? newLine = null, Encoding? encoding = null)
        {
            _writer = new StreamWriter(stream, encoding ?? Encoding.UTF8);
            _delimiter = delimiter;
            _qualifier = qualifier;
            _newLine = newLine ?? DefaultNewLine;
            _escapedQualifier = new string(qualifier, 2);
            _qualifierNeeded[0] = qualifier;
            _qualifierNeeded[1] = delimiter;
        }

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        public Stream BaseStream
        {
            get { return _writer.BaseStream; }
        }

        /// <summary>
        /// Writes a record to the file.
        /// </summary>
        /// <param name="record">The fields making up a single record to write.</param>
        public void Write(params string[] record)
        {
            Write((IEnumerable)record);
        }

        /// <summary>
        /// Writes a record to the file.
        /// </summary>
        /// <param name="record">The fields making up a single record to write.</param>
        public void Write(params object[] record)
        {
            Write((IEnumerable)record);
        }

        /// <summary>
        /// Writes a record to the file.
        /// </summary>
        /// <param name="record">The fields making up a single record to write.</param>
        public void Write(IEnumerable record)
        {
			ThrowIfDisposed();

            bool empty = true;

            foreach (var value in record)
            {
                if (empty)
                    empty = false;
                else
                    _writer.Write(_delimiter);

                _writer.Write(GetField(value));
            }

            if (empty)
                throw new ArgumentException("The record contains no fields.", nameof(record));
            else
                _writer.Write(_newLine);
        }

        /// <summary>
        /// Writes a record to the file.
        /// </summary>
        /// <param name="record">The fields making up a single record to write.</param>
        public async Task WriteAsync(params string[] record)
        {
            await WriteAsync((IEnumerable)record);
        }

        /// <summary>
        /// Writes a record to the file.
        /// </summary>
        /// <param name="record">The fields making up a single record to write.</param>
        public async Task WriteAsync(params object[] record)
        {
            await WriteAsync((IEnumerable)record);
        }

        /// <summary>
        /// Writes a record to the file.
        /// </summary>
        /// <param name="record">The fields making up a single record to write.</param>
        public async Task WriteAsync(IEnumerable record)
        {
            ThrowIfDisposed();

            bool empty = true;

            foreach (var value in record)
            {
                if (empty)
                    empty = false;
                else
                    await _writer.WriteAsync(_delimiter);

                await _writer.WriteAsync(GetField(value));
            }

            if (empty)
                throw new ArgumentException("The record contains no fields.", nameof(record));
            else
                await _writer.WriteAsync(_newLine);
        }

        private string GetField(object value)
        {
            var field = (value ?? string.Empty).ToString()!;

            if (field.IndexOfAny(_qualifierNeeded) > -1)
                field = _qualifier + field.Replace(_qualifier.ToString(), _escapedQualifier) + _qualifier;

            return field;
        }

        /// <summary>
        /// Clears all buffers for the writer and causes any buffered data to be written to the underlying stream.
        /// </summary>
		public void Flush()
        {
            ThrowIfDisposed();
            _writer.Flush();
        }

        /// <summary>
        /// Clears all buffers for the stream asynchronously and causes any buffered data to be written to the underlying device.
        /// </summary>
        public async Task FlushAsync()
        {
            ThrowIfDisposed();
            await _writer.FlushAsync();
        }

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(CsvWriter),  "Cannot invoke method after the object is disposed");
		}

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _writer?.Flush();
                _writer?.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;

                await _writer.FlushAsync();
                await _writer.DisposeAsync();
            }
        }
    }
}
