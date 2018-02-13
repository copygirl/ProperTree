using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProperTree
{
	public static class ReflectionUtils
	{
		static readonly Dictionary<Type, string> _friendlyNameLookup
			= new Dictionary<Type, string>() {
				[typeof(   void)] =    "void" , [typeof(object)] = "object" ,
				[typeof(  sbyte)] =   "sbyte" , [typeof(  byte)] =   "byte" ,
				[typeof(    int)] =     "int" , [typeof(  uint)] =   "uint" ,
				[typeof(  short)] =   "short" , [typeof(ushort)] = "ushort" ,
				[typeof(   long)] =    "long" , [typeof( ulong)] =  "ulong" ,
				[typeof(  float)] =   "float" , [typeof(double)] = "double" ,
				[typeof(decimal)] = "decimal" , [typeof(  bool)] =   "bool" ,
				[typeof(   char)] =    "char" , [typeof(string)] = "string" ,
			};
		
		/// <summary>
		///   Converts the specified type into a more human-friendly string.
		///   
		///   Examples:
		///   <code>
		/// var b = typeof(byte);
		/// b.Name == "Byte"
		/// b.ToString() == "System.Byte"
		/// b.ToFriendlyName() = "byte"
		/// 
		/// var f = typeof(Func&lt;byte&gt;);
		/// f.Name == "Func`1"
		/// f.ToString() == "System.Func`1[System.Byte]"
		/// f.ToFriendlyName() == "Func&lt;byte&gt;"
		/// f.ToFriendlyName(true) == "System.Func&lt;System.Byte&gt;"
		///   </code>
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified type is null. </exception>
		public static string ToFriendlyName(this Type type, bool full = false)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			
			if (!full && _friendlyNameLookup.TryGetValue(type, out var shortName)) return shortName;
			
			if (type.IsArray)
				return $"{ type.GetElementType().ToFriendlyName(full) }[{ new string(',', type.GetArrayRank() - 1) }]";
			
			var name = full ? type.ToString() : type.Name;
			if (!type.IsGenericType) return name;
			
			var typeArgumentNames = type.GenericTypeArguments.Select(t => ToFriendlyName(t, full));
			return $"{ name.Substring(0, name.IndexOf('`')) }<{ string.Join(",", typeArgumentNames) }>";
		}
	}
}
