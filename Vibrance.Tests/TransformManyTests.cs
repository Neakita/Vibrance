using System.Reactive.Linq;
using FluentAssertions;
using Vibrance.Changes;

namespace Vibrance.Tests;

public sealed class TransformManyTests
{
	[Fact]
	public void ShouldConcatenateInitialValues()
	{
		ObservableList<int> list1 = [1, 2, 3];
		ObservableList<int> list2 = [4, 5, 6];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		resultList.Should().ContainInOrder(1, 2, 3, 4, 5, 6);
	}

	[Fact]
	public void ShouldInsertList()
	{
		ObservableList<int> initialList = [4, 5, 6];
		ObservableList<ObservableList<int>> lists = [initialList];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		ObservableList<int> insertionList = [1, 2, 3];
		lists.Insert(0, insertionList);
		resultList.Should().ContainInOrder(1, 2, 3, 4, 5, 6);
	}

	[Fact]
	public void ShouldAddToFirstList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [4, 5, 6];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		list1.Add(3);
		resultList.Should().ContainInOrder(1, 2, 3, 4, 5, 6);
	}

	[Fact]
	public void ShouldRemoveFromFirstList()
	{
		ObservableList<int> list1 = [1, 2, 3];
		ObservableList<int> list2 = [4, 5, 6];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		list1.RemoveAt(2);
		resultList.Should().ContainInOrder(1, 2, 4, 5, 6);
	}

	[Fact]
	public void ShouldInsertAnotherList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [5, 6];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		ObservableList<int> insertionList = [3, 4];
		lists.Insert(1, insertionList);
		resultList.Should().ContainInOrder(1, 2, 3, 4, 5, 6);
	}

	[Fact]
	public void ShouldEmptySourceList()
	{
		ObservableList<ObservableList<int>> lists = [];
		var resultList = lists
			.TransformMany(ints => ints)
			.Do(_ => throw new Exception("Shouldn't be any changes"))
			.ToObservableList();
		resultList.Should().BeEmpty();
	}

	[Fact]
	public void ShouldEmptyInnerLists()
	{
		ObservableList<int> emptyList1 = [];
		ObservableList<int> emptyList2 = [];
		ObservableList<ObservableList<int>> lists = [emptyList1, emptyList2];
		var resultList = lists
			.TransformMany(ints => ints)
			.Do(_ => throw new Exception("Shouldn't be any changes"))
			.ToObservableList();
		resultList.Should().BeEmpty();
	}

	[Fact]
	public void ShouldRemoveFromMiddleList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [3, 4, 5];
		ObservableList<int> list3 = [6, 7];
		ObservableList<ObservableList<int>> lists = [list1, list2, list3];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		list2.RemoveAt(1);
		resultList.Should().ContainInOrder(1, 2, 3, 5, 6, 7);
	}

	[Fact]
	public void ShouldClearInnerList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [3, 4, 5];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		list2.Clear();
		resultList.Should().ContainInOrder(1, 2);
	}

	[Fact]
	public void ShouldReplaceInnerList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [3, 4];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		ObservableList<int> newList2 = [5, 6, 7];
		lists[1] = newList2;
		resultList.Should().ContainInOrder(1, 2, 5, 6, 7);
	}

	[Fact]
	public void ShouldRemoveSourceList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [3, 4];
		ObservableList<int> list3 = [5, 6];
		ObservableList<ObservableList<int>> lists = [list1, list2, list3];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		lists.RemoveAt(1);
		resultList.Should().ContainInOrder(1, 2, 5, 6);
	}

	[Fact]
	public void ShouldClearSourceList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [3, 4];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		lists.Clear();
		resultList.Should().BeEmpty();
	}

	[Fact]
	public void ShouldMoveInSourceList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [3, 4];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		lists.Move(0, 1);
		resultList.Should().ContainInOrder(3, 4, 1, 2);
	}

	[Fact]
	public void ShouldMultipleInnerListChanges()
	{
		ObservableList<int> list1 = [1];
		ObservableList<int> list2 = [5];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		list1.Add(2);
		list1.Add(3);
		list2.Insert(0, 4);
		list2.Add(6);
		resultList.Should().ContainInOrder(1, 2, 3, 4, 5, 6);
	}

	[Fact]
	public void ShouldNestedTransformMany()
	{
		ObservableList<ObservableList<ObservableList<int>>> nestedLists = [[[1, 2]], [[3, 4]]];
		var resultList = nestedLists
			.TransformMany(outer => outer
			.TransformMany(inner => inner))
			.ToObservableList();
		resultList.Should().ContainInOrder(1, 2, 3, 4);
	}

	[Fact]
	public void RemovedListShouldNotAffectResultList()
	{
		ObservableList<int> list1 = [1, 2];
		ObservableList<int> list2 = [3, 4];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		lists.RemoveAt(0);
		list1.Add(5);
		resultList.Should().ContainInOrder(3, 4);
	}

	[Fact]
	public void ShouldDispose()
	{
		ObservableList<ObservableList<int>> lists = [[1, 2]];
		var resultList = lists.TransformMany(ints => ints).ToObservableList();
		resultList.Dispose();
		lists[0].Add(3);
		lists.Add([3]);
		resultList.Should().NotContain(3);
	}

	[Fact]
	public void ShouldNotObserveChangesFromRemovedList()
	{
		ObservableList<int> list1 = [1, 2, 3];
		ObservableList<int> list2 = [4, 5, 6];
		ObservableList<ObservableList<int>> lists = [list1, list2];
		var resultList = lists
			.TransformMany(ints => ints)
			.ToObservableList();
		lists.RemoveAt(1);
		list2.Add(7);
		resultList.Should().NotContain(7);
	}
}