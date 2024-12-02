using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class Row : VisualElement, IFlexibleRowHeight
	{
		public Row(int rowIndex)
		{
			AddToClassList("ui-table__row");
			style.flexDirection = FlexDirection.Row;
			style.flexShrink = 0;
			
			// Assign even or odd class
			AddToClassList((rowIndex + 1) % 2 == 0 ? "ui-table__row--even" : "ui-table__row--odd");
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
	}
}