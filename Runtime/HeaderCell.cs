using UnityEngine.UIElements;

namespace Nonatomic.UIElements
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
		}

		public void SetLabel(string text)
		{
			_label.text = text;
		}
		
		public void SetWidth(float width)
		{
			style.width = width;
		}
	}
}