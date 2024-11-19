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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.HillCipherAttack
{
    // HOWTO: Plugin developer HowTo can be found here: https://github.com/CrypToolProject/CrypTool-2/wiki/Developer-HowTo

    // HOWTO: Change author name, email address, organization and URL.
    [Author("Sandra Matthies", "sandra_matthies@outlook.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("CrypTool.Plugins.HillCipherAttack.Properties.Resources", "HillCipherAttackCaption", "HillCipherAttackTooltip", "HillCipherAttack/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class HillCipherAttack : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly HillCipherAttackSettings _settings = new HillCipherAttackSettings();
        private readonly HillCipherAttackPresentation _presentation = new HillCipherAttackPresentation();

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

        private bool CheckInputs()
        {
            for (int j = 0; j < Plain.Length; j++)
            {
                if (_settings.Alphabet.IndexOf(Plain[j]) < 0)
                {
                    GuiLogMessage(string.Format(Properties.Resources.InputContainsIllegalCharacter, Plain[j], j), NotificationLevel.Warning);
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

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            try
            {
                if (!CheckInputs())
                {
                    return;
                }
            }
            catch (ArithmeticException ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Warning);
            }
            try
            {
                var alphabet_numbers = HillCipherAttackMapper.mapAlphabetToNumbers(_settings.Alphabet);
                int key_dimension = _settings.StartKeyDimension < 0 ? 1 : _settings.StartKeyDimension;

                HillCipherAttackMatrix key = new HillCipherAttackMatrix();
                HillCipherAttackMatrix[] cipher_mats;
                HillCipherAttackMatrix[] plain_mats;

                // For the case that the Plain is shorter then the Cipher
                var plain = Plain;
                for (int i = 0; i < Cipher.Length - Plain.Length; i++)
                {
                    plain += "A";
                }
                var isWrongKey = true;
                do
                {
                    key_dimension++;
                    cipher_mats = HillCipherAttackUtils.ConvertToVectors(Cipher, key_dimension, alphabet_numbers);
                    plain_mats = HillCipherAttackUtils.ConvertToVectors(plain, key_dimension, alphabet_numbers);

                    if (!CheckForEnoughData(plain, Cipher, key_dimension))
                    {
                        GuiLogMessage(string.Format(Properties.Resources.NotEnoughData, key_dimension), NotificationLevel.Warning);
                        return;
                    }
                    int iterationsForSquare = cipher_mats.Length/key_dimension;
                    for (int i =0; i< iterationsForSquare; i++) { 
                        var plain_sq_mat = HillCipherAttackUtils.GetSquareMatrix(plain_mats, key_dimension, i);
                        var cipher_sq_mat = HillCipherAttackUtils.GetSquareMatrix(cipher_mats, key_dimension,i);
                        GuiLogMessage(string.Format("Dimension: {0}", key_dimension), NotificationLevel.Info);
                        GuiLogMessage(plain_sq_mat.ToString(), NotificationLevel.Info);
                        if (HillCipherAttackUtils.GCD(plain_sq_mat.GetDeterminant(), _settings.Modulus) != 1)
                        {
                            GuiLogMessage(string.Format(Properties.Resources.NotInvertable), NotificationLevel.Warning);
                            continue;
                        }

                        key = KnownPlainTextAttack(plain_sq_mat, cipher_sq_mat);
                        //key = TrellisAttack(plain_sq_mat, cipher_sq_mat, _settings.Modulus);
                        if (key != null)
                        {
                            GuiLogMessage(string.Format("Key: {0}", key.ToString()), NotificationLevel.Info);
                        }
                        else
                        {
                            GuiLogMessage(string.Format(Properties.Resources.NotInvertable), NotificationLevel.Info);
                            continue;
                        }

                        isWrongKey = !CompareCipherText(key, plain_mats, alphabet_numbers);
                        // if wrongkey is false return to upper loop
                        if (!isWrongKey)
                        {
                            continue;
                        }
                        if(iterationsForSquare >= Cipher.Length / 3 && isWrongKey)
                        {
                            continue;
                        }

                    }

                } while (isWrongKey && key_dimension < 5);

                var res_key_numbers = HillCipherAttackUtils.CreatearrayFromMatrix(key);
                var key_text = HillCipherAttackMapper.mapNumbersByAlphabetToLetters(res_key_numbers, alphabet_numbers);
                KeyDimension = key_dimension;
                Key = key_text;
                KeyMatrix = key.ToOutputString();
                OnPropertyChanged(nameof(Key));
                OnPropertyChanged(nameof(KeyMatrix));

                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.AlphabetValue.Content = _settings.Alphabet;
                    _presentation.PlaintextValue.Content = Plain;
                    _presentation.CipherValue.Content = Cipher;
                    _presentation.KeyValue.Content = key_text;
                    _presentation.KeyDimensionValue.Content = key_dimension;
                }, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Properties.Resources.ErrorMessage, ex.Message), NotificationLevel.Error);
            }


            ProgressChanged(1, 1);
        }

        // TODO: check if it works
        static HillCipherAttackMatrix TrellisAttack(HillCipherAttackMatrix plaintext, HillCipherAttackMatrix ciphertext, int mod)
        {
            int n = plaintext.Rows;
            HillCipherAttackMatrix keyMatrix = new HillCipherAttackMatrix(n, n);

            //  
            var plain_eigen = plaintext.CalcEigen(mod);
            var cipher_eigen = ciphertext.CalcEigen(mod);

            var tmp = cipher_eigen.eigenvectors * plain_eigen.eigenvectors.Inverse();

            // Convert to HillCipherAttackMatrix
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    keyMatrix.Data[i, j] = (int) tmp[i, j];
                }
            }
            return keyMatrix;
        }

        private bool CompareCipherText(HillCipherAttackMatrix key, HillCipherAttackMatrix[] plain_mats, Dictionary<string, int> alphabet_numbers)
        {
            var _cipherText = Encrypt(key, plain_mats, alphabet_numbers);
            return _cipherText == Cipher;
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

        private HillCipherAttackMatrix KnownPlainTextAttack(HillCipherAttackMatrix plain_text_matrix, HillCipherAttackMatrix cipher_text_matrix)
        {
            if (!HillCipherAttackUtils.IsValidTextMatrix(plain_text_matrix, cipher_text_matrix))
            {
                GuiLogMessage(Properties.Resources.InvalidException, NotificationLevel.Error);
                return null;
            }

            // Create system of equations for K = C * P^-1 mod m

            // Create the inverse of the plain text matrix
            HillCipherAttackMatrix inversePlainText = HillCipherAttackMatrix.InverseMatrix(plain_text_matrix, _settings.Modulus);

            if (!inversePlainText.CheckForIdentitiyMatrix(plain_text_matrix, _settings.Modulus))
            {
                GuiLogMessage(Properties.Resources.NoIdentityMatrix, NotificationLevel.Error);
                return null;
            }

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
}
