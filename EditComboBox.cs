using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

using System.ComponentModel;
using System.Windows.Threading;

namespace Alpha.Quant.CustomControls.Controls
{

    /// <summary>
    /// 模块编号：自定义控件
    /// 作用：编辑搜索功能（输入文本，items自动过滤符合项）
    /// 作者：张钢
    /// 编写日期：2016-04-13 
    /// </summary>
    public class EditComboBox : ComboBox
    {
        //用于筛选数据的视图


        public static readonly DependencyProperty ClosedSelectedItemProperty = DependencyProperty.Register("ClosedSelectedItem", typeof(object), typeof(EditComboBox));

        //ListCollectionView view;
        private string editText = "";//编辑文本内容,this.text
        private TextBox editTextBox;

        private IEnumerable OriginSource;
        //下拉弹出框
        Popup popup;

        DispatcherTimer Filtertiemr = new DispatcherTimer();

        public object ClosedSelectedItem
        {
            get { return (object)GetValue(ClosedSelectedItemProperty); }
            set { SetValue(ClosedSelectedItemProperty, value); }
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.IsEditable = true;
            this.IsTextSearchEnabled = false;
            this.StaysOpenOnEdit = true;
            //SetValue(InputMethod.IsInputMethodEnabledProperty, false);
            //SetValue(InputMethod.IsInputMethodSuspendedProperty, false);
            IsSynchronizedWithCurrentItem = false;

            Filtertiemr.Interval = TimeSpan.FromMilliseconds(200);
            Filtertiemr.Tick += (ss, ee) =>
            {
                Filter();
            };
        }

        /// <summary>
        /// 下拉框获取焦点，首次搜索文本编辑框
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (IsEditable)
            {
                editTextBox.Focus();
                editTextBox.SelectionLength = 0;
                //editTextBox.CaretIndex = editTextBox.Text.Length;
                editTextBox.SelectAll();
            }
        }

