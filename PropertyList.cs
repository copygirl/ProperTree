using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProperTree
{
	public class PropertyList : Property
	{
		private IList _list;
		
		public override bool IsCollection => true;
		public override bool IsList => true;
		
		/// <summary> Gets the element type of the list, or null if there's no set type. </summary>
		public Type ElementType
			=> (_list is Array array) ? array.GetType().GetElementType()     // If Array, use array element type.
			 : (_list?.Count > 0)     ? ((Property)_list[0]).Value.GetType() // Otherwise, use type of first element.
			                          : null;
		
		
		public override Property this[int index] {
			get {
				if (_list == null) throw new ArgumentOutOfRangeException(nameof(index));
				return Property.Of(_list[index]);
			}
			set {
				if (_list == null) throw new ArgumentOutOfRangeException(nameof(index));
				if (value == null) throw new ArgumentNullException(nameof(value));
				EnsurePropertyMatchesCurrentType(value, nameof(value));
				_list[index] = (_list is Array) ? value.Value : value;
			}
		}
		
		public override void Insert(int index, Property property)
		{
			var count = _list?.Count ?? 0;
			if ((index < 0) || (index > count))
				throw new ArgumentOutOfRangeException(nameof(index));
			if (property == null) throw new ArgumentNullException(nameof(property));
			EnsurePropertyMatchesCurrentType(property, nameof(property));
			EnsureIsResizablePropertyList();
			_list.Insert(index, property);
		}
		
		public override Property RemoveAt(int index)
		{
			var count = _list?.Count ?? 0;
			if ((index < 0) || (index >= count))
				throw new ArgumentOutOfRangeException(nameof(index));
			EnsureIsResizablePropertyList();
			
			var value = _list[index];
			_list.RemoveAt(index);
			return (Property)value;
		}
		
		/// <summary> Ensures that the internal list is a resizable list of Property objects
		///           instead of null (the initial value) or an array of raw types. </summary>
		private void EnsureIsResizablePropertyList()
		{
			switch (_list) {
				case null:
					_list = new List<Property>();
				break;
				
				case Array array:
					_list = new List<Property>(array.Length);
					var converter = PropertyRegistry.GetToPropertyConverter(((IList)array)[0].GetType());
					foreach (var value in array) _list.Add(converter(value));
				break;
			}
		}
		
		/// <summary> Ensures that the specified property matches
		///           this list property's current element type. </summary>
		private void EnsurePropertyMatchesCurrentType(Property property, string paramName)
		{
			switch (_list) {
				case Array array:
					var elementType = array.GetType().GetElementType();
					if (!property.IsPrimitive || (property.Value.GetType() != elementType)) throw new ArgumentException(
						$"Property type '{ property.GetType().GetFriendlyName() }' isn't " +
						$"compatible with list type '{ elementType.GetFriendlyName() }'", paramName);
				break;
				
				case List<Property> list:
					if ((list.Count > 0) && (property.GetType() != list[0].GetType())) throw new ArgumentException(
						$"Property type '{ property.GetType().GetFriendlyName() }' isn't " +
						$"compatible with list type '{ list[0].GetType().GetFriendlyName() }'", paramName);
				break;
			}
		}
		
		
		// IReadOnlyCollection implementation
		
		public override int Count => _list?.Count ?? 0;
		
		public override IEnumerator<Property> GetEnumerator()
		{
			switch (_list) {
				case null: yield break;
				
				case Array array:
					if (array.Length == 0) yield break;
					var converter = PropertyRegistry.GetToPropertyConverter(_list[0].GetType());
					foreach (var value in array) yield return converter(value);
				break;
				
				case List<Property> list:
					foreach (var value in list) yield return value;
				break;
			}
		}
		
		
		public static void RegisterConverters<T>()
		{
			PropertyRegistry.RegisterToPropertyConverter(
				(T[] value) => new PropertyList { _list = value });
			PropertyRegistry.RegisterFromPropertyConverter(
				(PropertyList property)
					=> (property._list == null)        ? new T[0]
					 : (property._list is Array array) ? (T[])array
					 : ((List<Property>)property._list).Select(prop => prop.As<T>()).ToArray());
		}
	}
}
