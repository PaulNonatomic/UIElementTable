# Change Log

## [Unreleased]
- Added EditMode tests covering row and column add/remove, reindexing, striping, and cell rebinding

## [0.6.0] - Jun 21, 2026
- Refactored the table internals for single responsibility: Row now owns its cells, number cell, and index, and highlighting and row-height sizing moved into dedicated TableHighlighter and RowHeightController classes
- Replaced the parallel cell, row, and row-number lists with a single row list, removing the indexing fragility behind the 0.5.0 row and column fixes
- Expanded the public Row API (Index, NumberCell, Cells, CellCount, GetCell, AddCell, RemoveCellAt, SetIndex)

## [0.5.0] - Jun 20, 2026
- Added SetFillWidth to stretch the table to its container width
- Added a UITable Examples sample: an editor validation harness, runtime and world-space sample scenes, and Light/Ocean theme examples
- Tables now size to their columns by default so the header bar and body align (call SetFillWidth(true) for the previous fill-width behaviour)
- Fixed RemoveRow removing rows from the wrong scroll view, which left data rows on screen and made data-bound refresh leak and duplicate rows
- Fixed RemoveRow not reindexing and relabelling the remaining rows
- Fixed AddColumn assigning the wrong column index to body cells
- Fixed AddColumn not widening rows, which clipped the new column
- Fixed SetColumn not resizing body cells to match the header width
- Fixed column header hover throwing when the table had no rows
- Fixed body cell text colour so cell text renders correctly in runtime and world-space panels
- Fixed the malformed runtime assembly name in the asmdef

## [0.4.1] - Dec 27, 2024
- Fix for the class scoped content area being set after an attempt to create rows that uses the class scoped content area.

## [0.4.0] - Dec 27, 2024
- Added support for custom cell content with data binding

## [0.3.2] - Nov 29, 2024
- Abstracted classes from VisualElements created in the UITable

## [0.3.1] - Nov 27, 2024
- Added support for press styling on table cells
- Separated header cells into their own derived classes for column, row and corner
- Fix for scroll wheel effect on row numbers and column headers offsetting the scroll positions

## [0.3.0] - Nov 26, 2024
- Simplified UITable API

## [0.2.0] - Nov 22, 2024
- Added support for custom styling

## [0.1.0] - Nov 21, 2024
- Added support for optional row numbers
- Added support for flexible row number column width
- Added cell, row, column and table rollover highlights
- Added support for custom cell content
- Added support for defined column labels and widths

## [0.0.0-beta] - Nov 18, 2024
- First commit