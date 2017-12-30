using System;
using System.Collections.Generic;
using System.IO;

namespace ProperTree
{
	public static class PropertyRegistry
	{
		public static int MIN_ID { get; } = 1;
		public static int MAX_ID { get; } = byte.MaxValue;
		
		private static IPropertyBinaryDeSerializer[] _binaryDeSerializersByID
			= new IPropertyBinaryDeSerializer[MAX_ID];
		private static Dictionary<Type, IPropertyBinaryDeSerializer> _binaryDeSerializersByType
			= new Dictionary<Type, IPropertyBinaryDeSerializer>();
		private static Dictionary<Type, ToPropertyConverter> _toPropertyConverters
			= new Dictionary<Type, ToPropertyConverter>();
		private static Dictionary<Tuple<Type, Type>, FromPropertyConverter> _fromPropertyConverters
			= new Dictionary<Tuple<Type, Type>, FromPropertyConverter>();
		
		
		static PropertyRegistry()
		{
			RegisterValueType<bool>();
			RegisterValueType<byte>();
			RegisterValueType<short>();
			RegisterValueType<int>();
			RegisterValueType<long>();
			RegisterValueType<float>();
			RegisterValueType<double>();
			PropertyPrimitive<string>.RegisterConverters();
			
			void RegisterValueType<T>()
				where T : struct
			{
				PropertyPrimitive<T>.RegisterConverters();
				PropertyArray<T>.RegisterConverters();
			}
		}
		
		
		/// <summary> Registers a binary de/serializer for the specified property type with the specified ID. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified ID is outside the valid range (MIN_ID - MAX_ID). </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified de/serializer is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if the specified ID is already used. </exception>
		public static void RegisterDeSerializer<TProperty>(int id, PropertyBinaryDeSerializer<TProperty> deSerializer)
			where TProperty : Property
		{
			if ((id < MIN_ID) || (id > MAX_ID)) throw new ArgumentOutOfRangeException(nameof(id),
				$"The ID { id } is not within the valid range ({ MIN_ID } - { MAX_ID })");
			if (deSerializer == null) throw new ArgumentNullException(nameof(deSerializer));
			if (_binaryDeSerializersByID[id] != null) throw new InvalidOperationException(
				$"The ID { id } is already in use by de/serializer '{ _binaryDeSerializersByID[id].GetType().ToFriendlyString() }'");
			_binaryDeSerializersByID[id] = deSerializer;
		}
		
		/// <summary> Registers a value => property converter. Required to use <see cref="Property.Of{T}"/>. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified converter is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if the specified source value type is already registered. </exception>
		public static void RegisterToPropertyConverter<TFrom, TToProperty>(
				Converter<TFrom, TToProperty> converter)
			where TToProperty : Property
		{
			if (converter == null) throw new ArgumentNullException(nameof(converter));
			if (_toPropertyConverters.TryGetValue(typeof(TFrom), out var value)) throw new InvalidOperationException(
				$"There's already a value => property converter registered for type '{ typeof(TFrom).ToFriendlyString() }'");
			_toPropertyConverters.Add(typeof(TFrom), ToPropertyConverter.Create(converter));
		}
		
		/// <summary> Registers a property => value converter. Required to use <see cref="Property.As{T}"/>. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified converter is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if the specified source property and target value type are already registered. </exception>
		public static void RegisterFromPropertyConverter<TFromProperty, TTo>(
				Converter<TFromProperty, TTo> converter)
			where TFromProperty : Property
		{
			if (converter == null) throw new ArgumentNullException(nameof(converter));
			var typePair = Tuple.Create(typeof(TFromProperty), typeof(TTo));
			if (_fromPropertyConverters.TryGetValue(typePair, out var value)) throw new InvalidOperationException(
				"There's already a property => value converter registered for types " +
				$"('{ typeof(TFromProperty).ToFriendlyString() }' => '{ typeof(TTo).ToFriendlyString() }')");
			_fromPropertyConverters.Add(typePair, FromPropertyConverter.Create(converter));
		}
		
		
		/// <summary> Returns the de/serializer registered with the specified ID, or null if none. </summary>
		public static IPropertyBinaryDeSerializer GetDeSerializerByID(int id)
			=> (id > MIN_ID) && (id < MAX_ID) ? _binaryDeSerializersByID[id] : null;
		
		/// <summary> Returns the de/serializer for the specified property type, or null if none. </summary>
		/// <exception cref="ArgumentException"> Thrown if the specified property type not a Property type. </exception>
		public static IPropertyBinaryDeSerializer GetDeSerializerByType(Type propertyType)
		{
			if (!typeof(Property).IsAssignableFrom(propertyType)) throw new ArgumentException(
				$"The specified property type '{ propertyType.ToFriendlyString() }' is not actually a Property type", nameof(propertyType));
			return _binaryDeSerializersByType.TryGetValue(propertyType, out var value) ? value : null;
		}
		/// <summary> Returns the de/serializer for the specified property type, or null if none. </summary>
		public static IPropertyBinaryDeSerializer GetDeSerializerByType<TProperty>()
			where TProperty : Property
			=> _binaryDeSerializersByType.TryGetValue(typeof(TProperty), out var value) ? value : null;
		
		
		/// <summary> Attempts to get the value => property converter for the specified value type. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value type is null. </exception>
		public static bool TryGetToPropertyConverter(Type valueType, out Converter<object, Property> value)
		{
			if (valueType == null) throw new ArgumentNullException(nameof(valueType));
			var found = _toPropertyConverters.TryGetValue(valueType, out var converter);
			value = found ? converter.Get() : null;
			return found;
		}
		/// <summary> Gets the value => property converter for the specified value type. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value type is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified type has no ToPropertyConverter registered. </exception>
		public static Converter<object, Property> GetToPropertyConverter(Type valueType)
		{
			if (!TryGetToPropertyConverter(valueType, out var value)) throw new NotSupportedException(
				$"No ToPropertyConverter was registered for value type '{ valueType.ToFriendlyString() }'");
			return value;
		}
		
