using DiGi.Geometry.Planar.Classes;
using DiGi.GIS.Classes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DiGi.GIS.xUnit
{
    public partial class Facts
    {
        /// <summary>Loads a real GIS model from the 0207_GML.gmf sample file and verifies that <see cref="Create.OrtoRanges(GISModel?, IEnumerable{string}?, OrtoRangeOptions?, double)"/> (rewritten to use a <see cref="LinkedList{T}"/> for O(1) removals instead of O(n) <see cref="List{T}"/> shifting) produces the exact same clustering as a faithful reimplementation of the original List-based algorithm, confirming the data-structure change did not alter observable behavior.</summary>
        [Fact]
        public void OrtoRanges()
        {
            string? path = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "0207_GML.gmf");
            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(System.IO.File.Exists(path));

            GISModel? gISModel = null;
            using (GISModelFile gISModelFile = new(path))
            {
                Assert.True(gISModelFile.Open());
                gISModel = gISModelFile.Value;
            }

            Assert.NotNull(gISModel);

            List<OrtoRange>? ortoRanges = Create.OrtoRanges(gISModel);
            Assert.NotNull(ortoRanges);
            Assert.NotEmpty(ortoRanges);

            List<OrtoRange>? ortoRanges_Oracle = OrtoRanges_Oracle(gISModel);
            Assert.NotNull(ortoRanges_Oracle);

            Assert.Equal(ortoRanges_Oracle.Count, ortoRanges.Count);

            for (int i = 0; i < ortoRanges_Oracle.Count; i++)
            {
                OrtoRange ortoRange = ortoRanges[i];
                OrtoRange ortoRange_Oracle = ortoRanges_Oracle[i];

                Assert.Equal(ortoRange_Oracle.BoundingBox2D?.Min, ortoRange.BoundingBox2D?.Min);
                Assert.Equal(ortoRange_Oracle.BoundingBox2D?.Max, ortoRange.BoundingBox2D?.Max);

                HashSet<string> references_Inside_Oracle = ortoRange_Oracle.References_Inside ?? [];
                HashSet<string> references_Inside = ortoRange.References_Inside ?? [];
                Assert.True(references_Inside_Oracle.SetEquals(references_Inside));

                HashSet<string> references_Intersect_Oracle = ortoRange_Oracle.References_Intersect ?? [];
                HashSet<string> references_Intersect = ortoRange.References_Intersect ?? [];
                Assert.True(references_Intersect_Oracle.SetEquals(references_Intersect));
            }
        }

        /// <summary>Faithful reimplementation of the pre-optimization <see cref="Create.OrtoRanges(GISModel?, IEnumerable{string}?, OrtoRangeOptions?, double)"/> body, using a plain <see cref="List{T}"/> with index-0 and value-based removals, kept solely as a regression oracle for the LinkedList-based rewrite.</summary>
        private static List<OrtoRange>? OrtoRanges_Oracle(GISModel? gISModel)
        {
            List<Building2D>? building2Ds = gISModel?.GetObjects<Building2D>();
            if (building2Ds == null || building2Ds.Count == 0)
            {
                return null;
            }

            double tolerance = Core.Constants.Tolerance.Distance;

            List<Tuple<Building2D, BoundingBox2D>> tuples = [];
            foreach (Building2D building2D in building2Ds)
            {
                BoundingBox2D? boundingBox2D = gISModel!.GetRelatedObject<Building2DGeometryCalculationResult>(building2D)?.BoundingBox;
                boundingBox2D ??= building2D.PolygonalFace2D?.GetBoundingBox();

                if (boundingBox2D == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<Building2D, BoundingBox2D>(building2D, boundingBox2D));
            }

            OrtoRangeOptions ortoRangeOptions = new();

            List<OrtoRange> result = [];

            while (tuples.Count > 0)
            {
                Building2D building2D = tuples[0].Item1;
                BoundingBox2D boundingBox2D_Building2D = tuples[0].Item2;

                tuples.RemoveAt(0);

                if (building2D == null)
                {
                    continue;
                }

                Point2D? point2D = boundingBox2D_Building2D.GetCentroid();

                BoundingBox2D? boundingBox2D = Geometry.Planar.Create.BoundingBox2D(point2D, ortoRangeOptions.Width, ortoRangeOptions.Height);
                if (boundingBox2D is null)
                {
                    continue;
                }

                boundingBox2D.Add(boundingBox2D_Building2D);

                HashSet<string> references_Intersect = [];
                HashSet<string> references_Inside = [];

                for (int i = tuples.Count - 1; i >= 0; i--)
                {
                    string? reference = tuples[i].Item1.Reference;
                    if (string.IsNullOrWhiteSpace(reference))
                    {
                        continue;
                    }

                    BoundingBox2D boundingBox2D_Temp = tuples[i].Item2;

                    if (!boundingBox2D.InRange(boundingBox2D_Temp, tolerance))
                    {
                        continue;
                    }

                    if (!boundingBox2D.Inside(boundingBox2D_Temp, tolerance))
                    {
                        references_Intersect.Add(reference!);
                        continue;
                    }

                    references_Inside.Add(reference!);
                    tuples.Remove(tuples[i]);
                }

                result.Add(new OrtoRange(boundingBox2D, references_Inside, references_Intersect));
            }

            return result;
        }
    }
}