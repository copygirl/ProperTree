using System;
using Xunit;

namespace ProperTree.Tests
{
	public class MapTests
	{
		[Fact]
		public void TestMap()
		{
			var map = new PropertyMap {
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
				{ "float array", Property.Of(new []{ 1.0F, 2.0F, 3.0F }) },
				
				{ "map list", new PropertyList {
					new PropertyMap { { "type",   "zombie" }, { "health", 20 } },
					new PropertyMap { { "type", "skeleton" }, { "health", 15 } },
					new PropertyMap { { "type",  "creeper" }, { "health", 10 } },
				} },
			};
			
			Assert.Equal(map[  "bool"].As<  bool>(),           true);
			Assert.Equal(map[  "byte"].As<  byte>(),      (byte)128);
			Assert.Equal(map[ "short"].As< short>(),   (short)32000);
			Assert.Equal(map[   "int"].As<   int>(),           1337);
			Assert.Equal(map[  "long"].As<  long>(), 0xD34DB33FC4FE);
			Assert.Equal(map[ "float"].As< float>(),          3.14F);
			Assert.Equal(map["double"].As<double>(),       0.000002);
			Assert.Equal(map["string"].As<string>(),         "gosh");
			
			Assert.Equal(map["unknown"], null);
			Assert.Throws<ArgumentNullException>(() => map[null]);
			
			Assert.Equal(map["map list"].As<PropertyList>().Count, 3);
			Assert.Equal(map["map list"][1]["type"].As<string>(), "skeleton");
			Assert.Equal(map["map list"][2]["health"].As<int>(), 10);
			
			map.Add("new", "Hi I'm new!");
			Assert.NotNull(map["new"]);
			Assert.Throws<ArgumentException>(() => map.Add("new", "Hello new, I'm dad!"));
			map.Remove("new");
			Assert.Null(map["new"]);
		}
	}
}
