using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaubPlusCo.Foundation.HelixTemplating.Services
{
  public class WriteTraceService
  {
    public static void WriteToTrace(string message, string category = "Info", params string[] values)
    {
      Trace.WriteLine(message, category);
      if (values == null || !values.Any()) return;
      foreach (var value in values) Trace.WriteLine($"\t{value}");
    }
  }
}
