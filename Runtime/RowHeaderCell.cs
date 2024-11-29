using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class RowHeaderCell : HeaderCell
	{
		public int RowIndex { get; private set; }
		
		public RowHeaderCell(string text, float width, float height, int rowIndex) : base(text, width, height)
		{
			name = $"RowHeader_{rowIndex+1}";
			RowIndex = rowIndex;
			
			RegisterCallback<PointerEnterEvent>(evt => HandleRowHeaderPointerEnter(rowIndex));
			RegisterCallback<PointerLeaveEvent>(evt => HandleRowHeaderPointerLeave(rowIndex));
			AddToClassList((rowIndex + 1) % 2 == 0 ? "ui-table__fixed-column--even" : "ui-table__fixed-column--odd");
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

		private void HandleRowHeaderPointerLeave(int rowIndex)
		{
			//...
		}

		private void HandleRowHeaderPointerEnter(int rowIndex)
		{
			//...
		}
	}
}