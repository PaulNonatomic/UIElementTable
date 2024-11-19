namespace Nonatomic.UIElements
{
	public class ColumnDefinition
	{
		public string Label { get; set; }
		public float? Width { get; set; } // Width is now nullable

		public ColumnDefinition(string label, float? width = null)
		{
			Label = label;
			Width = width;
		}
	}
}