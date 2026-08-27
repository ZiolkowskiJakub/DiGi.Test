using DiGi.Serilog.Classes;
using Serilog.Core;
using System.Reflection;

namespace DiGi.Serilog.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// The explicit Directory override wins over per-assembly routing and the application's base directory.
        /// </summary>
        [Fact]
        public void ResolveBaseDirectory_DirectorySet_WinsOverEverything()
        {
            string directory = Path.Combine(Path.GetTempPath(), "DiGi.Serilog.xUnit", Guid.NewGuid().ToString("N"));
            string assemblyLocation = Path.Combine(Path.GetTempPath(), "elsewhere", "Some.dll");

            Assert.Equal(directory, LoggerManager.ResolveBaseDirectory(directory, true, assemblyLocation));
            Assert.Equal(directory, LoggerManager.ResolveBaseDirectory(directory, false, assemblyLocation));
        }

        /// <summary>
        /// With per-assembly routing enabled and a resolvable location, the assembly's own directory is used,
        /// which is how a modular host keeps one logs folder per extension sub-folder.
        /// </summary>
        [Fact]
        public void ResolveBaseDirectory_RoutePerAssembly_UsesAssemblyDirectory()
        {
            string assemblyLocation = Path.Combine(Path.GetTempPath(), "extensions", "gis", "DiGi.GIS.WebAPI.dll");

            Assert.Equal(Path.Combine(Path.GetTempPath(), "extensions", "gis"), LoggerManager.ResolveBaseDirectory(null, true, assemblyLocation));
        }

        /// <summary>
        /// A single-file bundled assembly reports no location, so per-assembly routing must fall back to the
        /// directory the application was launched from instead of silently disabling logging.
        /// </summary>
        [Fact]
        public void ResolveBaseDirectory_RoutePerAssembly_EmptyLocation_FallsBackToApplicationBase()
        {
            Assert.Equal(AppContext.BaseDirectory, LoggerManager.ResolveBaseDirectory(null, true, string.Empty));
            Assert.Equal(AppContext.BaseDirectory, LoggerManager.ResolveBaseDirectory(null, true, null));
        }

        /// <summary>
        /// By default the application's base directory is used even when the requesting assembly sits elsewhere,
        /// so one application writes one log no matter where its assemblies were deployed.
        /// </summary>
        [Fact]
        public void ResolveBaseDirectory_Default_UsesApplicationBase()
        {
            string assemblyLocation = Path.Combine(Path.GetTempPath(), "extensions", "gis", "DiGi.GIS.WebAPI.dll");

            Assert.Equal(AppContext.BaseDirectory, LoggerManager.ResolveBaseDirectory(null, false, assemblyLocation));
        }

        /// <summary>
        /// A null assembly never resolves to a logger.
        /// </summary>
        [Fact]
        public void GetLogger_NullAssembly_ReturnsNull()
        {
            LoggerManager loggerManager = new();

            Assert.Null(loggerManager.GetLogger(null));
        }

        /// <summary>
        /// Repeated requests for the same logger resolve to the same cached instance.
        /// </summary>
        [Fact]
        public void GetLogger_RepeatRequests_ReturnSameCachedInstance()
        {
            string baseDirectory = Path.Combine(Path.GetTempPath(), "DiGi.Serilog.xUnit", Guid.NewGuid().ToString("N"));

            try
            {
                LoggerManager loggerManager = new() { Directory = baseDirectory };
                Logger? first = loggerManager.GetLogger(Assembly.GetExecutingAssembly());
                Logger? second = loggerManager.GetLogger(Assembly.GetExecutingAssembly());

                Assert.NotNull(first);
                Assert.Same(first, second);
            }
            finally
            {
                if (Directory.Exists(baseDirectory))
                {
                    Directory.Delete(baseDirectory, true);
                }
            }
        }
    }
}
