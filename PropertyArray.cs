using System;
using System.Linq;

namespace ProperTree
{
	public class PropertyArray<T> : Property, IPropertyArray
		where T : struct
	{
		/// <summary>
		///   Gets the underlying array of this array property.
		///   <seealso cref="Property.As{T}"/>
		///   <seealso cref="PropertyPrimitiveExtensions.GetValue"/>
		/// </summary>
		public T[] Array { get; }
		
		public Type ElementType
			=> Array.GetType().GetElementType();
		
		private PropertyArray(T[] array)
			=> Array = array;
		
		public static void RegisterConverters()
		{
			PropertyRegistry.RegisterToPropertyConverter(
				(T[] value) => new PropertyArray<T>(value));
			PropertyRegistry.RegisterFromPropertyConverter(
				(PropertyArray<T> property) => property.Array);
		}
		
		public override bool Equals(Property property)
			=> (property is PropertyArray<T> array)
				&& Array.SequenceEqual(array.Array);
	}
	
	public interface IPropertyArray
	{
		Type ElementType { get; }
	}
}
