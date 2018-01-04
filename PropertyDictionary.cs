using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProperTree
{
	public class PropertyDictionary
		: Property, IDictionary<string, Property>
	{
		public static readonly int MAX_SIZE = ushort.MaxValue;
		
		
		private readonly Dictionary<string, Property> _dict
			= new Dictionary<string, Property>();
		
		public override Property this[string name] {
			get {
				if (name == null) throw new ArgumentNullException(nameof(name));
				return _dict.TryGetValue(name, out var value) ? value : null;
			}
			set {
				if (name == null) throw new ArgumentNullException(nameof(name));
				if (value == null)
					_dict.Remove(name);
				else if ((_dict.Count < MAX_SIZE) || _dict.ContainsKey(name))
					_dict[name] = value;
				else throw new InvalidOperationException(
					$"PropertyDictionary can't exceed MAX_SIZE (${ MAX_SIZE })");
			}
		}
		
		
		/// <summary> Adds the an entry with the specified name and property to this dictionary property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		public void Add(string name, Property property)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (property == null) throw new ArgumentNullException(nameof(property));
			if (_dict.Count == MAX_SIZE) throw new InvalidOperationException(
				$"PropertyDictionary can't exceed MAX_SIZE (${ MAX_SIZE })");
			_dict.Add(name, property);
		}
		
		/// <summary> Adds the an entry with the specified name and value to this dictionary property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name or value is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public void Add<T>(string name, T value)
			=> Add(name, Property.Of(value));
		
		/// <summary> Removes the entry with the specified name from this dictionary property,
		///           and returns the removed value, or null if the name was not found. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name is null. </exception>
		public Property Remove(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			_dict.TryGetValue(name, out var property);
			_dict.Remove(name);
			return property;
		}
		
		
		public override bool Equals(Property other)
			=> (other is PropertyDictionary dict)
				&& (Count == dict.Count) && _dict.All(entry =>
					(dict._dict.TryGetValue(entry.Key, out var value)
						&& value.Equals(entry.Value)));
		
		
		// IDictionary implementation
		
		/// <summary> Gets the number of entries in this dictionary property. </summary>
		public int Count => _dict.Count;
		
		/// <summary> Gets a collection containing the keys in this dictionary property. </summary>
		public ICollection<string> Keys => _dict.Keys;
		
		/// <summary> Gets a collection containing the values in this dictionary property. </summary>
		public ICollection<Property> Values => _dict.Values;
		
		bool IDictionary<string, Property>.TryGetValue(string key, out Property value)
			=> _dict.TryGetValue(key, out value);
		
		bool IDictionary<string, Property>.ContainsKey(string key)
			=> _dict.ContainsKey(key);
		
		bool IDictionary<string, Property>.Remove(string key)
			=> _dict.Remove(key);
		
		/// <summary> Clears all entries from this dictionary property. </summary>
		public void Clear() => _dict.Clear();
		
		// ICollection implementation
		
		bool ICollection<KeyValuePair<string, Property>>.IsReadOnly => false;
		
		void ICollection<KeyValuePair<string, Property>>.Add(KeyValuePair<string, Property> value)
			=> Add(value.Key, value.Value);
		
		bool ICollection<KeyValuePair<string, Property>>.Contains(KeyValuePair<string, Property> value)
			=> ((ICollection<KeyValuePair<string, Property>>)_dict).Contains(value);
		
		bool ICollection<KeyValuePair<string, Property>>.Remove(KeyValuePair<string, Property> value)
			=> ((ICollection<KeyValuePair<string, Property>>)_dict).Remove(value);
		
		void ICollection<KeyValuePair<string, Property>>.CopyTo(KeyValuePair<string, Property>[] array, int arrayIndex)
			=> ((ICollection<KeyValuePair<string, Property>>)_dict).CopyTo(array, arrayIndex);
		
		// IEnumerable implementation
		
		/// <summary> Returns an enumerator that iterates over
		///           tne entries in this dictionary property. </summary>
		public IEnumerator<KeyValuePair<string, Property>> GetEnumerator()
			=> _dict.GetEnumerator();
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		
		
		// De/serializer
		
		public class DeSerializer : PropertyBinaryDeSerializer<PropertyDictionary>
		{
			public override PropertyDictionary Read(BinaryReader reader)
			{
				var dictionary = new PropertyDictionary();
				var count = reader.ReadUInt16();
				if (count > MAX_SIZE) throw new Exception(
					$"PropertyDictionary count is larger than MAX_SIZE (${ count } > ${ MAX_SIZE })");
				for (var i = 0; i < count; i++) {
					var name     = reader.ReadString();
					var property = PropertyRegistry.ReadProperty(reader);
					dictionary.Add(name, property);
				}
				return dictionary;
			}
			
			public override void Write(BinaryWriter writer, PropertyDictionary dictionary)
			{
				writer.Write((ushort)dictionary.Count);
				foreach (var entry in dictionary) {
					writer.Write(entry.Key);
					PropertyRegistry.WriteProperty(writer, entry.Value);
				}
			}
		}
	}
	
	public static class PropertyDictionaryExtensions
	{
		// TODO: Add "TryAdd", "TryRemove", maybe "TryUpdate", etc instead..?
		
		private static PropertyDictionary AsDictionary(this Property self)
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (!(self is PropertyDictionary d)) throw new InvalidOperationException(
				$"Not a dictionary Property: '{ self.GetType().ToFriendlyString() }'");
			return d;
		}
		
		/// <summary> Adds the an entry with the specified name and property to the specified dictionary property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified dictionary, name or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		public static void Add(this Property self, string name, Property property)
			=> self.AsDictionary().Add(name, property);
		
		/// <summary> Adds the an entry with the specified name and value to the specified dictionary property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified dictionary, name or value is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public static void Add<T>(this Property self, string name, T value)
			=> self.AsDictionary().Add(name, value);
		
		/// <summary> Removes the entry with the specified name from the specified dictionary property,
		///           and returns the removed value, or null if the name was not found. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified dictionary or name is null. </exception>
		public static Property Remove(this Property self, string name)
			=> self.AsDictionary().Remove(name);
	}
}
