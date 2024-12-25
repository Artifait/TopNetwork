
namespace TopNetwork.MessageBuilderFactory
{
    public class AuthenticationRequest : IRequest
    {
        public string Login { get; }
        public string Password { get; }

        public string MessageType => nameof(AuthenticationRequest);

        public AuthenticationRequest(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }

    public class AuthenticationResponse : IResponse
    {
        public bool IsAuthenticated { get; }
        public string MessageType => nameof(AuthenticationResponse);

        public AuthenticationResponse(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
        }
    }

    public class AuthenticationMessageBuilder : IMessageBuilder<AuthenticationRequest, AuthenticationResponse>
    {
        private string _login;
        private string _password;

        public AuthenticationMessageBuilder SetLogin(string login)
        {
            _login = login;
            return this;
        }

        public AuthenticationMessageBuilder SetPassword(string password)
        {
            _password = password;
            return this;
        }

        public AuthenticationRequest BuildRequest()
        {
            return new AuthenticationRequest(_login, _password);
        }

        public AuthenticationResponse? ParseResponse(string rawResponse)
        {
            // Пример парсинга строки ответа
            if (rawResponse == "OK")
                return new AuthenticationResponse(true);
            else if (rawResponse == "FAIL")
                return new AuthenticationResponse(false);

            return null; // Некорректный ответ
        }
    }
}
