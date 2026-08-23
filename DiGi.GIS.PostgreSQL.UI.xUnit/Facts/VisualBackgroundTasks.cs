using DiGi.GIS.PostgreSQL.Classes;
using DiGi.GIS.PostgreSQL.UI.Enums;
using DiGi.GIS.WebAPI.Classes;
using DiGi.UI.WPF.Interfaces;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL.UI.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests that <see cref="Create.VisualBackgroundTasks(GISPostgreSQLConverterManager?, GISWebAPIManager?, Mode)"/> handles null managers without throwing exceptions, ensuring that a missing WebAPI configuration does not crash the server task list.
        /// </summary>
        [Fact]
        public void VisualBackgroundTasks()
        {
            // Null converter manager and null WebAPI manager should return empty list across all modes
            List<IVisualBackgroundTask>? visualBackgroundTasks_NullManagers_Server = Create.VisualBackgroundTasks(null, null, Mode.Server);
            Assert.NotNull(visualBackgroundTasks_NullManagers_Server);
            Assert.Empty(visualBackgroundTasks_NullManagers_Server);

            List<IVisualBackgroundTask>? visualBackgroundTasks_NullManagers_Client = Create.VisualBackgroundTasks(null, null, Mode.Client);
            Assert.NotNull(visualBackgroundTasks_NullManagers_Client);
            Assert.Empty(visualBackgroundTasks_NullManagers_Client);

            List<IVisualBackgroundTask>? visualBackgroundTasks_NullManagers_Both = Create.VisualBackgroundTasks(null, null, Mode.ServerAndCient);
            Assert.NotNull(visualBackgroundTasks_NullManagers_Both);
            Assert.Empty(visualBackgroundTasks_NullManagers_Both);

            // With converter manager but null WebAPI manager (the issue #1 scenario)
            GISPostgreSQLConverterManager gISPostgreSQLConverterManager = new();

            List<IVisualBackgroundTask>? visualBackgroundTasks_Server = Create.VisualBackgroundTasks(gISPostgreSQLConverterManager, null, Mode.Server);
            Assert.NotNull(visualBackgroundTasks_Server);
            Assert.NotEmpty(visualBackgroundTasks_Server);

            // OrtoDatasTask requires GISWebAPIManager and must be excluded when it is null
            Assert.DoesNotContain(visualBackgroundTasks_Server, x => x.TypeName == typeof(OrtoDatasTask).Name);

            // Client mode with null WebAPI manager should return empty list
            List<IVisualBackgroundTask>? visualBackgroundTasks_Client = Create.VisualBackgroundTasks(gISPostgreSQLConverterManager, null, Mode.Client);
            Assert.NotNull(visualBackgroundTasks_Client);
            Assert.Empty(visualBackgroundTasks_Client);
        }
    }
}
