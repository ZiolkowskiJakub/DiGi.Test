using DiGi.Geometry.Spatial;

namespace DiGi.Geometry.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the 3D rotation transformations, verifying that rotation around the standard axes matches the axis-specific rotation methods, and that rotation around an arbitrary axis and around a custom origin point behaves correctly.
        /// </summary>
        [Fact]
        public void Transform3D_Rotation()
        {
            double angle = System.Math.PI / 2.0;

            // 1. Verify rotation around standard axes matches axis-specific rotations
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_AxisX = new(1.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_AxisY = new(0.0, 1.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_AxisZ = new(0.0, 0.0, 1.0);

            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_RotX = Spatial.Create.Transform3D.Rotation(vector3D_AxisX, angle);
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_RotY = Spatial.Create.Transform3D.Rotation(vector3D_AxisY, angle);
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_RotZ = Spatial.Create.Transform3D.Rotation(vector3D_AxisZ, angle);

            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_SpecX = Spatial.Create.Transform3D.RotationX(angle);
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_SpecY = Spatial.Create.Transform3D.RotationY(angle);
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_SpecZ = Spatial.Create.Transform3D.RotationZ(angle);

            Assert.NotNull(transform3D_RotX);
            Assert.NotNull(transform3D_RotY);
            Assert.NotNull(transform3D_RotZ);

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Assert.Equal(transform3D_SpecX[row, col], transform3D_RotX[row, col], 9);
                    Assert.Equal(transform3D_SpecY[row, col], transform3D_RotY[row, col], 9);
                    Assert.Equal(transform3D_SpecZ[row, col], transform3D_RotZ[row, col], 9);
                }
            }

            // 2. Verify rotation around Z-axis by 90 degrees maps (1, 0, 0) to (0, 1, 0)
            DiGi.Geometry.Spatial.Classes.Point3D point3D_1 = new(1.0, 0.0, 0.0);
            bool bool_SuccessZ = point3D_1.Transform(transform3D_RotZ);
            Assert.True(bool_SuccessZ);
            Assert.Equal(0.0, point3D_1.X, 9);
            Assert.Equal(1.0, point3D_1.Y, 9);
            Assert.Equal(0.0, point3D_1.Z, 9);

            // 3. Verify rotation around diagonal axis (1, 1, 1) by 120 degrees permutes coordinates
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_Diag = new(1.0, 1.0, 1.0);
            double angle120 = 2.0 * System.Math.PI / 3.0;
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_RotDiag = Spatial.Create.Transform3D.Rotation(vector3D_Diag, angle120);
            Assert.NotNull(transform3D_RotDiag);

            DiGi.Geometry.Spatial.Classes.Point3D point3D_2 = new(1.0, 0.0, 0.0);
            bool bool_SuccessDiag = point3D_2.Transform(transform3D_RotDiag);
            Assert.True(bool_SuccessDiag);
            // With active right-handed rotation around (1,1,1) by 120 deg, (1,0,0) maps to (0, 1, 0)
            Assert.Equal(0.0, point3D_2.X, 9);
            Assert.Equal(1.0, point3D_2.Y, 9);
            Assert.Equal(0.0, point3D_2.Z, 9);

            // 4. Verify rotation around a custom origin point
            // Rotate (2, 0, 0) around origin (1, 0, 0) and Z-axis by 90 degrees -> should be (1, 1, 0)
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Origin = new(1.0, 0.0, 0.0);
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_RotAroundPt = Spatial.Create.Transform3D.Rotation(point3D_Origin, vector3D_AxisZ, angle);
            Assert.NotNull(transform3D_RotAroundPt);

            DiGi.Geometry.Spatial.Classes.Point3D point3D_3 = new(2.0, 0.0, 0.0);
            bool bool_SuccessAroundPt = point3D_3.Transform(transform3D_RotAroundPt);
            Assert.True(bool_SuccessAroundPt);
            Assert.Equal(1.0, point3D_3.X, 9);
            Assert.Equal(1.0, point3D_3.Y, 9);
            Assert.Equal(0.0, point3D_3.Z, 9);

            // 5. Verify serialization check
            DiGi.Core.xUnit.Query.SerializationCheck(transform3D_RotX);
            DiGi.Core.xUnit.Query.SerializationCheck(transform3D_RotAroundPt);
        }

        /// <summary>
        /// Tests additional functionalities of Transform3D, including vector transformations, point transformations, TRS decomposition, Euler angle rotations, and shearing.
        /// </summary>
        [Fact]
        public void Transform3D_AdditionalFunctionalities()
        {
            // 1. Test Vector3D.Transform (translation should be ignored)
            DiGi.Geometry.Spatial.Classes.Vector3D vector3D_Orig = new(1.0, 2.0, 3.0);
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_Translation = Spatial.Create.Transform3D.Translation(10.0, 20.0, 30.0);
            bool bool_SuccessVec = vector3D_Orig.Transform(transform3D_Translation);

            Assert.True(bool_SuccessVec);
            Assert.Equal(1.0, vector3D_Orig.X, 9);
            Assert.Equal(2.0, vector3D_Orig.Y, 9);
            Assert.Equal(3.0, vector3D_Orig.Z, 9);

            // 2. Test Point3D.Transform (translation should be applied)
            DiGi.Geometry.Spatial.Classes.Point3D point3D_Orig = new(1.0, 2.0, 3.0);
            bool bool_SuccessPt = point3D_Orig.Transform(transform3D_Translation);

            Assert.True(bool_SuccessPt);
            Assert.Equal(11.0, point3D_Orig.X, 9);
            Assert.Equal(22.0, point3D_Orig.Y, 9);
            Assert.Equal(33.0, point3D_Orig.Z, 9);

            // 3. Test TRS Decomposition
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_Scale = Spatial.Create.Transform3D.Scale(2.0, 3.0, 4.0);
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_Rotation = Spatial.Create.Transform3D.RotationX(System.Math.PI / 4.0);
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_Combined = transform3D_Translation * transform3D_Rotation * transform3D_Scale;

            Assert.NotNull(transform3D_Combined);

            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_DecTranslation;
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_DecRotation;
            DiGi.Geometry.Spatial.Classes.Vector3D? vector3D_DecScale;

            bool bool_DecomposeSuccess = transform3D_Combined.Decompose(out vector3D_DecTranslation, out transform3D_DecRotation, out vector3D_DecScale);

            Assert.True(bool_DecomposeSuccess);
            Assert.NotNull(vector3D_DecTranslation);
            Assert.NotNull(transform3D_DecRotation);
            Assert.NotNull(vector3D_DecScale);

            Assert.Equal(10.0, vector3D_DecTranslation.X, 9);
            Assert.Equal(20.0, vector3D_DecTranslation.Y, 9);
            Assert.Equal(30.0, vector3D_DecTranslation.Z, 9);

            Assert.Equal(2.0, vector3D_DecScale.X, 9);
            Assert.Equal(3.0, vector3D_DecScale.Y, 9);
            Assert.Equal(4.0, vector3D_DecScale.Z, 9);

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Assert.Equal(transform3D_Rotation[row, col], transform3D_DecRotation[row, col], 9);
                }
            }

            // 4. Test RotationYawPitchRoll (Euler angles)
            double yaw = System.Math.PI / 6.0;   // 30 deg
            double pitch = System.Math.PI / 4.0; // 45 deg
            double roll = System.Math.PI / 3.0;  // 60 deg

            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_YPR = Spatial.Create.Transform3D.RotationYawPitchRoll(yaw, pitch, roll);
            DiGi.Geometry.Spatial.Classes.Transform3D? transform3D_ManualYPR = Spatial.Create.Transform3D.RotationZ(yaw) * Spatial.Create.Transform3D.RotationY(pitch) * Spatial.Create.Transform3D.RotationX(roll);

            Assert.NotNull(transform3D_ManualYPR);

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Assert.Equal(transform3D_ManualYPR[row, col], transform3D_YPR[row, col], 9);
                }
            }

            // 5. Test ShearXY
            DiGi.Geometry.Spatial.Classes.Transform3D transform3D_Shear = Spatial.Create.Transform3D.ShearXY(2.0, 3.0);
            DiGi.Geometry.Spatial.Classes.Point3D point3D_ShearTest1 = new(0.0, 0.0, 1.0);
            bool bool_SuccessShear1 = point3D_ShearTest1.Transform(transform3D_Shear);

            Assert.True(bool_SuccessShear1);
            Assert.Equal(2.0, point3D_ShearTest1.X, 9);
            Assert.Equal(3.0, point3D_ShearTest1.Y, 9);
            Assert.Equal(1.0, point3D_ShearTest1.Z, 9);

            DiGi.Geometry.Spatial.Classes.Point3D point3D_ShearTest2 = new(1.0, 2.0, 0.0);
            bool bool_SuccessShear2 = point3D_ShearTest2.Transform(transform3D_Shear);

            Assert.True(bool_SuccessShear2);
            Assert.Equal(1.0, point3D_ShearTest2.X, 9);
            Assert.Equal(2.0, point3D_ShearTest2.Y, 9);
            Assert.Equal(0.0, point3D_ShearTest2.Z, 9);
        }
    }
}