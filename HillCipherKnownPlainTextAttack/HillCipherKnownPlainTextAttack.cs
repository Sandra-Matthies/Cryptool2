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
using System.ComponentModel.Design;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.Plugins.HillCipherKnownPlainTextAttack
{
    // HOWTO: Plugin developer HowTo can be found here: https://github.com/CrypToolProject/CrypTool-2/wiki/Developer-HowTo

    // HOWTO: Change author name, email address, organization and URL.
    [Author("Sandra Matthies", "sandra_matthies@outlook.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Known Plaintext Attack for Hill Cipher", "Implements the Kwnown Plaintext Attack for Hill Cipher", "HillCipherKnownPlainTextAttack/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class HillCipherKnownPlainTextAttack : ICrypComponent
    {
        #region Private Variables

        private readonly HillCipherKnownPlainTextAttackSettings _settings = new HillCipherKnownPlainTextAttackSettings();
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #endregion

        #region Data Properties


        [PropertyInfo(Direction.InputData, "Plaintext Input", "Plain Text Input")]
        public string Plain
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "Cipher Input", "Cipher Text Input")]
        public string Cipher
        {
            get;
            set;
        }

        
        [PropertyInfo(Direction.OutputData, "Key", "Key Text Output")]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Keydimension", "The Dimension of the key Matrix")]
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
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            try
            {
                var alphabet_numbers = HillCipherKnownPlainTextAttackMapper.mapAlphabetToNumbers(ALPHABET);
                var plain_numbers = HillCipherKnownPlainTextAttackMapper.mapLettersByAlphabetToNumbers(Plain, alphabet_numbers);
                var cipher_numbers = HillCipherKnownPlainTextAttackMapper.mapLettersByAlphabetToNumbers(Cipher, alphabet_numbers);
                int key_dimension = 0;

                int m = ALPHABET.Length + 1;

                HillCipherKnownPlainTextAttackMatrix key;
                HillCipherKnownPlainTextAttackMatrix[] cipher_matrices;
                HillCipherKnownPlainTextAttackMatrix[] plain_matrices;
                do
                {
                    key_dimension++;
                    cipher_matrices = HillCipherKnownPlainTextAttackUtils.createTextMatrices(cipher_numbers, key_dimension, alphabet_numbers);
                    plain_matrices = HillCipherKnownPlainTextAttackUtils.createTextMatrices(plain_numbers, key_dimension, alphabet_numbers);

                    var plain_text_matrix = HillCipherKnownPlainTextAttackUtils.getSquareMatrix(plain_matrices, key_dimension);
                    var cipher_text_matrix = HillCipherKnownPlainTextAttackUtils.getSquareMatrix(cipher_matrices, key_dimension);

                    key = KnownPlainTextAttack(plain_text_matrix, cipher_text_matrix, m);


                } while (CompareCipherText(key, plain_matrices, alphabet_numbers, m, Cipher) == false);

                var res_key_numbers = HillCipherKnownPlainTextAttackUtils.createarrayFromMatrix(key);
                var key_text = HillCipherKnownPlainTextAttackMapper.mapNumbersByAlphabetToLetters(res_key_numbers, alphabet_numbers);
                KeyDimension = key_dimension;
                Key = key_text;
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Error: {0}", ex.Message), NotificationLevel.Error);
            }


            ProgressChanged(1, 1);
        }

        private bool CompareCipherText(HillCipherKnownPlainTextAttackMatrix key, HillCipherKnownPlainTextAttackMatrix[] plain_matrices, Dictionary<string, int> alphabet_numbers, int m, object res_cipher)
        {
            var _cipherText = Encrypt(key, plain_matrices, alphabet_numbers, m);
            return _cipherText == Cipher;
        }

        private string Encrypt(HillCipherKnownPlainTextAttackMatrix key_matrix, HillCipherKnownPlainTextAttackMatrix[] plain_matrices, Dictionary<string, int> alphabet_numbers, int m)
        {
            HillCipherKnownPlainTextAttackMatrix[] cipher_matrices = new HillCipherKnownPlainTextAttackMatrix[plain_matrices.Length];
            for (int i = 0; i < plain_matrices.Length; i++)
            {
                cipher_matrices[i] = HillCipherKnownPlainTextAttackUtils.Encrypt(key_matrix, plain_matrices[i], m);
            }

            var cipher_matrix = HillCipherKnownPlainTextAttackUtils.mergeCipherText(cipher_matrices, m);
            var cipher_numbers = new int[cipher_matrix.Rows];

            for (int i = 0; i < cipher_matrix.Rows; i++)
            {
                cipher_numbers[i] = cipher_matrix.Data[i, 0];
            }

            var cipher_text = HillCipherKnownPlainTextAttackMapper.mapNumbersByAlphabetToLetters(cipher_numbers, alphabet_numbers);
            return cipher_text;
        }

        private HillCipherKnownPlainTextAttackMatrix KnownPlainTextAttack(HillCipherKnownPlainTextAttackMatrix plain_text_matrix, HillCipherKnownPlainTextAttackMatrix cipher_text_matrix, int m)
        {
            if(!HillCipherKnownPlainTextAttackUtils.isValidTextMatrix(plain_text_matrix, cipher_text_matrix))
            {
                throw new Exception("Invalid text matrix");
            }

            // Create system of equations for K = C * P^-1 mod m

            // Create the inverse of the plain text matrix
            HillCipherKnownPlainTextAttackMatrix inversePlainText = HillCipherKnownPlainTextAttackMatrix.inverseMatrix(plain_text_matrix, m);


            var key = HillCipherKnownPlainTextAttackMatrix.multiplyMatrix(cipher_text_matrix, inversePlainText);

            key = HillCipherKnownPlainTextAttackMatrix.modMatrix(key, m);
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
