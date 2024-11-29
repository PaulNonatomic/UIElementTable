using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class UITable : VisualElement
	{
		private ScrollView _headerScrollView;
		private List<List<VisualElement>> _contentCells;
		private List<RowHeaderCell> _rowNumberCells;
		private List<VisualElement> _contentRows;
		private List<ColumnDefinition> _columns;
		private VisualElement _topLeftCornerCell;
		private VisualElement _topRowContainer;
		private TableContentArea _contentArea;
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
			_rowNumberCells = new List<RowHeaderCell>();
			_contentRows = new List<VisualElement>();

			var styleSheet = Resources.Load<StyleSheet>("UITable");
			AddToClassList("ui-table");
			SetCustomStyleSheet(styleSheet);
			CreateTable(DefaultColumnCount, DefaultRowCount);
			HideRowNumbers();
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
			_rowNumberCells = new List<RowHeaderCell>();
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

			_contentArea = CreateContentArea(rowCount, columnDefinitions, DefaultColumnWidth, DefaultRowHeight, rowHeights);
			Add(_contentArea);

			if (_includeRowNumbers) return;
			HideRowNumbers();
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
			if (!_flexibleRowHeights) return;

			var contentRow = _contentRows[rowIndex];
			RowHeaderCell rowNumberCell = null;

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
			rowNumberCell?.SetRowHeight(maxHeight, _flexibleRowHeights);
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
			_topLeftCornerCell.RegisterCallback<PointerEnterEvent>(evt => OnTopLeftCellPointerEnter());
			_topLeftCornerCell.RegisterCallback<PointerLeaveEvent>(evt => OnTopLeftCellPointerLeave());

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

				var columnIndex = i - 1;
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
			var tableContent = new TableContentArea();

			for (var i = 0; i < rowCount; i++)
			{
				AddRowInternal(i, columns, defaultColumnWidth, defaultRowHeight, rowHeights);
			}

			return tableContent;
		}

		private void SynchronizeScrolling()
		{
			// Synchronize horizontal scrolling
			_contentArea.ContentScrollView.horizontalScroller.valueChanged += (value) =>
			{
				_headerScrollView.scrollOffset = new Vector2(value, _headerScrollView.scrollOffset.y);
			};
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

			var row = new Row(rowIndex);
			row.SetRowHeight(rowHeight, _flexibleRowHeights);

			// Calculate total row width based on column widths
			var totalRowWidth = 0f;

			var contentRowCells = new List<VisualElement>();
			for (var j = 1; j < columns.Count; j++)
			{
				var column = columns[j];
				var columnWidth = column.Width ?? defaultColumnWidth;
				totalRowWidth += columnWidth;

				var cell = new TableCell(j, rowIndex);
				cell.SetWidth(columnWidth);
				cell.SetRowHeight(rowHeight, _flexibleRowHeights);

				// Adjust columnIndex for event handlers
				var columnIndex = j - 1;

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

			row.SetRowWidth(totalRowWidth);

			// Add row to _contentRows and _contentCells
			_contentRows.Add(row);
			_contentCells.Add(contentRowCells);

			// Add row to content scroll view
			_contentArea.ContentScrollView.contentContainer.Add(row);

			// Update row numbers
			var rowNumberWidth = columns[0].Width ?? defaultColumnWidth;
			var rowNumberCell = new RowHeaderCell($"{_rowNumberCells.Count + 1}", rowNumberWidth, rowHeight, rowIndex);
			rowNumberCell.SetRowHeight(rowHeight, _flexibleRowHeights);
			rowNumberCell.RegisterCallback<PointerEnterEvent>(evt => OnRowHeaderPointerEnter(rowIndex));
			rowNumberCell.RegisterCallback<PointerLeaveEvent>(evt => OnRowHeaderPointerLeave(rowIndex));

			_rowNumberCells.Add(rowNumberCell);
			_contentArea.RowNumberScrollView.contentContainer.Add(rowNumberCell);
		}

		// Method to remove a row
		public void RemoveRow(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= _contentRows.Count)
			{
				throw new System.ArgumentOutOfRangeException(nameof(rowIndex));
			}

			// Remove row from UI
			_contentArea.RowNumberScrollView.contentContainer.Remove(_contentRows[rowIndex]);
			_contentRows.RemoveAt(rowIndex);
			_contentCells.RemoveAt(rowIndex);

			// Remove row number cell
			_contentArea.RowNumberScrollView.contentContainer.Remove(_rowNumberCells[rowIndex]);
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
			
			var headerCell = new ColumnHeaderCell(columnDefinition.Label, columnWidth, 30f, columnIndex);
			headerCell.RegisterCallback<PointerEnterEvent>(evt => OnHeaderCellPointerEnter(columnIndex));
			headerCell.RegisterCallback<PointerLeaveEvent>(evt => OnHeaderCellPointerLeave(columnIndex));
			_headerScrollView.contentContainer.Add(headerCell);

			// Add cells to each row
			for (var i = 0; i < _contentRows.Count; i++)
			{
				var row = _contentRows[i];
				var cell = new TableCell(columnIndex, i);
				cell.SetWidth(columnWidth);
				
				row.Add(cell);
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
			_contentArea.ShowRowNumbers();
			_topLeftCornerCell?.RemoveFromClassList("ui-table__top-left-cell--hidden");

			if (columnDefinition == null) return;
			
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
		
		public void HideRowNumbers()
		{
			_includeRowNumbers = false;
			_contentArea.HideRowNumbers();
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
