using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace ExceptionGraphVisualizer;

public partial class ExceptionVisualizerWindow
{
    public ExceptionVisualizerWindow()
    {
        InitializeComponent();
    }

    public void LoadSvg(string svg, GraphvizImageParameters parameters)
    {
        SvgImage.SvgSource = svg;

    }

    private void ZoomInButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        SvgScale.ScaleX *= 1.1;
        SvgScale.ScaleY *= 1.1;
    }

    private void ZoomOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        SvgScale.ScaleX /= 1.1;
        SvgScale.ScaleY /= 1.1;
    }

    private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Check if Ctrl key is pressed
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (e.Delta > 0) // Mouse scrolled up
            {
                ZoomInButton_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Delta < 0) // Mouse scrolled down
            {
                ZoomOutButton_Click(sender, e);
                e.Handled = true;
            }
        }
    }

    private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "SVG Files (*.svg)|*.svg|All Files (*.*)|*.*",
            DefaultExt = ".svg",
            FileName = "diagram.svg"
        };

        var result = saveDialog.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            string filename = saveDialog.FileName;
            File.WriteAllText(filename, SvgImage.SvgSource, Encoding.UTF8);
        }
    }
}
