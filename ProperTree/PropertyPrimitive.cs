using System;
using System.Collections.Generic;
using System.IO;
using ProperTree.Serialization;
using ProperTree.Utility;

namespace ProperTree
{
	/// <summary>
	///   Property which holds a single primitive value.
	///   
	///   Examples:
	///   <code>
	/// var intProp  = Property.Of(1234);
	/// var byteProp = Property.Of&lt;byte&gt;(123);
	/// object probablyString = "I'm a string! Wooo!";
	/// var stringProp = Property.Of(probablyString);
	/// 
	/// intProp.As&lt;int&gt;() == 1234
	/// byteProp.As&lt;byte&gt;() == (byte)123
	/// stringProp.As&lt;string&gt;().Length == 19
	/// 
	/// var all = new Property[]{ intProp, byteProp, stringProp };
	/// var rnd = all[new Random().Next(all.Length)];
	/// Console.WriteLine($"Value: { rnd.GetValue() }");
	///   </code>
	/// </summary>
	public class PropertyPrimitive<T>
		: Property, IPropertyPrimitive
	{
		/// <summary>
		///   Gets the underlying value of this primitive property.
		///   <seealso cref="Property.As{T}"/>
		///   <seealso cref="PropertyPrimitiveExtensions.GetValue"/>
		/// </summary>
		public T Value { get; protected set; }
		
		internal PropertyPrimitive(T value)
			=> Value = value;
		
		public static void RegisterConverters()
		{
			PropertyConverterRegistry.RegisterToProperty(
				(T value) => new PropertyPrimitive<T>(value));
			PropertyConverterRegistry.RegisterFromProperty(
				(PropertyPrimitive<T> property) => property.Value);
		}
		
		public override bool Equals(Property other)
			=> (other is PropertyPrimitive<T> primitive)
				&& EqualityComparer<T>.Default.Equals(Value, primitive.Value);
		
		// IPropertyPrimitive implementation
		object IPropertyPrimitive.Value => Value;
	}
	
	public interface IPropertyPrimitive
	{
		/// <summary> Gets the underlying value of this primitive property. </summary>
		object Value { get; }
	}
	
	public static class PropertyPrimitiveExtensions
	{
		/// <summary>
		///   Attempts to get the underlying value of the specified primitive property.
		///   
		///   If you wish to get the primitive property's value as a specific type,
		///   rather than an object, use <see cref="Property.As{T}(T)"/> instead.
		/// </summary>
		public static bool TryGetValue(this Property self, out object value)
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (self is IPropertyPrimitive primitive)
				{ value = primitive.Value; return true; }
			else { value = null; return false; }
		}
		
		/// <summary>
		///   Gets the underlying value of the specified primitive property.
		///   
		///   If you wish to get the primitive property's value as a specific type,
		///   rather than an object, use <see cref="Property.As{T}"/> instead.
		/// </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not primitive. </exception>
		public static object GetValue(this Property self)
		{
			if (!self.TryGetValue(out var value)) throw new InvalidOperationException(
				$"Not a primitive Property: '{ self.GetType().ToFriendlyName() }'");
			return value;
		}
	}
}
