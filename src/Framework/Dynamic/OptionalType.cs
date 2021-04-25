
namespace Framework.Dynamic
{
    public struct Optional<T> where T : new()
    {
        private bool _hasValue;
        public T Value;

        public bool HasValue
        {
            get { return _hasValue; }
            set
            {
                _hasValue = value;
                Value = _hasValue ? new T() : default;
            }
        }

        public void Set(T v)
        {
            _hasValue = true;
            Value = v;
        }

        public void Clear()
        {
            _hasValue = false;
            Value = default;
        }

        public static explicit operator T(Optional<T> value)
        {
            return (T)value;
        }
    }
}
