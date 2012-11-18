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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using mpost.WP7.Client.Notification;
using System.Threading;
using System.Diagnostics;
using mpost.WP7.PushServiceClient;
using GroupChat.Services;
using System.ComponentModel;
using mpost.WP7.PushServiceClient.WcfPushService;
using Microsoft.Phone.Marketplace;

namespace GroupChat
{
    public partial class App : Application
    {
        public static bool IsTrial = false;

        private static void LoadIsTrial()
        {
            IsTrial = new LicenseInformation().IsTrial();

#if DEBUG
            if (MessageBox.Show("Test in trial mode?", "Debug Trial",
                 MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                IsTrial = true;
            }
#endif
        }

        // Easy access to the root frame
        public PhoneApplicationFrame RootFrame { get; private set; }

        public static bool IsConnected { get; set; }
       
        private static NotificationProvider _notificationProvider = null;
        public static NotificationProvider NotificationProvider
        {
            get
            {
                // Delay creation of the view model until necessary
                if (_notificationProvider == null)
                    _notificationProvider = new NotificationProvider("Data", "GroupChat", true, false);

                return _notificationProvider;
            }
        }

        private static ClientInfo _clientInfo;
        public static ClientInfo ClientInfo
        {
            get
            {
                if (_clientInfo == null)
                    _clientInfo = ClientInfoService.GetClientInfo();

                return _clientInfo;
            }
            set
            {
                _clientInfo = value;

                ClientInfoService.SaveClientInfo(_clientInfo);
            }
        }
        public static void SaveClientInfo()
        {
            ClientInfoService.SaveClientInfo(_clientInfo);
        }
      

        public static event EventHandler<AsyncCompletedEventArgs> ConnectFinished;

               
        // Constructor
        public App()
        {
            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

           NotificationProvider.RegisterComplete += new mpost.WP7.Client.Notification.NotificationProvider.RegisterCompleteHandler(NotificationProvider_RegisterComplete);
           PushServiceClient.ConnectFinished += new EventHandler<AsyncCompletedEventArgs>(PushServiceClient_ConnectFinished);
            
        }

      

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            LoadIsTrial();
           // (new Thread(new ThreadStart(Connect))).Start();                          
        }

        private static void Connect()
        {
            if (!string.IsNullOrEmpty(ClientInfo.Uri)
               && !string.IsNullOrEmpty(ClientInfo.DisplayName)
               && !string.IsNullOrEmpty(ClientInfo.ChannelName))
            {
                Connect(ClientInfo.DisplayName, ClientInfo.ChannelName);
            }
        }

        public static void Disconnect()
        {
            App.IsConnected = false;

            try
            {
                if (!string.IsNullOrEmpty(App.ClientInfo.Uri))
                    PushServiceClient.Disconnect(App.ClientInfo.Uri);
            }
            catch { };

            try{
                NotificationProvider.Disconnect();
            }
            catch { };

            try{
            App.ClientInfo.Uri = null;
            App.SaveClientInfo();
            }
            catch { };
            
        }

        public static void Connect(string displayName, string groupName)
        {
            ClientInfo.DisplayName = displayName;
            ClientInfo.ChannelName = groupName;
            SaveClientInfo();

            NotificationProvider.Connect();
                        
        }

        void NotificationProvider_RegisterComplete(Uri uri)
        {
            if (uri != null)
            {
                ClientInfo.Uri = uri.AbsoluteUri;
                SaveClientInfo();

                PushServiceClient.Connect(ClientInfo, "GroupChat");
                Debug.WriteLine(uri.AbsoluteUri.ToString());
            }
            else
            {
                if (ConnectFinished != null)
                    ConnectFinished(this, new AsyncCompletedEventArgs(new Exception("Notification server not found"), true, null));           
            }

          
        }

        void PushServiceClient_ConnectFinished(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error == null)
                IsConnected = true;

            if (ConnectFinished != null)
                ConnectFinished(sender, e);           
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            LoadIsTrial();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

       
    }
}
