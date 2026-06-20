using System;
using System.Collections.Generic;
using System.Linq;
using Nonatomic.UIElements.Events;
using Nonatomic.UIElements.TableElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	/// <summary>
	/// Represents a customizable table component for use in Unity's UIElements framework.
	/// </summary>
	public class UITable : VisualElement
	{
		private ScrollView _headerScrollView;
		private readonly List<Row> _rows = new();
		private List<ColumnDefinition> _columns;
		private VisualElement _topLeftCornerCell;
		private VisualElement _topRowContainer;
		private TableContentArea _contentArea;
		private readonly bool _flexibleRowHeights;
		private bool _includeRowNumbers;
		private readonly TableHighlighter _highlighter;
		private readonly RowHeightController _rowHeights;

		private const float DefaultColumnWidth = 100;
		private const float DefaultRowHeight = 30;
		private const int DefaultRowCount = 0;
		private const int DefaultColumnCount = 0;

		public int RowCount => _rows.Count;
		public int ColumnCount => _rows.Count == 0 ? 0 : _rows[0].CellCount;

		/// <summary>
		/// Represents a customizable table component for creating and managing grid-like layouts in Unity's UIElements framework.
		/// Allows for defining rows and columns, setting custom cell content, customizing display properties, and handling
		/// flexible row heights and row numbers for advanced table behavior.
		/// </summary>
		public UITable()
		{
			_flexibleRowHeights = false;
			_includeRowNumbers = false;
			_highlighter = new TableHighlighter(this, _rows);
			_rowHeights = new RowHeightController(_flexibleRowHeights);

			var styleSheet = Resources.Load<StyleSheet>("UITable");
			AddToClassList("ui-table");
			SetCustomStyleSheet(styleSheet);
			CreateTable(DefaultColumnCount, DefaultRowCount);
			HideRowNumbers();
			SynchronizeScrolling();
		}

		/// <summary>
		/// Represents a customizable table component for creating and managing grid-like layouts in Unity's UIElements framework.
		/// Supports functionalities such as dynamic row and column management, content customization, flexible row heights, and optional row numbering.
		/// Provides synchronization of scrolling and enables styling through a custom stylesheet.
		/// </summary
		public UITable(
			int rowCount,
			int columnCount = 0,
			List<ColumnDefinition> columns = null,
			Dictionary<int, float> rowHeights = null,
			bool flexibleRowHeights = false,
			bool includeRowNumbers = true)
		{
			_flexibleRowHeights = flexibleRowHeights;
			_includeRowNumbers = includeRowNumbers;
			_highlighter = new TableHighlighter(this, _rows);
			_rowHeights = new RowHeightController(_flexibleRowHeights);

			var styleSheet = Resources.Load<StyleSheet>("UITable");
			AddToClassList("ui-table");
			SetCustomStyleSheet(styleSheet);
			CreateTable(columnCount, rowCount, columns, rowHeights);
			SynchronizeScrolling();
		}

		/// <summary>
		/// Applies a custom style sheet to the UITable, allowing for additional customization of the visuals.
		/// </summary>
		/// <param name="styleSheet">The style sheet to be applied to the UITable.</param>
		public void SetCustomStyleSheet(StyleSheet styleSheet)
		{
			styleSheets.Add(styleSheet);
		}

		/// <summary>
		/// Retrieves the cell located at the specified column and row indices in the table.
		/// </summary>
		/// <param name="columnIndex">The zero-based index of the column containing the desired cell.</param>
		/// <param name="rowIndex">The zero-based index of the row containing the desired cell.</param>
		/// <returns>The <see cref="VisualElement"/> representing the cell at the given position.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the provided columnIndex or rowIndex is outside the valid range of the table.
		/// </exception>
		public VisualElement GetCell(int columnIndex, int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= _rows.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex), $"Row index {rowIndex} is out of range. Valid range: 0 to {_rows.Count - 1}.");
			}

			if (columnIndex < 0 || columnIndex >= _rows[rowIndex].CellCount)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex), $"Column index {columnIndex} is out of range. Valid range: 0 to {_rows[rowIndex].CellCount - 1}.");
			}

			return _rows[rowIndex].GetCell(columnIndex);
		}

		/// <summary>
		/// Retrieves a specific row of cells from the table by its index.
		/// </summary>
		/// <param name="rowIndex">The zero-based index of the row to retrieve.</param>
		/// <returns>A list of <see cref="VisualElement"/> objects representing the cells in the specified row.</returns>
		/// <exception cref="InvalidOperationException">Thrown when there are no rows in the table.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown if the provided <paramref name="rowIndex"/> is less than zero or exceeds the total row count.
		/// </exception>
		public List<VisualElement> GetRow(int rowIndex)
		{
			if (_rows.Count == 0)
			{
				throw new InvalidOperationException("There are no rows in the table.");
			}

			if (rowIndex < 0 || rowIndex >= _rows.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex));
			}

			return _rows[rowIndex].Cells.Cast<VisualElement>().ToList();
		}

		/// <summary>
		/// Retrieves all the cells within a specific column in the table.
		/// </summary>
		/// <param name="columnIndex">The index of the column to retrieve cells from. Must be a non-negative integer less than the total column count.</param>
		/// <returns>A list of <see cref="VisualElement"/> objects representing the cells in the specified column.</returns>
		/// <exception cref="InvalidOperationException">Thrown when there are no rows in the table.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the specified column index is out of range.</exception>
		public List<VisualElement> GetColumn(int columnIndex)
		{
			if (_rows.Count == 0)
			{
				throw new InvalidOperationException("There are no rows in the table.");
			}

			if (columnIndex < 0 || columnIndex >= _rows[0].CellCount)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex));
			}

			return _rows.Select(row => (VisualElement)row.GetCell(columnIndex)).ToList();
		}

		public void SetCell(int rowIndex, int columnIndex, VisualElement content)
		{
			if (rowIndex < 0 || rowIndex >= _rows.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex));
			}

			if (columnIndex < 0 || columnIndex >= _rows[rowIndex].CellCount)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex));
			}

			var row = _rows[rowIndex];
			var cell = row.GetCell(columnIndex);
			cell.Clear(); // Remove any existing content
			cell.Add(content);

			if (_flexibleRowHeights)
			{
				// Recompute this row's height when its content resizes.
				content.RegisterCallback<GeometryChangedEvent>(evt => _rowHeights.UpdateRow(row));
			}
		}

		public void SynchronizeRowHeights()
		{
			_rowHeights.UpdateAll(_rows);
		}

		private void CreateTable(int columnCount, int rowCount, List<ColumnDefinition> columnDefinitions = null, Dictionary<int, float> rowHeights = null)
		{
			columnDefinitions ??= GenerateDefaultColumns(columnCount, DefaultColumnWidth);
			_columns = columnDefinitions;

			_topRowContainer = CreateTopRow(columnDefinitions, DefaultColumnWidth, DefaultRowHeight);
			Add(_topRowContainer);

			_contentArea = CreateContentArea(rowCount, columnDefinitions, DefaultColumnWidth, DefaultRowHeight, rowHeights);
			Add(_contentArea);

			if (_includeRowNumbers) return;
			HideRowNumbers();
		}

		private List<ColumnDefinition> GenerateDefaultColumns(int columnCount, float defaultColumnWidth)
		{
			var columns = new List<ColumnDefinition>();
			columns.Add(new ColumnDefinition("#", defaultColumnWidth));

			for (var i = 1; i < columnCount + 1; i++)
			{
				columns.Add(new ColumnDefinition($"Column {i}", defaultColumnWidth));
			}

			return columns;
		}

		private VisualElement CreateTopRow(
			List<ColumnDefinition> columns,
			float defaultColumnWidth,
			float defaultRowHeight)
		{
			var topRowContainer = new VisualElement();
			topRowContainer.AddToClassList("ui-table__top-row");

			// Always create the top-left corner cell (row header)
			var rowNumberColumn = columns[0];
			var rowNumberWidth = rowNumberColumn.Width ?? defaultColumnWidth;
			_topLeftCornerCell = new CornerHeaderCell(rowNumberColumn.Label, rowNumberWidth, defaultRowHeight);
			_topLeftCornerCell.AddToClassList("ui-table__header-cell");

			// Add pointer event handlers for top-left cell
			_topLeftCornerCell.RegisterCallback<PointerEnterEvent>(evt => _highlighter.SetTableHighlight(true));
			_topLeftCornerCell.RegisterCallback<PointerLeaveEvent>(evt => _highlighter.SetTableHighlight(false));

			topRowContainer.Add(_topLeftCornerCell);

			// Header ScrollView (non-interactive)
			_headerScrollView = TableScrollView.CreateHorizontal(isInteractive: false, hideHorizontalScrollbar: true);
			_headerScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-row");
			_headerScrollView.RegisterCallback<WheelEvent>(HandleHeaderScrollWheel, TrickleDown.TrickleDown);
			topRowContainer.Add(_headerScrollView);

			// Populate header cells
			for (var i = 1; i < columns.Count; i++)
			{
				var column = columns[i];
				var columnWidth = column.Width ?? defaultColumnWidth;
				var headerCell = new ColumnHeaderCell(column.Label, columnWidth, defaultRowHeight, i);
				headerCell.AddToClassList("ui-table__header-cell");

				var index = i-1;
				headerCell.RegisterCallback<PointerEnterEvent>(evt => _highlighter.SetColumnHighlight(index, true));
				headerCell.RegisterCallback<PointerLeaveEvent>(evt => _highlighter.SetColumnHighlight(index, false));
				headerCell.RegisterCallback<ClickEvent>(evt => HandleColumnHeaderClick(headerCell));

				_headerScrollView.contentContainer.Add(headerCell);
			}

			// Add spacer element to account for scrollbar width
			var spacer = new VisualElement();
			spacer.style.flexShrink = 0;
			spacer.style.flexGrow = 0;
			topRowContainer.Add(spacer);

			// Delay setting the spacer width until layout is ready
			RegisterCallback<GeometryChangedEvent>((evt) =>
			{
				spacer.style.width = _contentArea.ContentVerticalScrollerWidth;
			});

			return topRowContainer;
		}

		private void HandleHeaderScrollWheel(WheelEvent evt)
		{
			evt.StopImmediatePropagation();
		}

		private TableContentArea CreateContentArea(
			int rowCount,
			List<ColumnDefinition> columns,
			float defaultColumnWidth,
			float defaultRowHeight,
			Dictionary<int, float> rowHeights)
		{
			_contentArea = new TableContentArea();

			for (var i = 0; i < rowCount; i++)
			{
				AddRowInternal(i, columns, defaultColumnWidth, defaultRowHeight, rowHeights);
			}

			return _contentArea;
		}

		private void SynchronizeScrolling()
		{
			// Synchronize horizontal scrolling
			_contentArea.ContentScrollView.horizontalScroller.valueChanged += (value) =>
			{
				_headerScrollView.scrollOffset = new Vector2(value, _headerScrollView.scrollOffset.y);
			};
		}

		// Method to add a new row
		public void AddRow(Dictionary<int, VisualElement> cellContents = null)
		{
			AddRowInternal(_rows.Count, _columns, DefaultColumnWidth, DefaultRowHeight, null, cellContents);
		}

		// Internal method to add a row (used during initialization and dynamic addition)
		private void AddRowInternal(int rowIndex, List<ColumnDefinition> columns, float defaultColumnWidth, float defaultRowHeight, Dictionary<int, float> rowHeights = null, Dictionary<int, VisualElement> cellContents = null)
		{
			var rowHeight = rowHeights != null && rowHeights.ContainsKey(rowIndex) ? rowHeights[rowIndex] : defaultRowHeight;

			var row = new Row(rowIndex);
			row.SetRowHeight(rowHeight, _flexibleRowHeights);

			var totalRowWidth = 0f;
			for (var j = 1; j < columns.Count; j++)
			{
				var column = columns[j];
				var columnWidth = column.Width ?? defaultColumnWidth;
				totalRowWidth += columnWidth;

				var columnIndex = j - 1;
				var cell = new TableCell(columnIndex, rowIndex);
				cell.SetWidth(columnWidth);
				cell.SetRowHeight(rowHeight, _flexibleRowHeights);
				cell.RegisterCallback<ClickEvent>(evt => HandleTableCellClick(cell));

				if (cellContents != null && cellContents.ContainsKey(columnIndex))
				{
					cell.Add(cellContents[columnIndex]);
				}

				row.AddCell(cell);
			}

			row.SetRowWidth(totalRowWidth);

			var rowNumberWidth = columns[0].Width ?? defaultColumnWidth;
			var numberCell = new RowHeaderCell($"{_rows.Count + 1}", rowNumberWidth, rowHeight, rowIndex);
			numberCell.SetRowHeight(rowHeight, _flexibleRowHeights);
			numberCell.RegisterCallback<PointerEnterEvent>(evt => _highlighter.SetRowHighlight(row, true));
			numberCell.RegisterCallback<PointerLeaveEvent>(evt => _highlighter.SetRowHighlight(row, false));
			numberCell.RegisterCallback<ClickEvent>(evt => HandleRowHeaderClick(numberCell));
			row.NumberCell = numberCell;

			_rows.Add(row);
			_contentArea.ContentScrollView.contentContainer.Add(row);
			_contentArea.RowNumberScrollView.contentContainer.Add(numberCell);
		}

		// Method to remove a row
		public void RemoveRow(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= _rows.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex));
			}

			var row = _rows[rowIndex];

			var contentContainer = _contentArea.ContentScrollView.contentContainer;
			if (contentContainer.Contains(row)) contentContainer.Remove(row);

			var rowNumberContainer = _contentArea.RowNumberScrollView.contentContainer;
			if (row.NumberCell != null && rowNumberContainer.Contains(row.NumberCell)) rowNumberContainer.Remove(row.NumberCell);

			_rows.RemoveAt(rowIndex);

			// Reindex the rows that shifted up; Row.SetIndex propagates to cells, number cell, and striping.
			for (var i = rowIndex; i < _rows.Count; i++)
			{
				_rows[i].SetIndex(i);
			}
		}

		public void SetColumn(int columnIndex, ColumnDefinition columnDefinition)
		{
			//Add 1 because of the row number column
			columnIndex += 1;

			if (columnIndex < 0 || columnIndex >= _columns.Count)
			{
				AddColumn(columnDefinition);
				return;
			}

			_columns[columnIndex] = columnDefinition;
			var columnWidth = columnDefinition.Width ?? DefaultColumnWidth;

			//remove 1 from the column index because the row number is not included in this container
			columnIndex -= 1;
			var header = _headerScrollView.contentContainer.ElementAt(columnIndex) as HeaderCell;
			header.SetLabel(columnDefinition.Label);
			header.SetWidth(columnWidth);

			// Resize the body cells in this column so they stay aligned with the header
			foreach (var row in _rows)
			{
				if (columnIndex < row.CellCount)
				{
					row.GetCell(columnIndex).SetWidth(columnWidth);
				}
			}

			UpdateRowWidths();
		}

		public void AddColumn(ColumnDefinition columnDefinition)
		{
			_columns.Add(columnDefinition);

			// Add header cell
			var columnWidth = columnDefinition.Width ?? DefaultColumnWidth;
			var columnIndex = _columns.Count - 1;

			var headerCell = new ColumnHeaderCell(columnDefinition.Label, columnWidth, DefaultRowHeight, columnIndex);
			headerCell.RegisterCallback<PointerEnterEvent>(evt => _highlighter.SetColumnHighlight(columnIndex - 1, true));
			headerCell.RegisterCallback<PointerLeaveEvent>(evt => _highlighter.SetColumnHighlight(columnIndex - 1, false));
			headerCell.RegisterCallback<ClickEvent>(evt => HandleColumnHeaderClick(headerCell));
			_headerScrollView.contentContainer.Add(headerCell);

			// columnIndex includes the row-number column, so subtract 1 for the content-based cell index
			var contentColumnIndex = columnIndex - 1;
			foreach (var row in _rows)
			{
				var cell = new TableCell(contentColumnIndex, row.Index);
				cell.RegisterCallback<ClickEvent>(evt => HandleTableCellClick(cell));
				cell.SetWidth(columnWidth);
				row.AddCell(cell);
			}

			// Widen the rows to include the new column, otherwise the new cells render
			// past the row's fixed width and get clipped by the scroll viewport.
			UpdateRowWidths();
		}

		// Method to remove a column
		public void RemoveColumn(int columnIndex)
		{
			if (columnIndex < 0 || columnIndex >= _columns.Count - 1)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex));
			}

			// Remove header cell
			_headerScrollView.contentContainer.RemoveAt(columnIndex);

			// Remove the cell at this column from every row
			foreach (var row in _rows)
			{
				row.RemoveCellAt(columnIndex);
			}

			_columns.RemoveAt(columnIndex + 1);
			UpdateRowWidths();
		}

		// Method to show row numbers column
		public void ShowRowNumbers(ColumnDefinition columnDefinition = null)
		{
			_includeRowNumbers = true;
			_contentArea.ShowRowNumbers();
			_topLeftCornerCell?.RemoveFromClassList("ui-table__top-left-cell--hidden");

			if (columnDefinition == null) return;

			_columns[0] = columnDefinition;
			var columnWidth = columnDefinition.Width ?? DefaultColumnWidth;

			var header = _topRowContainer.ElementAt(0) as HeaderCell;
			header.SetLabel(columnDefinition.Label);
			header.SetWidth(columnWidth);

			foreach (var row in _rows)
			{
				row.NumberCell?.SetWidth(columnWidth);
			}
		}

		public void HideRowNumbers()
		{
			_includeRowNumbers = false;
			_contentArea.HideRowNumbers();
			_topLeftCornerCell?.AddToClassList("ui-table__top-left-cell--hidden");
		}

		/// <summary>
		/// Controls whether the table stretches to fill its container's width or sizes to its
		/// columns. Tables size to their columns by default; pass true to fill the available width.
		/// </summary>
		public void SetFillWidth(bool fillWidth)
		{
			EnableInClassList("ui-table--fill-width", fillWidth);
		}

		private void UpdateRowWidths()
		{
			if (_columns.Count == 0) return;

			var totalRowWidth = 0f;
			for (var j = 1; j < _columns.Count; j++)
			{
				var column = _columns[j];
				var columnWidth = column.Width ?? DefaultColumnWidth;
				totalRowWidth += columnWidth;
			}

			foreach (var row in _rows)
			{
				row.SetRowWidth(totalRowWidth);
			}
		}

		private void HandleColumnHeaderClick(ColumnHeaderCell headerCell)
		{
			var evt = ColumnHeaderClickEvent.GetPooled(headerCell.ColumnIndex);
			evt.target = this;
			SendEvent(evt);
		}

		private void HandleRowHeaderClick(RowHeaderCell rowNumberCell)
		{
			var evt = RowHeaderClickEvent.GetPooled(rowNumberCell.RowIndex);
			evt.target = this;
			SendEvent(evt);
		}

		private void HandleTableCellClick(TableCell cell)
		{
			var evt = TableCellClickEvent.GetPooled(cell.ColumnIndex, cell.RowIndex);
			evt.target = this;
			SendEvent(evt);
		}
	}
}
