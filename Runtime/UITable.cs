﻿using System;
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
		private List<List<VisualElement>> _contentCells = new();
		private List<RowHeaderCell> _rowNumberCells = new();
		private List<VisualElement> _contentRows = new();
		private List<ColumnDefinition> _columns;
		private VisualElement _topLeftCornerCell;
		private VisualElement _topRowContainer;
		private TableContentArea _contentArea;
		private readonly bool _flexibleRowHeights;
		private bool _includeRowNumbers;
		
		private const float DefaultColumnWidth = 100;
		private const float DefaultRowHeight = 30;
		private const int DefaultRowCount = 0;
		private const int DefaultColumnCount = 0;

		public int RowCount => _contentCells.Count;
		public int ColumnCount => _contentCells.Count == 0 ? 0 : _contentCells[0].Count;

		/// <summary>
		/// Represents a customizable table component for creating and managing grid-like layouts in Unity's UIElements framework.
		/// Allows for defining rows and columns, setting custom cell content, customizing display properties, and handling
		/// flexible row heights and row numbers for advanced table behavior.
		/// </summary>
		public UITable()
		{
			_flexibleRowHeights = false;
			_includeRowNumbers = false;

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
			if (rowIndex < 0 || rowIndex >= _contentCells.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex), $"Row index {rowIndex} is out of range. Valid range: 0 to {_contentCells.Count - 1}.");
			}

			if (columnIndex < 0 || columnIndex >= _contentCells[rowIndex].Count)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex), $"Column index {columnIndex} is out of range. Valid range: 0 to {_contentCells[rowIndex].Count - 1}.");
			}

			// Retrieve and return the cell
			return _contentCells[rowIndex][columnIndex];
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
			if (_contentCells.Count == 0)
			{
				throw new InvalidOperationException("There are no rows in the table.");
			}

			if (rowIndex < 0 || rowIndex >= _contentCells.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex));
			}
			
			return _contentCells[rowIndex];
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
			if (_contentCells.Count == 0)
			{
				throw new InvalidOperationException("There are no rows in the table.");
			}

			if (columnIndex < 0 || columnIndex >= _contentCells[0].Count)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex));
			}

			return _contentCells.Select(t => t[columnIndex]).ToList();
		}
	
		public void SetCell(int rowIndex, int columnIndex, VisualElement content)
		{
			if (rowIndex < 0 || rowIndex >= _contentCells.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rowIndex));
			}

			if (columnIndex < 0 || columnIndex >= _contentCells[rowIndex].Count)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex));
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

				var index = i-1;
				headerCell.RegisterCallback<PointerEnterEvent>(evt => OnHeaderCellPointerEnter(index));
				headerCell.RegisterCallback<PointerLeaveEvent>(evt => OnHeaderCellPointerLeave(index));
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

		private void OnRowHeaderPointerEnter(int rowIndex)
		{
			_contentRows[rowIndex].AddToClassList("ui-table__row--highlighted");
		}

		private void OnRowHeaderPointerLeave(int rowIndex)
		{
			_contentRows[rowIndex].RemoveFromClassList("ui-table__row--highlighted");
		}

		private void OnHeaderCellPointerEnter(int columnIndex)
		{
			if(columnIndex < 0 || columnIndex >= _contentCells[0].Count)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex), $"Column index {columnIndex} is out of range. Valid range: 0 to {_contentCells[0].Count - 1}.");
			}
			
			// Highlight all cells in the column
			foreach (var rowCells in _contentCells)
			{
				var cell = rowCells[columnIndex];
				cell.AddToClassList("ui-table__column--highlighted");
			}
		}

		private void OnHeaderCellPointerLeave(int columnIndex)
		{
			if(columnIndex < 0 || columnIndex >= _contentCells[0].Count)
			{
				throw new ArgumentOutOfRangeException(nameof(columnIndex), $"Column index {columnIndex} is out of range. Valid range: 0 to {_contentCells[0].Count - 1}.");
			}
			
			// Remove highlight from all cells in the column
			foreach (var rowCells in _contentCells)
			{
				var cell = rowCells[columnIndex];
				cell.RemoveFromClassList("ui-table__column--highlighted");
			}
		}

		private void OnTopLeftCellPointerEnter()
		{
			AddToClassList("ui-table--highlighted");
		}

		private void OnTopLeftCellPointerLeave()
		{
			RemoveFromClassList("ui-table--highlighted");
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

				// Adjust columnIndex for event handlers
				var columnIndex = j - 1;
				
				var cell = new TableCell(columnIndex, rowIndex);
				cell.SetWidth(columnWidth);
				cell.SetRowHeight(rowHeight, _flexibleRowHeights);
				cell.RegisterCallback<ClickEvent>(evt => HandleTableCellClick(cell));

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
			rowNumberCell.RegisterCallback<ClickEvent>(evt => HandleRowHeaderClick(rowNumberCell));
			
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
			var row = _contentRows[rowIndex];
			var content = _contentArea.RowNumberScrollView.contentContainer;

			if (content.Contains(row))
			{
				content.Remove(row);
			}
			
			_contentRows.RemoveAt(rowIndex);
			_contentCells.RemoveAt(rowIndex);

			// Remove row number cell
			var rowNum = _rowNumberCells[rowIndex];
			if (content.Contains(rowNum))
			{
				content.Remove(rowNum);
			}
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
			var columnIndex = _columns.Count - 1;
			
			var headerCell = new ColumnHeaderCell(columnDefinition.Label, columnWidth, 30f, columnIndex);
			headerCell.RegisterCallback<PointerEnterEvent>(evt => OnHeaderCellPointerEnter(columnIndex-1));
			headerCell.RegisterCallback<PointerLeaveEvent>(evt => OnHeaderCellPointerLeave(columnIndex-1));
			headerCell.RegisterCallback<ClickEvent>(evt => HandleColumnHeaderClick(headerCell));
			_headerScrollView.contentContainer.Add(headerCell);

			// Add cells to each row
			for (var i = 0; i < _contentRows.Count; i++)
			{
				var row = _contentRows[i];
				var cell = new TableCell(columnIndex, i);
				cell.RegisterCallback<ClickEvent>(evt => HandleTableCellClick(cell));
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
