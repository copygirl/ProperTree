using System;
using System.IO;

namespace ProperTree.Serialization.Binary
{
	public class BinaryDeSerializerMap
		: BinaryDeSerializer<PropertyMap>
	{
		public override PropertyMap Read(BinaryReader reader)
		{
			var dictionary = new PropertyMap();
			var count = reader.ReadUInt16();
			if (count > PropertyMap.MAX_SIZE) throw new Exception(
				$"{ nameof(PropertyMap) } count is larger than " +
				$"MAX_SIZE ({ count } > { PropertyMap.MAX_SIZE })");
			for (var i = 0; i < count; i++) {
				var name     = reader.ReadString();
				var property = BinaryDeSerializerRegistry.ReadProperty(reader);
				dictionary.Add(name, property);
			}
			return dictionary;
		}
		
		public override void Write(BinaryWriter writer, PropertyMap dictionary)
		{
			writer.Write((ushort)dictionary.Count);
			foreach (var entry in dictionary) {
				writer.Write(entry.Key);
				BinaryDeSerializerRegistry.WriteProperty(writer, entry.Value);
			}
		}
	}
}
