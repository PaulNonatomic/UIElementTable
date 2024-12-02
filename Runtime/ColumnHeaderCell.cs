using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class ColumnHeaderCell : HeaderCell
	{
		public int ColumnIndex { get; private set; }
		
		public ColumnHeaderCell(string text, float width, float height, int columnIndex) : base(text, width, height)
		{
			name = $"ColumnHeader_{columnIndex}";
			ColumnIndex = columnIndex;
			
			RegisterCallback<PointerEnterEvent>(evt => HandleCellPointerEnter());
			RegisterCallback<PointerLeaveEvent>(evt => HandleCellPointerLeave());
			RegisterCallback<PointerDownEvent>(evt => HandleCellPointerDown());
			RegisterCallback<PointerUpEvent>(evt => HandleCellPointerUp());
		}
		
		public void Highlight(bool enabled)
		{
			if (enabled)
			{
				AddToClassList("ui-table__column--highlighted");
			}
			else
			{
				RemoveFromClassList("ui-table__column--highlighted");
			}
		}

		private void HandleCellPointerUp()
		{
			Highlight(enabled: true);
		}

		private void HandleCellPointerDown()
		{
			Highlight(enabled: false);
			AddToClassList("ui-table__column--down");
		}

		private void HandleCellPointerLeave()
		{
			Highlight(enabled: false);
			RemoveFromClassList("ui-table__column--down");
		}

		private void HandleCellPointerEnter()
		{
			Highlight(enabled: true);
		}
	}
}