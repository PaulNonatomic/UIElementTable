using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements
{
	public class TableContentArea : VisualElement
	{
		public Action<float> OnContentVerticalScroll;
		public Action<float> OnContentHorizontalScroll;
		
		public float ContentVerticalScrollerWidth => _contentScrollView?.verticalScroller.resolvedStyle.width ?? 0;
		
		public VisualElement RowNumberContainer => _rowNumberContainer;
		public ScrollView ContentScrollView => _contentScrollView;
		public ScrollView RowNumberScrollView => _rowNumberColumnScrollView;
		
		private readonly VisualElement _rowNumberContainer;
		private readonly ScrollView _rowNumberColumnScrollView;
		private readonly VisualElement _spacer;
		private readonly ScrollView _contentScrollView;

		public TableContentArea()
		{
			AddToClassList("ui-table__content-row");
			
			_rowNumberContainer = new VisualElement();
			_rowNumberContainer.AddToClassList("ui-table__row-numbers-column");
			Add(_rowNumberContainer);
			
			_rowNumberColumnScrollView = TableScrollView.CreateVertical(isInteractive: false, hideVerticalScrollbar: true);
			_rowNumberColumnScrollView.AddToClassList("ui-table__scrollview-content-column");
			_rowNumberColumnScrollView.RegisterCallback<WheelEvent>(HandleRowNumberScrollWheel, TrickleDown.TrickleDown);
			_rowNumberContainer.Add(_rowNumberColumnScrollView);
			
			// Add spacer element to account for scrollbar height
			_spacer = new VisualElement();
			_spacer.style.flexShrink = 0;
			_spacer.style.flexGrow = 0;
			_rowNumberContainer.Add(_spacer);
			
			// Delay setting the spacer height until layout is ready
			RegisterCallback<GeometryChangedEvent>(HandleGeometryChange);
			
			// Main Content ScrollView (interactive with visible scrollbars)
			_contentScrollView = TableScrollView.CreateBoth(isInteractive: true);
			_contentScrollView.contentContainer.AddToClassList("ui-table__scrollview-content-column");
			_contentScrollView.horizontalScroller.valueChanged += HandleHorizontalScroll;
			_contentScrollView.verticalScroller.valueChanged += HandleVerticalScroll;
			
			Add(_contentScrollView);
		}

		private void HandleRowNumberScrollWheel(WheelEvent evt)
		{
			evt.StopImmediatePropagation();
		}

		private void HandleHorizontalScroll(float value)
		{
			OnContentHorizontalScroll?.Invoke(value);
		}
		
		private void HandleVerticalScroll(float value)
		{
			OnContentVerticalScroll?.Invoke(value);
			SetRowNumberScrollOffset(value);
		}

		public void ShowRowNumbers()
		{
			_rowNumberContainer.RemoveFromClassList("ui-table__row-numbers-column--hidden");
		}

		public void HideRowNumbers()
		{
			_rowNumberContainer.AddToClassList("ui-table__row-numbers-column--hidden");
		}

		public void SetRowNumberScrollOffset(float offset)
		{
			_rowNumberColumnScrollView.scrollOffset = new Vector2(_rowNumberColumnScrollView.scrollOffset.x, offset);
		}

		private void HandleGeometryChange(GeometryChangedEvent evt)
		{
			var scrollbarHeight = _contentScrollView?.horizontalScroller.resolvedStyle.height ?? 0;
			_spacer.style.height = scrollbarHeight > 0 ? scrollbarHeight : 0f;
		}
	}
}