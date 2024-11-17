using System;
using System.Reflection;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CrypTool.Plugins.HillCipherAttack
{
    public class HillCipherAttackMatrix
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int[,] Data { get; set; }

        public HillCipherAttackMatrix(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Data = new int[rows, cols];
        }

        public HillCipherAttackMatrix()
        {
            Cols = 0;
            Rows = 0;
            Data = new int[0, 0];
        }

        public HillCipherAttackMatrix(int[,] data)
        {
            Rows = data.GetLength(0);
            Cols = data.GetLength(1);
            Data = data;
        }

        public bool Equals(HillCipherAttackMatrix a)
        {
            if (Rows != a.Rows || Cols != a.Cols)
            {
                return false;
            }

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (Data[i, j] != a.Data[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public HillCipherAttackMatrix MultiplyMatrix(HillCipherAttackMatrix b)
        {
            if (Cols != b.Rows)
            {
                throw new Exception("The number of columns of the first matrix must be equal to the number of rows of the second matrix");
            }
            HillCipherAttackMatrix result = new HillCipherAttackMatrix(Rows, b.Cols);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < b.Cols; j++)
                {
                    result.Data[i, j] = 0;
                    for (int k = 0; k < Cols; k++)
                    {
                        result.Data[i, j] += Data[i, k] * b.Data[k, j];
                    }
                }
            }
            return result;
        }

        public static HillCipherAttackMatrix MultiplyMatrixWithNumber(HillCipherAttackMatrix a, int b)
        {
            HillCipherAttackMatrix result = new HillCipherAttackMatrix(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Cols; j++)
                {
                    result.Data[i, j] = a.Data[i, j] * b;
                }
            }
            return result;
        }

        public HillCipherAttackMatrix ModMatrix(int m)
        {
            HillCipherAttackMatrix result = new HillCipherAttackMatrix(Rows, Cols);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    var value = Data[i, j] % m;
                    result.Data[i, j] = value < 0 ? value + m : value;
                }
            }
            return result;
        }

        public static HillCipherAttackMatrix InverseMatrix(HillCipherAttackMatrix a, int m)
        {
            if (a.Rows != a.Cols)
            {
                throw new Exception("Matrix must be square");
            }

            int n = a.Rows;
            HillCipherAttackMatrix inv = new HillCipherAttackMatrix(n, n);
            int[,] adj = new int[n, n];
            int det = a.GetDeterminant();
            int invDet = ModInverse(det, m);

            a.Adjoint(adj);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    inv.Data[i, j] = (adj[i, j] * invDet) % m; // Every value in the adjoint matrix is multiplied by the inverse of the determinant
                    if (inv.Data[i, j] < 0) // Make sure the value is positive
                        inv.Data[i, j] += m;
                }
            }

            return inv;
        }


        // Calculate the Cofactors of the matrix and store them in adj and return the adjoint matrix transposed
        public void Adjoint(int[,] adj)
        {
            int n = Rows;
            if (n == 1)
            {
                adj[0, 0] = 1;
                return;
            }

            int sign;
            HillCipherAttackMatrix temp = new HillCipherAttackMatrix(n, n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    GetCofactor(temp.Data, i, j, n);
                    sign = ((i + j) % 2 == 0) ? 1 : -1;
                    adj[j, i] = (sign * temp.Determinant(n - 1));
                }
            }
        }

        public void GetCofactor(int[,] temp, int p, int q, int n) // Get the cofactor of the matrix
        {
            int i = 0, j = 0;
            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    if (row != p && col != q)
                    {
                        temp[i, j++] = Data[row, col];
                        if (j == n - 1)
                        {
                            j = 0;
                            i++;
                        }
                    }
                }
            }
        }
        public int Determinant(int n)
        {
            int det = 0;
            if (n == 1)
                return Data[0, 0];

            HillCipherAttackMatrix temp = new HillCipherAttackMatrix(n, n);
            int sign = 1;

            for (int f = 0; f < n; f++)
            {
                GetCofactor(temp.Data, 0, f, n);
                det += sign * Data[0, f] * temp.Determinant(n - 1);
                sign = -sign;
            }

            return det;
        }

        public bool CheckForIdentitiyMatrix(HillCipherAttackMatrix b, int m)
        {
            if (Rows > b.Cols)
            {
                return false;
            }
            HillCipherAttackMatrix i = new HillCipherAttackMatrix(Rows, Cols);
            for (int j = 0; j < Rows; j++)
            {
                for (int y = 0; y < Cols; y++)
                {
                    if (j == y)
                        i.Data[j, y] = 1;
                }
            }
            var res = MultiplyMatrix(b);
            res = res.ModMatrix(m);
            return res.Equals(i);
        }

        public static int ModInverse(int a, int m)
        {
            a = a % m;
            for (int x = 1; x < m; x++)
            {
                if ((a * x) % m == 1)
                    return x;
            }
            return 1;
        }


        /*public static int ModInverse(int a, int m)
        {
            // Get the Faktor x of a * x = 1 mod m
            int m0 = m;
            int y = 0, x = 1;

            if (m == 1)
                return 0;

            while (a > 1)
            {
                int q = a / m;
                int t = m;

                m = a % m;
                a = t;

                t = y;
                y = x - q * y;
                x = t;
            }

            // Make sure x is positive
            if (x < 0)
                x += m0;

            return x;


        }*/

        public int GetDeterminant()
        {
            if (Rows != Cols)
            {
                throw new Exception("Matrix must be square");
            }
            if (Rows == 1)
            {
                return Data[0, 0];
            }
            if (Rows == 2)
            {
                return Data[0, 0] * Data[1, 1] - Data[0, 1] * Data[1, 0];
            }
            int determinant = 0;
            for (int i = 0; i < Rows; i++)
            {
                HillCipherAttackMatrix minor = new HillCipherAttackMatrix(Rows - 1, Cols - 1);
                for (int j = 1; j < Rows; j++)
                {
                    for (int k = 0; k < Cols; k++)
                    {
                        if (k < i)
                        {
                            minor.Data[j - 1, k] = Data[j, k];
                        }
                        else if (k > i)
                        {
                            minor.Data[j - 1, k - 1] = Data[j, k];
                        }
                    }
                }
                int sign = (i % 2 == 0) ? 1 : -1;
                determinant += sign * Data[0, i] * minor.GetDeterminant();
            }
            return determinant;
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append("[ ");
            for (int i = 0; i < Rows; i++)
            {
                res.Append("( ");
                for (int j = 0; j < Cols; j++)
                {
                    res.Append(Data[i, j] + " ");
                }
                res.Append(")");
            }
            res.Append(" ]");
            res.AppendLine();
            return res.ToString();
        }

        internal (Vector<double> eigenvalues, Matrix<double> eigenvectors) CalcEigen(int mod)
        {
            // Convert int[,] to double[,] for compatibility with MathNet.Numerics
            double[,] doubleData = new double[Rows, Cols];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    doubleData[i, j] = Data[i, j];
                }
            }

            var matrix = Matrix<double>.Build.DenseOfArray(doubleData);

            return CalculateEigenValues(matrix, mod);
        }


        // Dieses Vorgehen ist nicht exakt da diese Berechnung normalerweise in R erfolgt
        private static (Vector<double> eigenvalues, Matrix<double> eigenvectors) CalculateEigenValues(Matrix<double> matrix, int modulus)
        {
            // Berechnung der Eigenwerte und Eigenvektoren
            var evd = matrix.Evd();

            // Reduktion der Eigenwerte im Modulo m
            var eigenvalues = evd.EigenValues.Map(x => x.Real % modulus);
            eigenvalues = eigenvalues.Map(x => x < 0 ? x + modulus : x);

            // Reduktion der Eigenvektoren im Modulo m
            var eigenvectors = evd.EigenVectors.Map(x => x % modulus);
            eigenvectors = eigenvectors.Map(x => x < 0 ? x + modulus : x);

            return (eigenvalues, eigenvectors);
        }
    }
}
