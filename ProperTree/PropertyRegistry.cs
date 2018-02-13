using System;
using System.Collections.Generic;
using System.IO;

namespace ProperTree
{
	// TODO: Split converter and de/serializer related code into 2 different classes?
	public static class PropertyRegistry
	{
		public static int MIN_ID { get; } = 1;
		public static int MAX_ID { get; } = byte.MaxValue;
		
		private static IPropertyBinaryDeSerializer[] _binaryDeSerializersByID
			= new IPropertyBinaryDeSerializer[MAX_ID];
		private static Dictionary<Type, Tuple<int, IPropertyBinaryDeSerializer>> _binaryDeSerializersByType
			= new Dictionary<Type, Tuple<int, IPropertyBinaryDeSerializer>>();
		private static Dictionary<Type, ToPropertyConverter> _toPropertyConverters
			= new Dictionary<Type, ToPropertyConverter>();
		private static Dictionary<Tuple<Type, Type>, FromPropertyConverter> _fromPropertyConverters
			= new Dictionary<Tuple<Type, Type>, FromPropertyConverter>();
		
		
		static PropertyRegistry()
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
			
			RegisterDeSerializer( 0x01 , new PropertyPrimitive<  bool>.DeSerializer( reader => reader.ReadBoolean() , (writer, value) => writer.Write(value) ));
			RegisterDeSerializer( 0x02 , new PropertyPrimitive<  byte>.DeSerializer( reader => reader.ReadByte()    , (writer, value) => writer.Write(value) ));
			RegisterDeSerializer( 0x03 , new PropertyPrimitive< short>.DeSerializer( reader => reader.ReadInt16()   , (writer, value) => writer.Write(value) ));
			RegisterDeSerializer( 0x04 , new PropertyPrimitive<   int>.DeSerializer( reader => reader.ReadInt32()   , (writer, value) => writer.Write(value) ));
			RegisterDeSerializer( 0x05 , new PropertyPrimitive<  long>.DeSerializer( reader => reader.ReadInt64()   , (writer, value) => writer.Write(value) ));
			RegisterDeSerializer( 0x06 , new PropertyPrimitive< float>.DeSerializer( reader => reader.ReadSingle()  , (writer, value) => writer.Write(value) ));
			RegisterDeSerializer( 0x07 , new PropertyPrimitive<double>.DeSerializer( reader => reader.ReadDouble()  , (writer, value) => writer.Write(value) ));
			RegisterDeSerializer( 0x10 , new PropertyPrimitive<string>.DeSerializer( reader => reader.ReadString()  , (writer, value) => writer.Write(value) ));
			
			RegisterDeSerializer( 0x21, new PropertyArray<  bool>.DeSerializer() );
			RegisterDeSerializer( 0x22, new PropertyByteArrayDeSerializer() );
			RegisterDeSerializer( 0x23, new PropertyArray< short>.DeSerializer() );
			RegisterDeSerializer( 0x24, new PropertyArray<   int>.DeSerializer() );
			RegisterDeSerializer( 0x25, new PropertyArray<  long>.DeSerializer() );
			RegisterDeSerializer( 0x26, new PropertyArray< float>.DeSerializer() );
			RegisterDeSerializer( 0x27, new PropertyArray<double>.DeSerializer() );
			
			RegisterDeSerializer( 0x30, new PropertyList.DeSerializer() );
			RegisterDeSerializer( 0x31, new PropertyDictionary.DeSerializer() );
		}
		
		
		// Value <=> Property converters
		
