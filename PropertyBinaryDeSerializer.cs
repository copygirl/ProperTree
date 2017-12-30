using System;
using System.IO;

namespace ProperTree
{
	public abstract class PropertyBinaryDeSerializer<TProperty>
			: IPropertyBinaryDeSerializer
		where TProperty : Property
	{
		public abstract TProperty Read(BinaryReader reader);
		
		public abstract void Write(BinaryWriter writer, TProperty value);
		
		// IPropertyBinaryDeSerializer implementation
		Property IPropertyBinaryDeSerializer.Read(BinaryReader reader)
			=> Read(reader);
		void IPropertyBinaryDeSerializer.Write(BinaryWriter writer, Property value)
			=> Write(writer, (TProperty)value);
	}
	
	public class PropertyBinaryDeSerializerLambda<TProperty>
			: PropertyBinaryDeSerializer<TProperty>
		where TProperty : Property
	{
		private readonly Func<BinaryReader, TProperty> _read;
		private readonly Action<BinaryWriter, TProperty> _write;
		
		public PropertyBinaryDeSerializerLambda(
				Func<BinaryReader, TProperty> read,
				Action<BinaryWriter, TProperty> write)
			{ _read = read; _write = write; }
		
		public override TProperty Read(BinaryReader reader)
			=> _read(reader);
		
		public override void Write(BinaryWriter writer, TProperty value)
			=> _write(writer, value);
	}
	
	public interface IPropertyBinaryDeSerializer
	{
		Property Read(BinaryReader reader);
		
		void Write(BinaryWriter writer, Property value);
	}
}
