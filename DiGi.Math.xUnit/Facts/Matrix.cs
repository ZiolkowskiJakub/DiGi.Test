using DiGi.Math.Classes;
using DiGi.Math.Interfaces;

namespace DiGi.Math.xUnit
{
    public partial class Facts
    {
        /// <summary>
        /// Tests the matrix construction and basic property methods.
        /// </summary>
        [Fact]
        public void Matrix_ConstructorsAndProperties()
        {
            // Rows and columns constructor
            Matrix matrix_1 = new(3, 2);
            Assert.Equal(3, matrix_1.RowCount());
            Assert.Equal(2, matrix_1.ColumnCount());
            Assert.False(matrix_1.IsSquare());

            // 1D array constructor (creates a 1xN matrix)
            double[] doubles_1 = [1.0, 2.0, 3.0];
            Matrix matrix_2 = new(doubles_1);
            Assert.Equal(1, matrix_2.RowCount());
            Assert.Equal(3, matrix_2.ColumnCount());
            Assert.Equal(1.0, matrix_2[0, 0]);
            Assert.Equal(2.0, matrix_2[0, 1]);
            Assert.Equal(3.0, matrix_2[0, 2]);

            // 2D array constructor
            double[,] doubles_2D = new double[2, 3]
            {
                { 1.0, 2.0, 3.0 },
                { 4.0, 5.0, 6.0 }
            };
            Matrix matrix_3 = new(doubles_2D);
            Assert.Equal(2, matrix_3.RowCount());
            Assert.Equal(3, matrix_3.ColumnCount());
            Assert.Equal(4.0, matrix_3[1, 0]);

            // Copy constructor
            Matrix matrix_Copy = new(matrix_3);
            Assert.Equal(matrix_3.RowCount(), matrix_Copy.RowCount());
            Assert.Equal(matrix_3.ColumnCount(), matrix_Copy.ColumnCount());
            Assert.Equal(matrix_3[1, 1], matrix_Copy[1, 1]);

            // Indexer get and set
            matrix_1[1, 1] = 9.9;
            Assert.Equal(9.9, matrix_1[1, 1]);

            // Explicit operator double[,] to Matrix
            Matrix? matrix_Explicit = (Matrix?)doubles_2D;
            Assert.NotNull(matrix_Explicit);
            Assert.Equal(2, matrix_Explicit.RowCount());
            Assert.Equal(3, matrix_Explicit.ColumnCount());
        }

        /// <summary>
        /// Tests the arithmetic operator overloads of the Matrix class.
        /// </summary>
        [Fact]
        public void Matrix_Operators()
        {
            double[,] doubles_A = new double[2, 3]
            {
                { 1.0, 2.0, 3.0 },
                { 4.0, 5.0, 6.0 }
            };
            double[,] doubles_B = new double[2, 3]
            {
                { 10.0, 20.0, 30.0 },
                { 40.0, 50.0, 60.0 }
            };

            Matrix matrix_A = new(doubles_A);
            Matrix matrix_B = new(doubles_B);

            // Matrix Addition
            Matrix? matrix_Sum = matrix_A + matrix_B;
            Assert.NotNull(matrix_Sum);
            Assert.Equal(11.0, matrix_Sum[0, 0]);
            Assert.Equal(66.0, matrix_Sum[1, 2]);

            // Matrix Subtraction
            Matrix? matrix_Diff = matrix_B - matrix_A;
            Assert.NotNull(matrix_Diff);
            Assert.Equal(9.0, matrix_Diff[0, 0]);
            Assert.Equal(54.0, matrix_Diff[1, 2]);

            // Matrix Multiplication (A is 2x3, C is 3x2)
            double[,] doubles_C = new double[3, 2]
            {
                { 1.0, 2.0 },
                { 3.0, 4.0 },
                { 5.0, 6.0 }
            };
            Matrix matrix_C = new(doubles_C);
            Matrix? matrix_Product = matrix_A * matrix_C; // Result should be 2x2
            Assert.NotNull(matrix_Product);
            Assert.Equal(2, matrix_Product.RowCount());
            Assert.Equal(2, matrix_Product.ColumnCount());
            // (0,0): 1*1 + 2*3 + 3*5 = 1 + 6 + 15 = 22
            Assert.Equal(22.0, matrix_Product[0, 0]);

            // Scalar Addition (testing fix for Dimension bounds swap bug on non-square matrix)
            double scalar = 2.0;
            Matrix? matrix_ScalarAdd = matrix_A + scalar;
            Assert.NotNull(matrix_ScalarAdd);
            Assert.Equal(3.0, matrix_ScalarAdd[0, 0]);
            Assert.Equal(8.0, matrix_ScalarAdd[1, 2]);

            // Scalar Subtraction
            Matrix? matrix_ScalarSub = matrix_A - scalar;
            Assert.NotNull(matrix_ScalarSub);
            Assert.Equal(-1.0, matrix_ScalarSub[0, 0]);
            Assert.Equal(4.0, matrix_ScalarSub[1, 2]);

            // Scalar Multiplication (testing fix for Dimension bounds swap bug on non-square matrix)
            Matrix? matrix_ScalarMult = matrix_A * scalar;
            Assert.NotNull(matrix_ScalarMult);
            Assert.Equal(2.0, matrix_ScalarMult[0, 0]);
            Assert.Equal(12.0, matrix_ScalarMult[1, 2]);
        }

