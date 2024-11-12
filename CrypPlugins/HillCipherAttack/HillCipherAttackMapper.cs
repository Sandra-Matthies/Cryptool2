using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.Plugins.HillCipherAttack
{
    public class HillCipherAttackMapper
    {
        public static Dictionary<string, int> mapAlphabetToNumbers(string aplphabet)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            for (int i = 0; i < aplphabet.Length; i++)
            {
                map.Add(aplphabet[i].ToString(), i);
            }

            return map;
        }

        // Map the given letters to numbers using the given alphabet
        public static int[] mapLettersByAlphabetToNumbers(string letters, Dictionary<string, int> alphabet)
        {

            int[] result = new int[letters.Length];
            for (int i = 0; i < letters.Length; i++)
            {
                result[i] = alphabet[letters[i].ToString()];
            }
            return result;
        }

        // Map the given numbers to letters using the given alphabet
        public static string mapNumbersByAlphabetToLetters(int[] numbers, Dictionary<string, int> alphabet)
        {
            string result = "";
            foreach (int number in numbers)
            {
                foreach (KeyValuePair<string, int> entry in alphabet)
                {
                    if (entry.Value == number)
                    {
                        result += entry.Key;
                    }
                }
            }
            return result;
        }

        public static string mapNumbersByAlphabetToLetters(BigInteger[] numbers, Dictionary<string, int> alphabet)
        {
            string result = "";
            foreach (int number in numbers)
            {
                foreach (KeyValuePair<string, int> entry in alphabet)
                {
                    if (entry.Value == number)
                    {
                        result += entry.Key;
                    }
                }
            }
            return result;
        }
    }
}
