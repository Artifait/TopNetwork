
using TopNetwork.Core;

namespace TopNetwork.MessageBuilderService
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
        private bool? _isAuthenticated = null;

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

        public AuthenticationMessageBuilder SetAuthentication(bool authentication)
        {
            _isAuthenticated = authentication;
            return this;
        }

        public Message BuildRequest()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(_login);
            ArgumentException.ThrowIfNullOrWhiteSpace(_password);

            return new()
            {
                Payload = $"{_login}:{_password}"
            };
        }
        public Message BuildResponse()
        {
            ArgumentNullException.ThrowIfNull(_isAuthenticated);

            return new()
            {
                Headers = { { , } }
            };
        }

        public AuthenticationRequest ParseRequest(Message msg)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponse ParseResponse(Message msg)
        {
            throw new NotImplementedException();
        }
    }
}
