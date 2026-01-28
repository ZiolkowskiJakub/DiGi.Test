using DiGi.Core.Enums;
using DiGi.Core.Interfaces;

namespace DiGi.Core.xUnit
{
    public partial class Classes
    {
        [Fact]
        public void ValuesFile()
        {
            string path = Path.GetTempFileName();

            List<ISerializableObject> values_1 = [];
            List<ISerializableObject>? values_2 = [];

            Core.Classes.Address address_1 = new("Street 1", "City 1", "Code 1", CountryCode.PL);
            values_1.Add(address_1);

            Core.Classes.Address address_2 = new("Street 2", "City 2", "Code 2", CountryCode.PL);
            values_1.Add(address_2);

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (IO.File.Classes.ValuesFile valuesFile = new(path))
                {
                    Assert.NotNull(valuesFile);

                    if (valuesFile != null)
                    {
                        valuesFile.Open();
                        valuesFile.Values = values_1;
                        valuesFile.Save();
                    }
                }

                using (IO.File.Classes.ValuesFile valuesFile = new(path))
                {
                    Assert.NotNull(valuesFile);

                    if (valuesFile != null)
                    {
                        valuesFile.Open();

                        values_2 = valuesFile.Values?.FilterNulls();
                    }
                }
            }
            catch
            {
            }

            Assert.NotNull(values_1);
            Assert.NotNull(values_2);

            if (values_1 is not null && values_2 is not null)
            {
                Assert.Equal(values_1.Count, values_2.Count);
                if (values_1.Count == values_2.Count)
                {
                    List<string?> jsons_1 = values_1.ConvertAll(x => x.ToSystem_String());
                    List<string?> jsons_2 = values_2.ConvertAll(x => x.ToSystem_String());

                    int index_1 = jsons_1.FindIndex(string.IsNullOrEmpty);
                    int index_2 = jsons_2.FindIndex(string.IsNullOrEmpty);
                    Assert.Equal(index_1, index_2);
                    Assert.Equal(-1, index_1);

                    if (index_1 == -1 && index_1 == index_2)
                    {
                        List<string?> district = [.. jsons_1];

                        foreach (string? json in district)
                        {
                            jsons_1.Remove(json);
                            jsons_2.Remove(json);
                        }

                        Assert.Empty(jsons_1);
                        Assert.Equal(jsons_2.Count, jsons_1.Count);
                    }
                }
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}