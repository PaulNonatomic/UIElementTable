using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements.TableElements
{
	/// <summary>
	/// A single table row. Owns its content cells and a reference to its row-number cell, and keeps
	/// its index (and the indices, label, and striping of those cells) in sync, so the table can
	/// manage rows as one list instead of parallel cell, row, and row-number collections.
	/// </summary>
	public class Row : VisualElement, IFlexibleRowHeight
	{
		public int Index { get; private set; }
		public RowHeaderCell NumberCell { get; set; }
		public IReadOnlyList<TableCell> Cells => _cells;
		public int CellCount => _cells.Count;

		private readonly List<TableCell> _cells = new List<TableCell>();

		public Row(int index)
		{
			Index = index;
			AddToClassList("ui-table__row");
			style.flexDirection = FlexDirection.Row;
			style.flexShrink = 0;
			ApplyStripeClass();
		}

		public TableCell GetCell(int columnIndex)
		{
			return _cells[columnIndex];
		}

		public void AddCell(TableCell cell)
		{
			_cells.Add(cell);
			Add(cell);
		}

		public void RemoveCellAt(int columnIndex)
		{
			Remove(_cells[columnIndex]);
			_cells.RemoveAt(columnIndex);
		}

		/// <summary>
		/// Reassigns this row's index and propagates it to the content cells and the row-number cell
		/// (index, label, and striping), keeping everything consistent after an insertion or removal.
		/// </summary>
		public void SetIndex(int index)
		{
			Index = index;
			ApplyStripeClass();

			foreach (var cell in _cells)
			{
				cell.SetRowIndex(index);
			}

			if (NumberCell == null) return;

			NumberCell.SetRowIndex(index);
			NumberCell.SetLabel((index + 1).ToString());
			NumberCell.RemoveFromClassList("ui-table__fixed-column--even");
			NumberCell.RemoveFromClassList("ui-table__fixed-column--odd");
			NumberCell.AddToClassList((index + 1) % 2 == 0 ? "ui-table__fixed-column--even" : "ui-table__fixed-column--odd");
		}

		public void SetRowHeight(float height, bool flexible = false)
		{
			if (flexible)
			{
				style.minHeight = height;
				style.height = StyleKeyword.Null;
			}
			else
			{
				style.height = height;
				style.minHeight = StyleKeyword.Null;
				style.overflow = Overflow.Hidden;
			}

			MarkDirtyRepaint();
		}

		public void SetRowWidth(float width)
		{
			style.width = width;
		}

		private void ApplyStripeClass()
		{
			RemoveFromClassList("ui-table__row--even");
			RemoveFromClassList("ui-table__row--odd");
			AddToClassList((Index + 1) % 2 == 0 ? "ui-table__row--even" : "ui-table__row--odd");
		}
	}
}
