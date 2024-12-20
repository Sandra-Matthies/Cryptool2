using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LanguageStatisticsLib;

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

            for (int i = 0; i < mat.Cols; i++)
            {
                for (int j = 0; j < mat.Rows; j++)
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

        public static HillCipherAttackMatrix[] ConvertToVectors(int[] textNumbers, int n)
        {

            int cols = (int)Math.Ceiling((double)textNumbers.Length / n);
            HillCipherAttackMatrix[] matrices = new HillCipherAttackMatrix[cols];
                for (int i = 0; i < cols; i++)
                {
                    matrices[i] = new HillCipherAttackMatrix(n, 1);
                }
                int index = 0;
                for (int j = 0; j < cols; j++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (index < textNumbers.Length)
                        {
                            matrices[j].Data[i, 0] = textNumbers[index];
                            index++;
                        }
                        else
                        {
                            matrices[j].Data[i, 0] = 0; // Add 0`s if the text is shorter then the matrix
                        }
                    }
                }

            return matrices;
        }

        public static HillCipherAttackMatrix GetSquareMatrix(HillCipherAttackMatrix[] mats, int n, int iterationCount)
        {
            HillCipherAttackMatrix[] nVectors = new HillCipherAttackMatrix[n];
            for (int i = 0; i < n; i++)
            {
                nVectors[i] = mats[iterationCount + i];
            }
            HillCipherAttackMatrix result = new HillCipherAttackMatrix(n, n);
            for (int i = 0; i < n; i++) 
            {
                for (int j = 0; j < n; j++)
                {
                    result.Data[i, j] = nVectors[j].Data[i, 0];
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

        internal static HillCipherAttackMatrix Encrypt(HillCipherAttackMatrix k, HillCipherAttackMatrix[] p, int m)
        {
            if (!IsValidKey(k, m))
            {
                return null;
            }
            var mat = ConvertListOfVectorsToMatrix(p.ToList());
            var result = k.MultiplyMatrix(mat).ModMatrix(m);
            return result.ModMatrix(m);
        }

        // Convert a List of ColumVectors to a Matrix
        internal static HillCipherAttackMatrix ConvertListOfVectorsToMatrix(List<HillCipherAttackMatrix> list)
        {
            int n = list[0].Rows;
            HillCipherAttackMatrix result = new HillCipherAttackMatrix(n, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result.Data[j, i] = list[i].Data[j, 0];
                }
            }
            return result;
        }

        internal static string GeneratePlainTextForCiphertextOnlyAttack(string[] words, int dimension)
        {
            StringBuilder plainText = new StringBuilder();

            // Add all words from the dictionary to the plaintext
            int currentLength = 0;
            int plaintextLength = dimension * dimension * 2;
            plaintextLength = plaintextLength > 20 ? plaintextLength : 20;
            // Select all words from the dictionary which have a length of min 6 letters
            // Filter the,if, what, where, when, how, why, who etc. words
            string[] filteredWords = words.Where(word => word.Length >= 6).ToArray();

            foreach (var word in filteredWords)
            {
               plainText.Append(word);
            }
            return plainText.ToString();
        }

        // Caculate the score for a given plaintext
        internal static double CalculateScore(string plaintext, Dictionary<string, int> alphabet, int language)
        {
            double score = 0;
            // English
            var letterFrequency = HillCipherAttackRessources.LetterFrequenciesEN;
            var bigramFrequency = HillCipherAttackRessources.BigramFrequenciesEN;
            var trigramFrequency = HillCipherAttackRessources.TrigramFrequenciesEN;
            if (language == 1)
            {
                // German
                bigramFrequency = HillCipherAttackRessources.BigramFrequenciesDE;
                trigramFrequency = HillCipherAttackRessources.TrigramFrequenciesDE;
                letterFrequency = HillCipherAttackRessources.LetterFrequenciesDE;
            }

            // Calculates the score for the plaintext with the alphabet from the settings and the frequencies of the language in the Ressources
            foreach (char c in plaintext)
            {
                if (alphabet.ContainsKey(c.ToString()))
                {
                    score += letterFrequency[c];
                }
            }
            for (int i = 0; i < plaintext.Length - 1; i++)
            {
                string bigram = plaintext.Substring(i, 2);
                if (bigramFrequency.ContainsKey(bigram))
                {
                    score += bigramFrequency[bigram];
                }
            }

            for (int i = 0; i < plaintext.Length - 2; i++)
            {
                string trigram = plaintext.Substring(i, 3);
                if (trigramFrequency.ContainsKey(trigram))
                {
                    score += trigramFrequency[trigram];
                }
            }
            return (score * 100)/ plaintext.Length;
            
        }
    }
}
