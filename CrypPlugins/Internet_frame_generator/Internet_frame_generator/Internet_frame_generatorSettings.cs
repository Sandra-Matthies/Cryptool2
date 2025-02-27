﻿using CrypTool.PluginBase;
using System.ComponentModel;

namespace CrypTool.Internet_frame_generator
{
    /// <summary>
    /// Some settings for the <see cref="IPPacketGenerator"/>IPPacketGenerator plugin. Only setting is the kind
    /// of traffic generated by this plugin: you can choose between IP packages and ARP request packages.
    /// </summary>
    public class Internet_frame_generatorSettings : ISettings
    {
        #region private variables

        private int action = 0; // 0 = "normal" IP packets, 1 = ARP packets (requests)
        private int numberOfPacketsToBeCreated = 100000;

        /// <summary>
        /// 0 = "normal" IP packets.
        /// 1 = ARP (request) packets.
        /// </summary>
        [ContextMenu("ActionCaption", "ActionTooltip",
            1,
            ContextMenuControlType.ComboBox,
            null,
            new string[] { "ActionList1", "ActionList2" })]
        [TaskPane("ActionCaption", "ActionTooltip",
            null,
            1,
            true,
            ControlType.ComboBox,
            new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get => action;
            set
            {
                if (action != value)
                {
                    action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        /// <summary>
        /// How many files are going to be saved (if saving is wanted).
        /// </summary>
        [TaskPane("NumberOfPacketsToBeCreatedCaption", "NumberOfPacketsToBeCreatedTooltip",
            null,
            2,
            false,
            ControlType.TextBox,
            ValidationType.RegEx,
            "^10{5}$|^[1-9][0-9]{0,4}$")]
        public int NumberOfPacketsToBeCreated
        {
            get => numberOfPacketsToBeCreated;
            set
            {
                if (value != numberOfPacketsToBeCreated)
                {
                    numberOfPacketsToBeCreated = value;
                    OnPropertyChanged("NumberOfPacketsToBeSaved");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIncon(int Icon)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
