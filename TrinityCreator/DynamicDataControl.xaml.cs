﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TrinityCreator
{
    /// <summary>
    ///     Interaction logic for DynamicDataControl.xaml
    /// </summary>
    public partial class DynamicDataControl : UserControl
    {
        private List<DockPanel> _lines = new List<DockPanel>();

        /// <summary>
        /// DDC With Combobox (key) & TextBox (value)
        /// </summary>
        /// <param name="keyOptions"></param>
        /// <param name="maxLines"></param>
        /// <param name="showAll"></param>
        /// <param name="header1"></param>
        /// <param name="header2"></param>
        /// <param name="defaultValue"></param>
        /// <param name="valueMySqlDt">MySql max value</param>
        public DynamicDataControl(object[] keyOptions, int maxLines, bool showAll = false, string header1 = "",
            string header2 = "", string defaultValue = "", string valueMySqlDt = "")
        {
            ValueMySqlDt = valueMySqlDt;
            Prepare(maxLines, header1, header2, defaultValue);
            KeyOptions = keyOptions;
            IsTextboxKey = false;

            if (showAll)
            {
                AddLineBtn.Visibility = Visibility.Collapsed;
                foreach (var key in KeyOptions)
                    AddLine(key);
            }
            else AddLine();
        }

        /// <summary>
        /// DDC with TextBox as key & value
        /// </summary>
        /// <param name="maxLines"></param>
        /// <param name="header1"></param>
        /// <param name="header2"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keyMySqlDt">MySql max value</param>
        /// <param name="valueMySqlDt">MySql max value</param>
        public DynamicDataControl(int maxLines, string header1 = "", string header2 = "", string defaultValue = "0", string keyMySqlDt = "", string valueMySqlDt = "")
        {
            KeyMySqlDt = keyMySqlDt;
            ValueMySqlDt = valueMySqlDt;
            Prepare(maxLines, header1, header2, defaultValue);
            IsTextboxKey = true;
            AddLine();
        }

        private void Prepare(int maxLines, string header1, string header2, string defaultValue)
        {
            InitializeComponent();

            MaxLines = maxLines;
            DefaultValue = defaultValue;

            if (header1 != "" && header2 != "")
            {
                var dp = new DockPanel();
                dp.Margin = new Thickness(0, 2, 0, 2);

                var l1 = new Label();
                l1.Content = header1;
                l1.Width = 150;
                HeaderDp.Children.Add(l1);

                var l2 = new Label();
                l2.Content = header2;
                HeaderDp.Children.Add(l2);
            }
        }

        private string DefaultValue { get; set; }
        private bool IsTextboxKey { get; set; }
        public int MaxLines { get; set; }
        public object[] KeyOptions { get; set; }
        public string KeyMySqlDt { get; private set; }
        public string ValueMySqlDt { get; private set; }

        public event EventHandler DynamicDataChanged = delegate { };

        /// <summary>
        ///     Returns the user input
        /// </summary>
        /// <returns></returns>
        public Dictionary<object, string> GetUserInput()
        {
            var d = new Dictionary<object, string>();
            foreach (var line in _lines)
            {
                if (IsTextboxKey)
                {
                    var tbKey = (TextBox)line.Children[0];
                    var tbValue = (TextBox)line.Children[1];
                    if (tbKey.Text != "" && tbValue.Text != "")
                        d.Add(tbKey.Text, tbValue.Text);
                }
                else
                {
                    var cb = (ComboBox)line.Children[0];
                    var tb = (TextBox)line.Children[1];

                    if (tb.Text != "")
                        d.Add(cb.SelectedValue, tb.Text);
                }
            }
            return d;
        }

        /// <summary>
        /// Adds dictionary SQL values for incrementing SQL fields
        /// </summary>
        /// <param name="d">Query dictionary reference</param>
        /// <param name="keyPrefix">field name without number</param>
        /// <param name="valuePrefix">field name without number</param>
        /// <param name="valueMultiplier">multiplies value by this before adding to dictionary</param>
        /// <returns></returns>
        public void AddValues(Dictionary<string,string> d, string keyPrefix, string valuePrefix, int valueMultiplier = 1)
        {
            try
            {
                int i = 0;
                foreach (KeyValuePair<object, string> line in GetUserInput())
                {
                    int key = 0;
                    if (IsTextboxKey) key = int.Parse((string)line.Key); // parse to validate
                    else key = ((IKeyValue)line.Key).Id;

                    int value = int.Parse(line.Value) * valueMultiplier;
                    i++;

                    d.Add(keyPrefix + i, key.ToString());
                    d.Add(valuePrefix + i, value.ToString());
                }
            }
            catch
            {
                throw new Exception("Invalid data in " + keyPrefix + " or " + valuePrefix);
            }
        }

        private void addLineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_lines.Count() < MaxLines)
                AddLine();
        }

        private void AddLine(object key = null)
        {
            var dp = new DockPanel();
            dp.Margin = new Thickness(0, 2, 0, 2);

            if (IsTextboxKey)
            {
                TextBox tbKey = new TextBox();
                tbKey.Width = 150;
                tbKey.Text = DefaultValue;
                tbKey.LostFocus += TbKey_LostFocus;
                dp.Children.Add(tbKey);
            }
            else // combobox key
            {
                var cb = new ComboBox();
                if (key == null)
                    cb.ItemsSource = KeyOptions;
                else
                {
                    cb.Items.Add(key);
                    cb.IsEnabled = false;
                }
                cb.SelectedIndex = 0;
                cb.Width = 150;
                cb.SelectionChanged += TriggerChangedEvent;
                dp.Children.Add(cb);
            }

            var tbValue = new TextBox();
            tbValue.Margin = new Thickness(5, 0, 0, 0);
            tbValue.Text = DefaultValue;
            tbValue.LostFocus += TbValue_LostFocus;
            dp.Children.Add(tbValue);

            _lines.Add(dp);
            DynamicSp.Children.Add(dp);

            TriggerChangedEvent(this, new EventArgs());
        }

        private void TbKey_LostFocus(object sender, RoutedEventArgs e)
        {
            if (KeyMySqlDt != "")
                LimitValue((TextBox)sender, KeyMySqlDt);
            TriggerChangedEvent(sender, e);
        }

        private void TbValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ValueMySqlDt != "")
                LimitValue((TextBox)sender, ValueMySqlDt);
            TriggerChangedEvent(sender, e);
        }

        /// <summary>
        /// Limit int value
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="MySqlDt"></param>
        private void LimitValue(TextBox tb, string MySqlDt)
        {
            string oldTxt = tb.Text;
            string newTxt = "0";
            try {
                newTxt = Database.DataType.LimitLength(int.Parse(oldTxt), MySqlDt).ToString();
            }
            catch {
                newTxt = Database.DataType.LimitLength(2147483647, MySqlDt).ToString();
            }

            if (oldTxt != newTxt)
                tb.Text = newTxt;
        }

        private void removeLineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_lines.Count > 1 && _lines.Count <= MaxLines)
            {
                var lastindex = _lines.Count - 1;
                _lines.RemoveAt(lastindex);
                DynamicSp.Children.RemoveAt(lastindex);
                TriggerChangedEvent(this, new EventArgs());
            }
        }

        private void TriggerChangedEvent(object sender, EventArgs e)
        {
            DynamicDataChanged(sender, e);
        }
    }
}