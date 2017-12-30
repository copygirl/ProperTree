﻿using System;
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
		/// b.ToString() == "System.Byte"
		/// b.ToFriendlyString() = "byte"
		/// 
		/// var f = typeof(Func&lt;byte&gt;);
		/// f.ToString() == "System.Func`1"
		/// f.ToFriendlyString() == "Func&lt;byte&gt;"
		/// f.ToFriendlyString(true) == "System.Func&lt;System.Byte&gt;";
		///   </code>
		/// </summary>
		/// <exception cref="ArgumentNullException"> Thrown if the specified type is null. </exception>
		public static string ToFriendlyString(this Type type, bool full = false)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			
			if (!full && _friendlyNameLookup.TryGetValue(type, out var shortName)) return shortName;
			
			if (type.IsArray)
				return $"{ type.GetElementType().ToFriendlyString(full) }[{ new string(',', type.GetArrayRank() - 1) }]";
			
			var name = full ? type.FullName : type.Name;
			if (!type.IsGenericType) return name;
			
			var typeArgumentNames = type.GenericTypeArguments.Select(t => ToFriendlyString(t, full));
			return $"{ name.Substring(0, name.LastIndexOf('`')) }<{ string.Join(",", typeArgumentNames) }>";
		}
	}
}
