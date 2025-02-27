/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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

using LanguageStatisticsLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace CrypTool.Alphabets
{
    /// <summary>
    /// Interaction logic for AlphabetPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Alphabets.Properties.Resources")]
    public partial class AlphabetPresentation : UserControl, INotifyPropertyChanged
    {
        public event EventHandler AlphabetChanged;

        private ObservableCollection<AlphabetItem> alphabets = new ObservableCollection<AlphabetItem>();
        public ObservableCollection<AlphabetItem> Alphabets
        {
            get => alphabets;
            private set
            {
                alphabets = value;                
            }
        }

        private ObservableCollection<OutputOrder> outputOrder = new ObservableCollection<OutputOrder>();
        public ObservableCollection<OutputOrder> OutputOrder
        {
            get => outputOrder;
            private set => outputOrder = value;
        }

        private ObservableCollection<BasicAlphabet> basic = new ObservableCollection<BasicAlphabet>();
        public ObservableCollection<BasicAlphabet> Basic { get => basic; private set { basic = value; OnPropertyChanged("Basic"); } }

        public static readonly DependencyProperty SelectedAlphabetProperty = DependencyProperty.Register("SelectedAlphabet",
            typeof(AlphabetItem), typeof(AlphabetPresentation), new FrameworkPropertyMetadata(null));

        public AlphabetItem SelectedAlphabet
        {
            get => (AlphabetItem)base.GetValue(SelectedAlphabetProperty);
            set => base.SetValue(SelectedAlphabetProperty, value);
        }

        public static readonly DependencyProperty IsConfigOpenProperty = DependencyProperty.Register("IsConfigOpen",
            typeof(bool), typeof(AlphabetPresentation), new FrameworkPropertyMetadata(false));

        public bool IsConfigOpen
        {
            get => (bool)base.GetValue(IsConfigOpenProperty);
            set => base.SetValue(IsConfigOpenProperty, value);
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        public AlphabetPresentation(AlphabetSettings setting)
        {
            this.setting = setting;
            this.setting.PropertyChanged += new PropertyChangedEventHandler(setting_PropertyChanged);

            OutputOrderView = CollectionViewSource.GetDefaultView(OutputOrder) as ListCollectionView;
            OutputOrderView.CustomSort = new IndexSorter<OutputOrder>(OutputOrder, OutputOrderView);

            AlphabetCollectionView = CollectionViewSource.GetDefaultView(Alphabets) as ListCollectionView;
            AlphabetCollectionView.CustomSort = new IndexSorter<AlphabetItem>(Alphabets, AlphabetCollectionView);

            timer.Tick += delegate (object o, EventArgs args)
            {
                timer.Stop();
                this.setting.PropertyChanged -= new PropertyChangedEventHandler(setting_PropertyChanged);
                SetAlphabetItems(setting.Default.AlphabetData);
                SetOutputOrders(setting.Default.OutputOrderData);
            };

            timer.Start();
            InitializeComponent();
        }

        private void setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Data")
            {
                setting.PropertyChanged -= new PropertyChangedEventHandler(setting_PropertyChanged);
                timer.Stop();
                Data data = AlphabetSettings.DeserializeData<Data>(setting.Data);
                SetAlphabetItems(data.AlphabetData);
                SetOutputOrders(data.OutputOrderData);
            }
        }

        private void addItem(AlphabetItem item)
        {
            item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
            Alphabets.Add(item);
            saveToSettings();
        }

        private void removeItem(AlphabetItem item)
        {
            item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
            Alphabets.Remove(item);
            saveToSettings();
        }

        private void saveToSettings()
        {
            List<AlphabetItemData> tmp = new List<AlphabetItemData>();
            List<OutputOrder> tmp2 = new List<OutputOrder>();
            if (Alphabets != null)
            {
                foreach (AlphabetItem item in Alphabets)
                {
                    tmp.Add(item.Data);
                }
            }

            if (OutputOrder != null)
            {
                foreach (OutputOrder item in OutputOrder)
                {
                    tmp2.Add(item);
                }
            }

            Data data = new Data() { AlphabetData = tmp, OutputOrderData = tmp2 };
            setting.Data = AlphabetSettings.SerializeData<Data>(data);
        }

        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            raiseChange();
        }

        public void SetOutputOrders(List<OutputOrder> data)
        {
            foreach (OutputOrder item in data)
            {
                OutputOrder.Add(item);
            }
            saveToSettings();
        }

        public void SetAlphabetItems(List<AlphabetItemData> data)
        {
            if (Alphabets != null)
            {
                int x = Alphabets.Count - 1;
                for (int i = x; i >= 0; --i)
                {
                    AlphabetItem item = Alphabets[i];
                    item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }

            Alphabets.Clear();

            foreach (AlphabetItemData item in data)
            {
                AlphabetItem tmp = new AlphabetItem(item);
                addItem(tmp);
                if (tmp.IsSelected == true)
                {
                    ActiveAlphabet = tmp;
                }
            }
        }

        internal class IndexSorter<T> : IComparer
        {
            private readonly ListCollectionView view;

            public IndexSorter(ObservableCollection<T> collection, ListCollectionView view)
            {
                this.view = view;
                applyIndex(collection);
                collection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ConnectorCollectionItemChanged);
            }

            private void applyIndex(IList collection)
            {
                IEnumerable<IIndex> tmp = collection.OfType<IIndex>();
                if (tmp.Any(x => x.Index == int.MaxValue))
                {
                    foreach (IIndex item in tmp)
                    {
                        item.Index = collection.IndexOf(item);
                    }
                }
            }

            public int Compare(object x, object y)
            {
                IIndex connA = x as IIndex;
                IIndex connB = y as IIndex;
                int val = connA.Index.CompareTo(connB.Index);
                return val;
            }

            private void AssignIndex(IList collection)
            {
                IEnumerable<IIndex> tmp = collection.OfType<IIndex>();
                foreach (IIndex connector in tmp)
                {
                    int index = collection.IndexOf(connector);
                    connector.Index = index;
                }
                view.Refresh();
            }

            private void ConnectorCollectionItemChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                AssignIndex((IList)sender);
            }
        }


        public static readonly DependencyProperty ActiveAlphabetProperty = DependencyProperty.Register("ActiveAlphabet",
            typeof(AlphabetItem), typeof(AlphabetPresentation), new FrameworkPropertyMetadata(null, OnActiveAlphabetChanged));
        private readonly AlphabetSettings setting;

        public AlphabetItem ActiveAlphabet
        {
            get => (AlphabetItem)base.GetValue(ActiveAlphabetProperty);
            set => base.SetValue(ActiveAlphabetProperty, value);
        }

        private static void OnActiveAlphabetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AlphabetPresentation b = (AlphabetPresentation)d;
            AlphabetItem newItem = (AlphabetItem)e.NewValue;
            AlphabetItem oldItem = (AlphabetItem)e.OldValue;

            if (newItem != null)
            {
                newItem.IsSelected = true;
            }

            if (oldItem != null)
            {
                oldItem.IsSelected = false;
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DeleteItemClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele == null)
            {
                return;
            }
            AlphabetItem alphabetItem = (AlphabetItem)ele.DataContext;
            MessageBoxResult result = MessageBox.Show(string.Format(Properties.Resources.DeleteAlphabetMessageBoxText, alphabetItem.Title), Properties.Resources.DeleteAlphabetMessageBoxTitle, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                removeItem(alphabetItem);
            }
        }

        private void AddItemClick(object sender, RoutedEventArgs e)
        {
            AlphabetItem item = new AlphabetItem();
            addItem(item);
            SelectedAlphabet = item;
        }

        private void EditItemClick(object sender, RoutedEventArgs e)
        {
            SelectedAlphabet = ActiveAlphabet;
        }

        private void ReturnToOverviewClick(object sender, RoutedEventArgs e)
        {
            SelectedAlphabet = null;
        }

        public string GetAlphabet()
        {
            if (ActiveAlphabet == null)
            {
                return string.Empty;
            }

            AlphabetItem.AlphabetType[] alp = ActiveAlphabet.GetAlphabet();
            StringBuilder builder = new StringBuilder();

            foreach (OutputOrder item in OutputOrder.OrderBy(x => x.Index).Where(x => x.IsActive))
            {
                try
                {
                    AlphabetItem.AlphabetType obj = alp.SingleOrDefault(x => x.OutputOrderType == item.OutputType);
                    builder.Append(obj.Output);
                }
                catch (Exception)
                { }
            }

            return builder.ToString();
        }

        private void UpClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele == null)
            {
                return;
            }

            AlphabetItem alp = (AlphabetItem)ele.DataContext;
            int index = Alphabets.IndexOf(alp);
            AlphabetItem tmp = null;
            if (ActiveAlphabet == alp)
            {
                tmp = alp;
            }

            index++;
            if (index < Alphabets.Count)
            {
                Alphabets.Remove(alp);
                Alphabets.Insert(index, alp);
                if (tmp != null)
                {
                    ActiveAlphabet = alp;
                }
            }

            AlphabetCollectionView.Refresh();
        }

        private void DownClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (ele == null)
            {
                return;
            }

            AlphabetItem alp = (AlphabetItem)ele.DataContext;
            int index = Alphabets.IndexOf(alp);
            AlphabetItem tmp = null;
            if (ActiveAlphabet == alp)
            {
                tmp = alp;
            }

            index--;
            if (index > -1)
            {
                Alphabets.Remove(alp);
                Alphabets.Insert(index, alp);
                if (tmp != null)
                {
                    ActiveAlphabet = alp;
                }
            }


            AlphabetCollectionView.Refresh();

        }

        public ListCollectionView AlphabetCollectionView { get; set; }
        private readonly ListCollectionView OutputOrderView;


        private void handleIndex(int index, OutputOrder item)
        {
            if (index > -1)
            {
                OutputOrder.Remove(item);
                OutputOrder.Insert(index, item);
            }
        }

        private void ToggleButton_Drop(object sender, DragEventArgs e)
        {
            ToggleButton btn = ((ToggleButton)sender);
            OutputOrder target = (OutputOrder)btn.DataContext;
            OutputOrder source = (OutputOrder)e.Data.GetData("CrypTool.Alphabets.OutputOrder");

            if (target == source)
            {
                btn.IsChecked = !btn.IsChecked;
                e.Handled = true;
                return;
            }

            int targetIndex = OutputOrder.IndexOf(target);
            int sourceIndex = OutputOrder.IndexOf(source);

            handleIndex(targetIndex, source);
            handleIndex(sourceIndex, target);

            OutputOrderView.Refresh();
            raiseChange();
        }

        private void raiseChange()
        {
            saveToSettings();
            if (AlphabetChanged != null)
            {
                AlphabetChanged.Invoke(this, null);
            }
        }

        private void ToggleButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;
            OutputOrder source = (OutputOrder)btn.DataContext;
            DragDrop.DoDragDrop((ToggleButton)sender, source, DragDropEffects.Copy);
            raiseChange();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private readonly bool isLoading = true;
    }

    [Serializable]
    public enum OutputType
    {
        Upper,
        Lower,
        Special,
        Numeric
    }

    [Serializable]
    public class Data
    {
        private List<AlphabetItemData> alphabetData = null;
        public List<AlphabetItemData> AlphabetData
        {
            get => alphabetData;
            set => alphabetData = value;
        }

        private List<OutputOrder> autputOrderData = null;
        public List<OutputOrder> OutputOrderData
        {
            get => autputOrderData;
            set => autputOrderData = value;
        }
    }

    [Serializable]
    public class OutputOrder : IIndex, INotifyPropertyChanged
    {
        public OutputType OutputType;

        private string caption = string.Empty;
        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                OnPropertyChanged("Caption");
            }
        }

        private bool isActive = true;
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        private int index = int.MaxValue;
        public int Index
        {
            get => index;
            set => index = value;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class UnicodeUtil
    {
        private readonly int from = 0;
        private readonly int to = 0;

        private readonly int lowerFrom = 0;
        private readonly int lowerTo = 0;

        public string output = string.Empty;
        public string outputLower = string.Empty;

        public string getStringFromUnicode(int from, int to)
        {
            string output = string.Empty;
            byte[] k = new byte[4];
            MemoryStream m = new MemoryStream(k);
            BinaryWriter w = new BinaryWriter(m);
            for (int i = from; i <= to; ++i)
            {
                w.Seek(0, SeekOrigin.Begin);
                w.Write(i);
                output += Encoding.UTF32.GetString(k);
            }
            return output;
        }

        public UnicodeUtil(int from, int to)
        {
            this.from = from;
            this.to = to;

            output = getStringFromUnicode(from, to);
        }

        public UnicodeUtil(int from, int to, int lowerFrom, int lowerTo)
        {
            this.from = from;
            this.to = to;

            this.lowerFrom = lowerFrom;
            this.lowerTo = lowerTo;

            output = getStringFromUnicode(from, to);
            outputLower = getStringFromUnicode(lowerFrom, lowerTo);
        }
    }
    [Serializable]
    public class BasicAlphabet
    {
        public static readonly BasicAlphabet[] BasicAlphabets;
        public string Name { get; set; }
        public string Small { get; set; }
        public string Capital { get; set; }
        public string Special { get; set; }

        static BasicAlphabet()
        {
            //generate basic alphabets using the alphabets defined in the LanguageStatistics
            BasicAlphabets = new BasicAlphabet[LanguageStatistics.Alphabets.Count];
            int counter = 0;
            foreach (KeyValuePair<string, string> alphabet in LanguageStatistics.Alphabets)
            {
                BasicAlphabets[counter] = new BasicAlphabet(alphabet.Value.ToUpper(), alphabet.Value.ToLower(), LanguageStatistics.SupportedLanguages[counter]);
                counter++;
            }
        }

        public BasicAlphabet(string capital, string small, string name)
        {
            Capital = capital;
            Small = small;
            Name = name;            
        }        
    }

    [Serializable]
    public class AlphabetItemData
    {
        private bool editable = true;
        public bool Editable
        {
            get => editable;
            set => editable = value;
        }

        private int index = int.MaxValue;
        public int Index
        {
            get => index;
            set => index = value;
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get => isSelected;
            set => isSelected = value;
        }

        private string title = Properties.Resources.NewTitle;
        public string Title
        {
            get => title;
            set => title = value;
        }

        private AlphabetItem.AlphabetType upper = new AlphabetItem.AlphabetType() { OutputOrderType = OutputType.Upper };
        public AlphabetItem.AlphabetType Upper
        {
            get => upper;
            set => upper = value;
        }

        private AlphabetItem.AlphabetType lower = new AlphabetItem.AlphabetType() { OutputOrderType = OutputType.Lower };
        public AlphabetItem.AlphabetType Lower
        {
            get => lower;
            set => lower = value;
        }

        private AlphabetItem.AlphabetType numeric = new AlphabetItem.AlphabetType() { Output = "0123456789", OutputOrderType = OutputType.Numeric };
        public AlphabetItem.AlphabetType Numeric
        {
            get => numeric;
            set => numeric = value;
        }

        private AlphabetItem.AlphabetType special = new AlphabetItem.AlphabetType() { Output = ".,:;!?()-+*/[]{}@_><#~=\"&%$§", OutputOrderType = OutputType.Special };
        public AlphabetItem.AlphabetType Special
        {
            get => special;
            set => special = value;
        }
    }

    public interface IIndex
    {
        int Index
        {
            get;
            set;
        }
    }

    public class AlphabetItem : INotifyPropertyChanged, IIndex
    {        
        public event PropertyChangedEventHandler PropertyChanged;

        public AlphabetItem(BasicAlphabet basic, string title, bool editable)
        {
            Data = new AlphabetItemData
            {
                Lower = new AlphabetType() { Output = basic.Small, OutputOrderType = OutputType.Lower },
                Upper = new AlphabetType() { Output = basic.Capital, OutputOrderType = OutputType.Upper }
            };
            OnPropertyChanged("Upper");
            OnPropertyChanged("Lower");

            Editable = editable;
            Title = title;
        }

        public AlphabetItem(AlphabetItem item)
        {
            Data = item.Data;
        }

        public AlphabetItem()
        {
            Data = new AlphabetItemData();
        }

        public AlphabetItem(AlphabetItemData data)
        {
            Data = data;
        }

        public CrypTool.Alphabets.AlphabetItem.AlphabetType[] GetAlphabet()
        {
            return new AlphabetType[] { Data.Upper, Data.Lower, Data.Numeric, Data.Special };
        }

        public AlphabetItemData data;

        public AlphabetItemData Data
        {
            get => data;
            set
            {
                data = value;
                OnPropertyChanged("Editable");
                OnPropertyChanged("IsSelected");
                OnPropertyChanged("Title");
                OnPropertyChanged("Upper");
                OnPropertyChanged("Lower");
                OnPropertyChanged("Numeric");
                OnPropertyChanged("Special");
                OnPropertyChanged("Index");
            }
        }

        [Serializable]
        public class AlphabetType
        {
            private string output = string.Empty;
            public string Output
            {
                get => output;
                set => output = value;
            }
            public OutputType OutputOrderType;
        }

        public bool Editable
        {
            get => Data.Editable;
            set { Data.Editable = value; OnPropertyChanged("Editable"); }
        }

        public bool IsSelected
        {
            get => Data.IsSelected;
            set { Data.IsSelected = value; OnPropertyChanged("IsSelected"); }
        }

        public string Title
        {
            get => Data.Title;
            set { Data.Title = value; OnPropertyChanged("Title"); }
        }

        public int Index
        {
            get => Data.Index;
            set { Data.Index = value; OnPropertyChanged("Index"); }
        }

        public string Upper
        {
            get => Data.Upper.Output;
            set { Data.Upper.Output = value; OnPropertyChanged("Upper"); }
        }

        public string Lower
        {
            get => Data.Lower.Output;
            set { Data.Lower.Output = value; OnPropertyChanged("Lower"); }
        }

        public string Numeric
        {
            get => Data.Numeric.Output;
            set { Data.Numeric.Output = value; OnPropertyChanged("Numeric"); }
        }

        public string Special
        {
            get => Data.Special.Output;
            set { Data.Special.Output = value; OnPropertyChanged("Special"); }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
