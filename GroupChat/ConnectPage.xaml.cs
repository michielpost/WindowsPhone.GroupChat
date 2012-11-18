using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using mpost.WP7.PushServiceClient;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Tasks;
using GroupChat.Models;
using GroupChat.Services;

namespace GroupChat
{
    public partial class ConnectPage : PhoneApplicationPage
    {
        private bool _validAge = false;

        AppInfo _appInfo;

        public ConnectPage()
        {
            InitializeComponent();
            App.ConnectFinished +=new EventHandler<AsyncCompletedEventArgs>(App_ConnectFinished);

            _appInfo = ClientInfoService.GetAppInfo();

            if (_appInfo.Age.HasValue)
            {
                DatePicker.Value = _appInfo.Age;
                CheckDate(_appInfo.Age);
            }
            if (_appInfo.AllowPush)
                AllowPush.IsChecked = true;
            
        }

        private void DatePicker_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            _appInfo.Age = e.NewDateTime;
            ClientInfoService.SaveAppInfo(_appInfo);

            DateTime? newDate = e.NewDateTime;
            CheckDate(newDate);
        }

        private void CheckDate(DateTime? newDate)
        {
            if (newDate < DateTime.Now.AddYears(-13))
                _validAge = true;
            else
                _validAge = false;
        }

               

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (App.IsTrial)
            {
                App.ClientInfo.ChannelName = "Trial";
                GroupTextBox.IsReadOnly = true;
                UpgradeStackPanel.Visibility = System.Windows.Visibility.Visible;
            }
           
            if(!string.IsNullOrEmpty(App.ClientInfo.DisplayName))
                DisplayNameTextBox.Text = App.ClientInfo.DisplayName;
            if (!string.IsNullOrEmpty(App.ClientInfo.ChannelName))
                GroupTextBox.Text = App.ClientInfo.ChannelName;

            if (!string.IsNullOrEmpty(App.ClientInfo.Uri)
              && !string.IsNullOrEmpty(App.ClientInfo.DisplayName)
              && !string.IsNullOrEmpty(App.ClientInfo.ChannelName))
            {
                MakeConnection(App.ClientInfo.DisplayName, App.ClientInfo.ChannelName);
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
             string displayName = DisplayNameTextBox.Text.Trim();
                string groupname = GroupTextBox.Text.Trim();

            if(!string.IsNullOrEmpty(displayName) &&
                !string.IsNullOrEmpty(groupname)
                && _validAge
                && AllowPush.IsChecked.HasValue && AllowPush.IsChecked.Value)
            {
                _appInfo.AllowPush = true;
                ClientInfoService.SaveAppInfo(_appInfo);

                MakeConnection(displayName, groupname);
            }
            else if (string.IsNullOrEmpty(displayName) ||
                string.IsNullOrEmpty(groupname))
            {
                MessageBox.Show("Enter a Displayname and Group");
            }
            else if (!_validAge)
            {
                MessageBox.Show("You must be at least 13 years old to use this application.");
            }
            else if (!AllowPush.IsChecked.HasValue || !AllowPush.IsChecked.Value)
            {
                MessageBox.Show("This application requires toast notifications. You can disable them at any time in the settings menu.");
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask detailTask = new MarketplaceDetailTask();
            detailTask.Show();
        }

        private void MakeConnection(string displayName, string groupname)
        {
            ConnectButton.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Visibility = System.Windows.Visibility.Collapsed;

            this.PageTitle.Text = "connecting...";
           
            App.Connect(displayName, groupname);
        }

        void App_ConnectFinished(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
                GoToMainPage();
            else
            {
                this.PageTitle.Text = "error";
                this.TitlePanel.Children.Add(new TextBlock() { Text = "Unable to connect to server." });
            }
        }

        private void GoToMainPage()
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void DisplayNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (App.IsTrial)
                    DatePicker.Focus();
                else
                    GroupTextBox.Focus();
            }

        }

        private void GroupTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DatePicker.Focus();
            }
        }
        
    }
}