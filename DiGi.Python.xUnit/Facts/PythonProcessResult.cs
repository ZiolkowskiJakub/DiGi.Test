using System.Collections.Generic;
using System.Linq;

namespace DiGi.Python.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Classes.PythonProcessResult"/> keeps the values given to it, survives a string round trip and <c>SerializationCheck</c>, and that its copy constructor copies every member, for a completed run and for a stalled run with no captured output.
        /// </summary>
        [Fact]
        public void PythonProcessResult()
        {
            Classes.PythonProcessResult pythonProcessResult = new(3, ["first", "second"], ["warning"], false, true);

            Assert.Equal(3, pythonProcessResult.ExitCode);
            Assert.Equal(["first", "second"], pythonProcessResult.StandardOutput!);
            Assert.Equal(["warning"], pythonProcessResult.StandardError!);
            Assert.False(pythonProcessResult.Stalled);
            Assert.True(pythonProcessResult.Cancelled);

            string? json = Core.Convert.ToSystem_String(pythonProcessResult);
            Assert.False(string.IsNullOrWhiteSpace(json));

            List<Classes.PythonProcessResult>? pythonProcessResults = Core.Convert.ToDiGi<Classes.PythonProcessResult>(json);
            Classes.PythonProcessResult? pythonProcessResult_RoundTrip = pythonProcessResults?.FirstOrDefault();
            Assert.NotNull(pythonProcessResult_RoundTrip);
            Assert.Equal(3, pythonProcessResult_RoundTrip!.ExitCode);
            Assert.Equal(["first", "second"], pythonProcessResult_RoundTrip.StandardOutput!);
            Assert.Equal(["warning"], pythonProcessResult_RoundTrip.StandardError!);
            Assert.False(pythonProcessResult_RoundTrip.Stalled);
            Assert.True(pythonProcessResult_RoundTrip.Cancelled);

            Core.xUnit.Query.SerializationCheck(pythonProcessResult);

            Classes.PythonProcessResult pythonProcessResult_Copy = new(pythonProcessResult);
            Assert.Equal(pythonProcessResult.ExitCode, pythonProcessResult_Copy.ExitCode);
            Assert.Equal(pythonProcessResult.StandardOutput!, pythonProcessResult_Copy.StandardOutput!);
            Assert.NotSame(pythonProcessResult.StandardOutput, pythonProcessResult_Copy.StandardOutput);
            Assert.Equal(pythonProcessResult.StandardError!, pythonProcessResult_Copy.StandardError!);
            Assert.NotSame(pythonProcessResult.StandardError, pythonProcessResult_Copy.StandardError);
            Assert.Equal(pythonProcessResult.Stalled, pythonProcessResult_Copy.Stalled);
            Assert.Equal(pythonProcessResult.Cancelled, pythonProcessResult_Copy.Cancelled);

            //A stalled run without captured output
            Classes.PythonProcessResult pythonProcessResult_Stalled = new(-1, null, null, true, false);
            Assert.Null(pythonProcessResult_Stalled.StandardOutput);
            Assert.Null(pythonProcessResult_Stalled.StandardError);
            Core.xUnit.Query.SerializationCheck(pythonProcessResult_Stalled);

            Classes.PythonProcessResult pythonProcessResult_StalledCopy = new(pythonProcessResult_Stalled);
            Assert.Equal(-1, pythonProcessResult_StalledCopy.ExitCode);
            Assert.Null(pythonProcessResult_StalledCopy.StandardOutput);
            Assert.Null(pythonProcessResult_StalledCopy.StandardError);
            Assert.True(pythonProcessResult_StalledCopy.Stalled);
            Assert.False(pythonProcessResult_StalledCopy.Cancelled);
        }
    }
}
