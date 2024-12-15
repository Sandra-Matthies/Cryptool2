using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.Plugins.HillCipherAttack
{
    internal class HillCipherAttackRessources
    {
        //https://en.wikipedia.org/wiki/Letter_frequency
        public static Dictionary<char, double> LetterFrequenciesEN = new Dictionary<char, double>
        {
            { 'A', 0.0820 },
            { 'B', 0.0150 },
            { 'C', 0.0280 },
            { 'D', 0.0430 },
            { 'E', 0.1270 },
            { 'F', 0.0220 },
            { 'G', 0.0200 },
            { 'H', 0.0610 },
            { 'I', 0.0700 },
            { 'J', 0.0015 },
            { 'K', 0.0077 },
            { 'L', 0.0400 },
            { 'M', 0.0240 },
            { 'N', 0.0670 },
            { 'O', 0.0750 },
            { 'P', 0.0190 },
            { 'Q', 0.0009 },
            { 'R', 0.0600 },
            { 'S', 0.0630 },
            { 'T', 0.0910 },
            { 'U', 0.0280 },
            { 'V', 0.0098 },
            { 'W', 0.0240 },
            { 'X', 0.0015 },
            { 'Y', 0.0200 },
            { 'Z', 0.0007 }
        };
        // https://en.wikipedia.org/wiki/Bigram
        public static Dictionary<string, double> BigramFrequenciesEN = new Dictionary<string, double>
        {
            { "TH", 0.0356 },
            { "HE", 0.0307},
            { "IN", 0.0243 },
            { "ER", 0.0205 },
            { "AN", 0.0199 },
        };
        //https://en.wikipedia.org/wiki/Trigram
        public static Dictionary<string, double> TrigramFrequenciesEN = new Dictionary<string, double>
        {
            { "THE", 0.0181 },
            { "AND", 0.0073 },
            { "ING", 0.0072 },
            { "ENT", 0.0042 },
            { "ION", 0.0042 },
        };


        //http://practicalcryptography.com/cryptanalysis/letter-frequencies-various-languages/german-letter-frequencies/

        public static Dictionary<char, double> LetterFrequenciesDE = new Dictionary<char, double>
        {
            { 'A', 0.0634 },
            { 'B', 0.0221 },
            { 'C', 0.0271 },
            { 'D', 0.0492 },
            { 'E', 0.1599 },
            { 'F', 0.0180 },
            { 'G', 0.0302 },
            { 'H', 0.0411 },
            { 'I', 0.0760 },
            { 'J', 0.0027 },
            { 'K', 0.0150 },
            { 'L', 0.0372 },
            { 'M', 0.0275 },
            { 'N', 0.0959 },
            { 'O', 0.0275 },
            { 'P', 0.0106 },
            { 'Q', 0.0004 },
            { 'R', 0.0771 },
            { 'S', 0.0641 },
            { 'T', 0.0643 },
            { 'U', 0.0376 },
            { 'V', 0.0094 },
            { 'W', 0.0140 },
            { 'X', 0.0007 },
            { 'Y', 0.0013 },
            { 'Z', 0.0122 },
            { 'Ä', 0.0054 },
            { 'Ö', 0.0024 },
            { 'Ü', 0.0063 }

        };
        public static Dictionary<string, double> BigramFrequenciesDE = new Dictionary<string, double>
        {
            { "ER", 0.0390 },
            { "EN", 0.0361 },
            { "CH", 0.0236 },
            { "DE", 0.0231 },
            { "EI", 0.0198 },
        };

        public static Dictionary<string, double> TrigramFrequenciesDE = new Dictionary<string, double>
        {
            { "DER", 0.0104 },
            { "EIN", 0.0083 },
            { "SCH", 0.0076 },
            { "ICH", 0.0075 },
            { "NDE", 0.0072 },
        };
    }
}
