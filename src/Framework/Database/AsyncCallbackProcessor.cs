using System.Collections.Generic;

namespace Framework.Database
{
    public interface ISqlCallback
    {
        bool InvokeIfReady();
    }

    public class AsyncCallbackProcessor<T> where T : ISqlCallback
    {   
        List<T> _callbacks = new List<T>();

        public T AddCallback(T query)
        {
            _callbacks.Add(query);
            return query;
        }

        public void ProcessReadyCallbacks()
        {
            if (_callbacks.Empty())
                return;

            _callbacks.RemoveAll(callback => callback.InvokeIfReady());
        }
    }
}
