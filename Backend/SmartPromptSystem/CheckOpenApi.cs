using System;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Microsoft.OpenApi");
        if (asm == null)
        {
            try {
                asm = Assembly.Load("Microsoft.OpenApi");
            } catch { }
        }
        
        if (asm != null)
        {
            var types = asm.GetTypes().Where(t => t.Name.Contains("OpenApiInfo") || t.Name.Contains("SecurityScheme")).Select(t => t.FullName);
            foreach(var t in types) {
                Console.WriteLine(t);
            }
        }
        else
        {
            Console.WriteLine("Microsoft.OpenApi not found");
        }
    }
}
