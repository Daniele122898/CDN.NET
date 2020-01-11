using System;
using System.Threading.Tasks;

namespace CDN.NET.Wrapper.Utils
{
    public class Maybe<T>
    {
        public T Value { get; private set; }
        public Exception Error { get; private set; }

        public bool HasValue { get; private set; } = false;
        public bool HasError { get; private set; } = false;

        public Maybe(Func<(T Value, Exception error)> initializer)
        {
            (T val, Exception ex) = initializer();
            this.Value = val;
            this.Error = ex;
            HasValue = true;
            HasError = true;
        }

        public Maybe(T value)
        {
            Value = value;
            HasValue = true;
        }

        public Maybe(Exception exception)
        {
            Error = exception;
            HasError = true;
        }

        public Maybe(string errorMessage)
        {
            Error = new Exception(errorMessage);
            HasError = true;
        }

        public void Get(Action<T> some, Action<Exception> none)
        {
            if (this.HasValue)
            {
                some(this.Value);
            }
            else if (this.HasError)
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
            else if (this.HasError)
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
            else if (this.HasError)
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

            if (this.HasError)
            {
                return none(this.Error);
            }

            throw new NullReferenceException($"Both {nameof(Value)}, {nameof(Error)} are null");
        }

        /// <summary>
        /// If this maybe has a value, do the action, otherwise do nothing and return this
        /// </summary>
        /// <param name="action">Action to perform</param>
        /// <returns>Returns itself</returns>
        public Maybe<T> Do(Action<T> action)
        {
            if (this.HasValue)
            {
                action(this.Value);
            }

            return this;
        }

        /// <summary>
        /// Transforms current maybe to success maybe. Thus if there is a value it will return a new maybe with a true boolean.
        /// Otherwise it will return a new maybe with the error
        /// </summary>
        /// <returns>Maybe indicating success</returns>
        public Maybe<bool> ToSuccessMaybe()
        {
            return this.Get((val) => Maybe.FromVal(true), Maybe.FromErr<bool>);
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
        
        public static Maybe<T> FromErr<T>(string error) 
        {
            return new Maybe<T>(new Exception(error));
        }

        public static Task<Maybe<T>> FromValTask<T>(T val) 
        {
            return Task.FromResult(new Maybe<T>(val));
        }
        
        public static Task<Maybe<T>> FromErrTask<T>(Exception e) 
        {
            return Task.FromResult(new Maybe<T>(e));
        }
        
        public static Task<Maybe<T>> FromErrTask<T>(string err) 
        {
            return Task.FromResult(new Maybe<T>(new Exception(err)));
        }
        
    }
}