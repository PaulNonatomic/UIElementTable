using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements.TableElements
{
	/// <summary>
	/// Applies the table's hover-highlight USS classes: a whole column, a single row, or the entire
	/// table. Holds a live reference to the table's rows so it always highlights the current cells.
	/// Kept separate from UITable so highlighting is one isolated responsibility.
	/// </summary>
	internal sealed class TableHighlighter
	{
		private readonly VisualElement _table;
		private readonly IReadOnlyList<Row> _rows;

		public TableHighlighter(VisualElement table, IReadOnlyList<Row> rows)
		{
			_table = table;
			_rows = rows;
		}

		public void SetColumnHighlight(int columnIndex, bool highlighted)
		{
			if (_rows.Count == 0) return;
			if (columnIndex < 0 || columnIndex >= _rows[0].CellCount) return;

			foreach (var row in _rows)
			{
				row.GetCell(columnIndex).EnableInClassList("ui-table__column--highlighted", highlighted);
			}
		}

		public void SetRowHighlight(Row row, bool highlighted)
		{
			row.EnableInClassList("ui-table__row--highlighted", highlighted);
		}

		public void SetTableHighlight(bool highlighted)
		{
			_table.EnableInClassList("ui-table--highlighted", highlighted);
		}
	}
}
