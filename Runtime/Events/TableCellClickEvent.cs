using UnityEngine.UIElements;

namespace Nonatomic.UIElements.Events
{
	public class TableCellClickEvent : EventBase<TableCellClickEvent>
	{
		public int RowIndex { get; private set; }
		public int ColumnIndex { get; private set; }
		
		public static TableCellClickEvent GetPooled(int columnIndex, int rowIndex)
		{
			var e = EventBase<TableCellClickEvent>.GetPooled();
			e.RowIndex = rowIndex;
			e.ColumnIndex = columnIndex;
			return e;
		}
	}
}