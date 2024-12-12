using Microsoft.Win32.SafeHandles;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ExceptionGraphVisualizer;

public class GraphvizImageParameters
{
    public double Width { get; }
    public double Height { get; }

    public GraphvizImageParameters(double width, double height)
    {
        Width = width;
        Height = height;
    }
}

internal static class GraphVizInterop
{
    private sealed class SafeGraphHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeGraphHandle() : base(true) { }
        public SafeGraphHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle) { SetHandle(handle); }
        protected override bool ReleaseHandle() { agclose(handle); return true; }
    }

    private sealed class SafeContextHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeContextHandle() : base(true) { }
        public SafeContextHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle) { SetHandle(handle); }
        protected override bool ReleaseHandle() { gvFreeContext(handle); return true; }
    }

    private sealed class SafeRenderDataHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeRenderDataHandle() : base(true) { }
        public SafeRenderDataHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle) { SetHandle(handle); }
        protected override bool ReleaseHandle() { gvFreeRenderData(handle); return true; }
    }

    // agmemread accepts char* that is null-teminated UTF-8 string
    [DllImport("cgraph.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern SafeGraphHandle agmemread(byte[] graphVizData);

    [DllImport("cgraph.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void agclose(IntPtr file);

    [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern SafeContextHandle gvContext();

    [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int gvFreeLayout(SafeContextHandle context, SafeGraphHandle graph);

    [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int gvLayout(SafeContextHandle context, SafeGraphHandle graph, string engine);

    [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int gvRenderData(SafeContextHandle context, SafeGraphHandle graph, string format, out SafeRenderDataHandle result, out uint length);

    [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gvFreeRenderData(IntPtr buffer);

    [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int gvFreeContext(IntPtr gvc);

    [DllImport("cgraph.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr agget(SafeGraphHandle obj, string name);

    [DllImport("cgraph.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern string aglasterr();

    public static (string, GraphvizImageParameters) Process(string graphDescriptionInDotLanguage, string layoutEngine, string outputFormat)
    {
        var thisDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var graphVizBinariesDir = Path.Combine(thisDllDir, "ExceptionGraphVisualizer_GraphViz_Binaries");
        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + graphVizBinariesDir);

        using var graph = agmemread(ToNullTerminatedUtf8String(graphDescriptionInDotLanguage));
        if (graph.IsInvalid)
            throw new ArgumentException($"Unable to read the given graph description\naglasterr: {aglasterr()}\nGraph description:\n{graphDescriptionInDotLanguage}");

        using var context = gvContext();

        if (gvLayout(context, graph, layoutEngine) != 0)
            throw new ArgumentException($"Unable to create the gvContext using the layoutEngine={layoutEngine}\naglasterr: {aglasterr()}");

        if (gvRenderData(context, graph, outputFormat, out var renderStringBuffer, out _) != 0)
            throw new ArgumentException($"Unable to generate {outputFormat}\naglasterr: {aglasterr()}\nGraph description:\n{graphDescriptionInDotLanguage}");

        var imageParameters = GetImageParameters(graph);

        return (FromNullTerminatedUtf8String(renderStringBuffer.DangerousGetHandle()), imageParameters);
    }

    private static GraphvizImageParameters GetImageParameters(SafeGraphHandle graph)
    {
        // Get bounding box to calculate recommended image Width and Height. 
        // Bounding box is a string like "0 0 1920 473"
        IntPtr bbPtr = agget(graph, "bb");
        if (bbPtr == IntPtr.Zero)
            throw new InvalidOperationException("Unable to get bounding box");
        var coordinates = Marshal.PtrToStringAnsi(bbPtr).Split(' ');
        return new GraphvizImageParameters(double.Parse(coordinates[2], CultureInfo.InvariantCulture), double.Parse(coordinates[3], CultureInfo.InvariantCulture));
    }

    private static byte[] ToNullTerminatedUtf8String(string str) => Encoding.UTF8.GetBytes(str + '\0');

    private static string FromNullTerminatedUtf8String(IntPtr str)
    {
        var length = 0;
        while (Marshal.ReadByte(str, length) != 0)
            length++;
        var buffer = new byte[length];
        Marshal.Copy(str, buffer, 0, length);
        return Encoding.UTF8.GetString(buffer);
    }
}
