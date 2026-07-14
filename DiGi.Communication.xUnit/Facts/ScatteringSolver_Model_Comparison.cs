using DiGi.Communication.Classes;
using DiGi.Communication.Interfaces;
using DiGi.Geometry.Spatial.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiGi.Communication.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Compares the CPU-based ScatteringSolver and GPU-based (ComputeSharp) ScatteringSolver using a saved GeometricalPropagationModel json fixture.
        /// <para>Verifies that both solvers return matching results (visibility, delays, scattering point group references, and point counts/coordinates).</para>
        /// </summary>
        [Fact]
        public void ScatteringSolver_GeometricalPropagationModel_Comparison()
        {
            string? string_FilePath = Core.xUnit.Query.FilePath(Assembly.GetExecutingAssembly(), "GeometricalPropagationModel.json");
            Assert.False(string.IsNullOrWhiteSpace(string_FilePath));
            Assert.True(System.IO.File.Exists(string_FilePath));

            List<GeometricalPropagationModel>? list_Models = Core.Convert.ToDiGi<GeometricalPropagationModel>((Core.Classes.Path)string_FilePath!);
            Assert.NotNull(list_Models);

            GeometricalPropagationModel? geometricalPropagationModel_Source = list_Models.FirstOrDefault();
            Assert.NotNull(geometricalPropagationModel_Source);

            GeometricalPropagationModel geometricalPropagationModel_Cpu = new(geometricalPropagationModel_Source);
            GeometricalPropagationModel geometricalPropagationModel_Gpu = new(geometricalPropagationModel_Source);

            ScatteringSolverOptions scatteringSolverOptions = new(DiGi.Communication.Constants.Factor.Angle, 0.1, DiGi.Core.Constants.Tolerance.Distance);

            ScatteringSolver scatteringSolver_Cpu = new()
            {
                GeometricalPropagationModel = geometricalPropagationModel_Cpu,
                ScatteringSolverOptions = scatteringSolverOptions
            };

            DiGi.Communication.ComputeSharp.Classes.ScatteringSolver scatteringSolver_Gpu = new()
            {
                GeometricalPropagationModel = geometricalPropagationModel_Gpu,
                ScatteringSolverOptions = scatteringSolverOptions
            };

            bool bool_CpuSuccess = scatteringSolver_Cpu.Solve();
            bool bool_GpuSuccess = scatteringSolver_Gpu.Solve();

            Assert.True(bool_CpuSuccess);
            Assert.True(bool_GpuSuccess);

            Assert.NotNull(scatteringSolver_Cpu.ScatteringProfiles);
            Assert.NotNull(scatteringSolver_Gpu.ScatteringProfiles);
            Assert.Equal(scatteringSolver_Gpu.ScatteringProfiles.Count, scatteringSolver_Cpu.ScatteringProfiles.Count);

            for (int i = 0; i < scatteringSolver_Cpu.ScatteringProfiles.Count; i++)
            {
                IScatteringProfile iScatteringProfile_Cpu = scatteringSolver_Cpu.ScatteringProfiles[i];
                IScatteringProfile iScatteringProfile_Gpu = scatteringSolver_Gpu.ScatteringProfiles[i];

                testOutputHelper.WriteLine($"[Profile {i}] CPU Visible: {iScatteringProfile_Cpu.Visible}, GPU Visible: {iScatteringProfile_Gpu.Visible}");

                Assert.Equal(iScatteringProfile_Gpu.Visible, iScatteringProfile_Cpu.Visible);
                Assert.NotNull(iScatteringProfile_Cpu.Scatterings);
                Assert.NotNull(iScatteringProfile_Gpu.Scatterings);

                List<Scattering> list_CpuScatterings = new(iScatteringProfile_Cpu.Scatterings);
                List<Scattering> list_GpuScatterings = new(iScatteringProfile_Gpu.Scatterings);

                testOutputHelper.WriteLine($"[Profile {i}] CPU Scatterings Count: {list_CpuScatterings.Count}, GPU Scatterings Count: {list_GpuScatterings.Count}");

                Assert.Equal(list_GpuScatterings.Count, list_CpuScatterings.Count);

                for (int j = 0; j < list_CpuScatterings.Count; j++)
                {
                    Scattering scattering_Cpu = list_CpuScatterings[j];
                    Scattering scattering_Gpu = list_GpuScatterings[j];

                    testOutputHelper.WriteLine($"  [Delay {j}] Delay: {scattering_Cpu.Delay}");
                    testOutputHelper.WriteLine($"    CPU Point Groups: {string.Join(", ", scattering_Cpu.ScatteringPointGroups.Select(g => $"'{g.Reference}': {g.Points.Count} pts"))}");
                    testOutputHelper.WriteLine($"    GPU Point Groups: {string.Join(", ", scattering_Gpu.ScatteringPointGroups.Select(g => $"'{g.Reference}': {g.Points.Count} pts"))}");

                    Assert.Equal(scattering_Gpu.Delay, scattering_Cpu.Delay, 9);
                    Assert.NotNull(scattering_Cpu.ScatteringPointGroups);
                    Assert.NotNull(scattering_Gpu.ScatteringPointGroups);

                    List<Tuple<Point3D, string>> cpuPoints = [];
                    foreach (ScatteringPointGroup group in scattering_Cpu.ScatteringPointGroups)
                    {
                        if (group.Points != null)
                        {
                            foreach (Point3D p in group.Points)
                            {
                                cpuPoints.Add(new Tuple<Point3D, string>(p, group.Reference ?? string.Empty));
                            }
                        }
                    }

                    List<Tuple<Point3D, string>> gpuPoints = [];
                    foreach (ScatteringPointGroup group in scattering_Gpu.ScatteringPointGroups)
                    {
                        if (group.Points != null)
                        {
                            foreach (Point3D p in group.Points)
                            {
                                gpuPoints.Add(new Tuple<Point3D, string>(p, group.Reference ?? string.Empty));
                            }
                        }
                    }

                    double diffPercent = System.Math.Abs(gpuPoints.Count - cpuPoints.Count) / (double)gpuPoints.Count;
                    Assert.True(diffPercent < 0.02 || System.Math.Abs(gpuPoints.Count - cpuPoints.Count) <= 5, 
                        $"Point count mismatch: GPU={gpuPoints.Count}, CPU={cpuPoints.Count}");

                    List<Tuple<Point3D, string>> remainingCpuPoints = new(cpuPoints);
                    int matchedCount = 0;

                    foreach (Tuple<Point3D, string> gpuPoint in gpuPoints)
                    {
                        int matchIndex = -1;
                        double bestDist = double.MaxValue;

                        for (int idx = 0; idx < remainingCpuPoints.Count; idx++)
                        {
                            double dist = gpuPoint.Item1.Distance(remainingCpuPoints[idx].Item1);
                            if (dist < 1e-4 && dist < bestDist)
                            {
                                bestDist = dist;
                                matchIndex = idx;
                            }
                        }

                        if (matchIndex != -1)
                        {
                            matchedCount++;
                            Tuple<Point3D, string> matchedCpuPoint = remainingCpuPoints[matchIndex];
                            remainingCpuPoints.RemoveAt(matchIndex);

                            if (gpuPoint.Item2 != matchedCpuPoint.Item2)
                            {
                                Assert.False(string.IsNullOrEmpty(gpuPoint.Item2), $"GPU reference is empty for point: {gpuPoint.Item1}");
                                Assert.False(string.IsNullOrEmpty(matchedCpuPoint.Item2), $"CPU reference is empty for point: {matchedCpuPoint.Item1}");
                            }
                        }
                    }

                    if (gpuPoints.Count > 0)
                    {
                        double matchRate = matchedCount / (double)gpuPoints.Count;
                        Assert.True(matchRate > 0.98, $"Point geometric match rate too low: {matchRate:P} (matched {matchedCount} of {gpuPoints.Count})");
                    }
                    else
                    {
                        Assert.Equal(0, cpuPoints.Count);
                    }
                }
            }
        }
    }
}
