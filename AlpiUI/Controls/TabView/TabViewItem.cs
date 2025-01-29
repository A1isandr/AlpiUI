using Avalonia.Controls;
using Avalonia.Input;

// ReSharper disable CheckNamespace
namespace AlpiUI.Controls;

/// <summary>
/// An item in a <see cref="TabView"/>. 
/// </summary>
public class TabViewItem : ListBoxItem
{
    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.Handled && ItemsControl.ItemsControlFromItemContainer(this) is TabView owner)
        {
            var pointer = e.GetCurrentPoint(e.Source as Control);

            switch (pointer.Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.RightButtonPressed:
                    e.Handled = true;
                    break;
                case PointerUpdateKind.LeftButtonPressed:
                {
                    if (pointer.Pointer.Type == PointerType.Mouse)
                    {
                        e.Handled = owner.UpdateSelectionFromPointerEvent(this, e);
                    }

                    break;
                }
                case PointerUpdateKind.MiddleButtonPressed:
                case PointerUpdateKind.XButton1Pressed:
                case PointerUpdateKind.XButton2Pressed:
                case PointerUpdateKind.LeftButtonReleased:
                case PointerUpdateKind.MiddleButtonReleased:
                case PointerUpdateKind.RightButtonReleased:
                case PointerUpdateKind.XButton1Released:
                case PointerUpdateKind.XButton2Released:
                case PointerUpdateKind.Other:
                    break;
                default:
                    return;
            }
        }

        base.OnPointerPressed(e);
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var pointer = e.GetCurrentPoint(e.Source as Control);

        if (pointer.Properties.PointerUpdateKind is PointerUpdateKind.RightButtonReleased)
        {
            e.Handled = true;
            return;
        }

        base.OnPointerReleased(e);
    }
}