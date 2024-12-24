using UnityEngine.UIElements;

namespace Nonatomic.UIElements.Events
{
	public class ColumnHeaderClickEvent : EventBase<ColumnHeaderClickEvent>
	{
		public int ColumnIndex { get; private set; }
		
		public static ColumnHeaderClickEvent GetPooled(int columnIndex)
		{
			var e = EventBase<ColumnHeaderClickEvent>.GetPooled();
			e.ColumnIndex = columnIndex;
			return e;
		}
	}
}