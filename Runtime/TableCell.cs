using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public static class TableCell
	{
		public static VisualElement Create(string text)
		{
			var cell = new VisualElement();
			cell.AddToClassList("ui-table__cell");

			var label = new Label(text);
			label.AddToClassList("ui-table__cell-label");

			cell.Add(label);
			return cell;
		}
	}
}