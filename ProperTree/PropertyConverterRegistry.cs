using System;
using System.Collections.Generic;
using ProperTree.Utility;

namespace ProperTree
{
	public static class PropertyConverterRegistry
	{
		private static Dictionary<Type, ToPropertyConverter> _toProperty
			= new Dictionary<Type, ToPropertyConverter>();
		private static Dictionary<Tuple<Type, Type>, FromPropertyConverter> _fromProperty
			= new Dictionary<Tuple<Type, Type>, FromPropertyConverter>();
		
		
		static PropertyConverterRegistry()
		{
			RegisterValueTypeConverters<bool>();
			RegisterValueTypeConverters<byte>();
			RegisterValueTypeConverters<short>();
			RegisterValueTypeConverters<int>();
			RegisterValueTypeConverters<long>();
			RegisterValueTypeConverters<float>();
			RegisterValueTypeConverters<double>();
			PropertyPrimitive<string>.RegisterConverters();
			
			void RegisterValueTypeConverters<T>()
				where T : struct
			{
				PropertyPrimitive<T>.RegisterConverters();
				PropertyArray<T>.RegisterConverters();
			}
		}
		
		
		/// <summary>
		///   Registers a value => property converter.
		///   Required for <see cref="Property.Of{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified converter is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if the specified source value type is already registered. </exception>
		public static void RegisterToProperty<TFrom>(
				Converter<TFrom, IProperty> converter)
		{
			if (converter == null) throw new ArgumentNullException(nameof(converter));
			if (_toProperty.TryGetValue(typeof(TFrom), out var value)) throw new InvalidOperationException(
				$"There's already a value => property converter registered for type '{ typeof(TFrom).ToFriendlyName() }'");
			_toProperty.Add(typeof(TFrom), ToPropertyConverter.Create(converter));
		}
		
		
		/// <summary>
		///   Registers a property => value converter.
		///   Required for <see cref="Property.As{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified converter is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if the specified source property and target value type are already registered. </exception>
		public static void RegisterFromProperty<TFromProperty, TTo>(
				Converter<TFromProperty, TTo> converter)
			where TFromProperty : IProperty
		{
			if (converter == null) throw new ArgumentNullException(nameof(converter));
			var typePair = Tuple.Create(typeof(TFromProperty), typeof(TTo));
			if (_fromProperty.TryGetValue(typePair, out var value)) throw new InvalidOperationException(
				"There's already a property => value converter registered for types " +
				$"('{ typeof(TFromProperty).ToFriendlyName() }' => '{ typeof(TTo).ToFriendlyName() }')");
			_fromProperty.Add(typePair, FromPropertyConverter.Create(converter));
		}
		
		
		/// <summary> Attempts to get the value => property converter for the specified value type. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value type is null. </exception>
		public static bool TryGetToProperty(Type valueType, out Converter<object, IProperty> value)
		{
			if (valueType == null) throw new ArgumentNullException(nameof(valueType));
			var found = _toProperty.TryGetValue(valueType, out var converter);
			value = found ? converter.Get() : null;
			return found;
		}
		/// <summary> Gets the value => property converter for the specified value type. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value type is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified type has no ToProperty converter registered. </exception>
		public static Converter<object, IProperty> GetToProperty(Type valueType)
		{
			if (!TryGetToProperty(valueType, out var value)) throw new NotSupportedException(
				$"No ToProperty converter was registered for type '{ valueType.ToFriendlyName() }'");
			return value;
		}
		
