# UIElement Table

## Overview
UIElement Table is a Unity package that provides a way to draw tables with UIElements.

## Installation
To install UIElement Table into your Unity project, add the package from the git URL: https://github.com/PaulNonatomic/UIElementTable.git using the Unity package manager.

## Usage
```csharp
// Create a new UITable with the default constructor
var table = new UITable();
table.ShowRowNumbers(new ColumnDefinition("#", width: 50f));
table.SetColumn(0, new ColumnDefinition("Name", 150f));
table.SetColumn(1, new ColumnDefinition("Age", 75f));
table.SetColumn(2, new ColumnDefinition("Country"));
table.SetCustomStyleSheet(Resources.Load<StyleSheet>("CustomTable"));
rootVisualElement.Add(table);

// Add some rows with content
AddPerson("Alice", "30", "USA");
AddPerson("Bob", "25", "Canada");
AddPerson("Charlie", "35", "UK");

// Cell content is provided as a dictionary of column index to VisualElement
private void AddPerson(string name, string age, string country)
{
    var cellContents = new Dictionary<int, VisualElement>
    {
        { 0, new Label(name) },
        { 1, new Label(age) },
        { 2, new Label(country) }
    };

    _table.AddRow(cellContents);
}
```

https://github.com/user-attachments/assets/ffc5bd06-1fdf-47f2-bb9f-8f13654c4ff3

