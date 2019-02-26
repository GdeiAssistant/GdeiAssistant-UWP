using GdeiAssistant.Entity;
using GdeiAssistant.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace GdeiAssistant.Page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Schedule : Windows.UI.Xaml.Controls.Page
    {
        private int? week { set; get; }

        private List<UIElement> elementList = new List<UIElement>();

        private List<Entity.Schedule> scheduleList = new List<Entity.Schedule>();

        public Schedule()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            if (!string.IsNullOrEmpty((Application.Current as App).accessToken))
            {
                ShowSchedulePage();
                LoadScheduleData(null);
            }
            else
            {
                DialogUtils.ShowMessageDialog("错误提示", "你未登录，请前往个人中心进行登录");
            }
        }

        //显示课程表页面
        private void ShowSchedulePage()
        {
            Grid.Visibility = Visibility.Visible;
        }

        //显示进度条页面
        private void ShowProgressbarPage()
        {
            LoadingPanel.Visibility = Visibility.Visible;
        }

        //隐藏进度条页面
        private void HideProgressbarPage()
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        //显示课程表详细信息
        private async void ShowScheduleDetailInfo(object sender, TappedRoutedEventArgs e)
        {
            Entity.Schedule schedule = (sender as Border).Tag as Entity.Schedule;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("课程类型：" + schedule.scheduleType + "\n");
            stringBuilder.Append("上课节数：" + schedule.scheduleLesson  + "\n");
            stringBuilder.Append("上课周数：" + "第"+ schedule.minScheduleWeek +"周至第" + schedule.maxScheduleWeek + "周\n");
            stringBuilder.Append("任课教师：" + schedule.scheduleTeacher + "\n");
            stringBuilder.Append("上课地点：" + schedule.scheduleLocation + "\n");
            var messageDialog = new MessageDialog(stringBuilder.ToString()) { Title = schedule.scheduleName };
            messageDialog.Commands.Add(new UICommand("确定"));
            await messageDialog.ShowAsync();
        }

        //添加课程信息
        private void AddSchedule(Entity.Schedule schedule)
        {
            Border border = new Border();
            border.Background = ColorUtils.GetSolidColorBrush(schedule.colorCode);
            border.Tag = schedule;
            border.Tapped += new TappedEventHandler(ShowScheduleDetailInfo);
            TextBlock textBlock = new TextBlock();
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontSize = 13;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.Text = schedule.scheduleName + "@" + schedule.scheduleLocation;
            border.Child = textBlock;
            Grid.Children.Add(border);
            Grid.SetRow(border, schedule.row.Value + 2);
            Grid.SetColumn(border, schedule.column.Value + 1);
            Grid.SetRowSpan(border, schedule.scheduleLength.Value);
            elementList.Add(border);
            scheduleList.Add(schedule);
        }

        //清空课程信息
        private void ClearSchedule()
        {
            foreach (UIElement uIElement in elementList)
            {
                Grid.Children.Remove(uIElement);
            }
            scheduleList.Clear();
        }

        //加载课程表信息
        private async void LoadScheduleData(int? week)
        {
            var accessToken = (Application.Current as App).accessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                DialogUtils.ShowMessageDialog("错误提示", "你未登录，请前往个人中心进行登录");
                return;
            }
            try
            {
                MoreButton.IsEnabled = false;
                RefreshButton.IsEnabled = false;
                ClearSchedule();
                ShowProgressbarPage();
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(15 * 1000);
                HttpClient httpClient = new HttpClient();
                HttpMultipartFormDataContent form = week.HasValue ?
                     new HttpMultipartFormDataContent
                {
                    { new HttpStringContent(accessToken), "token" },
                    {new HttpStringContent(week.Value.ToString()),"week" }
                } :
                    new HttpMultipartFormDataContent
                {
                    { new HttpStringContent(accessToken), "token" }
                };
                //接收请求响应结果
                var response = await httpClient.PostAsync(new Uri("https://www.gdeiassistant.cn/rest/schedulequery"), form)
                    .AsTask(cancellationTokenSource.Token);
                if (response.IsSuccessStatusCode)
                {
                    //反序列化并解析JSON结果信息
                    var data = await response.Content.ReadAsStringAsync();
                    var result = new DataContractJsonSerializer(typeof(DataJsonResult<ScheduleQueryResult>))
                        .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(data))) as DataJsonResult<ScheduleQueryResult>;
                    if (result.success)
                    {
                        //设置当前查询的周数
                        ChangeCurrentWeek(result.data.week.Value);
                        foreach (Entity.Schedule schedule in result.data.scheduleList)
                        {
                            AddSchedule(schedule);
                        }
                    }
                    else
                    {
                        //提示错误信息
                        DialogUtils.ShowMessageDialog("错误提示", result.message);
                    }
                }
                else
                {
                    //服务暂不可用
                    DialogUtils.ShowMessageDialog("错误提示", "服务暂不可用，请稍后再试");
                }
            }
            catch (COMException)
            {
                //网络连接异常
                DialogUtils.ShowMessageDialog("错误提示", "网络连接异常，请检查网络连接");
            }
            catch (TaskCanceledException)
            {
                //网络连接超时
                DialogUtils.ShowMessageDialog("错误提示", "网络连接超时，请重新尝试");
            }
            catch (Exception ex)
            {
                //出现异常
                DialogUtils.ShowMessageDialog("出现错误，请联系管理员", "错误信息为：" + ex.Message);
            }
            finally
            {
                MoreButton.IsEnabled = true;
                RefreshButton.IsEnabled = true;
                HideProgressbarPage();
            }
        }

        //更改当前选中的周数
        private void ChangeCurrentWeek(int week)
        {
            this.week = week;
            IList<MenuFlyoutItemBase> weekMenuItemList = WeekMenu.Items;
            foreach (MenuFlyoutItemBase item in weekMenuItemList)
            {
                item.Background = new SolidColorBrush(Colors.Transparent);
            }
            weekMenuItemList[week - 1].Background = ThemeUtils.GetCurrentTheme().Equals(ElementTheme.Dark) ? new SolidColorBrush(Colors.DimGray) : new SolidColorBrush(Colors.LightGray);
        }

        //刷新课程表信息
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadScheduleData(week);
        }

        //选择需要查询的周数
        private void WeekMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyoutItem = sender as MenuFlyoutItem;
            LoadScheduleData(int.Parse(menuFlyoutItem.Tag.ToString()));
        }
    }
}
