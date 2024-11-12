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
        private HillCipherAttackPresentation _presentation = new HillCipherAttackPresentation();

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
                    GuiLogMessage(string.Format(Properties.Resources.InputContainsIllegalCharacter, Plain[j], j), NotificationLevel.Error);
                    return false;
                }
            }

            for (int j = 0; j < Cipher.Length; j++)
            {
                if (_settings.Alphabet.IndexOf(Cipher[j]) < 0)
                {
                    GuiLogMessage(string.Format(Properties.Resources.InputContainsIllegalCharacter, Cipher[j], j), NotificationLevel.Error);
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
                var plain_numbers = HillCipherAttackMapper.mapLettersByAlphabetToNumbers(Plain, alphabet_numbers);
                var cipher_numbers = HillCipherAttackMapper.mapLettersByAlphabetToNumbers(Cipher, alphabet_numbers);
                int key_dimension = 0;


                ModMatrix key_mat;
                ModMatrix cipher_mat;
                ModMatrix plain_mat;
                do
                {
                    key_dimension++;
                    key_mat = new ModMatrix(key_dimension, _settings.Modulus);
                    cipher_mat = new ModMatrix(key_dimension, _settings.Modulus);
                    plain_mat = new ModMatrix(key_dimension, _settings.Modulus);

                    //TODO: Recheck
                    while (Plain.Length < key_dimension * key_dimension)
                    {
                        Plain += "A";
                    }
                    while (Cipher.Length < key_dimension * key_dimension)
                    {
                        Cipher += "A";
                    }
                    plain_mat.setElements(Plain, _settings.Alphabet);
                    cipher_mat.setElements(Cipher, _settings.Alphabet);

                    var det_plain = plain_mat.det();
                    if(HillCipherAttackUtils.GCD((int)det_plain, _settings.Modulus) !=1)
                    {
                        GuiLogMessage(string.Format(Properties.Resources.NotInvertable), NotificationLevel.Error);
                        GuiLogMessage(string.Format("Dimension: {0}", key_dimension), NotificationLevel.Info);
                        continue;
                    }
                    key_mat = new ModMatrix(KnownPlainTextAttack(plain_mat, cipher_mat));


                } while (CompareCipherText(key_mat, plain_mat, alphabet_numbers) == false);

                var res_key_numbers = HillCipherAttackUtils.ConverToArray(key_mat);
                var key_text = HillCipherAttackMapper.mapNumbersByAlphabetToLetters(res_key_numbers, alphabet_numbers);
                KeyDimension = key_dimension;
                Key = key_text;

                OnPropertyChanged(nameof(Key));

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

        private bool CompareCipherText(ModMatrix key, ModMatrix plain_mat, Dictionary<string, int> alphabet_numbers)
        {
            var _cipherText = Encrypt(key, plain_mat, alphabet_numbers);
            return _cipherText == Cipher;
        }

        private string Encrypt(ModMatrix key_matrix, ModMatrix plain_mat, Dictionary<string, int> alphabet_numbers)
        {
            ModMatrix cipher_mat = new ModMatrix(plain_mat.Dimension, _settings.Modulus);
            cipher_mat = plain_mat * key_matrix;
            

            var cipher_numbers = HillCipherAttackUtils.ConverToArray(cipher_mat);
            var cipher_text = HillCipherAttackMapper.mapNumbersByAlphabetToLetters(cipher_numbers, alphabet_numbers);
            return cipher_text;
        }

        private ModMatrix KnownPlainTextAttack(ModMatrix plain_mat, ModMatrix cipher_mat)
        {
            if (!HillCipherAttackUtils.isValidTextMatrix(plain_mat, cipher_mat))
            {
                throw new Exception(Properties.Resources.InvalidException);
            }

            // Create system of equations for K = C * P^-1 mod m

            // Create the inverse of the plain text matrix
            var inv_plain_mat = plain_mat.invert();


            var key = inv_plain_mat * cipher_mat;

            key = new ModMatrix(key);
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
