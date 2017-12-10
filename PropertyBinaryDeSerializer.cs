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
	
	public interface IPropertyBinaryDeSerializer
	{
		Property Read(BinaryReader reader);
		
		void Write(BinaryWriter writer, Property value);
	}
}
