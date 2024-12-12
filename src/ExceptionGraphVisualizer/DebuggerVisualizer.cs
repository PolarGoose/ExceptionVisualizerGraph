using ExceptionGraphVisualizer;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

[assembly: DebuggerVisualizer(
    visualizer: typeof(ExceptionDialogDebuggerVisualizer),
    visualizerObjectSource: typeof(ExceptionVisualizerObjectSource),
    Target = typeof(Exception),
    Description = "Exception Graph Visualizer")]

namespace ExceptionGraphVisualizer;

[DataContract]
public class DataFromDebuggeeToDebugger
{
    [DataMember]
    public string DotGraphDescription { get; set; } = "";

    [DataMember]
    public string ErrorMessage { get; set; } = "";
}

// This class is loaded into the 3rd party (client) process that is being debugged.
// It shouldn't depend on any external libraries.
// However, currently it depends on `Ben.Demystifier` nuget package. It is acceptable and doesn't seem to cause any issues.
// `GetData` method gets access to the real Exception object without any serialization in between.
public class ExceptionVisualizerObjectSource : VisualizerObjectSource
{
    public override void GetData(object target, Stream outgoingData)
    {
        var serializer = new DataContractSerializer(typeof(DataFromDebuggeeToDebugger));
        try
        {
            serializer.WriteObject(outgoingData, new DataFromDebuggeeToDebugger { DotGraphDescription = ExceptionToDotConverter.ToDot((Exception)target) });
        }
        catch (Exception ex)
        {
            serializer.WriteObject(outgoingData, new DataFromDebuggeeToDebugger
            {
                ErrorMessage = $"Failed to generate the graph from the exception:\n{target as Exception}\n\nError:\n{ex}"
            });
        }
    }
}

// This class is loaded into the debugger process.
public class ExceptionDialogDebuggerVisualizer : DialogDebuggerVisualizer
{
    public ExceptionDialogDebuggerVisualizer() : base(FormatterPolicy.Json) { }

    protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
    {
        using var stream = objectProvider.GetData();
        var serializer = new DataContractSerializer(typeof(DataFromDebuggeeToDebugger));
        var data = (DataFromDebuggeeToDebugger)serializer.ReadObject(stream);

        if (data.ErrorMessage != string.Empty)
        {
            new ErrorMessageWindow(data.ErrorMessage).ShowDialog();
            return;
        }

        try
        {
            var (svg, imageParameters) = GraphVizInterop.Process(data.DotGraphDescription, "dot", "svg");

            var wpfWindow = new ExceptionVisualizerWindow();
            wpfWindow.LoadSvg(svg, imageParameters);
            wpfWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            new ErrorMessageWindow($"Failed to generate the graph from the dot description:\n{data.DotGraphDescription}\n\nError message:\n{ex}").ShowDialog();
        }
    }
}
