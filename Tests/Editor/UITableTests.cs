using System;
using NUnit.Framework;
using Nonatomic.UIElements.TableElements;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements.Tests
{
	/// <summary>
	/// Exercises the table operations that the single-row-list refactor touched: row and column
	/// add/remove, reindexing, striping, and cell content rebinding. These assert on synchronous
	/// element state and class lists, not layout-resolved styles, so they run without a live panel.
	/// </summary>
	[TestFixture]
	public class UITableTests
	{
		private static UITable BuildTable(int rows, int columns)
		{
			return new UITable(rowCount: rows, columnCount: columns, includeRowNumbers: true);
		}

		private static TableCell Cell(UITable table, int column, int row)
		{
			return (TableCell)table.GetCell(column, row);
		}

		[Test]
		public void Constructor_SetsRowAndColumnCounts()
		{
			var table = BuildTable(3, 4);

			Assert.AreEqual(3, table.RowCount);
			Assert.AreEqual(4, table.ColumnCount);
		}

		[Test]
		public void Cells_HaveMatchingRowAndColumnIndices()
		{
			var table = BuildTable(2, 3);

			for (var row = 0; row < table.RowCount; row++)
			{
				for (var column = 0; column < table.ColumnCount; column++)
				{
					var cell = Cell(table, column, row);
					Assert.AreEqual(row, cell.RowIndex, $"RowIndex at ({column},{row})");
					Assert.AreEqual(column, cell.ColumnIndex, $"ColumnIndex at ({column},{row})");
				}
			}
		}

		[Test]
		public void AddRow_IncrementsRowCount_AndIndexesNewRow()
		{
			var table = BuildTable(2, 2);

			table.AddRow();

			Assert.AreEqual(3, table.RowCount);
			Assert.AreEqual(2, Cell(table, 0, 2).RowIndex);
			Assert.AreEqual(2, Cell(table, 1, 2).RowIndex);
		}

		[Test]
		public void RemoveRow_DecrementsCount_AndReindexesRemainingRows()
		{
			var table = BuildTable(3, 2);

			table.RemoveRow(0);

			Assert.AreEqual(2, table.RowCount);

			// Every remaining row must be reindexed 0..n with no gaps and its cells kept in sync.
			for (var row = 0; row < table.RowCount; row++)
			{
				Assert.AreEqual(row, Cell(table, 0, row).RowIndex);
				Assert.AreEqual(row, Cell(table, 1, row).RowIndex);
			}
		}

		[Test]
		public void RemoveRow_UpdatesRowStriping()
		{
			var table = BuildTable(3, 1);

			table.RemoveRow(0);

			var firstRow = (Row)Cell(table, 0, 0).parent;
			var secondRow = (Row)Cell(table, 0, 1).parent;

			// Row at index 0 is "odd" ((0+1)%2==1); index 1 is "even".
			Assert.AreEqual(0, firstRow.Index);
			Assert.IsTrue(firstRow.ClassListContains("ui-table__row--odd"));
			Assert.IsFalse(firstRow.ClassListContains("ui-table__row--even"));

			Assert.AreEqual(1, secondRow.Index);
			Assert.IsTrue(secondRow.ClassListContains("ui-table__row--even"));
			Assert.IsFalse(secondRow.ClassListContains("ui-table__row--odd"));
		}

		[Test]
		public void RemoveRow_OutOfRange_Throws()
		{
			var table = BuildTable(2, 2);

			Assert.Throws<ArgumentOutOfRangeException>(() => table.RemoveRow(5));
		}

		[Test]
		public void AddColumn_IncrementsColumnCount_AndIndexesNewCells()
		{
			var table = BuildTable(2, 2);

			table.AddColumn(new ColumnDefinition("Added", 80f));

			Assert.AreEqual(3, table.ColumnCount);

			for (var row = 0; row < table.RowCount; row++)
			{
				var newCell = Cell(table, 2, row);
				Assert.AreEqual(2, newCell.ColumnIndex, $"ColumnIndex on new cell, row {row}");
				Assert.AreEqual(row, newCell.RowIndex, $"RowIndex on new cell, row {row}");
			}
		}

		[Test]
		public void RemoveColumn_DecrementsColumnCount()
		{
			var table = BuildTable(2, 3);

			table.RemoveColumn(1);

			Assert.AreEqual(2, table.ColumnCount);
		}

		[Test]
		public void RemoveColumn_OutOfRange_Throws()
		{
			var table = BuildTable(2, 2);

			Assert.Throws<ArgumentOutOfRangeException>(() => table.RemoveColumn(10));
		}

		[Test]
		public void SetColumn_BeyondRange_AddsColumn()
		{
			var table = BuildTable(2, 2);

			table.SetColumn(5, new ColumnDefinition("Appended", 70f));

			Assert.AreEqual(3, table.ColumnCount);
		}

		[Test]
		public void GetCell_OutOfRange_Throws()
		{
			var table = BuildTable(2, 2);

			Assert.Throws<ArgumentOutOfRangeException>(() => table.GetCell(0, 9));
			Assert.Throws<ArgumentOutOfRangeException>(() => table.GetCell(9, 0));
		}

		[Test]
		public void GetRow_ReturnsCellsForRow()
		{
			var table = BuildTable(2, 3);

			var cells = table.GetRow(1);

			Assert.AreEqual(3, cells.Count);
		}

		[Test]
		public void GetColumn_ReturnsCellPerRow()
		{
			var table = BuildTable(3, 2);

			var cells = table.GetColumn(0);

			Assert.AreEqual(3, cells.Count);
		}

		[Test]
		public void SetCell_ReplacesExistingContent()
		{
			var table = BuildTable(1, 1);
			var first = new Label("first");
			var second = new Label("second");

			table.SetCell(0, 0, first);
			Assert.IsTrue(table.GetCell(0, 0).Contains(first));

			// Rebinding the same cell must clear the previous content, not stack it.
			table.SetCell(0, 0, second);
			Assert.IsFalse(table.GetCell(0, 0).Contains(first));
			Assert.IsTrue(table.GetCell(0, 0).Contains(second));
		}

		[Test]
		public void AddThenRemoveRows_LeavesContiguousIndices()
		{
			// Mirrors a data-bound refresh: rows are appended then pruned. The 0.5.0 bug left stale
			// and duplicate rows; this guards the single-list reindexing that replaced it.
			var table = BuildTable(2, 1);

			table.AddRow();
			table.AddRow();
			Assert.AreEqual(4, table.RowCount);

			table.RemoveRow(1);
			table.RemoveRow(0);
			Assert.AreEqual(2, table.RowCount);

			Assert.AreEqual(0, Cell(table, 0, 0).RowIndex);
			Assert.AreEqual(1, Cell(table, 0, 1).RowIndex);
		}
	}
}
