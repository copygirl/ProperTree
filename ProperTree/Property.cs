using System;
using ProperTree.Utility;

namespace ProperTree
{
	/// <summary>
	///   Static helper class which provides instantiation methods to
	///   create <see cref="IProperty"/> objects, as well as extension
	///   methods to turn them back into primitive and data types.
	///   
	///   <seealso cref="PropertyConverterRegistry"/>
	/// </summary>
	public static class Property
	{
		/// <summary>
		///   Creates an <see cref="IProperty"/> from the specified value.
		///   If the value is already a property, it is returned.
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a Property. </exception>
		public static IProperty Of<T>(T value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is IProperty property) return property;
			if (PropertyConverterRegistry.TryGetToProperty<T>(out var converter)) return converter(value);
			throw new NotSupportedException($"Can't convert to Property from type '{ typeof(T).ToFriendlyName() }'");
		}
		
		/// <summary>
		///   Creates an <see cref="IProperty"/> from the specified value.
		///   If the value is already a property, it is returned.
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified value can't be converted to a property. </exception>
		public static IProperty Of(object value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value is IProperty property) return property;
			if (PropertyConverterRegistry.TryGetToProperty(value.GetType(), out var converter)) return converter(value);
			throw new NotSupportedException($"Can't convert to Property from type '{ value.GetType().ToFriendlyName() }'");
		}
		
		
		/// <summary>
		///   Converts or casts the specified <see cref="IProperty"/> to the specified type.
		///   
		///   Note that this works as an extension of the "as" operator,
		///   if no conversion to the specified type has been registered.
		///   <code>
		/// property.As&lt;PropertyList&gt;() == (property as PropertyList)
		/// property.As&lt;object&gt;() == (property as object)
		///   </code>
		///   
		///   To get any primitive property's underlying value, use
		///   <see cref="PropertyPrimitiveExtensions.GetValue"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		/// <exception cref="NotSupportedException"> Thrown if the specified property can't be converted to the specified type. </exception>
		public static T As<T>(this IProperty property)
		{
			if (!property.TryAs<T>(out var result))
				throw new NotSupportedException(
					$"Can't convert from '{ property.GetType().ToFriendlyName() }' " +
					$"to type '{ typeof(T).ToFriendlyName() }'");
			return result;
		}
		
		/// <summary>
		///   Converts or casts the specified <see cref="IProperty"/> to the specified
		///   type, or the specified default value if the conversion did not succeed.
		///   
		///   Note that this works as an extension of the "as" operator,
		///   if no conversion to the specified type has been registered.
		///   <code>
		/// property.As(new PropertyList()) == (property as PropertyList) ?? new PropertyList()
		/// property.As(null) == (property as object) ?? null
		///   </code>
		///   
		///   To get any primitive property's underlying value, use
		///   <see cref="PropertyPrimitiveExtensions.TryGetValue"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		public static T As<T>(this IProperty property, T @default)
			=> property.TryAs<T>(out var result) ? result : @default;
		
		/// <summary>
		///   Attempts to converts or cast the specified
		///   <see cref="IProperty"/> to the specified type.
		///   
		///   Note that this works as an extension of the "as" operator,
		///   if no conversion to the specified type has been registered.
		///   
		///   To get any primitive property's underlying value, use
		///   <see cref="PropertyPrimitiveExtensions.TryGetValue"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified property is null. </exception>
		public static bool TryAs<T>(this IProperty property, out T result)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));
			if (property is T r) {
				result = r;
				return true;
			}
			if (PropertyConverterRegistry.TryGetFromProperty<T>(
					property.GetType(), out var converter)) {
				result = converter(property);
				return true;
			}
			result = default(T);
			return false;
		}
	}
}