		/// <summary> Attempts to get the value => property converter for the specified value type. </summary>
		public static bool TryGetToProperty<TValue>(out Converter<TValue, IProperty> value)
		{
			var found = _toProperty.TryGetValue(typeof(TValue), out var converter);
			value = found ? converter.Get<TValue>() : null;
			return found;
		}
		/// <summary> Gets the value => property converter for the specified value type. </summary>
		/// <exception cref="NotSupportedException"> Thrown if the specified type has no ToProperty converter registered. </exception>
		public static Converter<TValue, IProperty> GetToProperty<TValue>()
		{
			if (!TryGetToProperty<TValue>(out var value)) throw new NotSupportedException(
				$"No ToProperty converter was registered for type '{ typeof(TValue).ToFriendlyName() }'");
			return value;
		}
		
		
		/// <summary> Attempts to get the property => value converter for the specified value and property types. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property type is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified property type is not an <see cref="IProperty"/> type. </exception>
		public static bool TryGetFromProperty<TValue>(Type propertyType, out Converter<IProperty, TValue> value)
		{
			if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
			if (!typeof(IProperty).IsAssignableFrom(propertyType)) throw new ArgumentException(
				$"The specified property type is not actually an IProperty type", nameof(propertyType));
			var typePair = Tuple.Create(propertyType, typeof(TValue));
			var found = _fromProperty.TryGetValue(typePair, out var converter);
			value = found ? converter.Get<TValue>() : null;
			return found;
		}
		
		/// <summary> Gets the property => value converter for the specified value and property types. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property type is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified property type is not an <see cref="IProperty"/> type. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified types have no FromProperty converter registered. </exception>
		public static Converter<IProperty, TValue> GetFromProperty<TValue>(Type propertyType)
		{
			if (!TryGetFromProperty<TValue>(propertyType, out var value)) throw new NotSupportedException(
				$"No FromProperty converter was registered for type '{ typeof(TValue) }' " +
				$"and property type '{ propertyType.ToFriendlyName() }'");
			return value;
		}
		
		/// <summary> Attempts to get the property => value converter for the specified value and property types. </summary>
		public static bool TryGetFromProperty<TValue, TProperty>(out Converter<TProperty, TValue> value)
			where TProperty : IProperty
		{
			var typePair = Tuple.Create(typeof(TProperty), typeof(TValue));
			var found = _fromProperty.TryGetValue(typePair, out var converter);
			value = found ? converter.Get<TValue, TProperty>() : null;
			return found;
		}
		/// <summary> Gets the property => value converter for the specified value and property types. </summary>
		/// <exception cref="NotSupportedException"> Thrown if the specified types have no FromProperty converter registered. </exception>
		public static Converter<TProperty, TValue> GetFromProperty<TValue, TProperty>()
			where TProperty : IProperty
		{
			if (!TryGetFromProperty<TValue, TProperty>(out var value)) throw new NotSupportedException(
				$"No FromProperty converter was registered for type '{ typeof(TValue).ToFriendlyName() }' " +
				$"and property type '{ typeof(TProperty).ToFriendlyName() }'");
			return value;
		}
		
		
		// Utility classes
		
		private class ToPropertyConverter
		{
			private Converter<object, IProperty> _fromObject;
			private Delegate _fromValue;
			
			public static ToPropertyConverter Create<TValue, TProperty>(
					Converter<TValue, TProperty> converter)
				where TProperty : IProperty => new ToPropertyConverter {
					_fromObject = (value) => converter((TValue)value),
					_fromValue  = converter
				};
			
			public Converter<object, IProperty> Get()
				=> _fromObject;
			public Converter<TValue, IProperty> Get<TValue>()
				=> (Converter<TValue, IProperty>)_fromValue;
		}
		
		private class FromPropertyConverter
		{
			private Delegate _fromAny;
			private Delegate _fromGeneric;
			
			public static FromPropertyConverter Create<TProperty, TValue>(
					Converter<TProperty, TValue> converter)
				where TProperty : IProperty
				=> new FromPropertyConverter {
					_fromAny     = (Converter<IProperty, TValue>)((property) => converter((TProperty)property)),
					_fromGeneric = converter
				};
			
			public Converter<IProperty, TValue> Get<TValue>()
				=> (Converter<IProperty, TValue>)_fromAny;
			public Converter<TProperty, TValue> Get<TValue, TProperty>()
				where TProperty : IProperty
				=> (Converter<TProperty, TValue>)_fromGeneric;
		}
	}
}
