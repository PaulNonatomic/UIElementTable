using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements.Binding
{
	public class DataBindingUITable<T> : UITable
	{
		private readonly List<ColumnBinder<T>> _columnBinders = new List<ColumnBinder<T>>();
		private IEnumerable<T> _data;

		public void AddColumn(
			ColumnDefinition columnDefinition,
			Func<T, VisualElement> cellCreator)
		{
			if (columnDefinition == null)
			{
				throw new ArgumentNullException(nameof(columnDefinition));
			}
			if (cellCreator == null)
			{
				throw new ArgumentNullException(nameof(cellCreator));
			}

			// Store the binder so we can re-build rows when data changes
			var binder = new ColumnBinder<T>
			{
				ColumnDefinition = columnDefinition,
				CreateCell = cellCreator
			};
			_columnBinders.Add(binder);
			
			base.AddColumn(columnDefinition);
		}

		public void SetData(IEnumerable<T> data)
		{
			_data = data;
			Refresh();
		}

		public void Refresh()
		{
			ClearAllRows();

			if (_data == null)
			{
				return;
			}

			foreach (var item in _data)
			{
				// Build cell contents for this row
				var cellContents = new Dictionary<int, VisualElement>();
				for (var i = 0; i < _columnBinders.Count; i++)
				{
					var binder = _columnBinders[i];
					var cell = binder.CreateCell(item);
					cellContents[i] = cell;
				}

				// Add row to the underlying UITable
				AddRow(cellContents);
			}
		}

		/// <summary>
		/// Clears all rows from the underlying UITable. 
		/// </summary>
		private void ClearAllRows()
		{
			// A naive approach: remove rows from the last to the first
			// so indexing doesn't get messed up.
			for (var rowIndex = RowCount - 1; rowIndex >= 0; rowIndex--)
			{
				RemoveRow(rowIndex);
			}
		}
	}
}
