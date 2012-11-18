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
using mpost.WP7.Client.Storage;
using mpost.WP7.PushServiceClient.WcfPushService;
using GroupChat.Models;

namespace GroupChat.Services
{
    public static class ClientInfoService
    {
        public static ClientInfo GetClientInfo()
        {
            ClientInfo info =  IsolatedStorageCacheManager<ClientInfo>.Retrieve("ClientInfo.xml");
            if (info == null)
                info = new ClientInfo();

            return info;
        }

        public static void SaveClientInfo(ClientInfo clientInfo)
        {
            IsolatedStorageCacheManager<ClientInfo>.Store("ClientInfo.xml", clientInfo);
        }

        public static AppInfo GetAppInfo()
        {
            AppInfo info = IsolatedStorageCacheManager<AppInfo>.Retrieve("AppInfo.xml");
            if (info == null)
                info = new AppInfo();

            return info;
        }

        public static void SaveAppInfo(AppInfo appInfo)
        {
            IsolatedStorageCacheManager<AppInfo>.Store("AppInfo.xml", appInfo);
        }

    }
}
