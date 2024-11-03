using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CrypTool.Plugins.HillCipherKnownPlainTextAttack
{
    public class HillCipherKnownPlainTextAttackUtils
    {
        public static bool IsValidKey(HillCipherKnownPlainTextAttackMatrix key, int m)
        {
            if (key.Rows != key.Cols)
            {
                return false;
            }
            // Determinant of the key matrix must be != 0
            var det = HillCipherKnownPlainTextAttackMatrix.getDeterminant(key);
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

        public static bool isValidTextMatrix(HillCipherKnownPlainTextAttackMatrix plainText, HillCipherKnownPlainTextAttackMatrix cipherText)
        {
            if (plainText.Rows != cipherText.Rows)
            {
                return false;
            }
            if (plainText.Cols != cipherText.Cols)
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

        public static HillCipherKnownPlainTextAttackMatrix mergeCipherText(HillCipherKnownPlainTextAttackMatrix[] cipherText, int n)
        {
            var result = new HillCipherKnownPlainTextAttackMatrix(0, 1);
            // create a new vector with the all the rows of the cipherText and the data of the cipherText
            foreach (var cipher in cipherText)
            {
                // create a new matrix with the size of the result matrix + 1
                var newResult = new HillCipherKnownPlainTextAttackMatrix(result.Rows + cipher.Rows, 1);
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
        public static HillCipherKnownPlainTextAttackMatrix createKeyMatrix(int[] text)
        {
            int n = (int)Math.Sqrt(text.Length);
            HillCipherKnownPlainTextAttackMatrix key = new HillCipherKnownPlainTextAttackMatrix(n, n);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    key.Data[i, j] = text[i * n + j];
                }
            }
            return key;
        }

        public static HillCipherKnownPlainTextAttackMatrix[] createTextMatrices(int[] text, int n, Dictionary<string, int> alphabet)
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

            HillCipherKnownPlainTextAttackMatrix[] matrices = new HillCipherKnownPlainTextAttackMatrix[cols];
            for (int i = 0; i < cols; i++)
            {
                matrices[i] = new HillCipherKnownPlainTextAttackMatrix(n, 1);
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
                    var mat = new HillCipherKnownPlainTextAttackMatrix(n, 1);
                    for (int j = index; j < text.Length; j++)
                    {
                        mat.Data[j - index, 0] = text[j];
                        index++;
                    }
                }
            }
            return matrices;
        }

        public static HillCipherKnownPlainTextAttackMatrix getSquareMatrix(HillCipherKnownPlainTextAttackMatrix[] matrices, int n)
        {
            HillCipherKnownPlainTextAttackMatrix result = new HillCipherKnownPlainTextAttackMatrix(n, n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result.Data[j, i] = matrices[i].Data[j, 0];
                }
            }

            return result;
        }

        public static int[] createarrayFromMatrix(HillCipherKnownPlainTextAttackMatrix matrix)
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

        internal static HillCipherKnownPlainTextAttackMatrix Encrypt(HillCipherKnownPlainTextAttackMatrix k, HillCipherKnownPlainTextAttackMatrix p, int m)
        {
            if (!IsValidKey(k, m))
            {
                throw new Exception("Invalid key");
            }
            HillCipherKnownPlainTextAttackMatrix result = HillCipherKnownPlainTextAttackMatrix.multiplyMatrix(k, p);
            HillCipherKnownPlainTextAttackMatrix resultMod = HillCipherKnownPlainTextAttackMatrix.modMatrix(result, m);
            return resultMod;
        }
    }
}
