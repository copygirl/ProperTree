using System;
using System.IO;
using System.Text;
using ProperTree.Serialization;
using Xunit;

namespace ProperTree.Tests
{
	public class DeSerializationTests
	{
		[Fact]
		public void BasicTest()
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
			
			var stream = new MemoryStream();
			
			var writer = new BinaryWriter(stream, Encoding.UTF8);
			BinaryDeSerializerRegistry.WriteProperty(writer, dict);
			stream.Position = 0;
			
			var reader = new BinaryReader(stream, Encoding.UTF8);
			var readDict = BinaryDeSerializerRegistry.ReadProperty(reader);
			
			Assert.Equal(dict, readDict);
		}
	}
}