		/// <summary> Attempts to get the value => property converter for the specified value type. </summary>
		public static bool TryGetToPropertyConverter<TValue>(out Converter<TValue, Property> value)
		{
			var found = _toPropertyConverters.TryGetValue(typeof(TValue), out var converter);
			value = found ? converter.Get<TValue>() : null;
			return found;
		}
		/// <summary> Gets the value => property converter for the specified value type. </summary>
		/// <exception cref="NotSupportedException"> Thrown if the specified type has no ToPropertyConverter registered. </exception>
		public static Converter<TValue, Property> GetToPropertyConverter<TValue>()
		{
			if (!TryGetToPropertyConverter<TValue>(out var value)) throw new NotSupportedException(
				$"No ToPropertyConverter was registered for value type '{ typeof(TValue).ToFriendlyString() }'");
			return value;
		}
		
		
		/// <summary> Attempts to get the property => value converter for the specified value and property types. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property type is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified property type not a Property type. </exception>
		public static bool TryGetFromPropertyConverter<TValue>(Type propertyType, out Converter<Property, TValue> value)
		{
			if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
			if (!typeof(Property).IsAssignableFrom(propertyType)) throw new ArgumentException(
				$"The specified property type is not actually a Property type", nameof(propertyType));
			var typePair = Tuple.Create(propertyType, typeof(TValue));
			var found = _fromPropertyConverters.TryGetValue(typePair, out var converter);
			value = found ? converter.Get<TValue>() : null;
			return found;
		}
		/// <summary> Gets the property => value converter for the specified value and property types. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property type is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified property type not a Property type. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified types have no FromPropertyConverter registered. </exception>
		public static Converter<Property, TValue> GetFromPropertyConverter<TValue>(Type propertyType)
		{
			if (!TryGetFromPropertyConverter<TValue>(propertyType, out var value)) throw new NotSupportedException(
				$"No FromPropertyConverter was registered for value type '{ typeof(TValue) }' " +
				$"and property type '{ propertyType.ToFriendlyString() }'");
			return value;
		}
		
		/// <summary> Attempts to get the property => value converter for the specified value and property types. </summary>
		public static bool TryGetFromPropertyConverter<TValue, TProperty>(out Converter<TProperty, TValue> value)
			where TProperty : Property
		{
			var typePair = Tuple.Create(typeof(TProperty), typeof(TValue));
			var found = _fromPropertyConverters.TryGetValue(typePair, out var converter);
			value = found ? converter.Get<TValue, TProperty>() : null;
			return found;
		}
		/// <summary> Gets the property => value converter for the specified value and property types. </summary>
		/// <exception cref="NotSupportedException"> Thrown if the specified types have no FromPropertyConverter registered. </exception>
		public static Converter<TProperty, TValue> GetFromPropertyConverter<TValue, TProperty>()
			where TProperty : Property
		{
			if (!TryGetFromPropertyConverter<TValue, TProperty>(out var value)) throw new NotSupportedException(
				$"No FromPropertyConverter was registered for value type '{ typeof(TValue).ToFriendlyString() }' " +
				$"and property type '{ typeof(TProperty).ToFriendlyString() }'");
			return value;
		}
		
		
		private class ToPropertyConverter
		{
			private Converter<object, Property> _fromObject;
			private Delegate _fromValue;
			
			public static ToPropertyConverter Create<TValue, TProperty>(
					Converter<TValue, TProperty> converter)
				where TProperty : Property => new ToPropertyConverter {
					_fromObject = (value) => converter((TValue)value),
					_fromValue  = converter
				};
			
			public Converter<object, Property> Get()
				=> _fromObject;
			public Converter<TValue, Property> Get<TValue>()
				=> (Converter<TValue, Property>)_fromValue;
		}
		
		private class FromPropertyConverter
		{
			private Delegate _fromAny;
			private Delegate _fromGeneric;
			
			public static FromPropertyConverter Create<TProperty, TValue>(
					Converter<TProperty, TValue> converter)
				where TProperty : Property
				=> new FromPropertyConverter {
					_fromAny     = (Converter<Property, TValue>)((property) => converter((TProperty)property)),
					_fromGeneric = converter
				};
			
			public Converter<Property, TValue> Get<TValue>()
				=> (Converter<Property, TValue>)_fromAny;
			public Converter<TProperty, TValue> Get<TValue, TProperty>()
				where TProperty : Property
				=> (Converter<TProperty, TValue>)_fromGeneric;
		}
	}
}
