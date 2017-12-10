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
	///   Property types which can then be used to represent hold pre-defined
	///   data types and structures.
	/// </summary>
	public abstract class Property
		: IReadOnlyCollection<Property>
		, IReadOnlyCollection<KeyValuePair<string, Property>>
	{
		/// <summary> Gets whether this property is a primitive property. </summary>
		public virtual bool IsPrimitive => false;
		/// <summary> Gets whether this property is a list property. </summary>
		public virtual bool IsList => false;
		/// <summary> Gets whether this property is a dictionary property. </summary>
		public virtual bool IsDictionary => false;
		/// <summary> Gets whether this property is a collection property. </summary>
		public virtual bool IsCollection => false;
		
		
		/// <summary>
		///   
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public static Property Of<T>(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is Property property) return property;
			if (!PropertyRegistry.TryGetToPropertyConverter<T>(out var converter))
				throw new NotSupportedException($"Can't convert to Property from type '{ typeof(T).GetFriendlyName(true) }'");
			return converter(value);
		}
		
		/// <summary>
		///   
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public static Property Of(object value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is Property property) return property;
			if (!PropertyRegistry.TryGetToPropertyConverter(value.GetType(), out var converter))
				throw new NotSupportedException($"Can't convert to Property from type '{ value.GetType().GetFriendlyName(true) }'");
			return converter(value);
		}
		
		/// <summary>
		///   
		/// </summary>
		/// <exception cref="NotSupportedException"> Thrown if the specified property can't be converted to the specified type. </exception>
		public T As<T>()
		{
			if (!PropertyRegistry.TryGetFromPropertyConverter<T>(GetType(), out var converter))
				throw new NotSupportedException($"Can't convert from '{ GetType().GetFriendlyName() }' to type '{ typeof(T).GetFriendlyName(true) }'");
			return converter(this);
		}
		
		/// <summary>
		///   
		/// </summary>
		public T As<T>(T @default)
			=> PropertyRegistry.TryGetFromPropertyConverter<T>(GetType(), out var converter)
				? converter(this) : @default;
		
		
		/// <summary> Gets the underlying value of this property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not primitive. </exception>
		public virtual object Value
			=> throw new InvalidOperationException($"Not a primitive Property: '{ GetType().GetFriendlyName() }'");
		
		
		/// <summary> Gets or sets the property at the specified index of this list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a list. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified value's property type is not
		///                                      the same as the existing properties in this list. </exception>
		public virtual Property this[int index] {
			get => throw new InvalidOperationException($"Not a list Property: '{ GetType().GetFriendlyName() }'");
			set => throw new InvalidOperationException($"Not a list Property: '{ GetType().GetFriendlyName() }'");
		}
		
		/// <summary> Adds the specified property to the end of this list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a list. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified property's type is not
		///                                      the same as the existing properties in this list. </exception>
		public void Add(Property property)
			=> Insert(Count, property);
		
		/// <summary> Adds the specified value to the end of this list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a list. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified value's property type is not
		///                                      the same as the existing properties in this list. </exception>
		public void Add<T>(T value)
			=> Add(Property.Of(value));
		
		/// <summary> Inserts the specified property at the specified index in this list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a list. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified property's type is not
		///                                      the same as the existing properties in this list. </exception>
		public virtual void Insert(int index, Property property)
			=> throw new InvalidOperationException($"Not a list Property: '{ GetType().GetFriendlyName() }'");
		
		/// <summary> Inserts the specified value at the specified index in this list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a list. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified value's property type is not
		///                                      the same as the existing properties in this list. </exception>
		public void Insert<T>(int index, T value)
			=> Insert(index, Property.Of(value));
		
		/// <summary> Removes the child at the specified index from this list property, and returns the removed value. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a list. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		public virtual Property RemoveAt(int index)
			=> throw new InvalidOperationException($"Not a list Property: '{ GetType().GetFriendlyName() }'");
		
		
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
			get => throw new InvalidOperationException($"Not a dictionary Property: '{ GetType().GetFriendlyName() }'");
			set => throw new InvalidOperationException($"Not a dictionary Property: '{ GetType().GetFriendlyName() }'");
		}
		
		// TODO: Add "TryAdd", "TryRemove", etc instead..?
		
		/// <summary> Adds the an entry with the specified name and property to this dictionary property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		public virtual void Add(string name, Property property)
			=> throw new InvalidOperationException($"Not a dictionary Property: '{ GetType().GetFriendlyName() }'");
		
		/// <summary> Adds the an entry with the specified name and value to this dictionary property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name or value is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public void Add<T>(string name, T value)
			=> Add(name, Property.Of(value));
		
		/// <summary> Removes the entry with the specified name from this dictionary property,
		///           and returns the removed value, or null if the name was not found. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name is null. </exception>
		public virtual Property Remove(string name)
			=> throw new InvalidOperationException($"Not a dictionary Property: '{ GetType().GetFriendlyName() }'");
		
		
		// IReadOnlyCollection dummy implementation
		
		/// <summary> Gets the number of immediate child properties of this collection property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a collection. </exception>
		public virtual int Count => throw new InvalidOperationException($"Not a collection Property: '{ GetType().GetFriendlyName() }'");
		
		/// <summary> Returns an enumerator that iterates through this collection property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a collection. </exception>
		public virtual IEnumerator<Property> GetEnumerator()
			=> throw new InvalidOperationException($"Not a collection Property: '{ GetType().GetFriendlyName() }'");
		
		
		protected virtual IEnumerator<KeyValuePair<string, Property>> GetDictionaryEnumerator()
			=> throw new InvalidOperationException($"Not a dictionary Property: '{ GetType().GetFriendlyName() }'");
		
		IEnumerator<KeyValuePair<string, Property>> IEnumerable<KeyValuePair<string, Property>>.GetEnumerator()
			=> GetDictionaryEnumerator();
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
