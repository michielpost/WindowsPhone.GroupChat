using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Notification;
using System.Threading;
using System.Diagnostics;

namespace mpost.WP7.Client.Notification
{
    //HOW TO USE:
     //private void Application_Startup(object sender, StartupEventArgs e)
     //   {
     //       provider = new NotificationProvider("yourchannelname", "yourcloudservice");
     //       provider.Connect();
     //   }

    public class NotificationProvider : IDisposable
    {

        private HttpNotificationChannel _channel;
        private string _channelName;
        private string _serviceName;
        private Timer _retrySubscribeTimer = null; //retry of 2 mins if just started emulator/rebooted device - April CTP issue

        public delegate void RegisterCompleteHandler(Uri uri);

        public event RegisterCompleteHandler RegisterComplete;
        public event EventHandler<HttpNotificationEventArgs> HttpNotificationReceived;
        public event EventHandler<NotificationEventArgs> ShellToastNotificationReceived;
        public event EventHandler<NotificationChannelErrorEventArgs> ErrorOccurred;

        bool _supportsToast;
        bool _supportsTile;

        public NotificationProvider(string channelName, string serviceName, bool toast, bool tile)
        {
            _channelName = channelName;
            _serviceName = serviceName;

            _supportsTile = tile;
            _supportsToast = toast;
        }

        public void Connect(object stateInfo)
        {
            Connect();
            
        }

        private void Connect(Action actionIfNotFound)
        {
            try
            {
                _channel = HttpNotificationChannel.Find(_channelName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            if (_channel != null)
            {
                if (_channel.ChannelUri != null)
                {
                    SubscribeToChannelEvents();
                    RegisterChannel(_channel.ChannelUri);
                    SubscribeToNotifications();
                }
                else
                {
                    _channel.UnbindToShellTile();
                    _channel.UnbindToShellToast();
                    _channel.Close();
                    RetryChannelConnect();
                }
            }
            else
            {
                actionIfNotFound();
            }
        }

        public void Connect()
        {
            try
            {
                Connect(() =>
                {
                    RegisterNewChannel();
                });

            }
            catch (Exception ex)
            {
                //2mins after restart - documented bug in April CTP
                RetryChannelConnect();
               
            }
            //catch (Exception e)
            //{
            //    //This exception occurs if the notification channel was previously created and is not closed.             �
            //    Connect(() =>
            //    {
            //        RetryChannelConnect();
            //    });
            //}
        }

        private void RegisterNewChannel()
        {
            _channel = new HttpNotificationChannel(_channelName, _serviceName);
            SubscribeToChannelEvents();
            _channel.Open();
            SubscribeToNotifications();
        }

        private void SubscribeToNotifications()
        {
            try
            {
                //Remote tiles - currently not working I think this is a MPNS issue
                //ShellEntryPoint shellEntryPoint = new ShellEntryPoint();
                //shellEntryPoint.RemoteImageUri = new Uri("<A href="http://www.nickharris.net/wp-content/uploads/2010/06/Background1.png">http://www.nickharris.net/wp-content/uploads/2010/06/Background1.png</A>", UriKind.Absolute);            �
                //_channel.BindToShellEntryPoint(shellEntryPoint); // tile - remote

                if (_supportsTile)
                {
                    _channel.BindToShellTile(); // tile - local
                }
            }
            catch (Exception e)
            { } //do nothing - allready been subscribed to for current channel

            try
            {
                if (_supportsToast)
                {
                    _channel.BindToShellToast(); // - toast
                }
            }
            catch (Exception e)
            { } //do nothing - allready been subscribed to for current channel
        }

        private void SubscribeToChannelEvents()
        {
            _channel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(channel_ChannelUriUpdated); // channel URI returned from MPNS
            _channel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(channel_HttpNotificationReceived); // Raw
            //_channel.t += new EventHandler<NotificationEventArgs>(channel_ShellEntryPointNotificationReceived); // Tile
            _channel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(_channel_ShellToastNotificationReceived);
            _channel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(_channel_ErrorOccurred);
        }

        void _channel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            if (ShellToastNotificationReceived != null)
                ShellToastNotificationReceived(sender, e);

            //if (e.Collection != null)
            //{
            //    Dictionary<string, string> collection = (Dictionary<string, string>)e.Collection;
            //    System.Text.StringBuilder messageBuilder = new System.Text.StringBuilder();
            //    foreach (string elementName in collection.Keys)
            //    {
            //        //MessageBox.Show(string.Format("elementName:{0}, value:{1}", elementName, collection[elementName])) 

            //    }
            //}
        }

        void _channel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            if (ErrorOccurred != null)
                ErrorOccurred(sender, e);

            Debug.WriteLine(e.ErrorCode.ToString());
            //MessageBox.Show(e.ErrorCode.ToString());
        }





        //void channel_ShellEntryPointNotificationReceived(object sender, NotificationEventArgs e)
        //{
        //    if (e.Collection != null)
        //    {
        //        Dictionary<string, string> collection = (Dictionary<string, string>)e.Collection;
        //        System.Text.StringBuilder messageBuilder = new System.Text.StringBuilder();
        //        foreach (string elementName in collection.Keys)
        //        {
        //            MessageBox.Show(string.Format("elementName:{0}, value:{1}", elementName, collection[elementName]));
        //        }
        //    }
        //}

        void channel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            if (HttpNotificationReceived != null)
                HttpNotificationReceived(sender, e);

            //if (e.Notification.Body != null)
            //{
            //    using (System.IO.StreamReader reader = new System.IO.StreamReader(e.Notification.Body))
            //    {
            //        Debug.WriteLine(reader.ReadToEnd());
            //    }
            //}
        }

        void channel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            RegisterChannel(e.ChannelUri);
        }

        private void RegisterChannel(Uri uri)
        {
            if (RegisterComplete != null)
                RegisterComplete(uri);
        }

        private void RetryChannelConnect()
        {
            RegisterComplete(null);

            //if (_retrySubscribeTimer == null)
            //    _retrySubscribeTimer = new Timer(new TimerCallback(this.Connect), null, 2 * 60 * 1000, Timeout.Infinite);
            //else
            //    _retrySubscribeTimer.Change(2 * 60 * 1000, -1);
        }

        public void Dispose()
        {
            //if (_channel != null)
            //{
            //    _channel.Dispose();
            //}

            if (_retrySubscribeTimer != null)
                _retrySubscribeTimer.Dispose();
        }

        public void Disconnect()
        {
            if (_channel != null)
            {
                if(_channel.IsShellToastBound)
                    _channel.UnbindToShellToast();

                if (_channel.IsShellTileBound)
                    _channel.UnbindToShellTile();

                try
                {
                    _channel.Close();
                }
                finally
                {
                    _channel.Dispose();
                }
            }
        }
    }


}
