using AlpiUI.Controls;
using AlpiUI.Demo.ViewModels;

namespace AlpiUI.Demo.Views;

public partial class MainWindow : AlpiWindow
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
    }
}