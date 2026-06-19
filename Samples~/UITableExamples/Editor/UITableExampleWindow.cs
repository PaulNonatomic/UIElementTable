using System.Collections.Generic;
using System.Text;
using Nonatomic.UIElements.Binding;
using Nonatomic.UIElements.Events;
using Nonatomic.UIElements.Examples;
using Nonatomic.UIElements.TableElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nonatomic.UIElements.ExamplesEditor
{
	/// <summary>
	/// Interactive validation harness for the UITable package. Each toolbar button builds a
	/// fresh table that exercises part of the API, runs PASS/FAIL assertions, and feeds a
	/// running event log. Open via Window > UI Table Examples > Validation Harness.
	/// </summary>
	public class UITableExampleWindow : EditorWindow
	{
		private VisualElement _actionBar;
		private VisualElement _host;
		private ScrollView _log;
		private DataBindingUITable<Person> _bindingTable;

		[MenuItem("Window/UI Table Examples/Validation Harness")]
		public static void Open()
		{
			var window = GetWindow<UITableExampleWindow>();
			window.titleContent = new GUIContent("UI Table Examples");
			window.minSize = new Vector2(560, 480);
		}

		public void CreateGUI()
		{
			var root = rootVisualElement;
			root.style.flexDirection = FlexDirection.Column;
			root.style.paddingLeft = 6;
			root.style.paddingRight = 6;
			root.style.paddingTop = 6;
			root.style.paddingBottom = 6;

			var title = new Label("UITable validation harness");
			title.style.unityFontStyleAndWeight = FontStyle.Bold;
			title.style.marginBottom = 4;
			root.Add(title);

			var toolbar = Row();
			toolbar.style.flexWrap = Wrap.Wrap;
			toolbar.Add(new Button(BuildBasic) { text = "Basic" });
			toolbar.Add(new Button(BuildDataBinding) { text = "Data binding" });
			toolbar.Add(new Button(BuildRowOps) { text = "Row ops" });
			toolbar.Add(new Button(BuildColumnOps) { text = "Column ops" });
			toolbar.Add(new Button(BuildEmptyHover) { text = "Empty hover" });
			toolbar.Add(new Button(BuildFlexHeights) { text = "Flexible heights" });
			root.Add(toolbar);

			_actionBar = Row();
			_actionBar.style.flexWrap = Wrap.Wrap;
			_actionBar.style.marginTop = 2;
			_actionBar.style.marginBottom = 2;
			root.Add(_actionBar);

			_host = new VisualElement();
			_host.style.flexGrow = 1;
			_host.style.minHeight = 160;
			_host.style.paddingLeft = 4;
			_host.style.paddingRight = 4;
			_host.style.paddingTop = 4;
			_host.style.paddingBottom = 4;
			_host.style.backgroundColor = new Color(0f, 0f, 0f, 0.12f);
			root.Add(_host);

			var logHeader = Row();
			logHeader.style.marginTop = 4;
			var logLabel = new Label("Log / assertions (newest on top)");
			logLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			logLabel.style.flexGrow = 1;
			logHeader.Add(logLabel);
			logHeader.Add(new Button(() => _log.Clear()) { text = "Clear" });
			root.Add(logHeader);

			_log = new ScrollView();
			_log.style.height = 150;
			_log.style.backgroundColor = new Color(0f, 0f, 0f, 0.18f);
			root.Add(_log);

			// Default to the data-binding scenario: it covers the headline regression.
			BuildDataBinding();
		}

		// ---- scenarios -------------------------------------------------------

		private void BuildBasic()
		{
			BeginScenario("=== Basic 3x4 table (SetCell) ===");
			var table = new UITable(rowCount: 3, columnCount: 4, includeRowNumbers: true);
			table.style.flexGrow = 1;
			FillCells(table);
			WireEvents(table);
			_host.Add(table);
			AssertCellCount(table, "basic 3x4");
			AssertRowNumbers(table, "basic 3x4");
		}

		private void BuildDataBinding()
		{
			BeginScenario("=== Data binding (README scenario) ===");
			_bindingTable = new DataBindingUITable<Person>();
			_bindingTable.style.flexGrow = 1;
			_bindingTable.ShowRowNumbers(new ColumnDefinition("#", 40f));
			_bindingTable.AddColumn(new ColumnDefinition("Name", 150f), p => new Label(p.Name));
			_bindingTable.AddColumn(new ColumnDefinition("Age", 60f), p => new Label(p.Age.ToString()));
			_bindingTable.AddColumn(new ColumnDefinition("Country", 120f), p => new Label(p.Country));
			WireEvents(_bindingTable);
			_bindingTable.SetData(Person.SetA());
			_host.Add(_bindingTable);
			AssertCellCount(_bindingTable, "after SetData(3)");
			AssertRowNumbers(_bindingTable, "after SetData(3)");

			_actionBar.Add(new Button(() => Rebind(Person.SetB(), "5 rows")) { text = "Re-bind 5 rows" });
			_actionBar.Add(new Button(() => Rebind(Person.SetC(), "2 rows")) { text = "Re-bind 2 rows" });
			Log("Re-bind repeatedly. Pre-fix, rows leaked/duplicated on each SetData; the cell-count assertion catches that.");
		}

		private void Rebind(List<Person> data, string label)
		{
			_bindingTable.SetData(data);
			Log($"--- Re-bind to {label} ---");
			AssertCellCount(_bindingTable, $"re-bind {label}");
			AssertRowNumbers(_bindingTable, $"re-bind {label}");
		}

		private void BuildRowOps()
		{
			BeginScenario("=== Row ops (remove middle / add) ===");
			var table = new DataBindingUITable<Person>();
			table.style.flexGrow = 1;
			table.ShowRowNumbers(new ColumnDefinition("#", 40f));
			table.AddColumn(new ColumnDefinition("Name", 150f), p => new Label(p.Name));
			table.AddColumn(new ColumnDefinition("Country", 120f), p => new Label(p.Country));
			WireEvents(table);
			table.SetData(Person.SetB());
			_host.Add(table);
			AssertCellCount(table, "5 rows");
			AssertRowNumbers(table, "5 rows");

			_actionBar.Add(new Button(() =>
			{
				if (table.RowCount > 2) table.RemoveRow(2);
				Log("--- RemoveRow(2) ---");
				AssertCellCount(table, "after RemoveRow(2)");
				AssertRowNumbers(table, "after RemoveRow(2)");
			}) { text = "Remove middle row" });

			_actionBar.Add(new Button(() =>
			{
				table.AddRow();
				Log("--- AddRow() ---");
				AssertCellCount(table, "after AddRow");
				AssertRowNumbers(table, "after AddRow");
			}) { text = "Add empty row" });

			Log("After a removal, click rows/cells: the log should report the CURRENT indices, not stale ones.");
		}

		private void BuildColumnOps()
		{
			BeginScenario("=== Column ops on a populated table ===");
			var table = new UITable(rowCount: 3, columnCount: 2, includeRowNumbers: true);
			table.style.flexGrow = 1;
			FillCells(table);
			WireEvents(table);
			_host.Add(table);
			AssertCellCount(table, "3x2");
			AssertRowWidth(table, "3x2");

			_actionBar.Add(new Button(() =>
			{
				table.AddColumn(new ColumnDefinition("Filled", 90f));
				FillCells(table);
				Log("--- AddColumn('Filled') + populate ---");
				AssertCellCount(table, "after add filled column");
				AssertRowWidth(table, "after add filled column");
				Log($"Click a cell in the new last column: logged col should be {table.ColumnCount - 1} (fix #4).");
			}) { text = "Add filled column" });

			_actionBar.Add(new Button(() =>
			{
				table.AddColumn(new ColumnDefinition("Blank", 90f));
				Log("--- AddColumn('Blank'), no content ---");
				AssertCellCount(table, "after add blank column");
				AssertRowWidth(table, "after add blank column");
				Log("New column cells should appear as empty, styled boxes (border + row colour), not missing.");
			}) { text = "Add blank column" });

			_actionBar.Add(new Button(() =>
			{
				table.SetColumn(0, new ColumnDefinition("Wide", 200f));
				Log("--- SetColumn(0, width 200) ---");
				AssertRowWidth(table, "after resize column 0");
				Log("Header AND body cells of column 0 should both be 200 wide (fix #5).");
			}) { text = "Resize column 0" });

			_actionBar.Add(new Button(() =>
			{
				if (table.ColumnCount > 1)
				{
					table.RemoveColumn(0);
					FillCells(table);
					Log("--- RemoveColumn(0) ---");
					AssertCellCount(table, "after RemoveColumn(0)");
					AssertRowWidth(table, "after RemoveColumn(0)");
				}
			}) { text = "Remove column 0" });
		}

		private void BuildEmptyHover()
		{
			BeginScenario("=== Empty-table header hover (fix #2) ===");
			var table = new UITable(rowCount: 0, columnCount: 3, includeRowNumbers: true);
			table.style.flexGrow = 1;
			WireEvents(table);
			_host.Add(table);
			AssertCellCount(table, "0 rows");
			Log("Hover the column headers. Pre-fix this threw IndexOutOfRangeException into the Console; now it should be silent.");
		}

		private void BuildFlexHeights()
		{
			BeginScenario("=== Flexible row heights ===");
			var table = new UITable(rowCount: 3, columnCount: 2, flexibleRowHeights: true, includeRowNumbers: true);
			table.style.flexGrow = 1;
			table.SetCell(0, 0, new Label("Short"));
			table.SetCell(0, 1, new Label("Single line"));
			table.SetCell(1, 0, MultiLine(3));
			table.SetCell(1, 1, new Label("This row should grow to fit three lines on the left."));
			table.SetCell(2, 0, new Label("Short"));
			table.SetCell(2, 1, MultiLine(5));
			WireEvents(table);
			_host.Add(table);
			_actionBar.Add(new Button(() =>
			{
				table.SynchronizeRowHeights();
				Log("--- SynchronizeRowHeights() ---");
			}) { text = "Synchronize row heights" });
			Log("Rows with taller content should expand; the row-number cells should match their row height.");
		}

		// ---- helpers ---------------------------------------------------------

		private void BeginScenario(string title)
		{
			_host.Clear();
			_actionBar.Clear();
			Log("");
			Log(title);
		}

		private static void FillCells(UITable table)
		{
			for (var r = 0; r < table.RowCount; r++)
			for (var c = 0; c < table.ColumnCount; c++)
				table.SetCell(r, c, new Label($"R{r}C{c}"));
		}

		private void WireEvents(UITable table)
		{
			table.RegisterCallback<TableCellClickEvent>(e => Log($"CellClick         col={e.ColumnIndex} row={e.RowIndex}"));
			table.RegisterCallback<RowHeaderClickEvent>(e => Log($"RowHeaderClick     row={e.RowIndex}"));
			table.RegisterCallback<ColumnHeaderClickEvent>(e => Log($"ColumnHeaderClick  col={e.ColumnIndex}"));
		}

		private void AssertCellCount(UITable table, string context)
		{
			var expected = table.RowCount * table.ColumnCount;
			var actual = table.Query<TableCell>().ToList().Count;
			Assert(expected == actual,
				$"{context}: body cells expected {expected} ({table.RowCount} rows x {table.ColumnCount} cols), actual {actual}");
		}

		// Catches the column-clipping class of bug: if a row is narrower than the sum of its
		// cell widths, the trailing cells render past the row and get clipped by the viewport.
		private void AssertRowWidth(UITable table, string context)
		{
			var rows = table.Query<Row>().ToList();
			var ok = true;
			var detail = string.Empty;
			foreach (var row in rows)
			{
				var sum = 0f;
				foreach (var cell in row.Query<TableCell>().ToList())
					sum += cell.style.width.value.value;

				var rowWidth = row.style.width.value.value;
				if (Mathf.Abs(sum - rowWidth) > 0.5f)
				{
					ok = false;
					detail = $" (cells sum to {sum}, row width is {rowWidth} -> clipped)";
					break;
				}
			}

			Assert(ok, $"{context}: every row is wide enough for its cells{detail}");
		}

		private void AssertRowNumbers(UITable table, string context)
		{
			var cells = table.Query<RowHeaderCell>().ToList();
			var ok = true;
			for (var i = 0; i < cells.Count; i++)
			{
				var text = cells[i].Q<Label>()?.text;
				if (cells[i].RowIndex != i || text != (i + 1).ToString())
				{
					ok = false;
					break;
				}
			}

			Assert(ok, $"{context}: {cells.Count} row numbers sequential and labels match");
		}

		private void Assert(bool pass, string message)
		{
			Log($"[{(pass ? "PASS" : "FAIL")}] {message}", pass);
		}

		private void Log(string message, bool? pass = null)
		{
			var label = new Label(message);
			label.style.whiteSpace = WhiteSpace.Normal;
			if (pass.HasValue)
			{
				label.style.color = pass.Value
					? new Color(0.40f, 0.85f, 0.45f)
					: new Color(0.95f, 0.45f, 0.45f);
			}

			_log.Insert(0, label);
		}

		private static VisualElement Row()
		{
			var row = new VisualElement();
			row.style.flexDirection = FlexDirection.Row;
			return row;
		}

		private static VisualElement MultiLine(int lines)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < lines; i++) sb.AppendLine($"line {i + 1}");
			var label = new Label(sb.ToString().TrimEnd());
			label.style.whiteSpace = WhiteSpace.Normal;
			return label;
		}
	}
}
