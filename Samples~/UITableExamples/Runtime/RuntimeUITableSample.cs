using System.Collections.Generic;
using Nonatomic.UIElements.Binding;
using Nonatomic.UIElements.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements.Examples
{
	/// <summary>
	/// Minimal in-game example of a data-bound UITable rendered through a UIDocument.
	/// Put this on a GameObject that also has a UIDocument; it builds the table into the
	/// document root and wires up a few runtime controls. Built in Start so it runs after
	/// UIDocument.OnEnable has created the root visual element.
	/// </summary>
	[RequireComponent(typeof(UIDocument))]
	public class RuntimeUITableSample : MonoBehaviour
	{
		private readonly List<SamplePerson> _people = new List<SamplePerson>();
		private DataBindingUITable<SamplePerson> _table;
		private Label _status;
		private int _added;
		private UIDocument _document;
		private bool _initialized;

		private void Awake()
		{
			_document = GetComponent<UIDocument>();
		}

		private void Start()
		{
			Build();
		}

		private void Update()
		{
			// The editor rebuilds a UIDocument's rootVisualElement (clearing code-built content)
			// when the GameObject is selected or its component is validated, which wipes the table.
			// Rebuild if that has happened. This is a no-op in a player build.
			var current = _document != null ? _document.rootVisualElement : null;
			if (current != null && current.childCount == 0) Build();
		}

		private void Build()
		{
			var root = _document != null ? _document.rootVisualElement : null;
			if (root == null) return;

			if (!_initialized)
			{
				_people.AddRange(SamplePerson.SetA());
				_initialized = true;
			}

			root.Clear();
			root.style.flexGrow = 1;
			root.style.paddingTop = 12;
			root.style.paddingLeft = 12;
			root.style.paddingRight = 12;
			root.style.paddingBottom = 12;

			var heading = new Label("UITable runtime sample");
			heading.style.unityFontStyleAndWeight = FontStyle.Bold;
			heading.style.fontSize = 16;
			heading.style.marginBottom = 8;
			root.Add(heading);

			root.Add(BuildControls());

			_table = new DataBindingUITable<SamplePerson>();
			_table.style.flexGrow = 1;
			_table.style.marginTop = 8;
			// Sizes to its columns by default; uncomment to fill the panel width instead:
			// _table.SetFillWidth(true);
			_table.ShowRowNumbers(new ColumnDefinition("#", 40f));
			_table.AddColumn(new ColumnDefinition("Name", 160f), p => new Label(p.Name));
			_table.AddColumn(new ColumnDefinition("Age", 70f), p => Centered(p.Age.ToString()));
			_table.AddColumn(new ColumnDefinition("Country", 150f), p => new Label(p.Country));
			_table.RegisterCallback<TableCellClickEvent>(evt => SetStatus($"Cell clicked: row {evt.RowIndex}, col {evt.ColumnIndex}"));
			_table.RegisterCallback<RowHeaderClickEvent>(evt => SetStatus($"Row header clicked: row {evt.RowIndex}"));
			_table.RegisterCallback<ColumnHeaderClickEvent>(evt => SetStatus($"Column header clicked: col {evt.ColumnIndex}"));
			root.Add(_table);

			_status = new Label("Click a cell, a row number, or a column header.");
			_status.style.marginTop = 8;
			_status.style.whiteSpace = WhiteSpace.Normal;
			root.Add(_status);

			_table.SetData(_people);
		}

		private VisualElement BuildControls()
		{
			var bar = new VisualElement();
			bar.style.flexDirection = FlexDirection.Row;
			bar.Add(new Button(AddPerson) { text = "Add person" });
			bar.Add(new Button(RemoveLast) { text = "Remove last" });
			bar.Add(new Button(ResetData) { text = "Reset" });
			return bar;
		}

		private void AddPerson()
		{
			var names = new[] { "Kim", "Leo", "Mia", "Nia", "Omar", "Pia", "Rey", "Sam" };
			var countries = new[] { "Spain", "Kenya", "Chile", "India", "Peru", "Finland" };
			_added++;
			var name = $"{names[_added % names.Length]}{_added}";
			var country = countries[_added % countries.Length];
			var age = 18 + (_added * 5) % 50;
			_people.Add(new SamplePerson(name, age, country));
			_table.SetData(_people);
			SetStatus($"Added {name}. Rows: {_table.RowCount}");
		}

		private void RemoveLast()
		{
			if (_people.Count == 0) return;
			_people.RemoveAt(_people.Count - 1);
			_table.SetData(_people);
			SetStatus($"Removed last. Rows: {_table.RowCount}");
		}

		private void ResetData()
		{
			_people.Clear();
			_people.AddRange(SamplePerson.SetA());
			_added = 0;
			_table.SetData(_people);
			SetStatus($"Reset. Rows: {_table.RowCount}");
		}

		private static VisualElement Centered(string text)
		{
			var label = new Label(text);
			label.style.unityTextAlign = TextAnchor.MiddleCenter;
			return label;
		}

		private void SetStatus(string message)
		{
			if (_status != null) _status.text = message;
			Debug.Log($"[UITable] {message}");
		}
	}
}
