using System;
using System.Collections;
using System.Collections.Generic;

namespace ProperTree
{
	/// <summary>
	///   Base class for all Property classes. Used to store additional, extensible
	///   information on game objects in a common format that can be easily and
	///   compactly read from and written to file and network streams.
	///   
	///   <see cref="PropertyRegistry"/> is used to register default and custom
	///   Property types which can then be used to represent pre-defined data
	///   types and structures.
	/// </summary>
	public abstract class Property
		: IEquatable<Property>
	{
		/// <summary> Creates a Property from the specified value.
		///           If the value is already a Property, it is returned. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public static Property Of<T>(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is Property property) return property;
			if (PropertyRegistry.TryGetToPropertyConverter<T>(out var converter)) return converter(value);
			throw new NotSupportedException($"Can't convert to Property from type '{ typeof(T).ToFriendlyString() }'");
		}
		
		/// <summary> Creates a Property from the specified value.
		///           If the value is already a Property, it is returned. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public static Property Of(object value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is Property property) return property;
			if (PropertyRegistry.TryGetToPropertyConverter(value.GetType(), out var converter)) return converter(value);
			throw new NotSupportedException($"Can't convert to Property from type '{ value.GetType().ToFriendlyString() }'");
		}
		
		/// <summary>
		///   Converts or casts this Property to the specified type.
		///   
		///   Note that this works as an extension of the "as" operator:
		///   <code>
		/// property.As&lt;PropertyList&gt;() == (property as PropertyList)
		/// property.As&lt;object&gt;() == (property as object)
		/// // To get any primitive property's underlying value:
		/// var value = property.GetValue();
		///   </code>
		/// </summary>
		/// <exception cref="NotSupportedException"> Thrown if the specified property can't be converted to the specified type. </exception>
		public T As<T>()
		{
			if (this is T result) return result;
			if (PropertyRegistry.TryGetFromPropertyConverter<T>(GetType(), out var converter)) return converter(this);
			throw new NotSupportedException(
				$"Can't convert from '{ GetType().ToFriendlyString() }' to type '{ typeof(T).ToFriendlyString() }'");
		}
		
		/// <summary>
		///   Converts or casts this Property to the specified type, or the
		///   specified default value if the conversion did not succeed.
		///   
		///   Note that this works as an extension of the "as" operator:
		///   <code>
		/// property.As(new PropertyList()) == (property as PropertyList) ?? new PropertyList()
		/// property.As(null) == (property as object) ?? null
		/// 
		/// // To get any primitive property's underlying value:
		/// if (property.TryGetValue(out var value)) { ... }
		///   </code>
		/// </summary>
		public T As<T>(T @default)
			=> (this is T result) ? result
				: PropertyRegistry.TryGetFromPropertyConverter<T>(GetType(), out var converter)
					? converter(this) : @default;
		
		/// <summary> Gets or sets the property at the specified index of this list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a list. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		public virtual Property this[int index] {
			get => throw new InvalidOperationException($"Not a list Property: '{ GetType().ToFriendlyString() }'");
			set => throw new InvalidOperationException($"Not a list Property: '{ GetType().ToFriendlyString() }'");
		}
		
		/// <summary>
		///   Gets or sets the property with the specified name of this dictionary property.
		///   
		///   If the specified name is found, a get operation will return the associated property,
		///   and a set operation will overwrite the existing entry with the specified value,
		///   or remove it if the specified value is null.
		///   
		///   If the specified name is not found, a get operation will return null,
		///   and a set operation adds a new entry with the specified value.
		/// </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name is null. </exception>
		public virtual Property this[string name] {
			get => throw new InvalidOperationException($"Not a dictionary Property: '{ GetType().ToFriendlyString() }'");
			set => throw new InvalidOperationException($"Not a dictionary Property: '{ GetType().ToFriendlyString() }'");
		}
		
		
		// IEquatable implementation
		
		/// <summary> Returns if the specified property's value
		///           (recursively) equals this property's value. </summary>
		public abstract bool Equals(Property property);
	}
}