        /// <summary>
        /// Tests matrix methods such as Determinant, Transpose, Inverse, and Cofactor.
        /// </summary>
        [Fact]
        public void Matrix_Calculations()
        {
            // Determinant of 2x2
            double[,] doubles_2D = new double[2, 2]
            {
                { 4.0, 7.0 },
                { 2.0, 6.0 }
            };
            Matrix matrix_2D = new(doubles_2D);
            Assert.Equal(10.0, matrix_2D.Determinant()); // 4*6 - 7*2 = 24 - 14 = 10

            // Determinant of 3x3
            double[,] doubles_3D = new double[3, 3]
            {
                { 1.0, 2.0, 3.0 },
                { 0.0, 1.0, 4.0 },
                { 5.0, 6.0, 0.0 }
            };
            Matrix matrix_3D = new(doubles_3D);
            // det = 1*(0-24) - 2*(0-20) + 3*(0-5) = -24 + 40 - 15 = 1
            Assert.Equal(1.0, matrix_3D.Determinant());

            // Transpose (3x2 to 2x3)
            double[,] doubles_NonSquare = new double[3, 2]
            {
                { 1.0, 2.0 },
                { 3.0, 4.0 },
                { 5.0, 6.0 }
            };
            Matrix matrix_NonSquare = new(doubles_NonSquare);
            Matrix? matrix_Transposed = matrix_NonSquare.GetTransposed();
            Assert.NotNull(matrix_Transposed);
            Assert.Equal(2, matrix_Transposed.RowCount());
            Assert.Equal(3, matrix_Transposed.ColumnCount());
            Assert.Equal(3.0, matrix_Transposed[0, 1]); // row 0, col 1 in transpose is row 1, col 0 in original

            // Inversion in place and cloned inversion
            Matrix? matrix_Inversed = matrix_3D.GetInversed();
            Assert.NotNull(matrix_Inversed);
            // Identity product validation
            Matrix? matrix_IdentityProduct = matrix_3D * matrix_Inversed;
            Assert.NotNull(matrix_IdentityProduct);
            Assert.True(System.Math.Abs(matrix_IdentityProduct[0, 0] - 1.0) < 1e-9);
            Assert.True(System.Math.Abs(matrix_IdentityProduct[1, 1] - 1.0) < 1e-9);
            Assert.True(System.Math.Abs(matrix_IdentityProduct[2, 2] - 1.0) < 1e-9);
            Assert.True(System.Math.Abs(matrix_IdentityProduct[0, 1]) < 1e-9);

            // Cofactor matrix (signed minors): for [[4,7],[2,6]] this is [[6,-2],[-7,4]].
            Matrix? matrix_Cofactors = matrix_2D.GetCofactorsMatrix();
            Assert.NotNull(matrix_Cofactors);
            Assert.Equal(6.0, matrix_Cofactors[0, 0]); // +det([6]) = 6
            Assert.Equal(-2.0, matrix_Cofactors[0, 1]); // -det([2]) = -2
            Assert.Equal(-7.0, matrix_Cofactors[1, 0]); // -det([7]) = -7
            Assert.Equal(4.0, matrix_Cofactors[1, 1]); // +det([4]) = 4
        }

