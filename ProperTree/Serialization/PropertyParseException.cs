using System;
using System.IO;

namespace ProperTree.Serialization
{
	/// <summary> An exception that's thrown when an error occurs
	///           during parsing or deserialization of a property. </summary>
	public class PropertyParseException : Exception
	{
		/// <summary> Gets the stream in which the parse
		///           exception occured, or null if unavailable. </summary>
		public Stream Stream { get; }
		
		/// <summary> Gets the position in the stream where the
		///           parse exception occured, or -1 if unavailable. </summary>
		public long Position
			=> ((Stream != null) && Stream.CanSeek)
				? Stream.Position : -1;
		
		public PropertyParseException(string message, Stream stream)
			: this(message, stream, null) {  }
		
		public PropertyParseException(string message, Stream stream, Exception innerException)
			: base(AppendStreamPosition(message, stream), innerException) { Stream = stream; }
		
		/// <summary> Appends the position and length of the stream to the message, if available. </summary>
		private static string AppendStreamPosition(string message, Stream stream)
			=> ((stream != null) && stream.CanSeek)
				? $"{ message } (Stream pos/len: { stream.Position }/{ stream.Length })"
				: message;
	}
}
