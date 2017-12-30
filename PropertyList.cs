using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProperTree
{
	public class PropertyList
		: Property, IList<Property>
	{
		private List<Property> _list = new List<Property>();
		
		public override Property this[int index] {
			get => _list[index];
			set {
				if (value == null) throw new ArgumentNullException(nameof(value));
				_list[index] = value;
			}
		}
		
		/// <summary> Adds the specified property to the end of this list property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		public void Add(Property property)
			=> Insert(Count, property);
		
		/// <summary> Adds the specified value to the end of this list property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public void Add<T>(T value)
			=> Add(Property.Of(value));
		
		/// <summary> Inserts the specified property at the specified index in this list property. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		public void Insert(int index, Property property)
		{
			var count = _list?.Count ?? 0;
			if ((index < 0) || (index > count))
				throw new ArgumentOutOfRangeException(nameof(index));
			if (property == null) throw new ArgumentNullException(nameof(property));
			_list.Insert(index, property);
		}
		
		/// <summary> Inserts the specified value at the specified index in this list property. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public void Insert<T>(int index, T value)
			=> Insert(index, Property.Of(value));
		
		/// <summary> Removes the child at the specified index from this list property, and returns the removed value. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		public Property RemoveAt(int index)
		{
			var count = _list?.Count ?? 0;
			if ((index < 0) || (index >= count))
				throw new ArgumentOutOfRangeException(nameof(index));
			
			var value = _list[index];
			_list.RemoveAt(index);
			return (Property)value;
		}
		
		
		public override bool Equals(Property other)
			=> (other is PropertyList list)
				&& _list.SequenceEqual(list._list);
		
		
		// IList implementation
		
		/// <summary> Gets the number of elements in this list property. </summary>
		public int Count => _list?.Count ?? 0;
		
		/// <summary> Returns the index of a specific property
		///           in this list property, or -1 if not found. </summary>
		public int IndexOf(Property property)
			=> _list.IndexOf(property);
		
		/// <summary> Returns if a specific property exists in this list property. </summary>
		public bool Contains(Property property)
			=> (IndexOf(property) >= 0);
		
		/// <summary> Removes the child at the specified index from this list property. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		void IList<Property>.RemoveAt(int index)
			=> RemoveAt(index);
		
		/// <summary> Removes the the first occurance of a specific property
		///           from this list property, returning if successful. </summary>
		public bool Remove(Property property)
		{
			var index = IndexOf(property);
			if (index >= 0) RemoveAt(index);
			return (index >= 0);
		}
		
		/// <summary> Clears all elements from this list property. </summary>
		public void Clear()
			=> _list.Clear();
		
		// ICollection implementation
		
		bool ICollection<Property>.IsReadOnly => false;
		
		void ICollection<Property>.CopyTo(Property[] array, int arrayIndex)
			=> _list.CopyTo(array, arrayIndex);
		
		// IEnumerable implementation
		
		/// <summary> Returns an enumerator that iterates
		///           over the elements in this list property. </summary>
		public IEnumerator<Property> GetEnumerator()
			=> _list.GetEnumerator();
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	
	public static class ProperlyListExtensions
	{
		// TODO: Add "TryAdd" and similar methods.
		
		private static PropertyList AsList(this Property self)
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (!(self is PropertyList l)) throw new InvalidOperationException(
				$"Not a list Property: '{ self.GetType().ToFriendlyString() }'");
			return l;
		}
		
		/// <summary> Adds the specified property to the end of the specified list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified list is not actually a list. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list or property is null. </exception>
		public static void Add(this Property self, Property property)
			=> self.AsList().Add(property);
		
		/// <summary> Adds the specified value to the end of the specified list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified list is not actually a list. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list or value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public static void Add<T>(this Property self, T value)
			=> self.AsList().Add(value);
		
		/// <summary> Inserts the specified property at the specified index in the specified list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified list is not actually a list. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list or property is null. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		public static void Insert(this Property self, int index, Property property)
			=> self.AsList().Insert(index, property);
		
		/// <summary> Inserts the specified value at the specified index in the specified list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified list is not actually a list. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list or value is null. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public static void Insert<T>(this Property self, int index, T value)
			=> self.AsList().Insert(index, value);
		
		/// <summary> Removes the child at the specified index from the specified list property, and returns the removed value. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified list is not actually a list. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list is null. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		public static Property RemoveAt(this Property self, int index)
			=> self.AsList().RemoveAt(index);
	}
}
