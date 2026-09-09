namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that TransformGroup3D enumerates the stored transforms without deep-cloning on every pass.
        /// </summary>
        [Fact]
        public void TransformGroup3D_GetEnumeratorStreamsLiveTransforms()
        {
            Spatial.Classes.Transform3D transform3D_A = Spatial.Create.Transform3D.Translation(1.0, 2.0, 3.0)!;
            Spatial.Classes.Transform3D transform3D_B = Spatial.Create.Transform3D.RotationZ(0.6)!;
            Spatial.Classes.TransformGroup3D group = new([transform3D_A, transform3D_B]);

            List<Spatial.Interfaces.ITransform3D> firstPass = [];
            foreach (Spatial.Interfaces.ITransform3D transform in group)
            {
                firstPass.Add(transform);
            }

            List<Spatial.Interfaces.ITransform3D> secondPass = [];
            foreach (Spatial.Interfaces.ITransform3D transform in group)
            {
                secondPass.Add(transform);
            }

            Assert.Equal(2, firstPass.Count);
            Assert.Equal(2, secondPass.Count);

            // No deep clone per pass: both passes must surface the same stored instances.
            Assert.True(ReferenceEquals(firstPass[0], secondPass[0]), "GetEnumerator must not deep-clone on every enumeration pass.");
            Assert.True(ReferenceEquals(firstPass[1], secondPass[1]), "GetEnumerator must not deep-clone on every enumeration pass.");

            // Value parity: enumerated transforms still agree with the intended matrices.
            Assert.Equal(transform3D_A[0, 3], ((Spatial.Classes.Transform3D)firstPass[0])[0, 3], 9);
            Assert.Equal(transform3D_B[0, 1], ((Spatial.Classes.Transform3D)firstPass[1])[0, 1], 9);
        }
    }
}
