using GdeiAssistant.Entity;
using GdeiAssistant.Page;
using GdeiAssistant.Tools;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace GdeiAssistant
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {

        public string accessToken { set; get; }

        public string refreshToken { set; get; }

        public string username { set; get; }

        public Frame rootFrame { set; get; }

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            //从凭据保险箱加载令牌信息
            var accessToken = TokenUtils.QueryAccessTokenCredential();
            var refreshToken = TokenUtils.QueryRefreshTokenCredential();

            //校验令牌信息有效性
            if (!TokenUtils.VerifyToken(accessToken))
            {
                if (TokenUtils.VerifyToken(refreshToken))
                {
                    //刷新令牌信息
                    RefreshToken(refreshToken.signature);
                }
            }
            else
            {
                this.accessToken = accessToken.signature;
                this.refreshToken = refreshToken.signature;
                this.username = TokenUtils.ParseTokenUsername(accessToken.signature);
            }

            //设置应用最小尺寸
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 320, Height = 480 });            

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }
            this.rootFrame = rootFrame;

            //加载应用本地配置

            //加载应用主题
            ThemeUtils.LoadThemeSetting();

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        //刷新令牌信息
        private void RefreshToken(string token)
        {
            try
            {
                //加载用户头像
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(5 * 1000);
                HttpMultipartFormDataContent form = new HttpMultipartFormDataContent
                {
                    { new HttpStringContent(token), "token" }
                };
                HttpClient httpClient = new HttpClient();
                var response = httpClient.PostAsync(new Uri("https://www.gdeiassistant.cn/rest/token/refresh"), form)
                    .AsTask(cancellationTokenSource.Token).Result;
                if (response.IsSuccessStatusCode)
                {
                    //反序列化并解析JSON结果信息
                    var data = response.Content.ReadAsStringAsync().GetResults();
                    var result = new DataContractJsonSerializer(typeof(DataJsonResult<RefreshTokenResult>))
                        .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(data))) as DataJsonResult<RefreshTokenResult>;
                    if (result.success)
                    {
                        var username = TokenUtils.ParseTokenUsername(accessToken);
                        //保存新的令牌信息到凭据保险箱中
                        TokenUtils.SaveAccessTokenCredential(username, result.data.accessToken);
                        TokenUtils.SaveRefreshTokenCredential(username, result.data.refreshToken);
                        //缓存令牌和用户信息到应用中
                        this.accessToken = result.data.accessToken.signature;
                        this.refreshToken = result.data.refreshToken.signature;
                        this.username = username;
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
