using DiGi.Scripting.Classes;

namespace DiGi.Scripting.CSharp.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void Script()
        {
            List<VariableType> inputVariableTypes = [new VariableType("a", typeof(double))];
            List<VariableType> outputVariableTypes = [new VariableType("x", typeof(double))];

            Code code = new Code("int x = [a] + 2;");

            CSharp.Classes.Script script = new CSharp.Classes.Script(code, inputVariableTypes, outputVariableTypes);
            HashSet<string> imports = ["System", "System.Collections.Generic", "System.Linq"];
            HashSet<string> references = ["System.Runtime", "Microsoft.CSharp", "System.Dynamic.Runtime", "System.Collections"];

            script.Imports = imports;
            script.References = references;
            Dictionary<string, object> dictionary = [];
            dictionary["a"] = 10;
            
            Response? response = script.Execute(new Data(dictionary));

            Assert.NotNull(response);

            Assert.Null(response.Exception);

            Assert.True(response.Succeeded);

        }
    }
}