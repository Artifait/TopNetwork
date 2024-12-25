
using System.Collections.Concurrent;

namespace TopNetwork
{
    /// <summary>
    /// Регистр сервисов, чтоб не плодить статики
    /// </summary>
    public class ServiceRegistry
    {
        private readonly ConcurrentDictionary<Type, object> _services = new();

        public void Register<TService>(TService service) where TService : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _services[typeof(TService)] = service;
        }

        public TService? Get<TService>() where TService : class
        {
            _services.TryGetValue(typeof(TService), out var service);
            return service as TService;
        }
    }
}
