using System;

namespace CrypTool.Plugins.HillCipherKnownPlainTextAttack
{  
	public class HillCipherKnownPlainTextAttackMatrix
	{
		public int Rows { get; set; }
		public int Cols { get; set; }
		public int[,] Data { get; set; }

		public HillCipherKnownPlainTextAttackMatrix(int rows, int cols)
		{
			Rows = rows;
			Cols = cols;
			Data = new int[rows, cols];
		}

		public HillCipherKnownPlainTextAttackMatrix()
		{
			Cols = 0;
			Rows = 0;
			Data = new int[0, 0];
		}

		public HillCipherKnownPlainTextAttackMatrix(int[,] data)
		{
			Rows = data.GetLength(0);
			Cols = data.GetLength(1);
			Data = data;
		}

		public bool Equals(HillCipherKnownPlainTextAttackMatrix a)
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

		public static HillCipherKnownPlainTextAttackMatrix multiplyMatrix(HillCipherKnownPlainTextAttackMatrix a, HillCipherKnownPlainTextAttackMatrix b)
		{
			if (a.Cols != b.Rows)
			{
				throw new Exception("The number of columns of the first matrix must be equal to the number of rows of the second matrix");
			}
			HillCipherKnownPlainTextAttackMatrix result = new HillCipherKnownPlainTextAttackMatrix(a.Rows, b.Cols);
			for (int i = 0; i < a.Rows; i++)
			{
				for (int j = 0; j < b.Cols; j++)
				{
					for (int k = 0; k < a.Cols; k++)
					{
						result.Data[i, j] += a.Data[i, k] * b.Data[k, j];
					}
				}
			}
			return result;
		}

		public static HillCipherKnownPlainTextAttackMatrix multiplyMatrixWithNumber(HillCipherKnownPlainTextAttackMatrix a, int b)
		{
			HillCipherKnownPlainTextAttackMatrix result = new HillCipherKnownPlainTextAttackMatrix(a.Rows, a.Cols);
			for (int i = 0; i < a.Rows; i++)
			{
				for (int j = 0; j < a.Cols; j++)
				{
					result.Data[i, j] = a.Data[i, j] * b;
				}
			}
			return result;
		}

		public static HillCipherKnownPlainTextAttackMatrix modMatrix(HillCipherKnownPlainTextAttackMatrix a, int m)
		{
			HillCipherKnownPlainTextAttackMatrix result = new HillCipherKnownPlainTextAttackMatrix(a.Rows, a.Cols);
			for (int i = 0; i < a.Rows; i++)
			{
				for (int j = 0; j < a.Cols; j++)
				{
					var value = a.Data[i, j] % m;
					result.Data[i, j] = value < 0 ? value + m : value;
				}
			}
			return result;
		}

		public static HillCipherKnownPlainTextAttackMatrix inverseMatrix(HillCipherKnownPlainTextAttackMatrix a, int m)
		{
			if (a.Rows != a.Cols)
			{
				throw new Exception("Matrix must be square");
			}

			int n = a.Rows;
			HillCipherKnownPlainTextAttackMatrix result = new HillCipherKnownPlainTextAttackMatrix(n, n);

			// create augmented Matrix
			int[,] augmented = new int[n, n * 2];
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					augmented[i, j] = a.Data[i, j];
					augmented[i, j + n] = (i == j) ? 1 : 0;
				}
			}
			var aug = new HillCipherKnownPlainTextAttackMatrix(augmented);

			// Gaussian Elimination in mod m
			var g_matrix = GaussianEliminationMod(aug, m);

