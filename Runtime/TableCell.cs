using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class TableCell : VisualElement
	{
		public TableCell()
		{
			AddToClassList("ui-table__cell");
			RegisterCallback<PointerEnterEvent>(evt => OnCellPointerEnter());
			RegisterCallback<PointerLeaveEvent>(evt => OnCellPointerLeave());
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
		}
	}
}