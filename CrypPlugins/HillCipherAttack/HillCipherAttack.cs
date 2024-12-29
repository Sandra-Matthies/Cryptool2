/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.HillCipherAttack
{
    // HOWTO: Plugin developer HowTo can be found here: https://github.com/CrypToolProject/CrypTool-2/wiki/Developer-HowTo

    // HOWTO: Change author name, email address, organization and URL.
    [Author("Sandra Matthies", "sandra_matthies@outlook.de", "Softwaredeveloper", "https://www.cryptool.org")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("CrypTool.Plugins.HillCipherAttack.Properties.Resources", "HillCipherAttackCaption", "HillCipherAttackTooltip", "HillCipherAttack/userdoc.xml", new[] { "HillCipherAttack/ressources/hill-cipher.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class HillCipherAttack : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly HillCipherAttackSettings _settings = new HillCipherAttackSettings();
        private readonly HillCipherAttackPresentation _presentation = new HillCipherAttackPresentation();

        private const int MaxBestListEntries = 25;
        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "PCaption", "PTooltip")]
        public string Plain
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "CCaption", "CTooltip")]
        public string Cipher
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "DictCaption", "DictTooltip")]
        public string[] Dict { get; set; }


        [PropertyInfo(Direction.OutputData, "KCaption", "KTooltip")]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "KMatCaption", "KMatTooltip")]
        public string KeyMatrix
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "KDimCaption", "KDimTooltip")]
        public int KeyDimension
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return _presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        private bool CheckInputs(string plain)
        {
            for (int j = 0; j < plain.Length; j++)
            {
                if (_settings.Alphabet.IndexOf(plain[j]) < 0)
                {
                    GuiLogMessage(string.Format(Properties.Resources.InputContainsIllegalCharacter, plain[j], j), NotificationLevel.Warning);
                    return false;
                }
            }

            for (int j = 0; j < Cipher.Length; j++)
            {
                if (_settings.Alphabet.IndexOf(Cipher[j]) < 0)
                {
                    GuiLogMessage(string.Format(Properties.Resources.InputContainsIllegalCharacter, Cipher[j], j), NotificationLevel.Warning);
                    return false;
                }
            }
            return true;
        }

        private int[] getTextNumbers(string text, Dictionary<string, int> alphabet)
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
                    throw new ArgumentException(string.Format(Properties.Resources.InputContainsIllegalCharacter, text[i]));
                }
            }
            return textNumbers;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // Clear presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((HillCipherAttackPresentation)Presentation).ResultOutput.Clear();

            }, null);

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            ProgressChanged(0, 1);
            try
            {
                var alphabet_numbers = HillCipherAttackMapper.mapAlphabetToNumbers(_settings.Alphabet);
                int key_dimension = _settings.StartKeyDimension < 0 ? 1 : _settings.StartKeyDimension;

                HillCipherAttackMatrix key = new HillCipherAttackMatrix();
                HillCipherAttackMatrix[] cipher_mats;
                HillCipherAttackMatrix[] plain_mats;

                var plain = "";
                // Check which type of Attack is given
                if (!_settings.IsUnkownPlaintextAttack)
                {
                    plain = Plain;
                }
                else
                {
                    if (Dict.Length < 1)
                    {
                        GuiLogMessage(Properties.Resources.NoDictionary, NotificationLevel.Error);
                        return;
                    }
                    // for testing only 10 words
                    string[] words = { "WORDS", "VARIOUS", "LENGTHS", "ANOTHER", "SELECTION", "PROCESS", "TESTING", "FILTERING", "DEVELOPMENT", "ACTION" , "AUTOMOBIL", "WIRTSCHAFT"};
                    Dict = words;

                    plain = HillCipherAttackUtils.GeneratePlainTextForCiphertextOnlyAttack(Dict, key_dimension);

                }
                if (!CheckInputs(plain))
                {
                    return;
                }
                // For the case that the Plain is shorter then the Cipher
                for (int i = 0; i < Cipher.Length - plain.Length; i++)
                {
                    plain += "A";
                }
                var cipher_numbers = getTextNumbers(Cipher, alphabet_numbers);
                var plain_numbers = getTextNumbers(plain, alphabet_numbers);
                var isWrongKey = true;
                do
                {
                    key_dimension++;
                    cipher_mats = HillCipherAttackUtils.ConvertToVectors(cipher_numbers, key_dimension);
                    plain_mats = HillCipherAttackUtils.ConvertToVectors(plain_numbers, key_dimension);

                    if (!CheckForEnoughData(plain, Cipher, key_dimension))
                    {
                        GuiLogMessage(string.Format(Properties.Resources.NotEnoughData, key_dimension), NotificationLevel.Warning);
                        return;
                    }
                    int iterationsForSquare = cipher_mats.Length / key_dimension > 5 ? 5 : cipher_mats.Length / key_dimension;
                    int i = -1;
                    while (i < iterationsForSquare && isWrongKey)
                    {
                        i++;
                        var plain_sq_mat = HillCipherAttackUtils.GetSquareMatrix(plain_mats, key_dimension, i);
                        var cipher_sq_mat = HillCipherAttackUtils.GetSquareMatrix(cipher_mats, key_dimension, i);
                        if (HillCipherAttackUtils.GCD(plain_sq_mat.GetDeterminant(), _settings.Modulus) != 1)
                        {
                            GuiLogMessage(string.Format(Properties.Resources.NotInvertable, plain_sq_mat.ToString()), NotificationLevel.Warning);
                            continue;
                        }
                        if (_settings.IsAdjointCalculation)
                        {
                            key = KnownPlainTextAttackAdjointMatrix(plain_sq_mat, cipher_sq_mat);
                        }
                        else
                        {
                            key = KnownPlainTextAttackEigenvectors(plain_sq_mat, cipher_sq_mat, _settings.Modulus);
                        }
                        if (key != null)
                        {
                            GuiLogMessage(string.Format(Properties.Resources.KeyValue, key.ToString()), NotificationLevel.Info);
                        }
                        else
                        {
                            continue;
                        }
                        if (_settings.IsUnkownPlaintextAttack)
                        {
                            isWrongKey = CompareCipherTextWithTreshhold(key, plain_mats, alphabet_numbers, _settings.Treshhold);
                        }
                        else
                        {
                            isWrongKey = !CompareCipherText(key, plain_mats, alphabet_numbers);
                        }

                        if (iterationsForSquare >= (int)(Cipher.Length / 3) && isWrongKey)
                        {
                            GuiLogMessage(string.Format(Properties.Resources.NoValidKeyForDim, key_dimension.ToString()), NotificationLevel.Warning);
                            if (iterationsForSquare == (int)(Cipher.Length / 3))
                            {
                                i++;
                            }
                            continue;
                        }
                        if (!isWrongKey && _settings.IsUnkownPlaintextAttack)
                        {
                            
                            var cy = Encrypt(key, plain_mats, alphabet_numbers);
                            if (cy != null)
                            {
                                var resultEntry = new BestListEntry
                                {
                                    Key = HillCipherAttackMapper.mapNumbersByAlphabetToLetters(HillCipherAttackUtils.CreatearrayFromMatrix(key), alphabet_numbers),
                                    Plain = plain,
                                    KeyDimension = key.Cols,
                                    Score = HillCipherAttackUtils.CalculateScore(cy, alphabet_numbers, _settings.Language),
                                    KeyMatrix = key.ToOutputString()
                                };
                                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    try
                                    {
                                        //Insert new entry at correct place to sustain order of list:
                                        int insertIndex = _presentation.ResultOutput.BestList.TakeWhile(e => e.Score > resultEntry.Score).Count();
                                        _presentation.ResultOutput.BestList.Insert(insertIndex, resultEntry);


                                        if (_presentation.ResultOutput.BestList.Count > MaxBestListEntries)
                                        {
                                            _presentation.ResultOutput.BestList.RemoveAt(MaxBestListEntries);
                                        }
                                        int ranking = 1;
                                        foreach (BestListEntry e in _presentation.ResultOutput.BestList)
                                        {
                                            e.Ranking = ranking;
                                            ranking++;
                                        }
                                        _presentation.ResultOutput.Alphabet = _settings.Alphabet;
                                        _presentation.ResultOutput.Modulo = _settings.Modulus;
                                        _presentation.ResultOutput.Cipher = Cipher;
                                        _presentation.CrypAnalysisResultListView.ScrollIntoView(_presentation.CrypAnalysisResultListView.Items);
                                    }
                                    catch (Exception)
                                    {
                                        //do nothing
                                    }
                                }, null);

                                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    ((HillCipherAttackPresentation)Presentation).ResultOutput.BestList = new List<BestListEntry>(_presentation.ResultOutput.BestList);
                                }, null);
                                Key = _presentation.ResultOutput.BestList[0].Key;
                                KeyDimension = _presentation.ResultOutput.BestList[0].KeyDimension;
                                KeyMatrix = _presentation.ResultOutput.BestList[0].KeyMatrix;
                                OnPropertyChanged(nameof(Key));
                                OnPropertyChanged(nameof(KeyMatrix));
                                OnPropertyChanged(nameof(KeyDimension));
                                isWrongKey = true;
                            }
                            else
                            {
                                isWrongKey = true;
                            }

                        }

                    }

                } while ((isWrongKey && !_settings.IsUnkownPlaintextAttack) || (_settings.IsUnkownPlaintextAttack && key_dimension <= 10));

                var key_text = "";
                if (!isWrongKey)
                {
                    var res_key_numbers = HillCipherAttackUtils.CreatearrayFromMatrix(key);
                    key_text = HillCipherAttackMapper.mapNumbersByAlphabetToLetters(res_key_numbers, alphabet_numbers);
                    KeyDimension = key_dimension;
                    Key = key_text;
                    KeyMatrix = key.ToOutputString();
                }
                else
                {
                    KeyDimension = 0;
                    Key = null;
                    KeyMatrix = null;
                }
                //stopwatch.Stop();
                //GuiLogMessage($"Execution time: {stopwatch.ElapsedMilliseconds} ms", NotificationLevel.Info);

                OnPropertyChanged(nameof(Key));
                OnPropertyChanged(nameof(KeyMatrix));
                OnPropertyChanged(nameof(KeyDimension));




            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Properties.Resources.ErrorMessage, ex.Message), NotificationLevel.Error);
            }


            ProgressChanged(1, 1);
        }
        /// <summary>
        /// Known Plaintext Attack using the eigenvectors.
        /// It is possible that no key is found.
        /// It is using double values for the calculation.
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="ciphertext"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        static HillCipherAttackMatrix KnownPlainTextAttackEigenvectors(HillCipherAttackMatrix plaintext, HillCipherAttackMatrix ciphertext, int mod)
        {
            int n = plaintext.Rows;
            HillCipherAttackMatrix keyMatrix = new HillCipherAttackMatrix(n, n);

            var inverse_plain = plaintext.InverseByEigenVectors(mod);


            keyMatrix = inverse_plain.MultiplyMatrix(ciphertext);

            return keyMatrix.ModMatrix(mod);
        }

        private bool CompareCipherText(HillCipherAttackMatrix key, HillCipherAttackMatrix[] plain_mats, Dictionary<string, int> alphabet_numbers)
        {
            var _cipherText = Encrypt(key, plain_mats, alphabet_numbers);
            if (_cipherText == null)
            {
                return false;
            }
            if (!Cipher.Equals(_cipherText))
            {
                GuiLogMessage(Properties.Resources.IncorrectKey, NotificationLevel.Info);
                return false;
            }
            return true;
        }

        private bool CompareCipherTextWithTreshhold(HillCipherAttackMatrix key, HillCipherAttackMatrix[] plain_mats, Dictionary<string, int> alphabet_numbers, int treshold)
        {
            var _cipherText = Encrypt(key, plain_mats, alphabet_numbers);
            if (_cipherText == null)
            {
                return false;
            }

            // uses Levenshtein Distance with treshhold
            int sourceLength = Cipher.Length;
            int targetLength = _cipherText.Length;

            int[,] distance = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; distance[i, 0] = i++) { }
            for (int j = 0; j <= targetLength; distance[0, j] = j++) { }

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (_cipherText[j - 1] == Cipher[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength] <= treshold;
        }

        private bool CheckForEnoughData(string plain, string cipher, int key_dimension)
        {
            if (Math.Sqrt(plain.Length) < key_dimension || Math.Sqrt(cipher.Length) < key_dimension)
            {
                return false;
            }
            return true;
        }

        private string Encrypt(HillCipherAttackMatrix key_matrix, HillCipherAttackMatrix[] plain_mat, Dictionary<string, int> alphabet_numbers)
        {
            HillCipherAttackMatrix cipher_mat;

            var vec = HillCipherAttackUtils.Encrypt(key_matrix, plain_mat, _settings.Modulus);
            if (vec != null)
            {
                cipher_mat = vec;
            }
            else
            {
                return null;
            }


            var cipher_text = HillCipherAttackUtils.ConvertToText(cipher_mat, alphabet_numbers);

            return cipher_text;
        }

        /// <summary>
        /// Known Plaintext Attack using the adjoint matrix
        /// </summary>
        /// <param name="plain_text_matrix"></param>
        /// <param name="cipher_text_matrix"></param>
        /// <returns></returns>
        private HillCipherAttackMatrix KnownPlainTextAttackAdjointMatrix(HillCipherAttackMatrix plain_text_matrix, HillCipherAttackMatrix cipher_text_matrix)
        {
            if (!HillCipherAttackUtils.IsValidTextMatrix(plain_text_matrix, cipher_text_matrix))
            {
                GuiLogMessage(Properties.Resources.InvalidException, NotificationLevel.Warning);
                return null;
            }

            // Create the inverse of the plain text matrix
            HillCipherAttackMatrix inversePlainText = HillCipherAttackMatrix.InverseMatrix(plain_text_matrix, _settings.Modulus);
            if (!inversePlainText.CheckForIdentitiyMatrix(plain_text_matrix, _settings.Modulus))
            {
                GuiLogMessage(Properties.Resources.NoIdentityMatrix, NotificationLevel.Warning);
                return null;
            }

            // Calculate K = C * P^-1 mod m
            var key = cipher_text_matrix.MultiplyMatrix(inversePlainText);
            key = key.ModMatrix(_settings.Modulus);
            return key;
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }

    public class AttackResult : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int modulo;
        private string alphabet;
        private string cipher;
        private List<BestListEntry> bestList = new List<BestListEntry>();

        public int Modulo
        {
            get => modulo;
            set
            {
                modulo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Modulo)));
            }
        }
        public string Cipher
        {
            get => cipher;
            set
            {
                cipher = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cipher)));
            }
        }

        public string Alphabet
        {
            get => alphabet;
            set
            {
                alphabet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Alphabet)));
            }
        }
        
        public List<BestListEntry> BestList
        {
            get => bestList;
            set
            {
                bestList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BestList)));
            }
        }

        public void Clear()
        {
            BestList.Clear();
            Alphabet = "";
            Cipher = "";
            Modulo = 0;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BestList)));
        }


    }

    public class BestListEntry
    { 
        public int Ranking;
        public string Key { get; set; }

        public string KeyMatrix { get; set; }

        public string Plain { get; set; }
        public int KeyDimension { get; set; }
        public double Score { get; set; }
    }

}
