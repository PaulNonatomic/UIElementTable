using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class UITable : VisualElement
	{
		private ScrollView _headerScrollView;
		private ScrollView _firstColumnScrollView;
		private ScrollView _contentScrollView;
		private VisualElement _firstColumnContainer;
		private List<List<VisualElement>> _contentCells;

		public UITable(
			int rowCount,
			int columnCount = 0,
			float defaultColumnWidth = 100f,
			float defaultRowHeight = 30f,
			List<ColumnDefinition> columns = null,
			Dictionary<int, float> rowHeights = null)
		{
			//Style
			styleSheets.Add(Resources.Load<StyleSheet>("UITable"));
			AddToClassList("ui-table");
			
			_contentCells = new List<List<VisualElement>>();

			// Handle columns being null or empty
			if (columns == null || columns.Count == 0)
			{
				if (columnCount <= 0)
				{
					throw new System.ArgumentException("Either columns must be provided, or columnCount must be greater than 0.");
				}

				// Generate default columns
				columns = GenerateDefaultColumns(columnCount, defaultColumnWidth);
			}

			var topRowContainer = CreateTopRow(columns, defaultColumnWidth, defaultRowHeight);
			Add(topRowContainer);

			var contentRowContainer = CreateContentArea(rowCount, columns, defaultColumnWidth, defaultRowHeight, rowHeights);
			Add(contentRowContainer);

			SynchronizeScrolling();
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
		}


		private List<ColumnDefinition> GenerateDefaultColumns(int columnCount, float defaultColumnWidth)
		{
			var columns = new List<ColumnDefinition>();

			// First column is the row header
			columns.Add(new ColumnDefinition("Row Header", defaultColumnWidth));

			for (int i = 1; i < columnCount; i++)
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

			// Top-left corner cell (row header)
			var firstColumn = columns[0];
			var firstColumnWidth = firstColumn.Width ?? defaultColumnWidth;
			var topLeftCornerCell = TableCell.Create(firstColumn.Label, firstColumnWidth, defaultRowHeight);
			topLeftCornerCell.AddToClassList("ui-table__header-cell");
			topRowContainer.Add(topLeftCornerCell);

			// Header ScrollView (non-interactive)
			_headerScrollView = TableScrollView.CreateHorizontal(isInteractive: false, hideHorizontalScrollbar: true);
			_headerScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-row");
			topRowContainer.Add(_headerScrollView);

			// Populate header cells
			for (var i = 1; i < columns.Count; i++)
			{
				var column = columns[i];
				var columnWidth = column.Width ?? defaultColumnWidth;
				var headerCell = TableCell.Create(column.Label, columnWidth, defaultRowHeight);
				headerCell.AddToClassList("ui-table__header-cell");
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

			// First Column ScrollView (non-interactive)
			_firstColumnContainer = new VisualElement();
			_firstColumnContainer.AddToClassList("ui-table__first-content-column");
			contentRowContainer.Add(_firstColumnContainer);

			_firstColumnScrollView = TableScrollView.CreateVertical(isInteractive: false, hideVerticalScrollbar: true);
			_firstColumnScrollView.AddToClassList("ui-table__scrollview-content-column");
			_firstColumnContainer.Add(_firstColumnScrollView);

			// Get first column width
			var firstColumn = columns[0];
			var firstColumnWidth = firstColumn.Width ?? defaultColumnWidth;

			// Populate first column cells (row headers)
			for (var i = 1; i <= rowCount; i++)
			{
				var rowHeight = rowHeights != null && rowHeights.ContainsKey(i) ? rowHeights[i] : defaultRowHeight;
				var firstColumnCell = TableCell.Create($"Row {i}", firstColumnWidth, rowHeight);

				// Assign even or odd class
				firstColumnCell.AddToClassList(i % 2 == 0 ? "ui-table__fixed-column--even" : "ui-table__fixed-column--odd");

				_firstColumnScrollView.contentContainer.Add(firstColumnCell);
			}

			// Add spacer element to account for scrollbar height
			var spacer = new VisualElement();
			spacer.style.flexShrink = 0;
			spacer.style.flexGrow = 0;
			_firstColumnContainer.Add(spacer);

			// Delay setting the spacer height until layout is ready
			RegisterCallback<GeometryChangedEvent>((evt) =>
			{
				var scrollbarHeight = _contentScrollView.horizontalScroller.resolvedStyle.height;
				spacer.style.height = scrollbarHeight > 0 ? scrollbarHeight : 0f;
			});

			// Main Content ScrollView (interactive with visible scrollbars)
			_contentScrollView = TableScrollView.CreateBoth(isInteractive: true);
			_contentScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-column");
			contentRowContainer.Add(_contentScrollView);

			// Populate content cells
			for (var i = 1; i <= rowCount; i++)
			{
				var rowHeight = rowHeights != null && rowHeights.ContainsKey(i) ? rowHeights[i] : defaultRowHeight;

				var row = new VisualElement();
				row.AddToClassList("ui-table__row");
				row.style.flexDirection = FlexDirection.Row;
				row.style.flexShrink = 0;
				row.style.height = rowHeight;

				// Calculate total row width based on column widths
				float totalRowWidth = 0f;

				var contentRowCells = new List<VisualElement>();

				for (var j = 1; j < columns.Count; j++)
				{
					var column = columns[j];
					var columnWidth = column.Width ?? defaultColumnWidth;
					totalRowWidth += columnWidth;

					var cell = new VisualElement();
					cell.AddToClassList("ui-table__cell");
					cell.style.width = columnWidth;
					cell.style.height = rowHeight;

					// Do not add a label to the cell by default
					// Add cell to row
					row.Add(cell);

					// Store cell in contentRowCells
					contentRowCells.Add(cell);
				}

				row.style.width = totalRowWidth;

				// Assign even or odd class
				row.AddToClassList(i % 2 == 0 ? "ui-table__row--even" : "ui-table__row--odd");

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

			// Synchronize vertical scrolling
			_contentScrollView.verticalScroller.valueChanged += (value) =>
			{
				_firstColumnScrollView.scrollOffset = new Vector2(_firstColumnScrollView.scrollOffset.x, value);
			};
		}
	}
}
