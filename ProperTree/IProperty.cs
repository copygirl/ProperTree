using System;

namespace ProperTree
{
	/// <summary>
	///   Base interface for all Property classes. Used to store
	///   additional, extensible information on game objects in a
	///   common format that can be easily and compactly read from
	///   and written to files and network streams.
	///   
	///   <seealso cref="Property"/>
	/// </summary>
	public interface IProperty
		: IEquatable<IProperty>
	{
		/// <summary> Gets or sets the property at the specified index of this list property. </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a list. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> Thrown if the specified index is not valid. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified value is null. </exception>
		IProperty this[int index] { get; set; }
		
		/// <summary>
		///   Gets or sets the property with the specified name of this dictionary property.
		///   
		///   If the specified name is found, a get operation will return the associated property,
		///   and a set operation will overwrite the existing entry with the specified value,
		///   or remove it if the specified value is null.
		///   
		///   If the specified name is not found, a get operation will return null,
		///   and a set operation adds a new entry with the specified value.
		/// </summary>
		/// <exception cref="InvalidOperationException"> Thrown if this property is not a dictionary. </exception>
		/// <exception cref="ArgumentNullException"> Thrown if the specified name is null. </exception>
		IProperty this[string name] { get; set; }
	}
}
