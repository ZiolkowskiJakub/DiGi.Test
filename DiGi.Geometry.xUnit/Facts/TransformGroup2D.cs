namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Verifies that TransformGroup2D enumerates the stored transforms without deep-cloning on every pass.
        /// </summary>
        [Fact]
        public void TransformGroup2D_GetEnumeratorStreamsLiveTransforms()
        {
            Planar.Classes.Transform2D transform2D_A = Planar.Create.Transform2D.Translation(1.0, 2.0)!;
            Planar.Classes.Transform2D transform2D_B = Planar.Create.Transform2D.Rotation(0.6)!;
            Planar.Classes.TransformGroup2D group = new([transform2D_A, transform2D_B]);

            List<Planar.Interfaces.ITransform2D> firstPass = [];
            foreach (Planar.Interfaces.ITransform2D transform in group)
            {
                firstPass.Add(transform);
            }

            List<Planar.Interfaces.ITransform2D> secondPass = [];
            foreach (Planar.Interfaces.ITransform2D transform in group)
            {
                secondPass.Add(transform);
            }

            Assert.Equal(2, firstPass.Count);
            Assert.Equal(2, secondPass.Count);

            // No deep clone per pass: both passes must surface the same stored instances.
            Assert.True(ReferenceEquals(firstPass[0], secondPass[0]), "GetEnumerator must not deep-clone on every enumeration pass.");
            Assert.True(ReferenceEquals(firstPass[1], secondPass[1]), "GetEnumerator must not deep-clone on every enumeration pass.");

            // Value parity: enumerated transforms still agree with the intended matrices.
            Assert.Equal(transform2D_A[0, 2], ((Planar.Classes.Transform2D)firstPass[0])[0, 2], 9);
            Assert.Equal(transform2D_B[1, 0], ((Planar.Classes.Transform2D)firstPass[1])[1, 0], 9);
        }
    }
}