        /// <summary>
        /// Tests Matrix.GetCofactorsMatrix, verifying the signed-minor cofactor values for a 3x3 matrix, the adjugate identity A * transpose(Cofactor) = det(A) * I, the 1x1 convention, and that a non-square matrix returns null.
        /// </summary>
        [Fact]
        public void Matrix_GetCofactorsMatrix()
        {
            // A = [[1,2,3],[0,1,4],[5,6,0]] has det(A) = 1 and cofactor matrix
            // [[-24, 20, -5], [18, -15, 4], [5, -4, 1]].
            double[,] doubles_3D = new double[3, 3]
            {
                { 1.0, 2.0, 3.0 },
                { 0.0, 1.0, 4.0 },
                { 5.0, 6.0, 0.0 }
            };
            Matrix matrix_3D = new(doubles_3D);

            Matrix? matrix_Cofactors = matrix_3D.GetCofactorsMatrix();
            Assert.NotNull(matrix_Cofactors);

            double[,] doubles_ExpectedCofactors = new double[3, 3]
            {
                { -24.0, 20.0, -5.0 },
                { 18.0, -15.0, 4.0 },
                { 5.0, -4.0, 1.0 }
            };
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.True(System.Math.Abs(matrix_Cofactors[i, j] - doubles_ExpectedCofactors[i, j]) < 1e-9);
                }
            }

            // Adjugate identity: A * transpose(Cofactor(A)) == det(A) * I.
            Matrix? matrix_Adjugate = matrix_Cofactors.GetTransposed();
            Assert.NotNull(matrix_Adjugate);
            Matrix? matrix_Product = matrix_3D * matrix_Adjugate;
            Assert.NotNull(matrix_Product);

