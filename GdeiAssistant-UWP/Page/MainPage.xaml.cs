using GdeiAssistant.Tools;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GdeiAssistant.Page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Init();
        }

        private int index = 0;

        private void Init()
        {
            PersonalItemButton.Background = ThemeUtils.GetCurrentTheme().Equals(ElementTheme.Dark) ? new SolidColorBrush(Colors.DimGray) :new SolidColorBrush(Colors.LightGray);
            Frame.Navigate(typeof(Personal));
        }

        //点击SplitView按钮，展开或收起SplitView
        private void SplitViewButton_Click(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        //SplitView收起时的响应事件
        private void SplitView_PaneClosed(SplitView sender, object args)
        {

        }

        //选择个人中心菜单
        private void PersonalItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (index != 0)
            {
                PersonalItemButton.Background = ThemeUtils.GetCurrentTheme().Equals(ElementTheme.Dark) ? new SolidColorBrush(Colors.DimGray) : new SolidColorBrush(Colors.LightGray);
                ScheduleItemButton.Background = new SolidColorBrush(Colors.Transparent);
                SettingItemButton.Background = new SolidColorBrush(Colors.Transparent);
                Frame.Navigate(typeof(Personal));
                index = 0;
            }
            if (SplitView.IsPaneOpen)
            {
                SplitView.IsPaneOpen = false;
            }
        }

        //选择课表查询菜单
        private void ScheduleItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (index != 1)
            {
                ScheduleItemButton.Background = ThemeUtils.GetCurrentTheme().Equals(ElementTheme.Dark) ? new SolidColorBrush(Colors.DimGray) : new SolidColorBrush(Colors.LightGray);
                PersonalItemButton.Background = new SolidColorBrush(Colors.Transparent);
                SettingItemButton.Background = new SolidColorBrush(Colors.Transparent);
                Frame.Navigate(typeof(Schedule));
                index = 1;
            }
            if (SplitView.IsPaneOpen)
            {
                SplitView.IsPaneOpen = false;
            }
        }

        //切换主题时的回调方法，切换应用设置按钮的背景颜色
        public void OnChangeTheme()
        {
            SettingItemButton.Background = ThemeUtils.GetCurrentTheme().Equals(ElementTheme.Dark) ? new SolidColorBrush(Colors.DimGray) : new SolidColorBrush(Colors.LightGray);
        }

        //跳转到设置界面
        private void SettingItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (index != 3)
            {
                SettingItemButton.Background = ThemeUtils.GetCurrentTheme().Equals(ElementTheme.Dark) ? new SolidColorBrush(Colors.DimGray) : new SolidColorBrush(Colors.LightGray);
                ScheduleItemButton.Background = new SolidColorBrush(Colors.Transparent);
                PersonalItemButton.Background = new SolidColorBrush(Colors.Transparent);
                Frame.Navigate(typeof(Setting));
                index = 3;
            }
            if (SplitView.IsPaneOpen)
            {
                SplitView.IsPaneOpen = false;
            }
        }
    }
}
