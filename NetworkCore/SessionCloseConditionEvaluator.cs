
namespace TopNetwork.Core
{
    public interface ISessionCloseCondition
    {
        bool ShouldClose(ClientSession session);
    }

    public interface IAsyncSessionCloseCondition
    {
        Task<bool> ShouldCloseAsync(ClientSession session);
    }

    public class SessionCloseConditionEvaluator
    {
        private readonly List<ISessionCloseCondition> _conditions = [];
        private readonly List<IAsyncSessionCloseCondition> _asyncConditions = [];

        public void AddCondition(ISessionCloseCondition condition)
        {
            ArgumentNullException.ThrowIfNull(condition);
            _conditions.Add(condition);
        }

        public void AddAsyncCondition(IAsyncSessionCloseCondition asyncCondition)
        {
            ArgumentNullException.ThrowIfNull(asyncCondition);
            _asyncConditions.Add(asyncCondition);
        }

        public bool AnyShouldClose(ClientSession session)
            => _conditions.Count == 0 ? true : 
                _conditions.Any(condition => condition.ShouldClose(session));
        public async Task<bool> AnyShouldCloseAsync(ClientSession session)
        {
            if(_asyncConditions.Count == 0) 
                return true;

            foreach (var asyncCondition in _asyncConditions)
            {
                if (await asyncCondition.ShouldCloseAsync(session))
                    return true;
            }
            return false;
        }

        public bool AllShouldClose(ClientSession session)
            => _conditions.Count == 0 ? true :
                _conditions.All(condition => condition.ShouldClose(session));

        public async Task<bool> AllShouldCloseAsync(ClientSession session)
        {
            if (_asyncConditions.Count == 0)
                return true;

            foreach (var asyncCondition in _asyncConditions)
            {
                if (!await asyncCondition.ShouldCloseAsync(session))
                    return false;
            }
            return true;
        }

        public virtual bool ShouldClose(ClientSession session)
            => AnyShouldClose(session) && AnyShouldCloseAsync(session).Result;

        public async virtual Task<bool> ShouldCloseAsync(ClientSession session)
            => AnyShouldClose(session) && await AnyShouldCloseAsync(session);
    }
}
