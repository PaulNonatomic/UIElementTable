using UnityEngine.UIElements;

namespace Nonatomic.UIElements.TableElements
{
	public class HeaderCell : VisualElement
	{
		private readonly Label _label;

		public HeaderCell(string text, float width, float height)
		{
			AddToClassList("ui-table__cell");
			AddToClassList("ui-table__header-cell");
			
			style.width = width;
			style.height = height;

			_label = new Label(text);
			_label.AddToClassList("ui-table__cell-label");
			Add(_label);
			
			RegisterCallback<PointerEnterEvent>(evt => OnCellPointerEnter());
			RegisterCallback<PointerLeaveEvent>(evt => OnCellPointerLeave());
			RegisterCallback<PointerDownEvent>(evt => OnCellPointerDown());
			RegisterCallback<PointerUpEvent>(evt => OnCellPointerUp());
		}

		public void SetLabel(string text)
		{
			_label.text = text;
		}
		
		public void SetWidth(float width)
		{
			style.width = width;
		}
		
		protected void OnCellPointerUp()
		{
			//AddToClassList("ui-table__cell--highlighted");
			//RemoveFromClassList("ui-table__cell--down");
		}

		protected void OnCellPointerDown()
		{
			//RemoveFromClassList("ui-table__cell--highlighted");
			//AddToClassList("ui-table__cell--down");
		}
		
		protected void OnCellPointerEnter()
		{
			//AddToClassList("ui-table__cell--highlighted");
		}

		protected void OnCellPointerLeave()
		{
			//RemoveFromClassList("ui-table__cell--highlighted");
			//RemoveFromClassList("ui-table__cell--down");
		}
	}
}