			// Extract the inverse matrix from the augmented matrix
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					result.Data[i, j] = g_matrix.Data[i, j + n];
				}
			}

			return result;
		}

		public static HillCipherKnownPlainTextAttackMatrix GaussianEliminationMod(HillCipherKnownPlainTextAttackMatrix input, int m)
		{
			HillCipherKnownPlainTextAttackMatrix output = new HillCipherKnownPlainTextAttackMatrix(input.Rows, input.Cols);
			int n = input.Rows;
			int[,] a = input.Data;
			HillCipherKnownPlainTextAttackMatrix[] rows = new HillCipherKnownPlainTextAttackMatrix[n];

			// Create a matrix for each row
			for (int i = 0; i < n; i++)
			{
				rows[i] = new HillCipherKnownPlainTextAttackMatrix(1, input.Cols);
				for (int j = 0; j < input.Cols; j++)
				{
					rows[i].Data[0, j] = a[i, j];
				}
			}

			// Perform Gaussian Elimination
			int x = 0;
			foreach (var row in rows)
			{
				var pivot = row.Data[0, x];
				var pivot_inverse = modInverse(pivot, m);
				rows[x] = modMatrix(multiplyMatrixWithNumber(rows[x], pivot_inverse), m);
				for (int j = 0; j < n; j++)
				{
					if (j != x)
					{
						var factor = rows[j].Data[0, x];
						rows[j] = modMatrix(subMatrix(rows[j], modMatrix(multiplyMatrixWithNumber(rows[x], factor), m)), m);
					}
				}
				x++;
			}

			// create output matrix
			for (int i = 0; i < rows.Length; i++)
			{
				for (int j = 0; j < rows[i].Cols; j++)
				{
					output.Data[i, j] = rows[i].Data[0, j];
				}

			}
			return output;
		}


		static HillCipherKnownPlainTextAttackMatrix subMatrix(HillCipherKnownPlainTextAttackMatrix a, HillCipherKnownPlainTextAttackMatrix b)
		{
			if (a.Rows != b.Rows || a.Cols != b.Cols)
			{
				throw new Exception("Matrices must have the same dimensions");
			}
			HillCipherKnownPlainTextAttackMatrix result = new HillCipherKnownPlainTextAttackMatrix(a.Rows, a.Cols);
			for (int i = 0; i < a.Rows; i++)
			{
				for (int j = 0; j < a.Cols; j++)
				{
					result.Data[i, j] = a.Data[i, j] - b.Data[i, j];
				}
			}
			return result;
		}

		public static bool checkForIdentitiyMatrix(HillCipherKnownPlainTextAttackMatrix a, HillCipherKnownPlainTextAttackMatrix b, int m)
		{
			if (a.Rows > b.Cols)
			{
				return false;
			}
			HillCipherKnownPlainTextAttackMatrix i = new HillCipherKnownPlainTextAttackMatrix(a.Rows, a.Cols);
			for (int j = 0; j < a.Rows; j++)
			{
				for (int y = 0; y < a.Cols; y++)
				{
					if (j == y)
						i.Data[j, y] = 1;
				}
			}
			var res = multiplyMatrix(a, b);
			res = modMatrix(res, m);
			return res.Equals(i);
		}


		private static int modInverse(int a, int m)
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


		}

		public static int getDeterminant(HillCipherKnownPlainTextAttackMatrix a)
		{
			if (a.Rows != a.Cols)
			{
				throw new Exception("Matrix must be square");
			}
			if (a.Rows == 1)
			{
				return a.Data[0, 0];
			}
			if (a.Rows == 2)
			{
				return a.Data[0, 0] * a.Data[1, 1] - a.Data[0, 1] * a.Data[1, 0];
			}
			int determinant = 0;
			for (int i = 0; i < a.Rows; i++)
			{
				HillCipherKnownPlainTextAttackMatrix minor = new HillCipherKnownPlainTextAttackMatrix(a.Rows - 1, a.Cols - 1);
				for (int j = 1; j < a.Rows; j++)
				{
					for (int k = 0; k < a.Cols; k++)
					{
						if (k < i)
						{
							minor.Data[j - 1, k] = a.Data[j, k];
						}
						else if (k > i)
						{
							minor.Data[j - 1, k - 1] = a.Data[j, k];
						}
					}
				}
				int sign = (i % 2 == 0) ? 1 : -1;
				determinant += sign * a.Data[0, i] * getDeterminant(minor);
			}
			return determinant;
		}
	}
}