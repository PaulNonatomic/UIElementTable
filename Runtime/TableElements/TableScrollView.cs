using UnityEngine.UIElements;

namespace Nonatomic.UIElements.TableElements
{
	public static class TableScrollView
	{
		public static ScrollView CreateHorizontal(bool isInteractive, bool hideHorizontalScrollbar = false)
		{
			var horizontalVisibility = hideHorizontalScrollbar ? ScrollerVisibility.Hidden : ScrollerVisibility.Auto;
			return CreateScrollView(
				ScrollViewMode.Horizontal,
				isInteractive,
				horizontalVisibility,
				ScrollerVisibility.Hidden,
				"ui-table__scrollview--horizontal");
		}

		public static ScrollView CreateVertical(bool isInteractive, bool hideVerticalScrollbar = false)
		{
			var verticalVisibility = hideVerticalScrollbar ? ScrollerVisibility.Hidden : ScrollerVisibility.Auto;
			return CreateScrollView(
				ScrollViewMode.Vertical,
				isInteractive,
				ScrollerVisibility.Hidden,
				verticalVisibility,
				"ui-table__scrollview--vertical");
		}

		public static ScrollView CreateBoth(bool isInteractive)
		{
			return CreateScrollView(
				ScrollViewMode.VerticalAndHorizontal,
				isInteractive,
				ScrollerVisibility.Auto,
				ScrollerVisibility.Auto);
		}

		private static ScrollView CreateScrollView(
			ScrollViewMode mode,
			bool isInteractive,
			ScrollerVisibility horizontalVisibility,
			ScrollerVisibility verticalVisibility,
			string additionalClass = null)
		{
			var scrollView = new ScrollView(mode)
			{
				horizontalScrollerVisibility = horizontalVisibility,
				verticalScrollerVisibility = verticalVisibility
			};

			if (!string.IsNullOrEmpty(additionalClass))
			{
				scrollView.AddToClassList(additionalClass);
			}

			if (!isInteractive)
			{
				// Hide overflow and disable interaction for non-interactive scroll views
				scrollView.AddToClassList("ui-table__scrollview--non-interactive");
				DisableScrollInteraction(scrollView);
			}
			else
			{
				// For interactive scroll views, allow overflow to show scrollbars
				scrollView.style.overflow = Overflow.Visible;
			}

			return scrollView;
		}

		private static void DisableScrollInteraction(ScrollView scrollView)
		{
			// Disable scrolling via mouse wheel or touchpad
			scrollView.RegisterCallback<WheelEvent>(evt => evt.StopImmediatePropagation());
			scrollView.RegisterCallback<PointerMoveEvent>(evt => evt.StopImmediatePropagation());
			scrollView.RegisterCallback<PointerDownEvent>(evt => evt.StopImmediatePropagation());

			// Disable scrollbars
			scrollView.horizontalScroller.SetEnabled(false);
			scrollView.verticalScroller.SetEnabled(false);

			// Prevent user input on the content
			scrollView.contentContainer.pickingMode = PickingMode.Ignore;
		}
	}
}
