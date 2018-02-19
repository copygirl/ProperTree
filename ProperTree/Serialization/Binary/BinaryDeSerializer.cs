using System;
using System.IO;

namespace ProperTree.Serialization.Binary
{
	public abstract class BinaryDeSerializer<TProperty>
		: IBinaryDeSerializer
		where TProperty : Property
	{
		public abstract TProperty Read(BinaryReader reader);
		
		public abstract void Write(BinaryWriter writer, TProperty value);
		
		// IBinaryDeSerializer implementation
		
		public Type PropertyType => typeof(TProperty);
		
		Property IBinaryDeSerializer.Read(BinaryReader reader)
			=> Read(reader);
		void IBinaryDeSerializer.Write(BinaryWriter writer, Property value)
			=> Write(writer, (TProperty)value);
	}
	
	public interface IBinaryDeSerializer
	{
		Type PropertyType { get; }
		
		Property Read(BinaryReader reader);
		
		void Write(BinaryWriter writer, Property value);
	}
}
