using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class TableCell : VisualElement, IFlexibleRowHeight, IHighlight
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

		public void Highlight(bool enabled)
		{
			if (enabled)
			{
				AddToClassList("ui-table__cell--highlighted");
			}
			else
			{
				RemoveFromClassList("ui-table__cell--highlighted");
			}
		}

		public void SetWidth(float width)
		{
			style.width = width;
		}

		private void HandleCellPointerUp()
		{
			Highlight(enabled: true);
			RemoveFromClassList("ui-table__cell--down");
		}

		private void HandleCellPointerDown()
		{
			Highlight(enabled: false);
			AddToClassList("ui-table__cell--down");
		}

		private void HandleCellPointerEnter()
		{
			Highlight(enabled: true);
		}

		private void HandleCellPointerLeave()
		{
			Highlight(enabled: false);
			RemoveFromClassList("ui-table__cell--down");
		}
	}
}