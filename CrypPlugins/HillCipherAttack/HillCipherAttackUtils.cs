using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.Plugins.HillCipherAttack
{
    public class HillCipherAttackUtils
    {
        public static bool IsValidKey(HillCipherAttackMatrix key, int m)
        {
            if (key.Rows != key.Cols)
            {
                return false;
            }
            // Determinant of the key matrix must be != 0
            var det = HillCipherAttackMatrix.getDeterminant(key);
            if (det == 0)
            {
                return false;
            }

            // The Determinant and m must be coprime
            if (GCD(det, m) != 1)
            {
                return false;
            }
            return true;
        }

        public static bool isValidTextMatrix(ModMatrix plainText, ModMatrix cipherText)
        {
            if(plainText.Dimension != cipherText.Dimension)
            {
                return false;
            }
            return true;
        }

        public static int GCD(int a, int b)
        {
            if (b == 0)
            {
                return a;
            }
            else
            {
                return GCD(b, a % b);
            }
        }

        public static BigInteger[] ConverToArray(ModMatrix matrix)
        {
            BigInteger[] result = new BigInteger[matrix.Dimension * matrix.Dimension];
            for (int i = 0; i < matrix.Dimension; i++)
            {
                for (int j = 0; j < matrix.Dimension; j++)
                {
                    result[i * matrix.Dimension + j] = matrix[i, j];
                }
            }
            return result;
        }

        public static HillCipherAttackMatrix mergeCipherText(HillCipherAttackMatrix[] cipherText, int n)
        {
            var result = new HillCipherAttackMatrix(0, 1);
            // create a new vector with the all the rows of the cipherText and the data of the cipherText
            foreach (var cipher in cipherText)
            {
                // create a new matrix with the size of the result matrix + 1
                var newResult = new HillCipherAttackMatrix(result.Rows + cipher.Rows, 1);
                // copy the data of the result matrix to the new result matrix
                for (int i = 0; i < result.Rows; i++)
                {
                    newResult.Data[i, 0] = result.Data[i, 0];
                }
                // copy the data of the cipher matrix to the new result matrix
                for (int i = 0; i < cipher.Rows; i++)
                {
                    newResult.Data[result.Rows + i, 0] = cipher.Data[i, 0];
                }
                // set the result matrix to the new result matrix
                result = newResult;
            }
            return result;
        }

        // Create a n*n matrix from a string
        public static HillCipherAttackMatrix createKeyMatrix(int[] text)
        {
            int n = (int)Math.Sqrt(text.Length);
            HillCipherAttackMatrix key = new HillCipherAttackMatrix(n, n);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    key.Data[i, j] = text[i * n + j];
                }
            }
            return key;
        }

        public static HillCipherAttackMatrix[] createTextMatrices(int[] text, int n, Dictionary<string, int> alphabet)
        {
            int m = text.Length;
            int cols = m / n;
            while (cols * n < m)
            {
                cols++;
            }
            while (cols * n > m)
            {
                int[] tmp = new int[text.Length + 1];
                text.CopyTo(tmp, 0);
                alphabet.TryGetValue(" ", out int emptyValue);
                tmp[tmp.Length - 1] = emptyValue;
                m = tmp.Length;
                text = tmp;
            }

            HillCipherAttackMatrix[] matrices = new HillCipherAttackMatrix[cols];
            for (int i = 0; i < cols; i++)
            {
                matrices[i] = new HillCipherAttackMatrix(n, 1);
                for (int j = 0; j < n; j++)
                {
                    matrices[i].Data[j, 0] = text[i * n + j];
                }
            }
            if (text.Length > matrices.Length * n)
            {
                int index = matrices.Length * n;
                while (index < text.Length)
                {
                    var mat = new HillCipherAttackMatrix(n, 1);
                    for (int j = index; j < text.Length; j++)
                    {
                        mat.Data[j - index, 0] = text[j];
                        index++;
                    }
                }
            }
            return matrices;
        }

        public static HillCipherAttackMatrix getSquareMatrix(HillCipherAttackMatrix[] matrices, int n)
        {
            HillCipherAttackMatrix result = new HillCipherAttackMatrix(n, n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result.Data[j, i] = matrices[i].Data[j, 0];
                }
            }

            return result;
        }

        public static int[] createarrayFromMatrix(HillCipherAttackMatrix matrix)
        {
            int[] result = new int[matrix.Rows * matrix.Cols];
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    result[i * matrix.Cols + j] = matrix.Data[i, j];
                }
            }
            return result;
        }

        internal static HillCipherAttackMatrix Encrypt(HillCipherAttackMatrix k, HillCipherAttackMatrix p, int m)
        {
            if (!IsValidKey(k, m))
            {
                throw new Exception("Invalid key");
            }
            HillCipherAttackMatrix result = HillCipherAttackMatrix.multiplyMatrix(k, p);
            HillCipherAttackMatrix resultMod = HillCipherAttackMatrix.modMatrix(result, m);
            return resultMod;
        }
    }
}
