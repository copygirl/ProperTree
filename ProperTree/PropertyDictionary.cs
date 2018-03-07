using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProperTree.Serialization;
using ProperTree.Utility;

namespace ProperTree
{
	public class PropertyDictionary
		: IProperty, IDictionary<string, IProperty>
	{
		public static readonly int MAX_SIZE = ushort.MaxValue;
		
		
		private readonly Dictionary<string, IProperty> _dict
			= new Dictionary<string, IProperty>();
		
		public IProperty this[int index] {
			get => throw new InvalidOperationException($"Not a list property: '{ GetType().ToFriendlyName() }'");
			set => throw new InvalidOperationException($"Not a list property: '{ GetType().ToFriendlyName() }'");
		}
		
		public IProperty this[string name] {
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
					$"PropertyDictionary can't exceed MAX_SIZE ({ MAX_SIZE })");
			}
		}
		
		
		/// <summary> Adds the an entry with the specified name and property to this dictionary property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		public void Add(string name, IProperty property)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (property == null) throw new ArgumentNullException(nameof(property));
			if (_dict.Count == MAX_SIZE) throw new InvalidOperationException(
				$"PropertyDictionary can't exceed MAX_SIZE ({ MAX_SIZE })");
			_dict.Add(name, property);
		}
		
		/// <summary> Adds the an entry with the specified name and value to this dictionary property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name or value is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to an <see cref="IProperty"/>. </exception>
		public void Add<T>(string name, T value)
			=> Add(name, Property.Of(value));
		
		/// <summary> Removes the entry with the specified name from this dictionary property,
		///           and returns the removed value, or null if the name was not found. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name is null. </exception>
		public IProperty Remove(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			_dict.TryGetValue(name, out var property);
			_dict.Remove(name);
			return property;
		}
		
		
		// IEquatable implementation
		
		public bool Equals(IProperty other)
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
		public ICollection<IProperty> Values => _dict.Values;
		
		bool IDictionary<string, IProperty>.TryGetValue(string key, out IProperty value)
			=> _dict.TryGetValue(key, out value);
		
		bool IDictionary<string, IProperty>.ContainsKey(string key)
			=> _dict.ContainsKey(key);
		
		bool IDictionary<string, IProperty>.Remove(string key)
			=> _dict.Remove(key);
		
		/// <summary> Clears all entries from this dictionary property. </summary>
		public void Clear() => _dict.Clear();
		
		
		// ICollection implementation
		
		bool ICollection<KeyValuePair<string, IProperty>>.IsReadOnly => false;
		
		void ICollection<KeyValuePair<string, IProperty>>.Add(KeyValuePair<string, IProperty> value)
			=> Add(value.Key, value.Value);
		
		bool ICollection<KeyValuePair<string, IProperty>>.Contains(KeyValuePair<string, IProperty> value)
			=> ((ICollection<KeyValuePair<string, IProperty>>)_dict).Contains(value);
		
		bool ICollection<KeyValuePair<string, IProperty>>.Remove(KeyValuePair<string, IProperty> value)
			=> ((ICollection<KeyValuePair<string, IProperty>>)_dict).Remove(value);
		
		void ICollection<KeyValuePair<string, IProperty>>.CopyTo(KeyValuePair<string, IProperty>[] array, int arrayIndex)
			=> ((ICollection<KeyValuePair<string, IProperty>>)_dict).CopyTo(array, arrayIndex);
		
		
		// IEnumerable implementation
		
		/// <summary> Returns an enumerator that iterates over
		///           tne entries in this dictionary property. </summary>
		public IEnumerator<KeyValuePair<string, IProperty>> GetEnumerator()
			=> _dict.GetEnumerator();
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	
	public static class PropertyDictionaryExtensions
	{
		// TODO: Add "TryAdd", "TryRemove", maybe "TryUpdate", etc instead..?
		
		private static PropertyDictionary AsDictionary(this IProperty self)
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (!(self is PropertyDictionary d)) throw new InvalidOperationException(
				$"Not a dictionary Property: '{ self.GetType().ToFriendlyName() }'");
			return d;
		}
		
		/// <summary> Adds the an entry with the specified name and property to the specified dictionary property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified dictionary, name or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		public static void Add(this IProperty self, string name, IProperty property)
			=> self.AsDictionary().Add(name, property);
		
		/// <summary> Adds the an entry with the specified name and value to the specified dictionary property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified dictionary, name or value is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to an <see cref="IProperty"/>. </exception>
		public static void Add<T>(this IProperty self, string name, T value)
			=> self.AsDictionary().Add(name, value);
		
		/// <summary> Removes the entry with the specified name from the specified dictionary property,
		///           and returns the removed value, or null if the name was not found. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified dictionary or name is null. </exception>
		public static IProperty Remove(this IProperty self, string name)
			=> self.AsDictionary().Remove(name);
	}
}
