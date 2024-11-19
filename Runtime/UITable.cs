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

		public UITable(int rowCount, int columnCount)
		{
			AddToClassList("ui-table");

			styleSheets.Add(Resources.Load<StyleSheet>("UITable"));

			var topRowContainer = CreateTopRow(columnCount);
			Add(topRowContainer);

			var contentRowContainer = CreateContentArea(rowCount, columnCount);
			Add(contentRowContainer);

			SynchronizeScrolling();
		}

		private VisualElement CreateTopRow(int columnCount)
		{
			var topRowContainer = new VisualElement();
			topRowContainer.AddToClassList("ui-table__top-row");

			// Top-left corner cell
			var topLeftCornerCell = TableCell.Create("Header");
			topLeftCornerCell.AddToClassList("ui-table__header-cell");
			topRowContainer.Add(topLeftCornerCell);

			// Header ScrollView (non-interactive)
			_headerScrollView = TableScrollView.CreateHorizontal(isInteractive: false, hideHorizontalScrollbar: true);
			_headerScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-row");
			topRowContainer.Add(_headerScrollView);

			// Populate header cells
			for (var i = 1; i <= columnCount; i++)
			{
				var labelString = i == 0 ? string.Empty : $"Header {i}";
				var headerCell = TableCell.Create(labelString);
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
				if (scrollbarWidth == 0)
				{
					scrollbarWidth = 0f; 
				}
				spacer.style.width = scrollbarWidth;
			});

			return topRowContainer;
		}

		private VisualElement CreateContentArea(int rowCount, int columnCount)
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

			// Populate first column cells
			for (var i = 1; i <= rowCount; i++)
			{
				var firstColumnCell = TableCell.Create($"{i}");

				// Assign even or odd class
				if (i % 2 == 0)
				{
					firstColumnCell.AddToClassList("ui-table__fixed-column--even");
				}
				else
				{
					firstColumnCell.AddToClassList("ui-table__fixed-column--odd");
				}

				_firstColumnScrollView.contentContainer.Add(firstColumnCell);
			}
			
			// Add spacer element to account for scrollbar height
			var spacer = new VisualElement();
			spacer.style.flexShrink = 0;
			spacer.style.flexGrow = 0;
			_firstColumnContainer.Add(spacer);

			// Delay setting the spacer width until layout is ready
			RegisterCallback<GeometryChangedEvent>((evt) =>
			{
				var scrollbarHeight = _contentScrollView.horizontalScroller.resolvedStyle.height;
				if (scrollbarHeight == 0)
				{
					scrollbarHeight = 0f; // Default value, adjust as needed
				}
				spacer.style.height = scrollbarHeight;
			});

			// Main Content ScrollView (interactive with visible scrollbars)
			_contentScrollView = TableScrollView.CreateBoth(isInteractive: true);
			_contentScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-column");
			contentRowContainer.Add(_contentScrollView);

			// Populate content cells
			for (var i = 1; i <= rowCount; i++)
			{
				var row = new VisualElement();
				row.AddToClassList("ui-table__row");
				row.style.width = columnCount * 100;
				row.style.flexShrink = 0;
				
				// Assign even or odd class
				if (i % 2 == 0)
				{
					row.AddToClassList("ui-table__row--even");
				}
				else
				{
					row.AddToClassList("ui-table__row--odd");
				}

				for (var j = 1; j <= columnCount; j++)
				{
					var cell = TableCell.Create($"Cell {i},{j}");
					row.Add(cell);
				}

				_contentScrollView.contentContainer.Add(row);
			}

			return contentRowContainer;
		}

		private void SynchronizeScrolling()
		{
			// Synchronize horizontal scrolling
			_contentScrollView.horizontalScroller.valueChanged += (value) =>
			{
				_headerScrollView.scrollOffset = new(value, _headerScrollView.scrollOffset.y);
			};

			// Synchronize vertical scrolling
			_contentScrollView.verticalScroller.valueChanged += (value) =>
			{
				_firstColumnScrollView.scrollOffset = new (_firstColumnScrollView.scrollOffset.x, value);
			};
		}
	}
}
