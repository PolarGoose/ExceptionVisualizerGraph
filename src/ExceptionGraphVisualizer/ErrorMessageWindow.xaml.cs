using System.Windows;

namespace ExceptionGraphVisualizer;

public partial class ErrorMessageWindow : Window
{
    public ErrorMessageWindow(string errorMeessage)
    {
        InitializeComponent();
        ErrorMessageTextBox.Text = errorMeessage;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
