using CrypTool.PluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CrypTool.Plugins.HillCipherAttack
{
    public class HillCipherAttackUtils
    {
        public static bool IsValidKey(HillCipherAttackMatrix key, int m)
        {
            if (key == null)
            {
                return false;
            }
            if (key.Rows != key.Cols)
            {
                return false;
            }
            // Determinant of the key matrix must be != 0
            var det = key.GetDeterminant();
            det = det % m;
            if (det == 0)
            {
                return false;
            }

            if(det < 1)
            {
                det += m;
            }

            // The Determinant and m must be coprime
            if (GCD(det, m) != 1)
            {
                return false;
            }
            return true;
        }

        public static bool IsValidTextMatrix(HillCipherAttackMatrix plainText, HillCipherAttackMatrix cipherText)
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

        public static string ConvertToText(HillCipherAttackMatrix mat, Dictionary<string, int> alphabet)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < mat.Rows; i++)
            {
                for (int j = 0; j < mat.Cols; j++)
                {
                    int value = mat.Data[j, i];
                    result.Append(GetKeyFromValue(alphabet, value));
                }
            }

            return result.ToString();
        }

        public static string GetKeyFromValue(Dictionary<string, int> dictionary, int value)
        {
            foreach (var kvp in dictionary)
            {
                if (kvp.Value == value)
                {
                    return kvp.Key;
                }
            }
            throw new ArgumentException($"Index {value} not found in the alphabet.");
        }

        public static HillCipherAttackMatrix ConvertToMatrix(string text, int n, Dictionary<string, int> alphabet)
        {
            int[] textNumbers = new int[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                if (alphabet.ContainsKey(text[i].ToString()))
                {
                    textNumbers[i] = alphabet[text[i].ToString()];
                }
                else
                {
                    throw new ArgumentException($"Character {text[i]} is not in the alphabet.");
                }
            }

            int cols = (int)Math.Ceiling((double)textNumbers.Length / n);
            HillCipherAttackMatrix matrix = new HillCipherAttackMatrix(n, cols);
            int index = 0;
            for (int j = 0; j < cols ; j++)
            {
                for (int i = 0; i < n; i++)
                { 
                    if (index < textNumbers.Length)
                    {
                        matrix.Data[i, j] = textNumbers[index];
                        index++;
                    }
                    else
                    {
                        matrix.Data[i, j] = 0; // Auffüllen mit Nullen, wenn der Text kürzer als die Matrix ist
                    }
                }
            }

            return matrix;
        }

        public static HillCipherAttackMatrix GetSquareMatrix(HillCipherAttackMatrix mat, int n)
        {
            HillCipherAttackMatrix result = new HillCipherAttackMatrix(n, n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i < mat.Rows && j < mat.Cols)
                    {
                        result.Data[i, j] = mat.Data[i,j];
                    }
                    else
                    {
                        result.Data[i, j] = 0; // Auffüllen mit Nullen, wenn außerhalb der Dimension der Eingabematrix
                    }
                }
            }

            return result;
        }

        public static int[] CreatearrayFromMatrix(HillCipherAttackMatrix matrix)
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
                return null;
            }
            HillCipherAttackMatrix result = k.MultiplyMatrix(p);
            HillCipherAttackMatrix resultMod = result.ModMatrix(m);
            return resultMod;
        }
    }
}
