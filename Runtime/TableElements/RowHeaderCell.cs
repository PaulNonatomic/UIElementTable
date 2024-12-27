using UnityEngine.UIElements;

namespace Nonatomic.UIElements.TableElements
{
	public class RowHeaderCell : HeaderCell, IFlexibleRowHeight, IHighlight
	{
		public int RowIndex { get; private set; }

		public RowHeaderCell(string text, float width, float height, int rowIndex) : base(text, width, height)
		{
			name = $"RowHeader_{rowIndex+1}";
			RowIndex = rowIndex;
			
			AddToClassList((rowIndex + 1) % 2 == 0 ? "ui-table__fixed-column--even" : "ui-table__fixed-column--odd");
			
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
				AddToClassList("ui-table__row--highlighted");
			}
			else
			{
				RemoveFromClassList("ui-table__row--highlighted");
			}
		}

		private void HandleCellPointerUp()
		{
			Highlight(enabled: true);
			RemoveFromClassList("ui-table__row--down");
		}

		private void HandleCellPointerDown()
		{
			Highlight(enabled: false);
			AddToClassList("ui-table__row--down");
		}

		private void HandleCellPointerLeave()
		{
			Highlight(enabled: false);
			RemoveFromClassList("ui-table__row--down");
		}

		private void HandleCellPointerEnter()
		{
			Highlight(enabled: true);
		}
	}
}