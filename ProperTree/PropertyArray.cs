using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ProperTree.Serialization;
using ProperTree.Utility;

namespace ProperTree
{
	public class PropertyArray<T>
		: IProperty, IPropertyArray
		where T : struct
	{
		public Type ElementType
			=> Array.GetType().GetElementType();
		
		/// <summary>
		///   Gets the underlying array of this array property.
		///   
		///   <seealso cref="Property.As{T}"/>
		///   <seealso cref="PropertyPrimitiveExtensions.GetValue"/>
		/// </summary>
		public T[] Array { get; }
		
		internal PropertyArray(T[] array)
			=> Array = array;
		
		public IProperty this[int index] {
			get => throw new InvalidOperationException($"Not a list property: '{ GetType().ToFriendlyName() }'");
			set => throw new InvalidOperationException($"Not a list property: '{ GetType().ToFriendlyName() }'");
		}
		
		public IProperty this[string name] {
			get => throw new InvalidOperationException($"Not a map property: '{ GetType().ToFriendlyName() }'");
			set => throw new InvalidOperationException($"Not a map property: '{ GetType().ToFriendlyName() }'");
		}
		
		public static void RegisterConverters()
		{
			PropertyConverterRegistry.RegisterToProperty(
				(T[] value) => new PropertyArray<T>(value));
			PropertyConverterRegistry.RegisterFromProperty(
				(PropertyArray<T> property) => property.Array);
		}
		
		
		// IEquatable implementation
		
		public bool Equals(IProperty property)
			=> (property is PropertyArray<T> array)
				&& Array.SequenceEqual(array.Array);
	}
	
	public interface IPropertyArray
	{
		/// <summary> Gets the element type of this array property's underlying array. </summary>
		Type ElementType { get; }
	}
}
