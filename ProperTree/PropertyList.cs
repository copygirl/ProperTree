using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProperTree.Utility;

namespace ProperTree
{
	public class PropertyList
		: IProperty, IList<IProperty>
	{
		public static readonly int MAX_SIZE = ushort.MaxValue;
		
		
		private readonly List<IProperty> _list;
		
		public IProperty this[int index] {
			get => _list[index];
			set {
				if (value == null) throw new ArgumentNullException(nameof(value));
				_list[index] = value;
			}
		}
		
		public IProperty this[string name] {
			get => throw new InvalidOperationException($"Not a map property: '{ GetType().ToFriendlyName() }'");
			set => throw new InvalidOperationException($"Not a map property: '{ GetType().ToFriendlyName() }'");
		}
		
		internal PropertyList(List<IProperty> list)
			=> _list = list;
		public PropertyList()
			: this(new List<IProperty>()) {  }
		
		
		/// <summary> Adds the specified property to the end of this list property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if this list property's size is already at <see cref="MAX_SIZE"/>. </exception>
		public void Add(IProperty property)
			=> Insert(Count, property);
		
		/// <summary> Adds the specified value to the end of this list property. </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to an <see cref="IProperty"/>. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if this list property's size is already at <see cref="MAX_SIZE"/>. </exception>
		public void Add<T>(T value)
			=> Add(Property.Of(value));
		
		/// <summary> Inserts the specified property at the specified index in this list property. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if this list property's size is already at <see cref="MAX_SIZE"/>. </exception>
		public void Insert(int index, IProperty property)
		{
			var count = _list?.Count ?? 0;
			if ((index < 0) || (index > count))
				throw new ArgumentOutOfRangeException(nameof(index));
			if (property == null) throw new ArgumentNullException(nameof(property));
			if (_list.Count == MAX_SIZE) throw new InvalidOperationException(
				$"{ nameof(PropertyList) } can't exceed MAX_SIZE ({ MAX_SIZE })");
			_list.Insert(index, property);
		}
		
		/// <summary> Inserts the specified value at the specified index in this list property. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		/// <exception cref="InvalidOperationException"> Thrown if this list property's size is already at <see cref="MAX_SIZE"/>. </exception>
		public void Insert<T>(int index, T value)
			=> Insert(index, Property.Of(value));
		
		/// <summary> Removes the child at the specified index from this list property, and returns the removed value. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		public IProperty RemoveAt(int index)
		{
			var count = _list?.Count ?? 0;
			if ((index < 0) || (index >= count))
				throw new ArgumentOutOfRangeException(nameof(index));
			
			var value = _list[index];
			_list.RemoveAt(index);
			return (IProperty)value;
		}
		
		
		// IEquatable implementation
		
		public bool Equals(IProperty other)
			=> (other is PropertyList list)
				&& _list.SequenceEqual(list._list);
		
		
		// IList implementation
		
		/// <summary> Gets the number of elements in this list property. </summary>
		public int Count => _list?.Count ?? 0;
		
		/// <summary> Returns the index of a specific property
		///           in this list property, or -1 if not found. </summary>
		public int IndexOf(IProperty property)
			=> _list.IndexOf(property);
		
		/// <summary> Returns if a specific property exists in this list property. </summary>
		public bool Contains(IProperty property)
			=> (IndexOf(property) >= 0);
		
		/// <summary> Removes the child at the specified index from this list property. </summary>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		void IList<IProperty>.RemoveAt(int index)
			=> RemoveAt(index);
		
		/// <summary> Removes the the first occurance of a specific property
		///           from this list property, returning if successful. </summary>
		public bool Remove(IProperty property)
		{
			var index = IndexOf(property);
			if (index >= 0) RemoveAt(index);
			return (index >= 0);
		}
		
		/// <summary> Clears all elements from this list property. </summary>
		public void Clear()
			=> _list.Clear();
		
		
		// ICollection implementation
		
		bool ICollection<IProperty>.IsReadOnly => false;
		
		void ICollection<IProperty>.CopyTo(IProperty[] array, int arrayIndex)
			=> _list.CopyTo(array, arrayIndex);
		
		
		// IEnumerable implementation
		
		/// <summary> Returns an enumerator that iterates
		///           over the elements in this list property. </summary>
		public IEnumerator<IProperty> GetEnumerator()
			=> _list.GetEnumerator();
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	
	public static class ProperlyListExtensions
	{
		// TODO: Add "TryAdd" and similar methods.
		
		private static PropertyList AsList(this IProperty self)
		{
			if (self == null) throw new ArgumentNullException(nameof(self));
			if (!(self is PropertyList l)) throw new InvalidOperationException(
				$"Not a list property: '{ self.GetType().ToFriendlyName() }'");
			return l;
		}
		
		/// <summary> Adds the specified property to the end of the specified list property. </summary>
		/// <exception cref="InvalidOperationException">
		///   Thrown if the specified list is not actually a list
		///   -OR- if this list property's size is already at <see cref="PropertyList.MAX_SIZE"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list or property is null. </exception>
		public static void Add(this IProperty self, IProperty property)
			=> self.AsList().Add(property);
		
		/// <summary> Adds the specified value to the end of the specified list property. </summary>
		/// <exception cref="InvalidOperationException">
		///   Thrown if the specified list is not actually a list
		///   -OR- if this list property's size is already at <see cref="PropertyList.MAX_SIZE"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list or value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to an <see cref="IProperty"/>. </exception>
		public static void Add<T>(this IProperty self, T value)
			=> self.AsList().Add(value);
		
		/// <summary> Inserts the specified property at the specified index in the specified list property. </summary>
		/// <exception cref="InvalidOperationException">
		///   Thrown if the specified list is not actually a list
		///   -OR- if this list property's size is already at <see cref="PropertyList.MAX_SIZE"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list or property is null. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		public static void Insert(this IProperty self, int index, IProperty property)
			=> self.AsList().Insert(index, property);
		
		/// <summary> Inserts the specified value at the specified index in the specified list property. </summary>
		/// <exception cref="InvalidOperationException">
		///   Thrown if the specified list is not actually a list
		///   -OR- if this list property's size is already at <see cref="PropertyList.MAX_SIZE"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list or value is null. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to an <see cref="IProperty"/>. </exception>
		public static void Insert<T>(this IProperty self, int index, T value)
			=> self.AsList().Insert(index, value);
		
		/// <summary> Removes the child at the specified index from the specified list property, and returns the removed value. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if the specified list is not actually a list. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified list is null. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		public static IProperty RemoveAt(this IProperty self, int index)
			=> self.AsList().RemoveAt(index);
	}
}
