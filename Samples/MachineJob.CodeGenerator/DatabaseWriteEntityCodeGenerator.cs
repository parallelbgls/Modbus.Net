using Microsoft.CodeAnalysis;

namespace Modbus.Net.CodeGenerator
{
    [Generator]
    public class DatabaseWriteEntityCodeGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var content = "";
            for (int i = 1; i <= 10; i++)
            {
                content += $@"public double? Value{i} {{ get; set; }}
        ";
            }
            var source = $@"

namespace MachineJob
{{
    public partial class DatabaseWriteEntity
    {{
        {content}
    }}
}}";
            context.AddSource("DatabaseWriteContent.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }
}
