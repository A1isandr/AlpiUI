using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;

// ReSharper disable CheckNamespace
namespace AlpiUI.Controls;

/// <summary>
/// Control that displays a tab strip with the ability to create new or close tabs.
/// Work in progress. 
/// </summary>
public class TabView : SelectingItemsControl
{
    private static readonly Grid GridPanel = new();
    
    private static readonly FuncTemplate<Panel?> GridPanelTemplate =
        new(() => GridPanel);
    
    /// <summary>
    /// TabView constructor.
    /// </summary>
    public TabView()
    {
        // Will work only with grid as panel.
        ItemsPanelProperty.OverrideDefaultValue<TabView>(GridPanelTemplate);
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<TabView>(KeyboardNavigationMode.Once);
    }
    
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var tabBoxItem = new TabViewItem();
        
        AddNewColumnToGrid();
        SetColumnPropertyToTab(tabBoxItem);
        
        return tabBoxItem;
    }
    
    /// <inheritdoc/>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        if (container is TabViewItem tabViewItem)
        {
            AddNewColumnToGrid();
            SetColumnPropertyToTab(tabViewItem);
        }
        
        base.ContainerForItemPreparedOverride(container, item, index);
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TabViewItem>(item, out recycleKey);
    }

    private static void AddNewColumnToGrid()
    {
        GridPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
    }

    private static void SetColumnPropertyToTab(TabViewItem item)
    {
        item.SetValue(Grid.ColumnProperty, GridPanel.ColumnDefinitions.Count - 1);
    }
    
    internal bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        return UpdateSelectionFromEventSource(
            source,
            rightButton: e.GetCurrentPoint(source).Properties.IsRightButtonPressed);
    }
}