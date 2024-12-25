
namespace TopNetwork.Services.MessageBuilder
{
    public class MessageBuilderService
    {
        private readonly Dictionary<string, Func<IMessageBuilder<IMsgSourceData>>> _builders = [];

        public void Register<TMsgSourceData>(Func<IMessageBuilder<TMsgSourceData>> builderFactory)
            where TMsgSourceData : IMsgSourceData
        {
            _builders[typeof(TMsgSourceData).Name] = (Func<IMessageBuilder<IMsgSourceData>>)builderFactory;
        }

        public IMessageBuilder<TMsgSourceData> CreateBuilder<TMsgSourceData>()
            where TMsgSourceData : IMsgSourceData
        {
            var type = typeof(TMsgSourceData).Name;
            if (_builders.TryGetValue(type, out Func<IMessageBuilder<IMsgSourceData>>? value))
            {
                return (IMessageBuilder<TMsgSourceData>)value();
            }

            throw new InvalidOperationException($"No builder registered for type {type}");
        }
    }
}
