using System.IO;

namespace ProperTree.Serialization.Binary
{
	// TODO: Use protobuf to write these?
	public class BinaryDeSerializerArray<T>
		: BinaryDeSerializer<PropertyArray<T>>
		where T : struct
	{
		private readonly BinaryDeSerializerPrimitive<T> _primitiveDeSerializer
			= (BinaryDeSerializerPrimitive<T>)BinaryDeSerializerRegistry
				.GetByType<PropertyPrimitive<T>>(out var _);
		
		public override PropertyArray<T> Read(BinaryReader reader)
		{
			var length = reader.ReadInt32();
			var array  = new T[length];
			for (var i = 0; i < length; i++)
				array[i] = _primitiveDeSerializer.ReadFunction(reader);
			return new PropertyArray<T>(array);
		}
		
		public override void Write(BinaryWriter writer, PropertyArray<T> array)
		{
			writer.Write(array.Array.Length);
			foreach (var value in array.Array)
				_primitiveDeSerializer.WriteFunction(writer, value);
		}
	}
}
