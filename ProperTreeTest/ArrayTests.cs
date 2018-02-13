using System;
using ProperTree;
using Xunit;

namespace ProperTreeTest
{
	public class ArrayTests
	{
		[Fact]
		public void TestArrayConversions()
		{
			PackUnpack<bool>(true, false, true);
			PackUnpack<byte>(1, 2, 3);
			PackUnpack<short>(1, 2, 3);
			PackUnpack<int>(1, 2, 3);
			PackUnpack<long>(1, 2, 3);
			PackUnpack<float>(1.0F, 2.0F, 3.0F);
			PackUnpack<double>(1.0, 2.0, 3.0);
			
			void PackUnpack<T>(params T[] value)
				where T : struct
			{
				var generic     = Property.Of<T[]>(value);
				var objectified = Property.Of((object)value);
				
				Assert.IsType<PropertyArray<T>>(generic);
				Assert.IsType<PropertyArray<T>>(objectified);
				Assert.Equal(generic.As<IPropertyArray>().ElementType, typeof(T));
				
				Assert.Equal(generic, objectified);
				Assert.Equal(value, generic.As<T[]>());
				Assert.Equal(value, objectified.As<T[]>());
			}
		}
		
		[Fact]
		public void TestBadConversions()
		{
			Assert.Throws<NotSupportedException>(()
				=> Property.Of(new object[]{ 2, "two", 2.0 }));
			Assert.Throws<NotSupportedException>(()
				=> Property.Of(new string[]{ "a", "b", "c" }));
			
			Assert.Throws<ArgumentNullException>(() => Property.Of<byte[]>(null));
		}
	}
}