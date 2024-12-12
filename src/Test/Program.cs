using ExceptionGraphVisualizer;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Test;

class Program
{
    [STAThread]
    static void Main()
    {
        var testException = TestExceptionGenerator.Generate();
        var visualizerHost = new VisualizerDevelopmentHost(testException, typeof(ExceptionDialogDebuggerVisualizer), typeof(ExceptionVisualizerObjectSource));
        visualizerHost.ShowVisualizer();
    }
}
