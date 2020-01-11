using System;
using System.Threading.Tasks;

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
        
        public static async Task<Maybe<T>> InitAsync(Func<Task<(T Value, Exception error)>> initializer)
        {
            (T val, Exception ex) = await initializer().ConfigureAwait(false);
            if (val != null)
            {
                return new Maybe<T>(val);
            }

            if (ex != null)
            {
                return new Maybe<T>(ex);
            }
            throw new NullReferenceException("Your initializer failed to deliver a value or exception. At least one of both must be initialized");
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

        public async Task GetAsync(Func<T, Task> some, Action<Exception> none)
        {
            if (this.HasValue)
            {
                await some(this.Value).ConfigureAwait(false);
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
        
        public async Task GetAsync(Func<T, Task> some, Func<Exception, Task> none)
        {
            if (this.HasValue)
            {
                await some(this.Value).ConfigureAwait(false);
            }
            else if (this.HasException)
            {
                await none(this.Error).ConfigureAwait(false);
            }
            else
            {
                throw new NullReferenceException($"Both {nameof(Value)}, {nameof(Error)} are null");
            }
        }
        
        public T2 Get<T2>(Func<T, T2> some, Func<Exception, T2> none) where T2: class
        {
            if (this.HasValue)
            {
                return some(this.Value);
            }

            if (this.HasException)
            {
                return none(this.Error);
            }

            throw new NullReferenceException($"Both {nameof(Value)}, {nameof(Error)} are null");
        }
        
        public async Task<T2> GetAsync<T2>(Func<T, Task<T2>> some, Func<Exception, Task<T2>> none) where T2: class
        {
            if (this.HasValue)
            {
                return (await some(this.Value).ConfigureAwait(false));
            }

            if (this.HasException)
            {
                return (await none(this.Error).ConfigureAwait(false));
            }

            throw new NullReferenceException($"Both {nameof(Value)}, {nameof(Error)} are null");
        }
        
        public async Task<T2> GetAsync<T2>(Func<T, Task<T2>> some, Func<Exception, T2> none) where T2: class
        {
            if (this.HasValue)
            {
                return (await some(this.Value).ConfigureAwait(false));
            }

            if (this.HasException)
            {
                return none(this.Error);
            }

            throw new NullReferenceException($"Both {nameof(Value)}, {nameof(Error)} are null");
        }
    }
}