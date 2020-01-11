using System;
using System.Threading.Tasks;

namespace CDN.NET.Wrapper.Utils
{
    public class Maybe<T>
    {
        public T Value { get; private set; }
        public Exception Error { get; private set; }

        public bool HasValue { get; private set; } = false;
        public bool HasException { get; private set; } = false;

        public Maybe(Func<(T Value, Exception error)> initializer)
        {
            (T val, Exception ex) = initializer();
            this.Value = val;
            this.Error = ex;
            HasValue = true;
            HasException = true;
        }

        public Maybe(T value)
        {
            Value = value;
            HasValue = true;
        }

        public Maybe(Exception exception)
        {
            Error = exception;
            HasException = true;
        }

        public Maybe(string errorMessage)
        {
            Error = new Exception(errorMessage);
            HasException = true;
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
        
        public T2 Get<T2>(Func<T, T2> some, Func<Exception, T2> none)
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
    }

    public static class Maybe
    {
        public static Maybe<T> Init<T>(Func<(T Value, Exception error)> initializer) 
        {
            return new Maybe<T>(initializer);
        }
        
        public static async Task<Maybe<T>> InitAsync<T>(Func<Task<(T Value, Exception error)>> initializer) 
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

        public static Maybe<T> FromVal<T>(T val) 
        {
            return new Maybe<T>(val);
        }
        
        public static Maybe<T> FromErr<T>(Exception e) 
        {
            return new Maybe<T>(e);
        }

        public static Task<Maybe<T>> FromValTask<T>(T val) 
        {
            return Task.FromResult(new Maybe<T>(val));
        }
        
        public static Task<Maybe<T>> FromErrTask<T>(Exception e) 
        {
            return Task.FromResult(new Maybe<T>(e));
        }
    }
}