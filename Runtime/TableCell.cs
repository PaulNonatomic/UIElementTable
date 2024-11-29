using UnityEngine;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class TableCell : VisualElement
	{
		public int ColumnIndex { get; private set; }
		public int RowIndex { get; private set; }
		
		public TableCell(int columnIndex, int rowIndex)
		{
			name = $"Cell_{columnIndex}_{rowIndex}";
			ColumnIndex = columnIndex;
			RowIndex = rowIndex;
			
			AddToClassList("ui-table__cell");
			RegisterCallback<PointerEnterEvent>(evt => HandleCellPointerEnter());
			RegisterCallback<PointerLeaveEvent>(evt => HandleCellPointerLeave());
			RegisterCallback<PointerDownEvent>(evt => HandleCellPointerDown());
			RegisterCallback<PointerUpEvent>(evt => HandleCellPointerUp());
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

		private void HandleCellPointerUp()
		{
			AddToClassList("ui-table__cell--highlighted");
			RemoveFromClassList("ui-table__cell--down");
		}

		private void HandleCellPointerDown()
		{
			RemoveFromClassList("ui-table__cell--highlighted");
			AddToClassList("ui-table__cell--down");
		}

		public void SetWidth(float width)
		{
			style.width = width;
		}
		
		private void HandleCellPointerEnter()
		{
			AddToClassList("ui-table__cell--highlighted");
		}

		private void HandleCellPointerLeave()
		{
			RemoveFromClassList("ui-table__cell--highlighted");
			RemoveFromClassList("ui-table__cell--down");
		}
	}
}