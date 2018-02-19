using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ProperTree.Serialization;

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
	}
	
	public interface IPropertyArray
	{
		/// <summary> Gets the element type of this array property's underlying array. </summary>
		Type ElementType { get; }
	}
}
