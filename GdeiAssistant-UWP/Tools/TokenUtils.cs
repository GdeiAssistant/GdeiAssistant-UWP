using GdeiAssistant.Entity;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.Web.Http;

namespace GdeiAssistant.Tools
{
    public class TokenUtils
    {
        //保存AccessToken凭证
        public static void SaveAccessTokenCredential(string username, Token accessToken)
        {
            //序列化AccessToken
            var serializer = new DataContractJsonSerializer(typeof(Token));
            var memoryStream = new MemoryStream();
            serializer.WriteObject(memoryStream, accessToken);
            var token = Encoding.UTF8.GetString(memoryStream.ToArray());
            new PasswordVault().Add(new PasswordCredential(
                "GdeiAssistant-AccessToken", username, token));
        }

        //保存RefreshToken凭证
        public static void SaveRefreshTokenCredential(string username, Token refreshToken)
        {
            //序列化RefreshToken
            var serializer = new DataContractJsonSerializer(typeof(Token));
            var memoryStream = new MemoryStream();
            serializer.WriteObject(memoryStream, refreshToken);
            var token = Encoding.UTF8.GetString(memoryStream.ToArray());
            new PasswordVault().Add(new PasswordCredential(
                "GdeiAssistant-RefreshToken", username, token));
        }

        //获取AccessToken凭证
        public static Token QueryAccessTokenCredential()
        {
            try
            {
                var credentialList = new PasswordVault().FindAllByResource("GdeiAssistant-AccessToken");
                if (credentialList.Count > 0)
                {
                    var credential = credentialList[0];
                    //填充凭据密码
                    credential.RetrievePassword();
                    Token token = new DataContractJsonSerializer(typeof(Token))
                        .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(credential.Password))) as Token;
                    return token;
                }
                return null;
            }
            catch (COMException)
            {
                return null;
            }
        }

        //获取RefreshToken凭证
        public static Token QueryRefreshTokenCredential()
        {
            try
            {
                var credentialList = new PasswordVault().FindAllByResource("GdeiAssistant-RefreshToken");
                if (credentialList.Count > 0)
                {
                    var credential = credentialList[0];
                    //填充凭据密码
                    credential.RetrievePassword();
                    Token token = new DataContractJsonSerializer(typeof(Token))
                        .ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(credential.Password))) as Token;
                    return token;
                }
                return null;
            }
            catch (COMException)
            {
                return null;
            }
        }

        //清空Token凭证信息
        public static void ClearToken()
        {
            //清空缓存的令牌和用户信息
            (Application.Current as App).accessToken = null;
            (Application.Current as App).refreshToken = null;
            (Application.Current as App).username = null;
            //清空凭证保险箱的令牌信息
            var passwordVault = new PasswordVault();
            var credentialList = passwordVault.RetrieveAll();
            //填充凭据密码
            for (int i = 0; i < credentialList.Count; i++)
            {
                credentialList[i].RetrievePassword();
                passwordVault.Remove(new PasswordCredential(credentialList[i].Resource, credentialList[i].UserName, credentialList[i].Password));
            }
        }

        //解析AccessToken，获取用户名
        public static string ParseTokenUsername(string accessToken)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Payload["username"].ToString();
        }

        //判断Token有效性
        public static bool VerifyToken(Token token)
        {
            if (token == null)
            {
                return false;
            }
            //若过期时间戳大于当前时间戳一小时，则认为Token有效
            if ((token.expireTime - ToolUtils.GetCurrentTimestamp()) / (1000 * 60 * 60) < 1)
            {
                return false;
            }
            return true;
        }

        //主动使AccessToken凭证失效
        public static async void ExpireToken(string token)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(5 * 1000);
                HttpClient httpClient = new HttpClient();
                HttpMultipartFormDataContent form = new HttpMultipartFormDataContent
                {
                    { new HttpStringContent(token), "token" }
                };
                await httpClient.PostAsync(new Uri("https://www.gdeiassistant.cn/rest/token/expire"), form)
                    .AsTask(cancellationTokenSource.Token);
            }
            catch (Exception)
            {

            }
        }
    }
}
