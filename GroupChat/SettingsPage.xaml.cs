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
using GroupChat.Models;
using GroupChat.Services;

namespace GroupChat
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        AppInfo _appInfo;

        public SettingsPage()
        {
            InitializeComponent();

            _appInfo = ClientInfoService.GetAppInfo();

            if (_appInfo.Age.HasValue)
            {
                DatePicker.Value = _appInfo.Age;
            }

            if (_appInfo.AllowPush)
                AllowPush.IsChecked = true;

           
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _appInfo.Age = DatePicker.Value;

            if (AllowPush.IsChecked.HasValue && AllowPush.IsChecked.Value)
                _appInfo.AllowPush = true;
            else
            {
                _appInfo.AllowPush = false;

                //Disconnect from PUSH channel
                App.Disconnect();
            }


            ClientInfoService.SaveAppInfo(_appInfo);

            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));

        }
    }
}