using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProperTree.Utility;

namespace ProperTree
{
	public class PropertyMap
		: IProperty, IDictionary<string, IProperty>
	{
		public static readonly int MAX_SIZE = ushort.MaxValue;
		
		
		private readonly Dictionary<string, IProperty> _map
			= new Dictionary<string, IProperty>();
		
		public IProperty this[int index] {
			get => throw new InvalidOperationException($"Not a list property: '{ GetType().ToFriendlyName() }'");
			set => throw new InvalidOperationException($"Not a list property: '{ GetType().ToFriendlyName() }'");
		}
		
		public IProperty this[string name] {
			get {
				if (name == null) throw new ArgumentNullException(nameof(name));
				return _map.TryGetValue(name, out var value) ? value : null;
			}
			set {
				if (name == null) throw new ArgumentNullException(nameof(name));
				if (value == null)
					_map.Remove(name);
				else if ((_map.Count < MAX_SIZE) || _map.ContainsKey(name))
					_map[name] = value;
				else throw new InvalidOperationException(
					$"{ nameof(PropertyMap) } can't exceed MAX_SIZE ({ MAX_SIZE })");
			}
		}
		
		
		/// <summary> Adds the an entry with the specified name and property to this map property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		public void Add(string name, IProperty property)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (property == null) throw new ArgumentNullException(nameof(property));
			if (_map.Count == MAX_SIZE) throw new InvalidOperationException(
				$"{ nameof(PropertyMap) } can't exceed MAX_SIZE ({ MAX_SIZE })");
			_map.Add(name, property);
		}
		
		/// <summary> Adds the an entry with the specified name and value to this map property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name or value is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to an <see cref="IProperty"/>. </exception>
		public void Add<T>(string name, T value)
			=> Add(name, Property.Of(value));
		
		/// <summary> Removes the entry with the specified name from this map property,
		///           and returns the removed value, or null if the name was not found. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name is null. </exception>
		public IProperty Remove(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			_map.TryGetValue(name, out var property);
			_map.Remove(name);
			return property;
		}
		
		
		// IEquatable implementation
		
		public bool Equals(IProperty other)
			=> (other is PropertyMap map)
				&& (Count == map.Count) && _map.All(entry =>
					(map._map.TryGetValue(entry.Key, out var value)
						&& value.Equals(entry.Value)));
		
		
		// IDictionary implementation
		
		/// <summary> Gets the number of entries in this map property. </summary>
		public int Count => _map.Count;
		
		/// <summary> Gets a collection containing the keys in this map property. </summary>
		public ICollection<string> Keys => _map.Keys;
		
		/// <summary> Gets a collection containing the values in this map property. </summary>
		public ICollection<IProperty> Values => _map.Values;
		
		bool IDictionary<string, IProperty>.TryGetValue(string key, out IProperty value)
			=> _map.TryGetValue(key, out value);
		
		bool IDictionary<string, IProperty>.ContainsKey(string key)
			=> _map.ContainsKey(key);
		
		bool IDictionary<string, IProperty>.Remove(string key)
			=> _map.Remove(key);
		
		/// <summary> Clears all entries from this map property. </summary>
		public void Clear() => _map.Clear();
		
		
		// ICollection implementation
		
		bool ICollection<KeyValuePair<string, IProperty>>.IsReadOnly => false;
		
		void ICollection<KeyValuePair<string, IProperty>>.Add(KeyValuePair<string, IProperty> value)
			=> Add(value.Key, value.Value);
		
		bool ICollection<KeyValuePair<string, IProperty>>.Contains(KeyValuePair<string, IProperty> value)
			=> ((ICollection<KeyValuePair<string, IProperty>>)_map).Contains(value);
		
		bool ICollection<KeyValuePair<string, IProperty>>.Remove(KeyValuePair<string, IProperty> value)
			=> ((ICollection<KeyValuePair<string, IProperty>>)_map).Remove(value);
		
		void ICollection<KeyValuePair<string, IProperty>>.CopyTo(KeyValuePair<string, IProperty>[] array, int arrayIndex)
			=> ((ICollection<KeyValuePair<string, IProperty>>)_map).CopyTo(array, arrayIndex);
		
		
		// IEnumerable implementation
		
		/// <summary> Returns an enumerator that iterates
		///           over the entries in this map property. </summary>
		public IEnumerator<KeyValuePair<string, IProperty>> GetEnumerator()
			=> _map.GetEnumerator();
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	
	public static class PropertyMapExtensions
	{
		// TODO: Add "TryAdd", "TryRemove", maybe "TryUpdate", etc instead..?
		
		private static PropertyMap AsMap(this IProperty self)
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (!(self is PropertyMap d)) throw new InvalidOperationException(
				$"Not a map property: '{ self.GetType().ToFriendlyName() }'");
			return d;
		}
		
		/// <summary> Adds the an entry with the specified name and property to the specified map property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a map. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified map, name or property is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		public static void Add(this IProperty self, string name, IProperty property)
			=> self.AsMap().Add(name, property);
		
		/// <summary> Adds the an entry with the specified name and value to the specified map property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a map. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified map, name or value is null. </exception>
		/// <exception cref="ArgumentException"> Thrown if an entry with the same name already exists. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to an <see cref="IProperty"/>. </exception>
		public static void Add<T>(this IProperty self, string name, T value)
			=> self.AsMap().Add(name, value);
		
		/// <summary> Removes the entry with the specified name from the specified map property,
		///           and returns the removed value, or null if the name was not found. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified property is not a map. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified map or name is null. </exception>
		public static IProperty Remove(this IProperty self, string name)
			=> self.AsMap().Remove(name);
	}
}
