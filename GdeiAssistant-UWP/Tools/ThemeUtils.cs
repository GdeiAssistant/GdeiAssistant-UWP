using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GdeiAssistant.Tools
{
    public class ThemeUtils
    {
        //加载应用主题参数，自动切换主题
        public static void LoadThemeSetting()
        {
            //从本地应用配置加载主题参数
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string theme = localSettings.Values["theme"] as string;
            Frame frame = (Application.Current as App).rootFrame;
            if (string.IsNullOrEmpty(theme) || theme.Equals("light"))
            {
                frame.RequestedTheme = ElementTheme.Light;
            }
            else
            {
                frame.RequestedTheme = ElementTheme.Dark;
            }
        }

        //获取当前应用主题
        public static  ElementTheme GetCurrentTheme()
        {
            Frame frame = (Application.Current as App).rootFrame;
            return frame.RequestedTheme;
        }

        //切换至浅色主题
        public static void ChangeToLightTheme()
        {
            Frame frame = (Application.Current as App).rootFrame;
            frame.RequestedTheme = ElementTheme.Light;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["theme"] = "light";
        }

        //切换至深色主题
        public static void ChangeToDarkTheme()
        {
            Frame frame = (Application.Current as App).rootFrame;
            frame.RequestedTheme = ElementTheme.Dark;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["theme"] = "dark";
        }
    }
}
