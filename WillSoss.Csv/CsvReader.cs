using System.Text;

namespace WillSoss.Csv
{
    /// <summary>
    /// Reads comma separated value (CSV) files.
    /// </summary>
    public class CsvReader : IDisposable
	{
		public const char DefaultQualifier = '"';
		public const char DefaultDelimiter = ',';

		private bool _disposed = false;
		private StreamReader _reader;

		private char _qualifier;
		private char _delimiter;

		/// <summary>
		/// Initializes a new <see cref="CsvReader"/> to read the file at the <paramref name="filePath"/> specified.
		/// </summary>
		/// <param name="filePath">The file to read.</param>
		/// <param name="qualifier">The character that surrounds a single field. Quotation marks ("), also known as double quotes, by default.</param>
		/// <param name="delimiter">The character that separates one field from the next. Comma (,) by default.</param>
		/// <param name="encoding">The character encoding to use. If null, the <see cref="StreamReader"/> will attempt to detect the encoding used.</param>
        public CsvReader(string filePath, char qualifier = DefaultQualifier, char delimiter = DefaultDelimiter, Encoding? encoding = null)
            : this(File.OpenRead(filePath), qualifier, delimiter, encoding) { }

        /// <summary>
        /// Initializes a new <see cref="CsvReader"/> to read the <paramref name="stream"/> specified.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read.</param>
        /// <param name="qualifier">The character that surrounds a single field. Quotation marks ("), also known as double quotes, by default.</param>
        /// <param name="delimiter">The character that separates one field from the next. Comma (,) by default.</param>
        /// <param name="encoding">The character encoding to use. If null, the <see cref="StreamReader"/> will attempt to detect the encoding used.</param>
        public CsvReader(Stream stream, char qualifier = DefaultQualifier, char delimiter = DefaultDelimiter, Encoding? encoding = null)
        {
            _reader = new StreamReader(stream, encoding, encoding == null, -1, false);
            _delimiter = delimiter;
            _qualifier = qualifier;
        }

		public Stream BaseStream { get { return _reader.BaseStream; } }

		public bool EndOfFile { get { return _reader.EndOfStream; } }

        /// <summary>
        /// Reads the next record from the file. Returns null when the end of the file has been reached.
        /// </summary>
        /// <returns>A <see cref="string[]"/> of fields that make up the current record in the file.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="CsvReader"/> has been disposed.</exception>
        public string[]? Read()
		{
            if (_disposed)
                throw new ObjectDisposedException(nameof(CsvReader), "Cannot read after the object is disposed.");

            if (_reader.EndOfStream)
				return null;

			List<string> record = new List<string>();
			StringBuilder field = new StringBuilder();

			bool qualified = false;
			bool escapedQualifier = false;
			bool eatUntilDelimiter = false;

			while (!_reader.EndOfStream)
			{
				char c = (char)_reader.Read();

				if (!qualified && c == _delimiter)
				{
					record.Add(field.ToString());
					eatUntilDelimiter = false;
					field.Clear();
					continue;
				}

				if (!qualified && (c == '\r' || c == '\n'))
					eatUntilDelimiter = false;

				if (eatUntilDelimiter)
					continue;

				if (c == _qualifier)
				{
					if (escapedQualifier)
					{
						escapedQualifier = false;
						continue;
					}
					else if (!qualified)
					{
						qualified = true;
						field.Clear(); // dump anything before qualifier after delimeter
						continue;
					}
					else if (_reader.Peek() == -1 || ((char)_reader.Peek()) != _qualifier)
					{
						qualified = false;
						eatUntilDelimiter = true;
						continue;
					}
					else
					{
						escapedQualifier = true;
					}
				}

				if (!qualified)
				{
					if (c == '\n' || c == '\r')
					{
						if (_reader.Peek() != -1)
						{
							char next = (char)_reader.Peek();

							if ((c == '\r' && next == '\n') || (c == '\n' && next == '\r'))
								_reader.Read();
						}

						break;
					}
				}

				field.Append(c);
			}

			record.Add(field.ToString());

			return record.ToArray();
		}

		public void Dispose()
		{
			if (!this._disposed)
			{
				this._disposed = true;

				if (this._reader != null)
					this._reader.Dispose();
			}
		}
    }
}
