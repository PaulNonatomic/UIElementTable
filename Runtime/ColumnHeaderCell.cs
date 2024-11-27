namespace Nonatomic.UIElements
{
	public class ColumnHeaderCell : HeaderCell
	{
		public int ColumnIndex { get; private set; }
		
		public ColumnHeaderCell(string text, float width, float height, int columnIndex) : base(text, width, height)
		{
			name = $"ColumnHeader_{columnIndex}";
			ColumnIndex = columnIndex;
		}
	}
}