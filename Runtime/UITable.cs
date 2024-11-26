using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class UITable : VisualElement
	{
		private ScrollView _headerScrollView;
		private ScrollView _rowNumberColumnScrollView;
		private ScrollView _contentScrollView;
		private List<List<VisualElement>> _contentCells;
		private List<HeaderCell> _rowNumberCells;
		private List<VisualElement> _contentRows;
		private List<ColumnDefinition> _columns;
		private VisualElement _topLeftCornerCell;
		private VisualElement _rowNumberContainer;
		private VisualElement _topRowContainer;
		private VisualElement _contentRowContainer;
		private readonly bool _flexibleRowHeights;
		private bool _includeRowNumbers;
		
		private const float DefaultColumnWidth = 100;
		private const float DefaultRowHeight = 30;
		private const int DefaultRowCount = 0;
		private const int DefaultColumnCount = 2;

		public UITable()
		{
			_flexibleRowHeights = false;
			_includeRowNumbers = false;
			_contentCells = new List<List<VisualElement>>();
			_rowNumberCells = new List<HeaderCell>();
			_contentRows = new List<VisualElement>();

			var styleSheet = Resources.Load<StyleSheet>("UITable");
			AddToClassList("ui-table");
			SetCustomStyleSheet(styleSheet);
			CreateTable(DefaultColumnCount, DefaultRowCount);
			SynchronizeScrolling();
		}

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
			_contentCells = new List<List<VisualElement>>();
			_rowNumberCells = new List<HeaderCell>();
			_contentRows = new List<VisualElement>();

			var styleSheet = Resources.Load<StyleSheet>("UITable");

			AddToClassList("ui-table");
			SetCustomStyleSheet(styleSheet);
			CreateTable(columnCount, rowCount, columns, rowHeights);
			SynchronizeScrolling();
		}

		private void CreateTable(int columnCount, int rowCount, List<ColumnDefinition> columnDefinitions = null, Dictionary<int, float> rowHeights = null)
		{
			if (columnCount < 1)
			{
				throw new System.ArgumentException("Column count must be greater than 0.");
			}

			columnDefinitions ??= GenerateDefaultColumns(columnCount, DefaultColumnWidth);
			_columns = columnDefinitions;

			_topRowContainer = CreateTopRow(columnDefinitions, DefaultColumnWidth, DefaultRowHeight);
			Add(_topRowContainer);

			_contentRowContainer = CreateContentArea(rowCount, columnDefinitions, DefaultColumnWidth, DefaultRowHeight, rowHeights);
			Add(_contentRowContainer);

			if (_includeRowNumbers) return;
			
			// Hide the row numbers column
			_rowNumberContainer?.AddToClassList("ui-table__row-numbers-column--hidden");

			// Hide the top-left corner cell
			_topLeftCornerCell?.AddToClassList("ui-table__top-left-cell--hidden");
		}

		public void SetCustomStyleSheet(StyleSheet styleSheet)
		{
			styleSheets.Add(styleSheet);
		}

		public void SetCellContent(int rowIndex, int columnIndex, VisualElement content)
		{
			if (rowIndex < 0 || rowIndex >= _contentCells.Count)
			{
				throw new System.ArgumentOutOfRangeException(nameof(rowIndex));
			}

			if (columnIndex < 0 || columnIndex >= _contentCells[rowIndex].Count)
			{
				throw new System.ArgumentOutOfRangeException(nameof(columnIndex));
			}

			var cell = _contentCells[rowIndex][columnIndex];
			cell.Clear(); // Remove any existing content
			cell.Add(content);

			if (_flexibleRowHeights)
			{
				// Listen for geometry changes in the content
				content.RegisterCallback<GeometryChangedEvent>((evt) =>
				{
					UpdateRowHeight(rowIndex);
				});
			}
		}

		public void SynchronizeRowHeights()
		{
			if (!_flexibleRowHeights) return;

			for (var i = 0; i < _contentRows.Count; i++)
			{
				UpdateRowHeight(i);
			}
		}

		private void UpdateRowHeight(int rowIndex)
		{
			if (!_flexibleRowHeights)
				return;

			var contentRow = _contentRows[rowIndex];
			VisualElement rowNumberCell = null;

			if (_includeRowNumbers)
			{
				rowNumberCell = _rowNumberCells[rowIndex];
			}

			// Calculate the maximum height among all cells in the row
			var maxHeight = 0f;

			// Check the height of each cell in the row
			foreach (var cell in _contentCells[rowIndex])
			{
				maxHeight = Mathf.Max(maxHeight, cell.resolvedStyle.height);
			}

			// Also consider the row number column cell if it exists
			if (rowNumberCell != null)
			{
				maxHeight = Mathf.Max(maxHeight, rowNumberCell.resolvedStyle.height);
			}

			// Apply the maximum height to the content row
			contentRow.style.height = maxHeight;
			contentRow.MarkDirtyRepaint();

			// Also apply to the row number column cell if it exists
			if (rowNumberCell != null)
			{
				rowNumberCell.style.height = maxHeight;
				rowNumberCell.MarkDirtyRepaint();
			}
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
			_topLeftCornerCell = new HeaderCell(rowNumberColumn.Label, rowNumberWidth, defaultRowHeight);
			_topLeftCornerCell.AddToClassList("ui-table__header-cell");

			// Add pointer event handlers for top-left cell
			_topLeftCornerCell.RegisterCallback<PointerEnterEvent>(evt => OnTopLeftCellPointerEnter());
			_topLeftCornerCell.RegisterCallback<PointerLeaveEvent>(evt => OnTopLeftCellPointerLeave());

			topRowContainer.Add(_topLeftCornerCell);

			// Header ScrollView (non-interactive)
			_headerScrollView = TableScrollView.CreateHorizontal(isInteractive: false, hideHorizontalScrollbar: true);
			_headerScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-row");
			topRowContainer.Add(_headerScrollView);

			// Populate header cells
			for (var i = 1; i < columns.Count; i++)
			{
				var column = columns[i];
				var columnWidth = column.Width ?? defaultColumnWidth;
				var headerCell = new HeaderCell(column.Label, columnWidth, defaultRowHeight);
				headerCell.AddToClassList("ui-table__header-cell");

				var columnIndex = i - 1; // Adjust index

				headerCell.RegisterCallback<PointerEnterEvent>(evt => OnHeaderCellPointerEnter(columnIndex));
				headerCell.RegisterCallback<PointerLeaveEvent>(evt => OnHeaderCellPointerLeave(columnIndex));

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
				var scrollbarWidth = _contentScrollView?.verticalScroller.resolvedStyle.width ?? 0;
				spacer.style.width = scrollbarWidth > 0 ? scrollbarWidth : 0f;
			});

			return topRowContainer;
		}

		private VisualElement CreateContentArea(
			int rowCount,
			List<ColumnDefinition> columns,
			float defaultColumnWidth,
			float defaultRowHeight,
			Dictionary<int, float> rowHeights)
		{
			var contentRowContainer = new VisualElement();
			contentRowContainer.AddToClassList("ui-table__content-row");

			// Always create the row number Column ScrollView (non-interactive)
			_rowNumberContainer = new VisualElement();
			_rowNumberContainer.AddToClassList("ui-table__row-numbers-column");
			contentRowContainer.Add(_rowNumberContainer);

			_rowNumberColumnScrollView = TableScrollView.CreateVertical(isInteractive: false, hideVerticalScrollbar: true);
			_rowNumberColumnScrollView.AddToClassList("ui-table__scrollview-content-column");
			_rowNumberContainer.Add(_rowNumberColumnScrollView);

			// Add spacer element to account for scrollbar height
			var spacer = new VisualElement();
			spacer.style.flexShrink = 0;
			spacer.style.flexGrow = 0;
			_rowNumberContainer.Add(spacer);

			// Delay setting the spacer height until layout is ready
			RegisterCallback<GeometryChangedEvent>((evt) =>
			{
				var scrollbarHeight = _contentScrollView?.horizontalScroller.resolvedStyle.height ?? 0;
				spacer.style.height = scrollbarHeight > 0 ? scrollbarHeight : 0f;
			});

			// Main Content ScrollView (interactive with visible scrollbars)
			_contentScrollView = TableScrollView.CreateBoth(isInteractive: true);
			_contentScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-column");
			contentRowContainer.Add(_contentScrollView);

			// Populate content cells
			for (var i = 0; i < rowCount; i++)
			{
				AddRowInternal(i, columns, defaultColumnWidth, defaultRowHeight, rowHeights);
			}

			return contentRowContainer;
		}

		private void SynchronizeScrolling()
		{
			// Synchronize horizontal scrolling
			_contentScrollView.horizontalScroller.valueChanged += (value) =>
			{
				_headerScrollView.scrollOffset = new Vector2(value, _headerScrollView.scrollOffset.y);
			};

			// Synchronize vertical scrolling
			_contentScrollView.verticalScroller.valueChanged += (value) =>
			{
				_rowNumberColumnScrollView.scrollOffset = new Vector2(_rowNumberColumnScrollView.scrollOffset.x, value);
			};
		}

		private void OnCellPointerEnter(VisualElement cell)
		{
			cell.AddToClassList("ui-table__cell--highlighted");
		}

		private void OnCellPointerLeave(VisualElement cell)
		{
			cell.RemoveFromClassList("ui-table__cell--highlighted");
		}

		private void OnRowHeaderPointerEnter(int rowIndex)
		{
			_contentRows[rowIndex].AddToClassList("ui-table__row--highlighted");

			_rowNumberCells[rowIndex].AddToClassList("ui-table__row--highlighted");
		}

		private void OnRowHeaderPointerLeave(int rowIndex)
		{
			_contentRows[rowIndex].RemoveFromClassList("ui-table__row--highlighted");

			_rowNumberCells[rowIndex].RemoveFromClassList("ui-table__row--highlighted");
		}

		private void OnHeaderCellPointerEnter(int columnIndex)
		{
			// Highlight all cells in the column
			foreach (var rowCells in _contentCells)
			{
				var cell = rowCells[columnIndex];
				cell.AddToClassList("ui-table__column--highlighted");
			}

			// Also highlight the header cell
			var headerCell = _headerScrollView.contentContainer[columnIndex];
			headerCell.AddToClassList("ui-table__column--highlighted");
		}

		private void OnHeaderCellPointerLeave(int columnIndex)
		{
			// Remove highlight from all cells in the column
			foreach (var rowCells in _contentCells)
			{
				var cell = rowCells[columnIndex];
				cell.RemoveFromClassList("ui-table__column--highlighted");
			}

			// Also remove highlight from the header cell
			var headerCell = _headerScrollView.contentContainer[columnIndex];
			headerCell.RemoveFromClassList("ui-table__column--highlighted");
		}

		private void OnTopLeftCellPointerEnter()
		{
			this.AddToClassList("ui-table--highlighted");
		}

		private void OnTopLeftCellPointerLeave()
		{
			this.RemoveFromClassList("ui-table--highlighted");
		}

		// Method to add a new row
		public void AddRow(Dictionary<int, VisualElement> cellContents = null)
		{
			var rowIndex = _contentRows.Count;
			AddRowInternal(rowIndex, _columns, DefaultColumnWidth, DefaultRowHeight, null, cellContents);
		}

		// Internal method to add a row (used during initialization and dynamic addition)
		private void AddRowInternal(int rowIndex, List<ColumnDefinition> columns, float defaultColumnWidth, float defaultRowHeight, Dictionary<int, float> rowHeights = null, Dictionary<int, VisualElement> cellContents = null)
		{
			var rowHeight = rowHeights != null && rowHeights.ContainsKey(rowIndex) ? rowHeights[rowIndex] : defaultRowHeight;

			var row = new VisualElement();
			row.AddToClassList("ui-table__row");
			row.style.flexDirection = FlexDirection.Row;
			row.style.flexShrink = 0;

			// Set row height based on flexibleRowHeights
			if (_flexibleRowHeights)
			{
				row.style.minHeight = rowHeight;
				row.style.height = StyleKeyword.Null;
			}
			else
			{
				row.style.height = rowHeight;
				row.style.minHeight = StyleKeyword.Null;
			}

			// Assign even or odd class
			row.AddToClassList((rowIndex + 1) % 2 == 0 ? "ui-table__row--even" : "ui-table__row--odd");

			// Calculate total row width based on column widths
			var totalRowWidth = 0f;

			var contentRowCells = new List<VisualElement>();
			for (var j = 1; j < columns.Count; j++)
			{
				var column = columns[j];
				var columnWidth = column.Width ?? defaultColumnWidth;
				totalRowWidth += columnWidth;

				var cell = new VisualElement();
				cell.AddToClassList("ui-table__cell");
				cell.style.width = columnWidth;

				// Set cell height based on flexibleRowHeights
				if (_flexibleRowHeights)
				{
					cell.style.minHeight = rowHeight;
					cell.style.height = StyleKeyword.Null;
				}
				else
				{
					cell.style.height = rowHeight;
					cell.style.minHeight = StyleKeyword.Null;
					cell.style.overflow = Overflow.Hidden;
				}

				// Adjust columnIndex for event handlers
				var columnIndex = j - 1;

				// Add pointer event handlers for content cells
				cell.RegisterCallback<PointerEnterEvent>(evt => OnCellPointerEnter(cell));
				cell.RegisterCallback<PointerLeaveEvent>(evt => OnCellPointerLeave(cell));

				// Add cell content if provided
				if (cellContents != null && cellContents.ContainsKey(columnIndex))
				{
					cell.Add(cellContents[columnIndex]);
				}

				// Add cell to row
				row.Add(cell);

				// Store cell in contentRowCells
				contentRowCells.Add(cell);
			}

			row.style.width = totalRowWidth;

			// Add row to _contentRows and _contentCells
			_contentRows.Add(row);
			_contentCells.Add(contentRowCells);

			// Add row to content scroll view
			_contentScrollView.contentContainer.Add(row);

			// Update row numbers
			var rowNumberWidth = columns[0].Width ?? defaultColumnWidth;
			var rowNumberCell = new HeaderCell($"{_rowNumberCells.Count + 1}", rowNumberWidth, rowHeight);

			rowNumberCell.RegisterCallback<PointerEnterEvent>(evt => OnRowHeaderPointerEnter(rowIndex));
			rowNumberCell.RegisterCallback<PointerLeaveEvent>(evt => OnRowHeaderPointerLeave(rowIndex));

			rowNumberCell.AddToClassList((rowIndex + 1) % 2 == 0 ? "ui-table__fixed-column--even" : "ui-table__fixed-column--odd");

			// Set height based on flexibleRowHeights
			if (_flexibleRowHeights)
			{
				rowNumberCell.style.minHeight = rowHeight;
				rowNumberCell.style.height = StyleKeyword.Null;
			}
			else
			{
				rowNumberCell.style.height = rowHeight;
				rowNumberCell.style.minHeight = StyleKeyword.Null;
				rowNumberCell.style.overflow = Overflow.Hidden;
			}

			_rowNumberCells.Add(rowNumberCell);
			_rowNumberColumnScrollView.contentContainer.Add(rowNumberCell);
		}

		// Method to remove a row
		public void RemoveRow(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= _contentRows.Count)
			{
				throw new System.ArgumentOutOfRangeException(nameof(rowIndex));
			}

			// Remove row from UI
			_contentScrollView.contentContainer.Remove(_contentRows[rowIndex]);
			_contentRows.RemoveAt(rowIndex);
			_contentCells.RemoveAt(rowIndex);

			// Remove row number cell
			_rowNumberColumnScrollView.contentContainer.Remove(_rowNumberCells[rowIndex]);
			_rowNumberCells.RemoveAt(rowIndex);

			// Update row indices and classes
			for (var i = rowIndex; i < _contentRows.Count; i++)
			{
				var currentRow = _contentRows[i];
				currentRow.RemoveFromClassList("ui-table__row--even");
				currentRow.RemoveFromClassList("ui-table__row--odd");
				currentRow.AddToClassList((i + 1) % 2 == 0 ? "ui-table__row--even" : "ui-table__row--odd");

				var rowNumberCell = _rowNumberCells[i];
				// rowNumberCell.text = $"{i + 1}";
				rowNumberCell.RemoveFromClassList("ui-table__fixed-column--even");
				rowNumberCell.RemoveFromClassList("ui-table__fixed-column--odd");
				rowNumberCell.AddToClassList((i + 1) % 2 == 0 ? "ui-table__fixed-column--even" : "ui-table__fixed-column--odd");
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
			var columnWidth = columnDefinition.Width ?? 100f;
			
			//remove 1 from the column index because the row number is not included in this container
			columnIndex -= 1;
			var header = _headerScrollView.contentContainer.ElementAt(columnIndex) as HeaderCell;
			header.SetLabel(columnDefinition.Label);
			header.SetWidth(columnWidth);
		}

		public void AddColumn(ColumnDefinition columnDefinition)
		{
			_columns.Add(columnDefinition);

			// Add header cell
			var columnWidth = columnDefinition.Width ?? 100f;
			var columnIndex = _columns.Count - 2;
			
			var headerCell = new HeaderCell(columnDefinition.Label, columnWidth, 30f);
			headerCell.RegisterCallback<PointerEnterEvent>(evt => OnHeaderCellPointerEnter(columnIndex));
			headerCell.RegisterCallback<PointerLeaveEvent>(evt => OnHeaderCellPointerLeave(columnIndex));
			_headerScrollView.contentContainer.Add(headerCell);

			// Add cells to each row
			for (var i = 0; i < _contentRows.Count; i++)
			{
				var row = _contentRows[i];
				var cell = new VisualElement();
				cell.AddToClassList("ui-table__cell");
				cell.style.width = columnWidth;

				// Adjust columnIndex for event handlers
				cell.RegisterCallback<PointerEnterEvent>(evt => OnCellPointerEnter(cell));
				cell.RegisterCallback<PointerLeaveEvent>(evt => OnCellPointerLeave(cell));

				// Add cell to row
				row.Add(cell);

				// Add cell to contentCells
				_contentCells[i].Add(cell);
			}
		}

		// Method to remove a column
		public void RemoveColumn(int columnIndex)
		{
			if (columnIndex < 0 || columnIndex >= _columns.Count - 1)
			{
				throw new System.ArgumentOutOfRangeException(nameof(columnIndex));
			}

			// Remove header cell
			_headerScrollView.contentContainer.RemoveAt(columnIndex);

			// Remove cells from each row
			for (var i = 0; i < _contentRows.Count; i++)
			{
				var row = _contentRows[i];
				var cell = _contentCells[i][columnIndex];
				row.Remove(cell);
				_contentCells[i].RemoveAt(columnIndex);
			}

			_columns.RemoveAt(columnIndex + 1);
			UpdateRowWidths();
		}

		// Method to show row numbers column
		public void ShowRowNumbers(ColumnDefinition columnDefinition = null)
		{
			_includeRowNumbers = true;
			_rowNumberContainer?.RemoveFromClassList("ui-table__row-numbers-column--hidden");
			_topLeftCornerCell?.RemoveFromClassList("ui-table__top-left-cell--hidden");

			if (columnDefinition != null)
			{
				_columns[0] = columnDefinition;
				var columnWidth = columnDefinition.Width ?? 100f;

				var header = _topRowContainer.ElementAt(0) as HeaderCell;
				header.SetLabel(columnDefinition.Label);
				header.SetWidth(columnWidth);

				foreach (var cell in _rowNumberCells)
				{
					cell.SetWidth(columnWidth);
				}
			}
		}
		
		public void HideRowNumbers()
		{
			_includeRowNumbers = false;
			_rowNumberContainer?.AddToClassList("ui-table__row-numbers-column--hidden");
			_topLeftCornerCell?.AddToClassList("ui-table__top-left-cell--hidden");
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

			foreach (var row in _contentRows)
			{
				row.style.width = totalRowWidth;
			}
		}
	}
}