        /// <summary>
        /// 搜索编辑文本框，添加文本改变事件
        /// </summary>
        /// <param name="ob"></param>
        private void FindTextBox(DependencyObject ob)
        {
            popup = GetTemplateChild("PART_Popup") as Popup;
            editTextBox = (TextBox)GetTemplateChild("PART_EditableTextBox");
            if (editTextBox == null)
            {
                int length = VisualTreeHelper.GetChildrenCount(ob);
                for (int i = 0; i < length; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(ob, i);
                    if (child != null && child is TextBox)
                    {
                        editTextBox = child as TextBox;
                    }
                    else
                    {
                        FindTextBox(child);
                    }
                }
            }
            if (editTextBox != null)
            {
                //注册文本改变事件
                editTextBox.TextChanged += EditComboBox_TextChanged;
                //editTextBox.KeyDown += EditComboBox_KeyDown1;
                //editTextBox.SelectionChanged += EditTextBox_SelectionChanged;
                //editTextBox.PreviewKeyDown += EditTextBox_PreviewKeyDown;
                //editTextBox.PreviewKeyUp += EditTextBox_PreviewKeyUp;

                //editTextBox.SetValue(InputMethod.IsInputMethodEnabledProperty, false);
                //editTextBox.SetValue(InputMethod.IsInputMethodSuspendedProperty, false);
                if (IsEditable == true)
                {
                    editTextBox.Focus();
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            FindTextBox(this);
            if (popup == null) return;
            popup.Closed += Popup_Closed;
            popup.Opened += Popup_Opened;
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            if (editTextBox.SelectionLength > 0)
            {
                editTextBox.SelectionLength = 0;
            }
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            if (ClosedSelectedItem != SelectedItem)
            {
                ClosedSelectedItem = SelectedItem;
            }
        }


        /// <summary>
        /// 文本改变，动态控制下拉条数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //SelectedItem = null;
            TextBox tb = sender as TextBox;
            if (tb.IsFocused)
            {
                if (!this.IsDropDownOpen)
                    this.IsDropDownOpen = true;

                //if (editText == this.Text || (string.IsNullOrEmpty(this.Text)))
                //    return;

                //editText = this.Text;
                if (!string.IsNullOrEmpty(Text) && Text.Length == 1 && tb.CaretIndex == 0)
                {
                    tb.CaretIndex = 1;
                }
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            if (OriginSource == null)
            {
                OriginSource = newValue;//获取一开始的列表
            }
        }


        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (IsDropDownOpen)
                    {
                        if (Items != null && Items.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(this.Text))
                            {
                                if (Items.CurrentItem != null)
                                {
                                    SelectedItem = Items.CurrentItem;
                                }
                                else
                                {
                                    object Firstitem = Items.GetItemAt(0);
                                    if (SelectedItem != Firstitem)
                                        SelectedItem = Firstitem;
                                }
                            }
                            else
                            {
                                SelectedItem = null;
                            }
                        }
                        IsDropDownOpen = false;
                    }
                    break;
                case Key.Down:
                    if (!IsDropDownOpen)
                    {
                        IsDropDownOpen = true;
                        e.Handled = true;
                        return;
                    }
                    if (IsDropDownOpen)
                    {
                        if (Items.CurrentItem == null)
                            Items.MoveCurrentToFirst();
                        else
                            Items.MoveCurrentToNext();
                        SelectedItem = Items.CurrentItem;
                    }
                    break;
                case Key.Up:
                    if (IsDropDownOpen)
                    {
                        if (Items.CurrentItem == null)
                            Items.MoveCurrentToFirst();
                        else
                            Items.MoveCurrentToPrevious();
                        if (SelectedItem == null)
                        {
                            Items.Refresh();
                        }
                        SelectedItem = Items.CurrentItem;
                    }
                    break;
                case Key.Back:
                    SelectedItem = null;
                    backText();
                    Filtertiemr.Start();
                    break;
                case Key.Delete:
                    SelectedItem = null;
                    deleteText();
                    Filtertiemr.Start();
                    break;
                case Key.Escape:
                    if (this.Text != null)
                    {
                        Text = "";
                        SelectedItem = null;
                    }
                    base.OnKeyUp(e);
                    break;
                default:
                    if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                        || ModifierKeys.Shift == (e.KeyboardDevice.Modifiers & ModifierKeys.Shift))
                    {
                        return;
                    }
                    if (e.Key == Key.Left || e.Key == Key.Right)
                        return;
                    base.OnKeyUp(e);
                    Filtertiemr.Start();
                    break;
            }
            e.Handled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = true;
                return;
            }
            base.OnPreviewKeyDown(e);
        }


        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (SelectedItem != null)
            {
                base.OnSelectionChanged(e);
            }
        }

        private void Filter()
        {
            Filtertiemr.Stop();
            if (string.IsNullOrEmpty(DisplayMemberPath))
                return;
            //if (view == null)
            //    return;
            if (OriginSource == null)
                return;
            if (string.IsNullOrEmpty(this.Text))
            {
                ItemsSource = OriginSource;
                return;
            }
            string value2 = editTextBox.Text.ToUpper();

            List<object> Source = new List<object>();
            foreach (var item in OriginSource)
            {
                if (getDisplayMemberPathValue(item).ToUpper().StartsWith(value2))
                    Source.Add(item);
            }

            foreach (var item in OriginSource)
            {
                if (!getDisplayMemberPathValue(item).ToUpper().StartsWith(value2) && getDisplayMemberPathValue(item).ToUpper().Contains(value2))
                    Source.Add(item);
            }
            ItemsSource = Source;
            //startwithLst.AddRange(ContainsLst);
            //ItemsSource = startwithLst;
            //view.Filter = new Predicate<object>(Filteritem);
            //foreach (var item in view)
            //{
            //    if (FilterContains(item))
            //    {
            //        view.Remove(item);
            //        view.AddNewItem(item);
            //    }
            //}

        }
        private bool Filteritem(object item)
        {
            var propertyinfo = item.GetType().GetProperty(DisplayMemberPath);
            if (propertyinfo == null)
                return true;
            string value = propertyinfo.GetValue(item, null).ToString();
            string value1 = value.ToUpper();
            string value2 = editTextBox.Text.ToUpper();
            if (value1.StartsWith(value2))
                return true;
            else
            {
                return false;
            }
        }

        private bool FilterContains(object item)
        {
            var propertyinfo = item.GetType().GetProperty(DisplayMemberPath);
            if (propertyinfo == null)
                return true;
            string value = propertyinfo.GetValue(item, null).ToString();
            string value1 = value.ToUpper();
            string value2 = editTextBox.Text.ToUpper();
            if (!value1.StartsWith(value2) && value1.Contains(value2))
                return true;
            else
            {
                return false;
            }
        }

        private string getDisplayMemberPathValue(object item)
        {
            var propertyinfo = item.GetType().GetProperty(DisplayMemberPath);
            if (propertyinfo == null)
                return "";
            return propertyinfo.GetValue(item, null).ToString();
        }


        private void deleteText()
        {
            string str2 = this.Text;
            //光标一开始的位置
            int start2 = editTextBox.SelectionStart;
            if (!string.IsNullOrEmpty(this.Text))
            {
                //光标位置

                if (editTextBox.SelectionLength == 0)
                {
                    if (editTextBox.SelectionStart != editTextBox.Text.Length)
                    {
                        str2 = this.Text.Remove(start2, 1);
                    }
                }
                else
                {
                    str2 = this.Text.Remove(editTextBox.SelectionStart, editTextBox.SelectionLength);
                    //start -= editTextBox.SelectionLength;
                }
            }
            if (SelectedItem != null)
                SelectedItem = null;
            editTextBox.Text = str2;

            if (start2 >= 0 && start2 <= editTextBox.Text.Length)
                editTextBox.SelectionStart = start2;
        }

        private void backText()
        {
            string str = "";
            //光标一开始的位置
            int start = editTextBox.SelectionStart;
            if (!string.IsNullOrEmpty(this.Text))
            {
                //光标位置

                if (editTextBox.SelectionLength == 0)
                {
                    if (editTextBox.SelectionStart != 0)
                    {
                        str = this.Text.Remove(editTextBox.SelectionStart - 1, 1);
                        start--;
                    }
                }
                else
                {
                    str = this.Text.Remove(editTextBox.SelectionStart, editTextBox.SelectionLength);
                    start -= editTextBox.SelectionLength;
                }

            }
            if (SelectedItem != null)
                SelectedItem = null;
            editTextBox.Text = str;
            if (start >= 0 && start <= editTextBox.Text.Length)
                editTextBox.SelectionStart = start;
        }
    }
}
