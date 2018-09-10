using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace AuctionClient
{
    /// <summary>
    /// Логика взаимодействия для AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public AddProductWindow(Account account)
        {
            this.account = account;
            InitializeComponent();
            InitDictionary();
        }

        private void InitDictionary()
        {
            UIElementCollection children = MainGrid.Children;
            foreach (var item in children)
            {
                if (item is TextBox)
                {
                    dictionary.Add(((TextBox)item).Name, false);
                }
            }
        }

        Account account;
        Dictionary<string, bool> dictionary = new Dictionary<string, bool>();

        private void ImgSource_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Изображения | *.jpeg; *.jpg; *.png; *.gif"
            };
            if (dlg.ShowDialog() == true)
            {
                ImagePath.Text = dlg.FileName;
            }
        }

        private async void AddProd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!System.IO.File.Exists(ImagePath.Text))
                    MessageBox.Show("Файл не найден", "Ошибка", MessageBoxButton.OK);
                else if (Regex.IsMatch(Price.Text, @"[^0-9.,]+"))
                    MessageBox.Show("Цена задана не верно", "Ошибка", MessageBoxButton.OK);
                else
                {
                    RequestMethods methods = RequestMethods.GetRequestMethods();
                    Product product = new Product(ImagePath.Text, float.Parse(Price.Text, System.Globalization.CultureInfo.InvariantCulture), Name.Text, rtbDescription.Text, account.AccountId);
                    byte[] imageBt = File.ReadAllBytes(ImagePath.Text);
                    int id = await methods.AddProductAsync(product);
                    bool isAdded = await methods.SetProductPhotoAsync(id, imageBt);
                    if(isAdded)
                    {
                        MessageBox.Show("Лот добавлен", "Уведомление");
                    }
                    else
                    {
                        MessageBox.Show("Лот не добавлен", "Уведомление");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void Cancell_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool CheckFields()
        {
            foreach (bool value in dictionary.Values)
            {
                if (value == false) return false;
            }
            return true;
        }

        private void ImagePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text != "")
            {
                dictionary[((TextBox)sender).Name] = true;
            }
            else
            {
                dictionary[((TextBox)sender).Name] = false;
            }
            if (CheckFields()) AddProd.IsEnabled = true;
            else AddProd.IsEnabled = false;
        }
    }
}
