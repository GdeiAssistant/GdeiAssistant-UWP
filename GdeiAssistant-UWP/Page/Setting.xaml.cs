using GdeiAssistant.Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GdeiAssistant.Page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Setting : Windows.UI.Xaml.Controls.Page
    {
        public Setting()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            if ((Application.Current as App).rootFrame.RequestedTheme.Equals(ElementTheme.Dark))
            {
                DarkThemeToggleSwitch.IsOn = true;
            }
            else
            {
                DarkThemeToggleSwitch.IsOn = false;
            }
        }

        //切换深色主题开关
        private void DarkThemeToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn)
                {
                    ThemeUtils.ChangeToDarkTheme();
                }
                else
                {
                    ThemeUtils.ChangeToLightTheme();
                }
                ((Window.Current.Content as Frame).Content as MainPage).OnChangeTheme();
            }
        }
    }
}
