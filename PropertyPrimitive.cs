using System;

namespace ProperTree
{
	public class PropertyPrimitive<T> : Property
	{
		public override bool IsPrimitive => true;
		
		public override object Value => _value;
		
		private T _value;
		
		public PropertyPrimitive(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			_value = value;
		}
		
		
		public static void RegisterConverters()
		{
			PropertyRegistry.RegisterToPropertyConverter(
				(T value) => new PropertyPrimitive<T>(value));
			PropertyRegistry.RegisterFromPropertyConverter(
				(PropertyPrimitive<T> property) => property._value);
		}
	}
}
