using System;
using System.IO;

namespace ProperTree.Serialization.Binary
{
	public class BinaryDeSerializerDictionary
		: BinaryDeSerializer<PropertyDictionary>
	{
		public override PropertyDictionary Read(BinaryReader reader)
		{
			var dictionary = new PropertyDictionary();
			var count = reader.ReadUInt16();
			if (count > PropertyDictionary.MAX_SIZE) throw new Exception(
				"PropertyDictionary count is larger than MAX_SIZE " +
				$"({ count } > { PropertyDictionary.MAX_SIZE })");
			for (var i = 0; i < count; i++) {
				var name     = reader.ReadString();
				var property = BinaryDeSerializerRegistry.ReadProperty(reader);
				dictionary.Add(name, property);
			}
			return dictionary;
		}
		
		public override void Write(BinaryWriter writer, PropertyDictionary dictionary)
		{
			writer.Write((ushort)dictionary.Count);
			foreach (var entry in dictionary) {
				writer.Write(entry.Key);
				BinaryDeSerializerRegistry.WriteProperty(writer, entry.Value);
			}
		}
	}
}
