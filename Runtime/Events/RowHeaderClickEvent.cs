using UnityEngine.UIElements;

namespace Nonatomic.UIElements.Events
{
	public class RowHeaderClickEvent : EventBase<RowHeaderClickEvent>
	{
		public int RowIndex { get; private set; }
		
		public static RowHeaderClickEvent GetPooled(int rowIndex)
		{
			var e = EventBase<RowHeaderClickEvent>.GetPooled();
			e.RowIndex = rowIndex;
			return e;
		}
	}
}