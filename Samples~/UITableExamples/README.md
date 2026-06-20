# UITable examples

Examples for the `com.nonatomic.uitable` package: an editor validation harness, a runtime
sample scene, and a world-space sample scene.

The runtime and world-space scenes use UI Toolkit runtime input, so they need the Input System
package (`com.unity.inputsystem`) with **Active Input Handling** set to Input System or Both. The
editor harness has no such requirement.

## Editor validation harness

`Editor/UITableExampleWindow.cs` is an EditorWindow that builds a fresh table per scenario, runs
PASS/FAIL assertions, and shows a live event log. Open it from
**Window > UI Table Examples > Validation Harness**.

Scenarios:
- **Basic** - 3x4 table populated with `SetCell`.
- **Data binding** - the `DataBindingUITable<T>` flow with re-bind buttons; the cell-count
  assertion guards against rows leaking on `SetData`.
- **Row ops** - remove a middle row / add a row; asserts cell count and that row numbers stay
  sequential and indices stay correct.
- **Column ops** - add a column (filled or blank), resize a column, remove a column. Checks the
  new cell's column index, that body cells track the header width, and that each row stays wide
  enough for its cells. The blank-column button verifies empty cells still render as styled boxes.
- **Empty hover** - a table with columns and zero rows; hovering a header used to throw.
- **Flexible heights** - variable-height content with `SynchronizeRowHeights`.
- **Styling** - swap between the default look and the included Light / Ocean themes live, to show
  restyling via `SetCustomStyleSheet`. The themes are in `Runtime/Themes/` and only override the
  `.ui-table` USS variables.

`Person.cs` is the shared demo model, kept outside the `Editor/` folder so the runtime
samples can reuse it.

## Runtime sample

`Runtime/RuntimeUITableSample.cs` builds a data-bound table into a UIDocument at runtime, with
Add / Remove / Reset controls and a click-status line.

Open the included `Runtime/UITableSampleScene.unity` and press Play. To regenerate the scene and
its PanelSettings from scratch, run **Window > UI Table Examples > Create Runtime Sample Scene**.

## World-space sample

The same table rendered in 3D. Open `Runtime/UITableWorldSpaceScene.unity` and press Play, or
regenerate it with **Window > UI Table Examples > Create World Space Sample Scene** (it sets up a
world-space PanelSettings, a `PanelInputConfiguration` for clicks, and a camera framing the panel).

World-space placement is visual, so expect to adjust: rotate `World UI Table` 180 on Y if it faces
away, and tune its position/scale, the UIDocument fixed size, or PanelSettings > Pixels Per Unit
(default 100 = 100px per world unit).
