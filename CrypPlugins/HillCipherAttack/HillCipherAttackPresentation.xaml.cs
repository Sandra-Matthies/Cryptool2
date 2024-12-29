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

        public AttackResult ResultOutput { get; set; } = new AttackResult();

        public HillCipherAttackPresentation()
        {
            InitializeComponent();
            DataContext = ResultOutput;

        }

    }
}