            double determinant = matrix_3D.Determinant();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    double expected = i == j ? determinant : 0.0;
                    Assert.True(System.Math.Abs(matrix_Product[i, j] - expected) < 1e-9);
                }
            }

            // 1x1 convention: the cofactor matrix is [[1]].
            Matrix matrix_1x1 = new(new double[,] { { 5.0 } });
            Matrix? matrix_Cofactors_1x1 = matrix_1x1.GetCofactorsMatrix();
            Assert.NotNull(matrix_Cofactors_1x1);
            Assert.Equal(1, matrix_Cofactors_1x1.RowCount());
            Assert.Equal(1, matrix_Cofactors_1x1.ColumnCount());
            Assert.Equal(1.0, matrix_Cofactors_1x1[0, 0]);

            // Non-square input returns null (cofactor matrix is only defined for square matrices).
            Matrix matrix_NonSquare = new(new double[2, 3]);
            Assert.Null(matrix_NonSquare.GetCofactorsMatrix());
        }

        /// <summary>
        /// Tests specialized square matrices Matrix2D, Matrix3D, and Matrix4D.
        /// </summary>
        [Fact]
        public void Matrix2D_3D_4D()
        {
            // Matrix2D constructor, identity, and operators
            Matrix2D matrix2D_A = Create.Matrix2D.Identity();
            Matrix2D matrix2D_B = new();
            matrix2D_B[0, 0] = 5.0;
            matrix2D_B[0, 1] = 2.0;
            matrix2D_B[1, 0] = -1.0;
            matrix2D_B[1, 1] = 3.0;

            // Matrix2D addition
            Matrix2D? matrix2D_Sum = matrix2D_A + matrix2D_B;
            Assert.NotNull(matrix2D_Sum);
            Assert.Equal(6.0, matrix2D_Sum[0, 0]);
            Assert.Equal(4.0, matrix2D_Sum[1, 1]);

            // Matrix2D subtraction (tests the subtraction bug fix)
            Matrix2D? matrix2D_Diff = matrix2D_B - matrix2D_A;
            Assert.NotNull(matrix2D_Diff);
            Assert.Equal(4.0, matrix2D_Diff[0, 0]); // 5.0 - 1.0 = 4.0
            Assert.Equal(2.0, matrix2D_Diff[0, 1]); // 2.0 - 0.0 = 2.0
            Assert.Equal(-1.0, matrix2D_Diff[1, 0]); // -1.0 - 0.0 = -1.0
            Assert.Equal(2.0, matrix2D_Diff[1, 1]); // 3.0 - 1.0 = 2.0

            // Matrix3D constructor, identity, and operators
            Matrix3D matrix3D_A = Create.Matrix3D.Identity();
            Matrix3D matrix3D_B = new();
            matrix3D_B[0, 0] = 10.0;
            matrix3D_B[1, 1] = 10.0;
            matrix3D_B[2, 2] = 10.0;

            // Matrix3D subtraction (tests the subtraction bug fix)
            Matrix3D? matrix3D_Diff = matrix3D_B - matrix3D_A;
            Assert.NotNull(matrix3D_Diff);
            Assert.Equal(9.0, matrix3D_Diff[0, 0]);
            Assert.Equal(9.0, matrix3D_Diff[1, 1]);
            Assert.Equal(9.0, matrix3D_Diff[2, 2]);

            // Matrix4D constructor, identity, and operators
            Matrix4D matrix4D_A = Create.Matrix4D.Identity();
            Matrix4D matrix4D_B = new();
            matrix4D_B[0, 0] = 7.0;
            matrix4D_B[1, 1] = 7.0;
            matrix4D_B[2, 2] = 7.0;
            matrix4D_B[3, 3] = 7.0;

            // Matrix4D subtraction (tests the subtraction bug fix)
            Matrix4D? matrix4D_Diff = matrix4D_B - matrix4D_A;
            Assert.NotNull(matrix4D_Diff);
            Assert.Equal(6.0, matrix4D_Diff[0, 0]);
            Assert.Equal(6.0, matrix4D_Diff[3, 3]);

            // Convert back and forth between general Matrix and SquareMatrix
            Matrix matrix_Square = new(new double[2, 2] { { 1, 2 }, { 3, 4 } });
            ISquareMatrix? squareMatrix = matrix_Square.ToDiGi_SquareMatrix();
            Assert.NotNull(squareMatrix);
            Assert.IsType<Matrix2D>(squareMatrix);

            // Serialization checks
            Core.xUnit.Query.SerializationCheck(matrix2D_A);
            Core.xUnit.Query.SerializationCheck(matrix3D_A);
            Core.xUnit.Query.SerializationCheck(matrix4D_A);
        }

        /// <summary>
        /// Tests conversions between DiGi Matrix classes and MathNet Numerics matrices.
        /// </summary>
        [Fact]
        public void Matrix_MathNetConversions()
        {
            double[,] values = new double[,] { { 1.0, 2.0 }, { 3.0, 4.0 } };
            Matrix matrix = new(values);

            // To MathNet
            MathNet.Numerics.LinearAlgebra.Matrix<double>? mathNetMatrix = Convert.ToMathNet(matrix);
            Assert.NotNull(mathNetMatrix);
            Assert.Equal(1.0, mathNetMatrix[0, 0]);
            Assert.Equal(4.0, mathNetMatrix[1, 1]);

            // From MathNet
            Matrix? matrix_Back = Convert.ToDiGi(mathNetMatrix);
            Assert.NotNull(matrix_Back);
            Assert.Equal(2, matrix_Back.RowCount());
            Assert.Equal(2, matrix_Back.ColumnCount());
            Assert.Equal(1.0, matrix_Back[0, 0]);
        }
    }
}