using System;
using System.IO;

namespace ProperTree.Serialization.Binary
{
	public class BinaryDeSerializerPrimitive<T>
		: BinaryDeSerializer<PropertyPrimitive<T>>
	{
		public Func<BinaryReader, T> ReadFunction { get; }
		public Action<BinaryWriter, T> WriteFunction { get; }
		
		public BinaryDeSerializerPrimitive(Func<BinaryReader, T> read,
		                                   Action<BinaryWriter, T> write)
		{
			ReadFunction  = read;
			WriteFunction = write;
		}
		
		public override PropertyPrimitive<T> Read(BinaryReader reader)
			=> new PropertyPrimitive<T>(ReadFunction(reader));
		
		public override void Write(BinaryWriter writer, PropertyPrimitive<T> property)
			=> WriteFunction(writer, property.Value);
	}
}
