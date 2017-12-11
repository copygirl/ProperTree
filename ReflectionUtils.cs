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
		
		public static string GetFriendlyName(this Type type, bool full = false)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			
			if (!full && _friendlyNameLookup.TryGetValue(type, out var shortName)) return shortName;
			
			if (type.IsArray)
				return $"{ type.GetElementType().GetFriendlyName(full) }[{ new string(',', type.GetArrayRank() - 1) }]";
			
			var name = full ? type.FullName : type.Name;
			if (!type.IsGenericType) return name;
			
			var typeArgumentNames = type.GenericTypeArguments.Select(t => GetFriendlyName(t, full));
			return $"{ name.Substring(0, name.LastIndexOf('`')) }<{ string.Join(",", typeArgumentNames) }>";
		}
	}
}
