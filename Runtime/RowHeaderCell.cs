namespace Nonatomic.UIElements
{
	public class RowHeaderCell : HeaderCell
	{
		public int RowIndex { get; private set; }
		
		public RowHeaderCell(string text, float width, float height, int rowIndex) : base(text, width, height)
		{
			name = $"RowHeader_{rowIndex+1}";
			RowIndex = rowIndex;
		}
	}
}