using System;
using System.IO;

namespace DiGi.Laya.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that <see cref="Query.LayaModelPath(string?)"/> resolves a checkpoint directory - one holding rl_agent_config.json - given absolutely (with or without a trailing separator), relative to the application base directory, and with a leading "user files" segment that CopyUserFiles flattens away, to the same absolute path without a trailing separator; and that it refuses everything laya would otherwise resolve or download: a directory without rl_agent_config.json, the configuration file itself, a bare model name, a missing directory, and a blank value.
        /// </summary>
        [Fact]
        public void Query_LayaModelPath()
        {
            string name = string.Concat("Laya_ModelPath_", Guid.NewGuid().ToString("N"));
            string directory_Temporary = Path.Combine(Path.GetTempPath(), name);
            string directory_Base = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);

            try
            {
                string directory_Checkpoint = Path.Combine(directory_Temporary, "checkpoint");
                Directory.CreateDirectory(directory_Checkpoint);
                File.WriteAllText(Path.Combine(directory_Checkpoint, "rl_agent_config.json"), "{}");

                string directory_Empty = Path.Combine(directory_Temporary, "empty");
                Directory.CreateDirectory(directory_Empty);

                string expected = Path.GetFullPath(directory_Checkpoint);
                Assert.Equal(expected, Query.LayaModelPath(directory_Checkpoint));
                Assert.Equal(expected, Query.LayaModelPath(string.Concat(directory_Checkpoint, Path.DirectorySeparatorChar)));

                Directory.CreateDirectory(directory_Base);
                File.WriteAllText(Path.Combine(directory_Base, "rl_agent_config.json"), "{}");
                string expected_Base = Path.GetFullPath(directory_Base);
                Assert.Equal(expected_Base, Query.LayaModelPath(name));
                Assert.Equal(expected_Base, Query.LayaModelPath(string.Concat("user files/", name)));
                Assert.Equal(expected_Base, Query.LayaModelPath(string.Concat("user files\\", name)));

                Assert.Null(Query.LayaModelPath(directory_Empty));
                Assert.Null(Query.LayaModelPath(Path.Combine(directory_Checkpoint, "rl_agent_config.json")));
                Assert.Null(Query.LayaModelPath(Path.Combine(directory_Temporary, "missing")));
                Assert.Null(Query.LayaModelPath("multilingual"));
                Assert.Null(Query.LayaModelPath("convaiinnovations/laya"));
                Assert.Null(Query.LayaModelPath(" "));
                Assert.Null(Query.LayaModelPath(null));
            }
            finally
            {
                foreach (string directory in new string[] { directory_Temporary, directory_Base })
                {
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }
            }
        }
    }
}
