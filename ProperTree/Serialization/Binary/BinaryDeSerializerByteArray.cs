using System.IO;

namespace ProperTree.Serialization.Binary
{
	public class BinaryDeSerializerByteArray
		: BinaryDeSerializer<PropertyArray<byte>>
	{
		public override PropertyArray<byte> Read(BinaryReader reader)
		{
			var length = reader.ReadInt32();
			var array  = reader.ReadBytes(length);
			return new PropertyArray<byte>(array);
		}
		
		public override void Write(BinaryWriter writer, PropertyArray<byte> value)
		{
			writer.Write(value.Array.Length);
			writer.Write(value.Array);
		}
	}
}
