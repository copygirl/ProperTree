using System;
using Xunit;

namespace ProperTree.Tests
{
	public class ConvertTests
	{
		public ConvertTests()
		{
			PropertyConverterRegistry.RegisterToProperty<BlockPos>(
				(pos) => new PropertyMap { { "X", pos.X }, { "Y", pos.Y }, { "Z", pos.Z } });
			PropertyConverterRegistry.RegisterFromProperty<PropertyMap, BlockPos>((map) => {
				int x = 0, y = 0, z = 0;
				// FIXME: This syntax is stupid and annoying to use.
				if (map["X"]?.TryAs(out x) != true) throw new Exception("int X not found");
				if (map["Y"]?.TryAs(out y) != true) throw new Exception("int Y not found");
				if (map["Z"]?.TryAs(out z) != true) throw new Exception("int Z not found");
				return new BlockPos(x, y, z);
			});
		}
		
		[Fact]
		public void TestCustomConversion()
		{
			var pos  = new BlockPos(2, 10, 42);
			var prop = Property.Of(pos);
			Assert.Equal(pos, prop.As<BlockPos>());
			
			prop.Add("W", "not int");
			Assert.Equal(pos, prop.As<BlockPos>());
			
			prop["Y"] = Property.Of("not int");
			Assert.Throws<Exception>(() => prop.As<BlockPos>());
		}
		
		struct BlockPos
		{
			public int X, Y, Z;
			
			public BlockPos(int x, int y, int z)
				{ X = x; Y = y; Z = z; }
		}
	}
}
