using System;
using System.Collections.Generic;
using System.IO;
using ProperTree.Serialization.Binary;
using ProperTree.Utility;

namespace ProperTree.Serialization
{
	public static class BinaryDeSerializerRegistry
	{
		public static int MIN_ID { get; } = 1;
		public static int MAX_ID { get; } = byte.MaxValue;
		
		static BinaryDeSerializerRegistry()
		{
			Register( 0x01 , new BinaryDeSerializerPrimitive<  bool>( reader => reader.ReadBoolean() , (writer, value) => writer.Write(value) ));
			Register( 0x02 , new BinaryDeSerializerPrimitive<  byte>( reader => reader.ReadByte()    , (writer, value) => writer.Write(value) ));
			Register( 0x03 , new BinaryDeSerializerPrimitive< short>( reader => reader.ReadInt16()   , (writer, value) => writer.Write(value) ));
			Register( 0x04 , new BinaryDeSerializerPrimitive<   int>( reader => reader.ReadInt32()   , (writer, value) => writer.Write(value) ));
			Register( 0x05 , new BinaryDeSerializerPrimitive<  long>( reader => reader.ReadInt64()   , (writer, value) => writer.Write(value) ));
			Register( 0x06 , new BinaryDeSerializerPrimitive< float>( reader => reader.ReadSingle()  , (writer, value) => writer.Write(value) ));
			Register( 0x07 , new BinaryDeSerializerPrimitive<double>( reader => reader.ReadDouble()  , (writer, value) => writer.Write(value) ));
			
			Register( 0x10 , new BinaryDeSerializerPrimitive<string>( reader => reader.ReadString()  , (writer, value) => writer.Write(value) ));
			
			Register( 0x21, new BinaryDeSerializerArray<  bool>() );
			Register( 0x22, new BinaryDeSerializerByteArray() );
			Register( 0x23, new BinaryDeSerializerArray< short>() );
			Register( 0x24, new BinaryDeSerializerArray<   int>() );
			Register( 0x25, new BinaryDeSerializerArray<  long>() );
			Register( 0x26, new BinaryDeSerializerArray< float>() );
			Register( 0x27, new BinaryDeSerializerArray<double>() );
			
			Register( 0x30, new BinaryDeSerializerList() );
			Register( 0x31, new BinaryDeSerializerDictionary() );
		}
		
		
		// Reading / writing entire properties
		
		/// <summary> Reads a full property using the specified BinaryReader. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified reader is null. </exception>
		/// <exception cref="PropertyParseException"> Thrown if there was an error while reading the property. </exception>
		public static IProperty ReadProperty(BinaryReader reader)
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
		
		/// <summary> Writes a full property using the specified BinaryWriter. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified writer or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if the specified property type is not an <see cref="IProperty"/> type. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified property has no de/serializer registered. </exception>
		public static void WriteProperty(BinaryWriter writer, IProperty property)
		{
			var deSerializer = GetAndWriteDeSerializer(writer, property);
			deSerializer.Write(writer, property);
		}
		
		
		// Reading / writing de/serializers
		
		/// <summary> Reads a property de/serializer using the specified reader. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified reader is null. </exception>
		/// <exception cref="PropertyParseException"> Thrown if there was an error while reading the de/serializer. </exception>
		public static IBinaryDeSerializer ReadDeSerializer(BinaryReader reader)
		{
			if (reader == null) throw new ArgumentNullException(nameof(reader));
			
			int id;
			try { id = reader.ReadByte(); }
			catch (Exception ex) { throw new PropertyParseException(
				$"Exception while reading property de/serializer ID: { ex.Message }",
				reader.BaseStream, ex); }
			
			var deSerializer = GetByID(id);
			if (deSerializer == null) throw new PropertyParseException(
				$"Unknown property de/serializer ID { id }", reader.BaseStream);
			
			return deSerializer;
		}
		
		/// <exception cref="ArgumentNullException"> Thrown if the specified writer or property is null. </exception>
		public static IBinaryDeSerializer GetAndWriteDeSerializer(BinaryWriter writer, IProperty property)
		{
			if (writer == null) throw new ArgumentNullException(nameof(writer));
			if (property == null) throw new ArgumentNullException(nameof(property));
			
			var deSerializer = GetFor(property, out int id);
			if (deSerializer == null) throw new NotSupportedException(
				$"No de/serializer registered for property '{ property.GetType().ToFriendlyName() }'");
			
			writer.Write((byte)id);
			return deSerializer;
		}
		
		
		// Registering / getting de/serializers
		
		private static IBinaryDeSerializer[] _byID
			= new IBinaryDeSerializer[MAX_ID];
		private static Dictionary<Type, Tuple<int, IBinaryDeSerializer>> _byType
			= new Dictionary<Type, Tuple<int, IBinaryDeSerializer>>();
		
		/// <summary> Registers a binary de/serializer for the specified property type with the specified ID. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified ID is outside the valid range (MIN_ID - MAX_ID). </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified de/serializer is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if the specified ID is already used. </exception>
		public static void Register<TProperty>(int id, BinaryDeSerializer<TProperty> deSerializer)
			where TProperty : IProperty
		{
			if ((id < MIN_ID) || (id > MAX_ID)) throw new ArgumentOutOfRangeException(nameof(id),
				$"The ID { id } is not within the valid range ({ MIN_ID } - { MAX_ID })");
			if (deSerializer == null) throw new ArgumentNullException(nameof(deSerializer));
			if (_byID[id] != null) throw new InvalidOperationException(
				$"The ID { id } is already in use by de/serializer '{ _byID[id].GetType().ToFriendlyName() }'");
			
			_byID[id] = deSerializer;
			_byType.Add(typeof(TProperty),
				Tuple.Create(id, (IBinaryDeSerializer)deSerializer));
		}
		
		/// <summary> Returns the de/serializer registered with the specified ID, or null if none. </summary>
		public static IBinaryDeSerializer GetByID(int id)
			=> (id >= MIN_ID) && (id <= MAX_ID) ? _byID[id] : null;
		
		/// <summary> Returns the de/serializer for the specified property, or null if none. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		public static IBinaryDeSerializer GetFor(IProperty property, out int id)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));
			return GetByTypeInternal(property.GetType(), out id);
		}
		/// <summary> Returns the de/serializer for the specified property type, or null if none. </summary>
		public static IBinaryDeSerializer GetByType<TProperty>(out int id)
			where TProperty : IProperty
			=> GetByTypeInternal(typeof(TProperty), out id);
		
		private static IBinaryDeSerializer GetByTypeInternal(
			Type propertyType, out int id)
		{
			if (_byType.TryGetValue(propertyType, out var entry))
				{ id = entry.Item1; return entry.Item2; }
			else { id = -1; return null; }
		}
	}
}
