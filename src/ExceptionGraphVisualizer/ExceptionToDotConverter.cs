using System.Diagnostics;
using System.Text;

namespace ExceptionGraphVisualizer;

internal static class ExceptionToDotConverter
{
    public static string ToDot(Exception ex)
    {
        var dot = new StringBuilder();
        dot.AppendLine("""
digraph ExceptionGraph {
  node [fontname="Consolas", fontsize=12];
""");

        ex = ex.Demystify();

        int id = 0;
        AddException(dot, ex, ref id);

        dot.AppendLine("}");

        return dot.ToString();
    }

    private static void AddException(StringBuilder dot, Exception ex, ref int nodeId)
    {
        dot.AppendLine($"""
  a{nodeId} [shape=none margin=0 label=<
    <TABLE border="0" cellborder="1" cellspacing="0">
      <TR><TD><b>{Escape(ex.GetType())}</b></TD></TR>
      <TR><TD ALIGN="LEFT" BALIGN="LEFT">{Escape(ex.Message)}</TD></TR>
      <TR><TD ALIGN="LEFT" BALIGN="LEFT">{FormatStackTrace(ex.StackTrace)}</TD></TR>
    </TABLE>
  >];
""");

        if(ex is AggregateException aggregate)
        {
            var currentNodeId = nodeId;
            foreach (var inner in aggregate.InnerExceptions)
            {
                nodeId++;
                dot.AppendLine($"a{currentNodeId} -> a{nodeId}");
                AddException(dot, inner, ref nodeId);
            }
        }
        else if (ex.InnerException != null)
        {
            nodeId++;
            dot.AppendLine($"a{nodeId - 1} -> a{nodeId}");
            AddException(dot, ex.InnerException, ref nodeId);
        }
    }

    private static string Escape<T>(T obj)
    {
        if(obj == null)
        {
            return "";
        }

        var text = obj.ToString();
        text = text.Replace("&", "&amp;");
        text = text.Replace("<", "&lt;");
        text = text.Replace(">", "&gt;");
        text = text.Replace("\r\n", "<BR/>");
        text = text.Replace("\n", "<BR/>");

        return text;
    }

    private static string FormatStackTrace(string stackTrace)
    {
        var escaped = Escape(stackTrace);
        escaped = escaped.Replace("   ", "");
        return escaped;
    }
}
