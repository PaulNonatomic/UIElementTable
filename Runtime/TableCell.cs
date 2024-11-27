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
			RegisterCallback<PointerEnterEvent>(evt => OnCellPointerEnter());
			RegisterCallback<PointerLeaveEvent>(evt => OnCellPointerLeave());
			RegisterCallback<PointerDownEvent>(evt => OnCellPointerDown());
			RegisterCallback<PointerUpEvent>(evt => OnCellPointerUp());
		}

		private void OnCellPointerUp()
		{
			AddToClassList("ui-table__cell--highlighted");
			RemoveFromClassList("ui-table__cell--down");
		}

		private void OnCellPointerDown()
		{
			RemoveFromClassList("ui-table__cell--highlighted");
			AddToClassList("ui-table__cell--down");
		}

		public void SetWidth(float width)
		{
			style.width = width;
		}
		
		private void OnCellPointerEnter()
		{
			AddToClassList("ui-table__cell--highlighted");
		}

		private void OnCellPointerLeave()
		{
			RemoveFromClassList("ui-table__cell--highlighted");
			RemoveFromClassList("ui-table__cell--down");
		}
	}
}