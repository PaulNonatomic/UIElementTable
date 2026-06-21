using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.UIElements.TableElements
{
	/// <summary>
	/// Sizes rows to fit their tallest cell when the table uses flexible row heights. A no-op when
	/// flexible heights are disabled, so callers can invoke it unconditionally. Kept separate from
	/// UITable so row-height measurement is one isolated responsibility.
	/// </summary>
	internal sealed class RowHeightController
	{
		private readonly bool _enabled;

		public RowHeightController(bool enabled)
		{
			_enabled = enabled;
		}

		public void UpdateRow(Row row)
		{
			if (!_enabled) return;

			// The row is as tall as its tallest cell. A hidden row-number cell measures zero, so it
			// can be considered unconditionally.
			var maxHeight = 0f;
			foreach (var cell in row.Cells)
			{
				maxHeight = Mathf.Max(maxHeight, cell.resolvedStyle.height);
			}

			if (row.NumberCell != null)
			{
				maxHeight = Mathf.Max(maxHeight, row.NumberCell.resolvedStyle.height);
			}

			row.style.height = maxHeight;
			row.MarkDirtyRepaint();
			row.NumberCell?.SetRowHeight(maxHeight, _enabled);
		}

		public void UpdateAll(IReadOnlyList<Row> rows)
		{
			if (!_enabled) return;

			foreach (var row in rows)
			{
				UpdateRow(row);
			}
		}
	}
}