		/// <summary> Registers a value => property converter. Required to use <see cref="Property.Of{T}"/>. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified converter is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if the specified source value type is already registered. </exception>
		public static void RegisterToPropertyConverter<TFrom, TToProperty>(
				Converter<TFrom, TToProperty> converter)
			where TToProperty : Property
		{
			if (converter == null) throw new ArgumentNullException(nameof(converter));
			if (_toPropertyConverters.TryGetValue(typeof(TFrom), out var value)) throw new InvalidOperationException(
				$"There's already a value => property converter registered for type '{ typeof(TFrom).ToFriendlyName() }'");
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
				$"('{ typeof(TFromProperty).ToFriendlyName() }' => '{ typeof(TTo).ToFriendlyName() }')");
			_fromPropertyConverters.Add(typePair, FromPropertyConverter.Create(converter));
		}
		
		
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
				$"No ToPropertyConverter was registered for value type '{ valueType.ToFriendlyName() }'");
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
				$"No ToPropertyConverter was registered for value type '{ typeof(TValue).ToFriendlyName() }'");
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
				$"and property type '{ propertyType.ToFriendlyName() }'");
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
				$"No FromPropertyConverter was registered for value type '{ typeof(TValue).ToFriendlyName() }' " +
				$"and property type '{ typeof(TProperty).ToFriendlyName() }'");
			return value;
		}
		
		
		// De/Serialization
		
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
				$"The ID { id } is already in use by de/serializer '{ _binaryDeSerializersByID[id].GetType().ToFriendlyName() }'");
			
			_binaryDeSerializersByID[id] = deSerializer;
			_binaryDeSerializersByType.Add(typeof(TProperty),
				Tuple.Create(id, (IPropertyBinaryDeSerializer)deSerializer));
		}
		
		
		/// <summary> Returns the de/serializer registered with the specified ID, or null if none. </summary>
		public static IPropertyBinaryDeSerializer GetDeSerializerByID(int id)
			=> (id >= MIN_ID) && (id <= MAX_ID) ? _binaryDeSerializersByID[id] : null;
		
		/// <summary> Returns the de/serializer for the specified property, or null if none. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		public static IPropertyBinaryDeSerializer GetDeSerializerFor(Property property, out int id)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));
			return GetDeSerializerByTypeInternal(property.GetType(), out id);
		}
		/// <summary> Returns the de/serializer for the specified property type, or null if none. </summary>
		public static IPropertyBinaryDeSerializer GetDeSerializerByType<TProperty>(out int id)
			where TProperty : Property
			=> GetDeSerializerByTypeInternal(typeof(TProperty), out id);
		
		private static IPropertyBinaryDeSerializer GetDeSerializerByTypeInternal(
			Type propertyType, out int id)
		{
			if (_binaryDeSerializersByType.TryGetValue(propertyType, out var entry))
				{ id = entry.Item1; return entry.Item2; }
			else { id = -1; return null; }
		}
		
		
		
		/// <summary> Reads a property de/serializer using the specified reader. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified reader is null. </exception>
		/// <exception cref="PropertyParseException"> Thrown if there was an error while reading the de/serializer. </exception>
		public static IPropertyBinaryDeSerializer ReadDeSerializer(BinaryReader reader)
		{
			if (reader == null) throw new ArgumentNullException(nameof(reader));
			
			int id;
			try { id = reader.ReadByte(); }
			catch (Exception ex) { throw new PropertyParseException(
				$"Exception while reading property de/serializer ID: { ex.Message }",
				reader.BaseStream, ex); }
			
			var deSerializer = GetDeSerializerByID(id);
			if (deSerializer == null) throw new PropertyParseException(
				$"Unknown property de/serializer ID { id }", reader.BaseStream);
			
			return deSerializer;
		}
		
		/// <summary> Reads a full property using the specified BinaryReader. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified reader is null. </exception>
		/// <exception cref="PropertyParseException"> Thrown if there was an error while reading the property. </exception>
		public static Property ReadProperty(BinaryReader reader)
		{
			if (reader == null) throw new ArgumentNullException(nameof(reader));
			var deSerializer = ReadDeSerializer(reader);
			try { return deSerializer.Read(reader); }
			catch (PropertyParseException) { throw; }
			catch (Exception ex) {
				var propertyName = deSerializer.PropertyType.ToFriendlyName();
				throw new PropertyParseException(
					$"Exception while reading property '{ propertyName }': { ex.Message }",
					reader.BaseStream, ex);
			}
		}
		
		
		/// <exception cref="ArgumentNullException"> Thrown if the specified writer or property is null. </exception>
		public static IPropertyBinaryDeSerializer GetAndWriteDeSerializer(BinaryWriter writer, Property property)
		{
			if (writer == null) throw new ArgumentNullException(nameof(writer));
			if (property == null) throw new ArgumentNullException(nameof(property));
			
			var deSerializer = GetDeSerializerFor(property, out int id);
			if (deSerializer == null) throw new NotSupportedException(
				$"No de/serializer registered for property '{ property.GetType().ToFriendlyName() }'");
			
			writer.Write((byte)id);
			return deSerializer;
		}
		
		/// <summary> Writes a full property using the specified BinaryWriter. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified writer or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified property type is not a Property type. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified property has no de/serializer registered. </exception>
		public static void WriteProperty(BinaryWriter writer, Property property)
		{
			var deSerializer = GetAndWriteDeSerializer(writer, property);
			deSerializer.Write(writer, property);
		}
		
		
		// Utility classes
		
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
	
	/// <summary> An exception that's thrown when an error occurs
	///           during parsing or deserialization of a property. </summary>
	public class PropertyParseException : Exception
	{
		/// <summary> Gets the stream in which the parse
		///           exception occured, or null if unavailable. </summary>
		public Stream Stream { get; }
		
		/// <summary> Gets the position in the stream where the
		///           parse exception occured, or -1 if unavailable. </summary>
		public long Position
			=> ((Stream != null) && Stream.CanSeek)
				? Stream.Position : -1;
		
		public PropertyParseException(string message, Stream stream)
			: this(message, stream, null) {  }
		
		public PropertyParseException(string message, Stream stream, Exception innerException)
			: base(AppendStreamPosition(message, stream), innerException) { Stream = stream; }
		
		/// <summary> Appends the position and length of the stream to the message, if available. </summary>
		private static string AppendStreamPosition(string message, Stream stream)
			=> ((stream != null) && stream.CanSeek)
				? $"{ message } (Stream pos/len: { stream.Position }/{ stream.Length })"
				: message;
	}
}
