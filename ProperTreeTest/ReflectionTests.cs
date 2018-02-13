using System;
using System.Collections.Generic;
using ProperTree;
using Xunit;

namespace ProperTreeTest
{
	public class ReflectionTests
	{
		[Theory]
		[InlineData( typeof(Guid)       , "Guid" )]
		[InlineData( typeof(byte)       , "Byte" )]
		[InlineData( typeof(Func<Guid>) , "Func`1" )]
		[InlineData( typeof(Func<byte>) , "Func`1" )]
		[InlineData( typeof(Tuple<byte, Action, Action<Tuple<string, Guid>, ushort>>) ,
		             "Tuple`3" )]
		public void BuiltIn_Name(Type type, string expected)
			=> Assert.Equal(expected, type.Name);
		
		[Theory]
		[InlineData( typeof(Guid)       , "System.Guid" )]
		[InlineData( typeof(byte)       , "System.Byte" )]
		[InlineData( typeof(Func<Guid>) , "System.Func`1[System.Guid]" )]
		[InlineData( typeof(Func<byte>) , "System.Func`1[System.Byte]" )]
		[InlineData( typeof(Tuple<byte, Action, Action<Tuple<string, Guid>, ushort>>) ,
		             "System.Tuple`3[System.Byte,System.Action,System.Action`2[System.Tuple`2[System.String,System.Guid],System.UInt16]]" )]
		public void BuiltIn_ToString(Type type, string expected)
			=> Assert.Equal(expected, type.ToString());
		
		[Theory]
		[InlineData( typeof(Guid)       , "Guid" )]
		[InlineData( typeof(byte)       , "byte" )]
		[InlineData( typeof(Func<Guid>) , "Func<Guid>" )]
		[InlineData( typeof(Func<byte>) , "Func<byte>" )]
		[InlineData( typeof(Tuple<byte, Action, Action<Tuple<string, Guid>, ushort>>) ,
		             "Tuple<byte,Action,Action<Tuple<string,Guid>,ushort>>" )]
		public void BuiltIn_FriendlyName(Type type, string expected)
			=> Assert.Equal(expected, type.ToFriendlyName());
		
		[Theory]
		[InlineData( typeof(Guid)       , "System.Guid" )]
		[InlineData( typeof(byte)       , "System.Byte" )]
		[InlineData( typeof(Func<Guid>) , "System.Func<System.Guid>" )]
		[InlineData( typeof(Func<byte>) , "System.Func<System.Byte>" )]
		[InlineData( typeof(Tuple<byte, Action, Action<Tuple<string, Guid>, ushort>>) ,
		             "System.Tuple<System.Byte,System.Action,System.Action<System.Tuple<System.String,System.Guid>,System.UInt16>>" )]
		public void BuiltIn_FriendlyName_Full(Type type, string expected)
			=> Assert.Equal(expected, type.ToFriendlyName(true));
	}
}
