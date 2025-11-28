using System;

namespace DynamicScroll
{
    public class ReactiveProperty<T>
    {
        private T _value;
        
        public Action<T> OnValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public ReactiveProperty(T value)
        {
            _value = value;
        }
    }
}