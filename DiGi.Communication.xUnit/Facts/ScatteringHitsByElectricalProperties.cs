using DiGi.Communication.Classes;
using DiGi.Communication.Interfaces;
using DiGi.Core.Classes;
using DiGi.Geometry.Spatial.Classes;
using System.Collections.Generic;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the extraction and grouping of scattering hits by electrical properties via <see cref="Query.ScatteringHitsByElectricalProperties{TScatteringHit}"/>.
        /// </summary>
        [Fact]
        public void ScatteringHitsByElectricalProperties()
        {
            ElectricalProperties electricalProperties_Concrete = Constants.ElectricalProperties.Concrete;
            ElectricalProperties electricalProperties_Custom = new("CustomMaterial", 3.0, 0.0, 0.01, 0.5, new Range<double>(1, 100));

            ScatteringObject scatteringObject_1 = new("Ref_Building", null, electricalProperties_Concrete);
            ScatteringObject scatteringObject_2 = new("Ref_Window", null, electricalProperties_Custom);

            GeometricalPropagationModel geometricalPropagationModel = new();
            Assert.True(geometricalPropagationModel.Update(scatteringObject_1));
            Assert.True(geometricalPropagationModel.Update(scatteringObject_2));

            Ray3D ray3D_1 = new(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
            Ray3D ray3D_2 = new(new Point3D(1, 1, 1), new Vector3D(0, 1, 0));

            ScatteringHit hit_1 = new("Ref_Building", ray3D_1);
            ScatteringHit hit_2 = new("Ref_Window", ray3D_2);

            SphericalDistributionScatteringHitCollection hitCollection = new();
            double azimuth = 0.5;
            double elevation = 1.0;

            hitCollection.AddValue(azimuth, elevation, hit_1);
            hitCollection.AddValue(azimuth, elevation, hit_2);

            AngularPowerDistribution angularPowerDistribution = new(1e-9, hitCollection);

            Dictionary<ElectricalProperties, List<ScatteringHit>>? result = geometricalPropagationModel.ScatteringHitsByElectricalProperties<ScatteringHit>(angularPowerDistribution, azimuth, elevation);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey(electricalProperties_Concrete));
            Assert.True(result.ContainsKey(electricalProperties_Custom));

            List<ScatteringHit> hits_Concrete = result[electricalProperties_Concrete];
            Assert.Single(hits_Concrete);
            Assert.Equal("Ref_Building", hits_Concrete[0].Reference);

            List<ScatteringHit> hits_Custom = result[electricalProperties_Custom];
            Assert.Single(hits_Custom);
            Assert.Equal("Ref_Window", hits_Custom[0].Reference);

            // Null and invalid inputs check
            GeometricalPropagationModel? nullModel = null;
            Assert.Null(nullModel!.ScatteringHitsByElectricalProperties<ScatteringHit>(angularPowerDistribution, azimuth, elevation));
            Assert.Null(geometricalPropagationModel.ScatteringHitsByElectricalProperties<ScatteringHit>(null, azimuth, elevation));
            Assert.Null(geometricalPropagationModel.ScatteringHitsByElectricalProperties<ScatteringHit>(angularPowerDistribution, double.NaN, elevation));
            Assert.Null(geometricalPropagationModel.ScatteringHitsByElectricalProperties<ScatteringHit>(angularPowerDistribution, azimuth, double.PositiveInfinity));
        }

        /// <summary>
        /// Tests <see cref="Query.ScatteringHitsByElectricalProperties{TScatteringHit}"/> for grouping multiple hits across distinct objects with equivalent electrical properties, de-duplication, and handling missing references.
        /// </summary>
        [Fact]
        public void ScatteringHitsByElectricalProperties_MultipleHits_GroupingAndDeduplication()
        {
            // Concrete property 1
            ElectricalProperties concrete_1 = Constants.ElectricalProperties.Concrete;
            // Distinct instance of Concrete with matching properties
            ElectricalProperties concrete_2 = new("Concrete", 5.31, 0.0, 0.0326, 0.8095, new Range<double>(1, 100));

            ScatteringObject obj1 = new("Ref_WallA", null, concrete_1);
            ScatteringObject obj2 = new("Ref_WallB", null, concrete_2);
            // Duplicate reference sharing same electrical property
            ScatteringObject obj3 = new("Ref_WallA", null, concrete_1);

            GeometricalPropagationModel model = new();
            Assert.True(model.Update(obj1));
            Assert.True(model.Update(obj2));
            Assert.True(model.Update(obj3));

            Ray3D ray1 = new(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
            Ray3D ray2 = new(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));
            Ray3D ray3 = new(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));

            ScatteringHit hit1 = new("Ref_WallA", ray1);
            ScatteringHit hit2 = new("Ref_WallB", ray2);
            ScatteringHit hit3_UnknownRef = new("Ref_Unknown", ray3);

            SphericalDistributionScatteringHitCollection hitCollection = new();
            double azimuth = 1.5;
            double elevation = 0.5;

            hitCollection.AddValue(azimuth, elevation, hit1);
            hitCollection.AddValue(azimuth, elevation, hit2);
            hitCollection.AddValue(azimuth, elevation, hit3_UnknownRef);

            AngularPowerDistribution apd = new(2e-9, hitCollection);

            Dictionary<ElectricalProperties, List<ScatteringHit>>? result = model.ScatteringHitsByElectricalProperties<ScatteringHit>(apd, azimuth, elevation);

            Assert.NotNull(result);
            // All objects use Concrete (equivalent properties), so they should group into exactly 1 dictionary key
            Assert.Single(result);

            KeyValuePair<ElectricalProperties, List<ScatteringHit>> pair = Assert.Single(result);
            Assert.Equal("Concrete", pair.Key.Name);

            List<ScatteringHit> groupedHits = pair.Value;
            // hit1 and hit2 should be present. hit1 should be de-duplicated (not added twice despite obj1 and obj3 having Ref_WallA)
            Assert.Equal(2, groupedHits.Count);
            Assert.Contains(groupedHits, h => h.Reference == "Ref_WallA");
            Assert.Contains(groupedHits, h => h.Reference == "Ref_WallB");
            Assert.DoesNotContain(groupedHits, h => h.Reference == "Ref_Unknown");
        }
    }
}
