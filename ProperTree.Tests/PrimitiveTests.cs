using System;
using Xunit;

namespace ProperTree.Tests
{
	public class PrimitiveTests
	{
		[Fact]
		public void TestConversions()
		{
			PackUnpack<bool>(true);
			PackUnpack<byte>(1);
			PackUnpack<short>(1);
			PackUnpack<int>(1);
			PackUnpack<long>(1);
			PackUnpack<float>(1.0F);
			PackUnpack<double>(1.0);
			PackUnpack<string>("");
			
			void PackUnpack<T>(T value) {
				var generic     = Property.Of<T>(value);
				var objectified = Property.Of((object)value);
				Assert.IsType<PropertyPrimitive<T>>(generic);
				Assert.IsType<PropertyPrimitive<T>>(objectified);
				Assert.Equal(generic, objectified);
				Assert.Equal(value, generic.As<T>());
				Assert.Equal(value, objectified.As<T>());
			}
		}
		
		[Fact]
		public void TestBadConversions()
		{
			// This may be confusing, because one might think As<object>() converts to the
			// underlying primitive value, but instead it simply returns the property itself.
			Assert.IsType<PropertyPrimitive<string>>(
				Property.Of("don't actually do this").As<object>());
			
			Assert.Throws<ArgumentNullException>(() => Property.Of(null));
			Assert.Throws<ArgumentNullException>(() => Property.Of<string>(null));
			
			Assert.Throws<NotSupportedException>(() => Property.Of("foo").As<bool>());
			Assert.Throws<NotSupportedException>(() => Property.Of(true).As<string>());
			Assert.Throws<NotSupportedException>(() => Property.Of<byte>(1).As<int>());
			Assert.Throws<NotSupportedException>(() => Property.Of<int>(1).As<byte>());
			
			Assert.Equal(Property.Of<byte>(1).As<int>(2), 2);
			Assert.Equal(Property.Of<int>(1).As<byte>(2), 2);
		}
		
		[Fact]
		public void TestValue()
		{
			Check<bool>(true);
			Check<byte>(1);
			Check<short>(1);
			Check<int>(1);
			Check<long>(1);
			Check<float>(1.0F);
			Check<double>(1.0);
			Check<string>("");
			
			void Check<T>(T value) {
				var property = Property.Of(value);
				Assert.IsAssignableFrom<IPropertyPrimitive>(property);
				Assert.Equal(property.GetValue(), value);
			}
		}
		
		[Fact]
		public void TestBadValue()
		{
			var notPrimitive = new PropertyList();
			Assert.False(notPrimitive.TryGetValue(out var _));
			Assert.Throws<InvalidOperationException>(() => notPrimitive.GetValue());
		}
	}
}