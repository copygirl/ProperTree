using System;
using System.Collections.Generic;
using System.IO;

namespace ProperTree.Serialization.Binary
{
	public class BinaryDeSerializerList
		: BinaryDeSerializer<PropertyList>
	{
		public override PropertyList Read(BinaryReader reader)
		{
			var count = reader.ReadUInt16();
			if (count > PropertyList.MAX_SIZE) throw new Exception(
				"PropertyList count is larger than MAX_SIZE " +
				$"({ count } > { PropertyList.MAX_SIZE })");
			var list = new List<Property>(count);
			for (var i = 0; i < count; i++)
				list.Add(BinaryDeSerializerRegistry.ReadProperty(reader));
			return new PropertyList(list);
		}
		
		public override void Write(BinaryWriter writer, PropertyList list)
		{
			writer.Write((ushort)list.Count);
			foreach (var property in list)
				BinaryDeSerializerRegistry.WriteProperty(writer, property);
		}
	}
}
