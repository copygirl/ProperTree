using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ProperTree.Tests
{
	public class DictionaryTests
	{
		[Fact]
		public void TestDictionary()
		{
			var dict = new PropertyDictionary {
				{   "bool" ,           true },
				{   "byte" ,      (byte)128 },
				{  "short" ,   (short)32000 },
				{    "int" ,           1337 },
				{   "long" , 0xD34DB33FC4FE },
				{  "float" ,          3.14F },
				{ "double" ,       0.000002 },
				{ "string" ,         "gosh" },
				
				{ "primitive list", new PropertyList { "a", "b", "c" } },
				{ "byte array", Property.Of(new byte[]{ 1, 2, 3 }) },
				{ "float array", Property.Of(new float[]{ 1.0F, 2.0F, 3.0F }) },
				
				{ "dictionary list", new PropertyList {
					new PropertyDictionary { { "type",   "zombie" }, { "health", 20 } },
					new PropertyDictionary { { "type", "skeleton" }, { "health", 15 } },
					new PropertyDictionary { { "type",  "creeper" }, { "health", 10 } },
				} },
			};
			
			Assert.Equal(dict[  "bool"].As<  bool>(),           true);
			Assert.Equal(dict[  "byte"].As<  byte>(),      (byte)128);
			Assert.Equal(dict[ "short"].As< short>(),   (short)32000);
			Assert.Equal(dict[   "int"].As<   int>(),           1337);
			Assert.Equal(dict[  "long"].As<  long>(), 0xD34DB33FC4FE);
			Assert.Equal(dict[ "float"].As< float>(),          3.14F);
			Assert.Equal(dict["double"].As<double>(),       0.000002);
			Assert.Equal(dict["string"].As<string>(),         "gosh");
			
			Assert.Equal(dict["unknown"], null);
			Assert.Throws<ArgumentNullException>(() => dict[null]);
			
			Assert.Equal(dict["dictionary list"].As<PropertyList>().Count, 3);
			Assert.Equal(dict["dictionary list"][1]["type"].As<string>(), "skeleton");
			Assert.Equal(dict["dictionary list"][2]["health"].As<int>(), 10);
			
			dict.Add("new", "Hi I'm new!");
			Assert.NotNull(dict["new"]);
			Assert.Throws<ArgumentException>(() => dict.Add("new", "Hello new, I'm dad!"));
			dict.Remove("new");
			Assert.Null(dict["new"]);
		}
	}
}
