using CrypTool.CrypAnalysisViewControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrypTool.Plugins.HillCipherAttack
{
    /// <summary>
    /// Interaktionslogik für HillCipherAttackPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.HillCipherAttack.Properties.Resources")]
    public partial class HillCipherAttackPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> BestList { get; set; } = new ObservableCollection<ResultEntry>();
        public string Cipher { get; set; } = "";
        public string Alphabet { get; set; } = "";

        public string Modulo { get; set; } = "";

        public event Action<ResultEntry> SelectedResultEntry;
        public HillCipherAttackPresentation()
        {
            InitializeComponent();
            DataContext = this;

        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                SelectedResultEntry(resultItem);
            }
        }

    }
}
