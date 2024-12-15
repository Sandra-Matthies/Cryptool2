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
using System.ComponentModel;

namespace CrypTool.Plugins.HillCipherAttack
{
   
    public class HillCipherAttackSettings : ISettings
    {
        #region Private Variables

        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private int modulus => alphabet.Length;

        private int startKeyDimension = 1;

        private bool unkownPlaintextAttack = false;    // false = known plaintext attack, true = unknown plaintext attack

        private int language = 0; // 0 = English, 1 = German

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// </summary>
        /// 

        [TaskPane("Attack Type", "Select between Kown and Unkown Plaintext Attack ", null, 1, false, ControlType.ComboBox, new string[] { "Kown Plaintext Attack", "Unkown Plaintext Attack" })]
        public bool UnkownPlaintextAttack
        {
            get => unkownPlaintextAttack;
            set
            {
                if (value != unkownPlaintextAttack)
                {
                    unkownPlaintextAttack = value;
                    OnPropertyChanged("UnkownPlaintextAttack");
                }
            }
        }

        [TaskPane("Alphabet", "Alphabet which is used for encryption and decryption", null, 1, false, ControlType.TextBox)]
        public string Alphabet
        {
            get
            {
                return alphabet;
            }
            set
            {
                if (alphabet != value)
                {
                    alphabet = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("Alphabet");
                    OnPropertyChanged("Modulus");
                }
            }
        }

        [TaskPane("Modulus", "Modulus is the length of the alphabet", null, 2, false, ControlType.TextBoxReadOnly)]
        public int Modulus
        {
            get
            {
                return modulus;
            }
        }

        [TaskPane("StartKeyDimension", "The Calculation of the key starts at this dimension", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int StartKeyDimension
        {
            get
            {
                return startKeyDimension;
            }
            set
            {
                if (startKeyDimension != value)
                {
                    startKeyDimension = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("StartKeyDimension");
                }
            }
        }

        [TaskPane("Language", "Select the language of the text", null, 4, false, ControlType.ComboBox, new string[] { "English", "German" })]
        public int Language
        {
            get => language;
            set
            {
                if (value != language)
                {
                    language = value;
                    OnPropertyChanged("Language");
                }
            }
        }


        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {

        }
    }
}
