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
using mpost.WP7.PushServiceClient;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Marketplace;
using GroupChat.Models;
using GroupChat.Services;

namespace GroupChat
{
    public partial class MainPage : PhoneApplicationPage
    {
        AppInfo _appInfo;

        private int errorCount = 0;
        private bool _isHit = false;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            App.NotificationProvider.ShellToastNotificationReceived += new EventHandler<Microsoft.Phone.Notification.NotificationEventArgs>(NotificationProvider_ShellToastNotificationReceived);
            App.NotificationProvider.ErrorOccurred += new EventHandler<Microsoft.Phone.Notification.NotificationChannelErrorEventArgs>(NotificationProvider_ErrorOccurred);
            PushServiceClient.SendMessageCompleted += new EventHandler<mpost.WP7.PushServiceClient.WcfPushService.SendMessageCompletedEventArgs>(PushServiceClient_SendMessageCompleted);
            PushServiceClient.GetLastMessagesCompleted += new EventHandler<mpost.WP7.PushServiceClient.WcfPushService.GetLastMessagesCompletedEventArgs>(PushServiceClient_GetLastMessagesCompleted);
        }


        void PushServiceClient_GetLastMessagesCompleted(object sender, mpost.WP7.PushServiceClient.WcfPushService.GetLastMessagesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                OutputStack.Children.Clear();

                foreach (string msg in e.Result)
                {
                    ShowString(msg);
                }
            }
        }

        void PushServiceClient_SendMessageCompleted(object sender, mpost.WP7.PushServiceClient.WcfPushService.SendMessageCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                errorCount++;

                if (errorCount >= 3)
                    Disconnect();

                StatusTextBlock.Text = "Status: Error occured sending message.";
            }
            else
                StatusTextBlock.Text = string.Format("{0}: message sent", DateTime.Now);

           
        }

        void NotificationProvider_ErrorOccurred(object sender, Microsoft.Phone.Notification.NotificationChannelErrorEventArgs e)
        {
            Disconnect();

            StatusTextBlock.Text = "Status: Error occured.";
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!App.IsConnected)
            {
                if (!_isHit)
                {
                    _isHit = true;
                    NavigationService.Navigate(new Uri("/ConnectPage.xaml", UriKind.Relative));
                }
                else
                    ShowDisconnected();

            }
            else
            {
                StatusTextBlock.Text = string.Format("Connected as: {0}", App.ClientInfo.DisplayName);
                PageTitle.Text = App.ClientInfo.ChannelName;

                SendButton.Visibility = System.Windows.Visibility.Visible;
                InputTextBox.Visibility = System.Windows.Visibility.Visible;

                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).IsEnabled = true;
                ((ApplicationBarIconButton)this.ApplicationBar.Buttons[1]).IsEnabled = false;

                PushServiceClient.GetLastMessages(App.ClientInfo);
            }

            _appInfo = ClientInfoService.GetAppInfo();

            if (App.IsTrial)
                BuyButton.Visibility = System.Windows.Visibility.Visible;
            else
                BuyButton.Visibility = System.Windows.Visibility.Collapsed;

        }

        void NotificationProvider_ShellToastNotificationReceived(object sender, Microsoft.Phone.Notification.NotificationEventArgs e)
        {
            string msg = string.Empty;
            Dictionary<string, string> collection = (Dictionary<string, string>)e.Collection;
            if (collection.Values.Count > 1)
                msg = collection.Values.Skip(1).First();

            if (!string.IsNullOrEmpty(msg))
            {
                this.Dispatcher.BeginInvoke(() => ShowString(msg));
            }
        }

        private void ShowString(string msg)
        {
            //OutputTextBlock.Text += "\r\n" + msg;

            TextBlock lastText = new TextBlock() { Text = msg, TextWrapping = TextWrapping.Wrap };
            lastText.FontSize = 25;
            OutputStack.Children.Add(lastText);

            //Scroll to end
            this.Dispatcher.BeginInvoke(() =>
            {
                OutputScrollViewer.ScrollToVerticalOffset(OutputScrollViewer.ScrollableHeight);
            });

        }

        private void DisconnectMenuItem_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void ConnectMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ConnectPage.xaml", UriKind.Relative));
        }

        private void Disconnect()
        {
            App.Disconnect();

            ShowDisconnected();
           
        }

        private void ShowDisconnected()
        {
            StatusTextBlock.Text = "Disconnected";
            PageTitle.Text = "disconnected";
            SendButton.Visibility = System.Windows.Visibility.Collapsed;
            InputTextBox.Visibility = System.Windows.Visibility.Collapsed;

            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[0]).IsEnabled = false;
            ((ApplicationBarIconButton)this.ApplicationBar.Buttons[1]).IsEnabled = true;

            //DisconnectMenuItem.IsEnabled = false;
            //ConnectMenuItem.IsEnabled = true;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendInput();
        }

        private void SendInput()
        {
            string input = InputTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(input))
            {
                InputTextBox.Text = string.Empty;
                PushServiceClient.SendMessage(App.ClientInfo, input);
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask detailTask = new MarketplaceDetailTask();
            detailTask.Show();
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendInput();
            }

        }
               
    }
}
