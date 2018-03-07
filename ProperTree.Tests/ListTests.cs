using System;
using System.Linq;
using Xunit;

namespace ProperTree.Tests
{
	public class ListTests
	{
		[Fact]
		public void TestList()
		{
			var primitiveList = new PropertyList { 1, 2, 3 };
			
			Assert.Equal(primitiveList.Count, 3);
			// TODO: Add As<IEnumerable<int>> conversion?!
			Assert.Equal(primitiveList.Select(p => p.As<int>()), new int[]{ 1, 2, 3 });
			Assert.Equal(primitiveList[0].As<int>(), 1);
			Assert.Equal(primitiveList[1].As<int>(), 2);
			Assert.Equal(primitiveList[2].As<int>(), 3);
			Assert.Throws<ArgumentOutOfRangeException>(() => primitiveList[-1]);
			Assert.Throws<ArgumentOutOfRangeException>(() => primitiveList[3]);
			
			primitiveList[1] = Property.Of(-1);
			Assert.Equal(primitiveList[1].As<int>(), -1);
			primitiveList.Add(4);
			Assert.Equal(primitiveList.Count, 4);
			Assert.Equal(primitiveList[3].As<int>(), 4);
			primitiveList.RemoveAt(3);
			Assert.Equal(primitiveList.Count, 3);
			primitiveList.Add("not int");
			Assert.Equal(primitiveList.Count, 4);
			primitiveList[1] = Property.Of("not int");
			Assert.Equal(primitiveList.Count, 4);
		}
	}
}
