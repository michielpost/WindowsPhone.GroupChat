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
using mpost.WP7.PushServiceClient.WcfPushService;
using System.ComponentModel;

namespace mpost.WP7.PushServiceClient
{
    public static class PushServiceClient
    {
        public static event EventHandler<AsyncCompletedEventArgs> ConnectFinished;
        public static event EventHandler<SendMessageCompletedEventArgs> SendMessageCompleted;
        public static event EventHandler<GetLastMessagesCompletedEventArgs> GetLastMessagesCompleted;

        public static void Connect(ClientInfo clientInfo, string application)
        {
            PushClient client = new PushClient();

            try
            {
                client.ConnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_ConnectCompleted);
                client.ConnectAsync(clientInfo, application, 1, false);

                client.CloseAsync();
            }
            catch (Exception e)
            {
                client.Abort();
            }
            finally
            {
                client.CloseAsync();
            }
        }

        static void client_ConnectCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {            
            if (ConnectFinished != null)
                ConnectFinished(sender, e);
        }

        public static void Disconnect(string uri)
        {
            PushClient client = new PushClient();

            try
            {
                client.DisconnectAsync(uri);

                client.CloseAsync();
            }
            catch (Exception e)
            {
                client.Abort();
            }
            finally
            {
                client.CloseAsync();
            }
        }

        public static void SendMessage(ClientInfo clientInfo, string message)
        {
            PushClient client = new PushClient();

            try
            {
                client.SendMessageCompleted += new EventHandler<SendMessageCompletedEventArgs>(client_SendMessageCompleted);
                client.SendMessageAsync(clientInfo, message);
                client.CloseAsync();
            }
            catch (Exception e)
            {
                client.Abort();
            }
            finally
            {
                client.CloseAsync();
            }
        }

        static void client_SendMessageCompleted(object sender, SendMessageCompletedEventArgs e)
        {
            if (SendMessageCompleted != null)
                SendMessageCompleted(sender, e);
        }

        public static void GetLastMessages(ClientInfo clientInfo)
        {
            PushClient client = new PushClient();

            try
            {
                client.GetLastMessagesCompleted += new EventHandler<GetLastMessagesCompletedEventArgs>(client_GetLastMessagesCompleted);
                client.GetLastMessagesAsync(clientInfo);
                client.CloseAsync();
            }
            catch (Exception e)
            {
                client.Abort();
            }
            finally
            {
                client.CloseAsync();
            }
        }

        static void client_GetLastMessagesCompleted(object sender, GetLastMessagesCompletedEventArgs e)
        {
            if (GetLastMessagesCompleted != null)
                GetLastMessagesCompleted(sender, e);
        }
    }
}
