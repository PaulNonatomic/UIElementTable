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
		private VisualElement _rowNumberContainer;
		private List<List<VisualElement>> _contentCells;
		private List<VisualElement> _rowNumberCells;
		private List<VisualElement> _contentRows;
		private readonly bool _flexibleRowHeights;
		private readonly bool _includeRowNumbers;

		public UITable(
			int rowCount,
			int columnCount = 0,
			float defaultColumnWidth = 100f,
			float defaultRowHeight = 30f,
			List<ColumnDefinition> columns = null,
			Dictionary<int, float> rowHeights = null,
			bool flexibleRowHeights = false,
			bool includeRowNumbers = true)
		{
			_flexibleRowHeights = flexibleRowHeights;
			_includeRowNumbers = includeRowNumbers;
			_contentCells = new List<List<VisualElement>>();
			_rowNumberCells = new List<VisualElement>();
			_contentRows = new List<VisualElement>();

			var styleSheet = Resources.Load<StyleSheet>("UITable");
			
			AddToClassList("ui-table");
			SetCustomStyleSheet(styleSheet);
			CreateTable(columnCount, rowCount, defaultColumnWidth, defaultRowHeight, columns, rowHeights);
			SynchronizeScrolling();
		}
		
		private void CreateTable(int columns, int rows, float defaultColumnWidth, float defaultRowHeight, List<ColumnDefinition> columnDefinitions = null, Dictionary<int, float> rowHeights = null)
		{
			if (columns < 1)
			{
				throw new System.ArgumentException("Column count must be greater than 0.");
			}

			if (columnDefinitions == null)
			{
				columnDefinitions = GenerateDefaultColumns(columns, defaultColumnWidth);
			}
			
			var topRowContainer = CreateTopRow(columnDefinitions, defaultColumnWidth, defaultRowHeight);
			Add(topRowContainer);
			
			var contentRowContainer = CreateContentArea(rows, columnDefinitions, defaultColumnWidth, defaultRowHeight, rowHeights);
			Add(contentRowContainer);
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

			// Row number column is the row header
			columns.Add(new ColumnDefinition("Row Header", defaultColumnWidth));

			for (var i = 1; i < columnCount; i++)
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
			
			var startIndex = 0;

			if (_includeRowNumbers)
			{
				// Top-left corner cell (row header)
				var rowNumberColumn = columns[0];
				var rowNumberWidth = rowNumberColumn.Width ?? defaultColumnWidth;
				var topLeftCornerCell = TableCell.Create(rowNumberColumn.Label, rowNumberWidth, defaultRowHeight);
				topLeftCornerCell.AddToClassList("ui-table__header-cell");

				// Add pointer event handlers for top-left cell
				topLeftCornerCell.RegisterCallback<PointerEnterEvent>(evt => OnTopLeftCellPointerEnter());
				topLeftCornerCell.RegisterCallback<PointerLeaveEvent>(evt => OnTopLeftCellPointerLeave());

				topRowContainer.Add(topLeftCornerCell);

				startIndex = 1; // Start from the second column
			}

			// Header ScrollView (non-interactive)
			_headerScrollView = TableScrollView.CreateHorizontal(isInteractive: false, hideHorizontalScrollbar: true);
			_headerScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-row");
			topRowContainer.Add(_headerScrollView);

			// Populate header cells
			for (var i = startIndex; i < columns.Count; i++)
			{
				var column = columns[i];
				var columnWidth = column.Width ?? defaultColumnWidth;
				var headerCell = TableCell.Create(column.Label, columnWidth, defaultRowHeight);
				headerCell.AddToClassList("ui-table__header-cell");

				int columnIndex = _includeRowNumbers ? i - 1 : i; // Adjust index

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
				var scrollbarWidth = _contentScrollView.verticalScroller.resolvedStyle.width;
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

			var startIndex = 0;

			if (_includeRowNumbers)
			{
				var rowNumberColumn = columns[0];
				var rowNumberWidth = rowNumberColumn.Width ?? defaultColumnWidth;
				
				// Row number Column ScrollView (non-interactive)
				_rowNumberContainer = new VisualElement();
				_rowNumberContainer.AddToClassList("ui-table__row-numbers-column");
				contentRowContainer.Add(_rowNumberContainer);

				_rowNumberColumnScrollView = TableScrollView.CreateVertical(isInteractive: false, hideVerticalScrollbar: true);
				_rowNumberColumnScrollView.AddToClassList("ui-table__scrollview-content-column");
				_rowNumberContainer.Add(_rowNumberColumnScrollView);

				// Populate row number column cells (row headers)
				for (var i = 0; i < rowCount; i++)
				{
					var rowHeight = rowHeights != null && rowHeights.ContainsKey(i) ? rowHeights[i] : defaultRowHeight;
					var rowNumberCell = TableCell.Create($"{i + 1}", rowNumberWidth, rowHeight);

					var rowIndex = i;
					rowNumberCell.RegisterCallback<PointerEnterEvent>(evt => OnRowHeaderPointerEnter(rowIndex));
					rowNumberCell.RegisterCallback<PointerLeaveEvent>(evt => OnRowHeaderPointerLeave(rowIndex));
					
					_rowNumberCells.Add(rowNumberCell);

					// Assign even or odd class
					rowNumberCell.AddToClassList((i + 1) % 2 == 0 ? "ui-table__fixed-column--even" : "ui-table__fixed-column--odd");

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

					_rowNumberColumnScrollView.contentContainer.Add(rowNumberCell);
				}

				// Add spacer element to account for scrollbar height
				var spacer = new VisualElement();
				spacer.style.flexShrink = 0;
				spacer.style.flexGrow = 0;
				_rowNumberContainer.Add(spacer);

				// Delay setting the spacer height until layout is ready
				RegisterCallback<GeometryChangedEvent>((evt) =>
				{
					var scrollbarHeight = _contentScrollView.horizontalScroller.resolvedStyle.height;
					spacer.style.height = scrollbarHeight > 0 ? scrollbarHeight : 0f;
				});

				startIndex = 1; // Start from the second column
			}

			// Main Content ScrollView (interactive with visible scrollbars)
			_contentScrollView = TableScrollView.CreateBoth(isInteractive: true);
			_contentScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-column");
			contentRowContainer.Add(_contentScrollView);

			// Populate content cells
			for (var i = 0; i < rowCount; i++)
			{
				var rowHeight = rowHeights != null && rowHeights.ContainsKey(i) ? rowHeights[i] : defaultRowHeight;

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

				_contentRows.Add(row);

				// Calculate total row width based on column widths
				var totalRowWidth = 0f;

				var contentRowCells = new List<VisualElement>();
				for (var j = startIndex; j < columns.Count; j++)
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
					var columnIndex = _includeRowNumbers ? j - 1 : j;

					// Add pointer event handlers for content cells
					cell.RegisterCallback<PointerEnterEvent>(evt => OnCellPointerEnter(cell));
					cell.RegisterCallback<PointerLeaveEvent>(evt => OnCellPointerLeave(cell));

					// Add cell to row
					row.Add(cell);

					// Store cell in contentRowCells
					contentRowCells.Add(cell);
				}

				row.style.width = totalRowWidth;

				// Assign even or odd class
				row.AddToClassList((i + 1) % 2 == 0 ? "ui-table__row--even" : "ui-table__row--odd");

				_contentCells.Add(contentRowCells);
				_contentScrollView.contentContainer.Add(row);
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

			// Synchronize vertical scrolling if row number column exists
			if (_includeRowNumbers)
			{
				_contentScrollView.verticalScroller.valueChanged += (value) =>
				{
					_rowNumberColumnScrollView.scrollOffset = new Vector2(_rowNumberColumnScrollView.scrollOffset.x, value);
				};
			}
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

			if (_includeRowNumbers)
			{
				_rowNumberCells[rowIndex].AddToClassList("ui-table__row--highlighted");
			}
		}

		private void OnRowHeaderPointerLeave(int rowIndex)
		{
			_contentRows[rowIndex].RemoveFromClassList("ui-table__row--highlighted");

			if (_includeRowNumbers)
			{
				_rowNumberCells[rowIndex].RemoveFromClassList("ui-table__row--highlighted");
			}
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
	}
}
