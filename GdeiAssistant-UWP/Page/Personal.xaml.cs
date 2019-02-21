using GdeiAssistant.Entity;
using GdeiAssistant.Tools;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace GdeiAssistant.Page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Personal : Windows.UI.Xaml.Controls.Page
    {
        public Personal()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            string accessToken = (Application.Current as App).accessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                //用户未登录，切换至登录界面
                ShowLoginPage();
            }
            else
            {
                //用户已登录，切换至个人主页界面
                ShowPersonalPage();
            }
        }

        //显示登录界面
        private void ShowLoginPage()
        {
            PersonalPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }

        //显示个人主页界面
        private void ShowPersonalPage()
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            PersonalPanel.Visibility = Visibility.Visible;
            //加载个人资料和头像
            LoadAvatar();
            LoadUserInfo();
        }

        //点击登录按钮
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            UserLogin();
        }

        //点击退出登录按钮
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            DialogUtils.ShowMessageDialog("退出登录", "你确定要退出当前账号吗？", cmd =>
            {
                var accessToken = (Application.Current as App).accessToken;
                var refreshToken = (Application.Current as App).refreshToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    TokenUtils.ExpireToken(accessToken);
                }
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    TokenUtils.ExpireToken(refreshToken);
                }
                //清空凭据保险箱的令牌信息
                TokenUtils.ClearToken();
                //切换到登录界面
                ShowLoginPage();
            }, null);
        }

        //用户登录
        private async void UserLogin()
        {
            if (string.IsNullOrEmpty(Username.Text) || string.IsNullOrEmpty(Password.Password))
            {
                DialogUtils.ShowMessageDialog("请填写教务系统账号信息", "用户名和密码不能为空");
            }
            else
            {
                try
                {
                    //显示进度条和禁用控件
                    ProgressBar.Visibility = Visibility.Visible;
                    Username.IsEnabled = false;
                    Password.IsEnabled = false;
                    LoginButton.IsEnabled = false;
                    //获取设备ID
                    var deviceId = ToolUtils.GetDeviceID();
                    //生成随机值、时间戳和签名
                    var nonce = ToolUtils.GenerateRandomData();
                    var timestamp = ToolUtils.GetCurrentTimestamp().ToString();
                    var signature = ToolUtils.SHA1HashStringForUTF8String(timestamp + nonce + "GdeiAssistantRequest");
                    //提交登录请求
                    await SendUserLoginRequest(Username.Text, Password.Password, deviceId, nonce, timestamp, signature);
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
                    //隐藏进度条和启用控件
                    ProgressBar.Visibility = Visibility.Collapsed;
                    Username.IsEnabled = true;
                    Password.IsEnabled = true;
                    LoginButton.IsEnabled = true;
                }
            }
        }

        //发送用户登录请求
        private async Task SendUserLoginRequest(string username, string password, string deviceId, string nonce, string timestamp, string signature)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(15 * 1000);
            HttpClient httpClient = new HttpClient();
            HttpMultipartFormDataContent form = new HttpMultipartFormDataContent
                {
                    { new HttpStringContent(deviceId), "unionid" },
                    { new HttpStringContent(Username.Text), "username" },
                    { new HttpStringContent(Password.Password), "password" },
                    { new HttpStringContent(nonce), "nonce" },
                    { new HttpStringContent(timestamp), "timestamp" },
                    { new HttpStringContent(signature), "signature" }
                };
            //接收请求响应结果
            var response = await httpClient.PostAsync(new Uri("https://www.gdeiassistant.cn/rest/userlogin"), form)
                .AsTask(cancellationTokenSource.Token);
            if (response.IsSuccessStatusCode)
            {
                //反序列化并解析JSON结果信息
                var data = await response.Content.ReadAsStringAsync();
                var result = new DataContractJsonSerializer(typeof(DataJsonResult<UserLoginResult>))
                    .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(data))) as DataJsonResult<UserLoginResult>;
                if (result.success)
                {
                    //用户登录成功，保存令牌和用户信息到APP缓存中
                    (Application.Current as App).accessToken = result.data.accessToken.signature;
                    (Application.Current as App).refreshToken = result.data.refreshToken.signature;
                    (Application.Current as App).username = result.data.user.username;
                    //保存令牌信息到凭据保险箱中
                    TokenUtils.SaveAccessTokenCredential(username, result.data.accessToken);
                    TokenUtils.SaveRefreshTokenCredential(username, result.data.refreshToken);
                    //切换到个人主页界面
                    ShowPersonalPage();
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

        //加载用户个人资料
        private async void LoadUserInfo()
        {
            var accessToken = (Application.Current as App).accessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    //加载用户头像
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(5 * 1000);
                    HttpMultipartFormDataContent form = new HttpMultipartFormDataContent
                {
                    { new HttpStringContent(accessToken), "token" },
                };
                    HttpClient httpClient = new HttpClient();
                    var response = await httpClient.PostAsync(new Uri("https://www.gdeiassistant.cn/rest/profile"), form).AsTask(cancellationTokenSource.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        //反序列化并解析JSON结果信息
                        var data = await response.Content.ReadAsStringAsync();
                        var result = new DataContractJsonSerializer(typeof(DataJsonResult<Profile>))
                            .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(data))) as DataJsonResult<Profile>;
                        if (result.success)
                        {
                            Kickname.Text = result.data.kickname;
                            Kickname.Visibility = Visibility.Visible;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        //加载用户头像
        private async void LoadAvatar()
        {
            var username = (Application.Current as App).username;
            if (!string.IsNullOrEmpty(username))
            {
                try
                {
                    //加载用户头像
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(5 * 1000);
                    HttpClient httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(new Uri("https://www.gdeiassistant.cn/rest/avatar/" + username)).AsTask(cancellationTokenSource.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        //反序列化并解析JSON结果信息
                        var data = await response.Content.ReadAsStringAsync();
                        var result = new DataContractJsonSerializer(typeof(DataJsonResult<string>))
                            .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(data))) as DataJsonResult<string>;
                        if (result.success && !string.IsNullOrEmpty(result.data))
                        {
                            ImageBrush imageBrush = new ImageBrush();
                            imageBrush.ImageSource = new BitmapImage(new Uri(result.data));
                            Avatar.Fill = imageBrush;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        //用户名输入框按下回车时焦点转移到密码输入框
        private void Username_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Password.Focus(FocusState.Programmatic);
            }
        }

        //密码输入框按下回车时进行登录
        private void Password_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                UserLogin();
            }
        }
    }
}
