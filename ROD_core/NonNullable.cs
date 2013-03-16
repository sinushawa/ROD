using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core
{
    public struct NonNullable<T> where T : struct
    {
        private readonly T value;

        public NonNullable(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get
            {
                return value;
            }
        }

        public static implicit operator NonNullable<T>(T value)
        {
            return new NonNullable<T>(value);
        }

        public static implicit operator T(NonNullable<T> wrapper)
        {
            return wrapper.Value;
        }
    }
}
