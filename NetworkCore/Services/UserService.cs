
using System.Text.Json;

namespace TopNetwork.Services
{
    // Интерфейс для репозитория пользователей
    public interface IUserRepository
    {
        void AddUser(string login, string password);
        void RemoveUser(string login);
        Dictionary<string, string> GetAllUsers();
    }

    // Реализация репозитория пользователей
    public class UserRepository : IUserRepository
    {
        private readonly string _filePath;
        private readonly object _locker = new();

        public UserRepository(string filePath)
        {
            _filePath = filePath;
        }

        public void AddUser(string login, string password)
        {
            lock (_locker)
            {
                var users = GetAllUsers();
                users[login] = password;
                SaveToFile(users);
            }
        }

        public void RemoveUser(string login)
        {
            lock (_locker)
            {
                var users = GetAllUsers();
                if (users.Remove(login))
                    SaveToFile(users);
            }
        }

        public Dictionary<string, string> GetAllUsers()
        {
            lock (_locker)
            {
                if (!File.Exists(_filePath))
                    return [];

                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            }
        }

        private void SaveToFile(Dictionary<string, string> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    // Сервис для работы с пользователями
    public class UserService(IUserRepository repository)
    {
        private readonly IUserRepository _repository = repository;

        public void AddUser(string login, string password) => _repository.AddUser(login, password);
        public void RemoveUser(string login) => _repository.RemoveUser(login);
        public Dictionary<string, string> GetAllUsers() => _repository.GetAllUsers();
    }
}
