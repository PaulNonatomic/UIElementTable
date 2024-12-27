using System;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements.Binding
{
	/// <summary>
	/// Simple container for a column definition plus a delegate that creates a cell.
	/// </summary>
	public class ColumnBinder<T>
	{
		public ColumnDefinition ColumnDefinition { get; set; }
		public Func<T, VisualElement> CreateCell { get; set; }
	}
}