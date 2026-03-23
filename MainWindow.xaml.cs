using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace laba8_WPF
{
    public partial class MainWindow : Window
    {
        private int documentCounter = 0; // починаю з 0

        public MainWindow()
        {
            InitializeComponent();

            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

            // при запуску програми автоматично створюю першу вкладку
            NewTab_Click(null, null);
        }

        private RichTextBox GetActiveRichTextBox()
        {
            if (tabControl != null && tabControl.SelectedItem is TabItem selectedTab)
                return selectedTab.Content as RichTextBox;
            return null;
        }

        // створення вкладок та їх закриття
        private void NewTab_Click(object sender, ExecutedRoutedEventArgs e)
        {
            documentCounter++;
            RichTextBox newRtb = new RichTextBox { Padding = new Thickness(15), BorderThickness = new Thickness(0) };
            newRtb.SelectionChanged += rtbEditor_SelectionChanged;

            bool isUk = cmbLanguage.SelectedIndex == 0;
            string headerPrefix = isUk ? "Документ" : "Document";

            // створення контейнера для заголовка вкладки
            StackPanel headerPanel = new StackPanel { Orientation = Orientation.Horizontal };

            // текст вкладки
            TextBlock headerText = new TextBlock { Text = $"{headerPrefix} {documentCounter}", VerticalAlignment = VerticalAlignment.Center };

            // кнопка закриття
            Button closeButton = new Button
            {
                Content = "✕",
                Foreground = Brushes.DarkRed,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(10, 0, 0, 0),
                Padding = new Thickness(5, 0, 5, 0),
                Cursor = Cursors.Hand
            };

            TabItem newTab = new TabItem { Content = newRtb };

            // логіка закриття по кліку на хрестик
            closeButton.Click += (s, ev) =>
            {
                if (tabControl.Items.Count > 1)
                {
                    tabControl.Items.Remove(newTab);
                }
                else
                {
                    string msg = isUk ? "Останню вкладку не можна закрити!" : "Cannot close the last tab!";
                    MessageBox.Show(msg, isUk ? "Увага" : "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            // все разом
            headerPanel.Children.Add(headerText);
            headerPanel.Children.Add(closeButton);
            newTab.Header = headerPanel;

            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;
        }

        private void CloseAllTabs_Click(object sender, RoutedEventArgs e)
        {
            bool isUk = cmbLanguage.SelectedIndex == 0;
            string msg = isUk ? "Ви впевнені, що хочете закрити всі вкладки?" : "Are you sure you want to close all tabs?";
            string title = isUk ? "Підтвердження" : "Confirmation";

            if (MessageBox.Show(msg, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                tabControl.Items.Clear();
                documentCounter = 0;
                NewTab_Click(null, null); // чистий документ
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl) rtbEditor_SelectionChanged(null, null);
        }

        // відкриття і збереження файлів
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RichTextBox currentRtb = GetActiveRichTextBox();
            if (currentRtb == null) return;

            OpenFileDialog dlg = new OpenFileDialog { Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open))
                {
                    TextRange range = new TextRange(currentRtb.Document.ContentStart, currentRtb.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Rtf);

                    // зміна імені вкладки
                    TabItem activeTab = (TabItem)tabControl.SelectedItem;
                    if (activeTab.Header is StackPanel sp && sp.Children[0] is TextBlock tb)
                        tb.Text = System.IO.Path.GetFileName(dlg.FileName);
                }
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RichTextBox currentRtb = GetActiveRichTextBox();
            if (currentRtb == null) return;

            SaveFileDialog dlg = new SaveFileDialog { Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create))
                {
                    TextRange range = new TextRange(currentRtb.Document.ContentStart, currentRtb.Document.ContentEnd);
                    range.Save(fileStream, DataFormats.Rtf);

                    TabItem activeTab = (TabItem)tabControl.SelectedItem;
                    if (activeTab.Header is StackPanel sp && sp.Children[0] is TextBlock tb)
                        tb.Text = System.IO.Path.GetFileName(dlg.FileName);
                }
            }
        }

        // переклад інтерфейсу
        private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (btnOpen == null) return;
            bool isUk = cmbLanguage.SelectedIndex == 0;

            if (isUk)
            {
                mainWindow.Title = "Лабораторна робота 8 Зубко Олександр 201-ТК";
                btnOpen.Content = "Відкрити";
                btnSave.Content = "Зберегти";
                btnNew.Content = "Створити";
                btnCloseAll.Content = "Закрити всі";
                btnImage.Content = "Вставити фото";

                cmbFontColor.ToolTip = "Колір тексту";
                if (cbiColorBlack != null) { cbiColorBlack.Content = "Чорний"; cbiColorRed.Content = "Червоний"; cbiColorBlue.Content = "Синій"; cbiColorGreen.Content = "Зелений"; }
                if (btnAlignLeft != null) { btnAlignLeft.ToolTip = "Ліворуч"; btnAlignCenter.ToolTip = "По центру"; btnAlignRight.ToolTip = "Праворуч"; }
            }
            else
            {
                mainWindow.Title = "Lab 8 Zubko Olexandr 201-TK";
                btnOpen.Content = "Open";
                btnSave.Content = "Save";
                btnNew.Content = "New";
                btnCloseAll.Content = "Close All";
                btnImage.Content = "Insert Image";

                cmbFontColor.ToolTip = "Text Color";
                if (cbiColorBlack != null) { cbiColorBlack.Content = "Black"; cbiColorRed.Content = "Red"; cbiColorBlue.Content = "Blue"; cbiColorGreen.Content = "Green"; }
                if (btnAlignLeft != null) { btnAlignLeft.ToolTip = "Align Left"; btnAlignCenter.ToolTip = "Align Center"; btnAlignRight.ToolTip = "Align Right"; }
            }

            // переклад заголовка
            if (tabControl != null)
            {
                foreach (TabItem tab in tabControl.Items)
                {
                    if (tab.Header is StackPanel sp && sp.Children.Count > 0 && sp.Children[0] is TextBlock tb)
                    {
                        if (tb.Text.StartsWith("Документ") || tb.Text.StartsWith("Document"))
                        {
                            string num = tb.Text.Split(' ')[1];
                            tb.Text = isUk ? $"Документ {num}" : $"Document {num}";
                        }
                    }
                }
            }
        }

        // оновлення стану кнопок форматування
        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRtb = GetActiveRichTextBox();
            if (currentRtb == null) return;

            object temp = currentRtb.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));

            temp = currentRtb.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));

            temp = currentRtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

            temp = currentRtb.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            cmbFontFamily.SelectedItem = temp;

            temp = currentRtb.Selection.GetPropertyValue(Inline.FontSizeProperty);
            cmbFontSize.Text = temp?.ToString();
        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetActiveRichTextBox()?.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
        }

        private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(cmbFontSize.Text, out _))
                GetActiveRichTextBox()?.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
        }

        private void cmbFontColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RichTextBox currentRtb = GetActiveRichTextBox();
            if (currentRtb != null && cmbFontColor.SelectedItem is ComboBoxItem selectedItem)
            {
                string colorName = selectedItem.Tag.ToString();
                var color = (Color)ColorConverter.ConvertFromString(colorName);
                currentRtb.Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(color));
            }
        }

        //вставка фоток
        private void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox currentRtb = GetActiveRichTextBox();
            if (currentRtb == null) return;

            OpenFileDialog dlg = new OpenFileDialog { Filter = "Зображення (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Всі файли (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(dlg.FileName));
                    Image image = new Image { Source = bitmap, Width = 300, Stretch = Stretch.Uniform };
                    new InlineUIContainer(image, currentRtb.Selection.Start);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при завантаженні зображення: " + ex.Message);
                }
            }
        }
    }
}