namespace GdeiAssistant.Entity
{
    public class UserLoginResult
    {
        public User user { set; get; }

        public Token accessToken { set; get; }

        public Token refreshToken { set; get; }
    }
}
