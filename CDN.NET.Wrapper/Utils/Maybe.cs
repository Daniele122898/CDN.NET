using System;

namespace CDN.NET.Wrapper.Utils
{
    public class Maybe<T> where T: class
    {
        public T Value { get; private set; }
        public Exception Error { get; private set; }
        
        public bool HasValue => Value != null;
        public bool HasException => Error != null;
        
        public static Maybe<T> Init<T>(Func<(T Value, Exception error)> initializer) where T: class
        {
            return new Maybe<T>(initializer);
        }

        public Maybe(Func<(T Value, Exception error)> initializer)
        {
            (T val, Exception ex) = initializer();
            this.Value = val;
            this.Error = ex;
        }

        public Maybe(T value)
        {
            Value = value;
        }

        public Maybe(Exception exception)
        {
            Error = exception;
        }

        public Maybe(string errorMessage)
        {
            Error = new Exception(errorMessage);
        }

        public void Get(Action<T> some, Action<Exception> none)
        {
            if (this.HasValue)
            {
                some(this.Value);
            }
            else if (this.HasException)
            {
                none(this.Error);
            }
            else
            {
                throw new NullReferenceException($"Both {nameof(Value)}, {nameof(Error)} are null");
            }
        }
    }
}