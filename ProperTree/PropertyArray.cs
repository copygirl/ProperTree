using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ProperTree
{
	public class PropertyArray<T>
		: Property, IPropertyArray
			where T : struct
	{
		public Type ElementType
			=> Array.GetType().GetElementType();
		
		/// <summary>
		///   Gets the underlying array of this array property.
		///   <seealso cref="Property.As{T}"/>
		///   <seealso cref="PropertyPrimitiveExtensions.GetValue"/>
		/// </summary>
		public T[] Array { get; }
		
		internal PropertyArray(T[] array)
			=> Array = array;
		
		public static void RegisterConverters()
		{
			PropertyConverterRegistry.RegisterToProperty(
				(T[] value) => new PropertyArray<T>(value));
			PropertyConverterRegistry.RegisterFromProperty(
				(PropertyArray<T> property) => property.Array);
		}
		
		public override bool Equals(Property property)
			=> (property is PropertyArray<T> array)
				&& Array.SequenceEqual(array.Array);
		
		
		// De/serializer
		
		// TODO: Use protobuf to write these?
		public class DeSerializer
			: BinaryDeSerializer<PropertyArray<T>>
		{
			private readonly PropertyPrimitive<T>.DeSerializer _primitiveDeSerializer
				= (PropertyPrimitive<T>.DeSerializer)BinaryDeSerializerRegistry
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
	
	public interface IPropertyArray
	{
		/// <summary> Gets the element type of this array property's underlying array. </summary>
		Type ElementType { get; }
	}
	
	public class PropertyByteArrayDeSerializer
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
