using System;
using System.Threading.Tasks;

namespace CDN.NET.Wrapper.Utils
{
    public class Maybe<TVal>
    {
        /// <summary>
        /// The value that might be stored in the Maybe
        /// </summary>
        public TVal Value { get; private set; }
        
        /// <summary>
        /// The Exception that might be stored in the Maybe
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Check if the maybe has a value
        /// </summary>
        public bool HasValue { get; private set; } = false;
        
        /// <summary>
        /// Check if the maybe has an error (eg. an Exception)
        /// </summary>
        public bool HasError { get; private set; } = false;

        /// <summary>
        /// Create a Maybe using an init function/lambda that returns a tuple with the value and/or exception
        /// </summary>
        /// <param name="initializer"></param>
        public Maybe(Func<(TVal Value, Exception error)> initializer)
        {
            (TVal val, Exception ex) = initializer();
            this.Value = val;
            this.Error = ex;
            HasValue = val != null;
            HasError = ex != null;
        }

        /// <summary>
        /// Create a maybe with a value if you already have it
        /// </summary>
        /// <param name="value">Object you wish to store</param>
        public Maybe(TVal value)
        {
            Value = value;
            HasValue = true;
        }

        /// <summary>
        /// Create a maybe with an exception
        /// </summary>
        /// <param name="exception">Exception to store</param>
        public Maybe(Exception exception)
        {
            Error = exception;
            HasError = true;
        }

        /// <summary>
        /// Create a maybe with an error message. This will create a new Exception with the specified message
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        public Maybe(string errorMessage)
        {
            Error = new Exception(errorMessage);
            HasError = true;
        }

        /// <summary>
        /// Pass in two actions. The some action will be called if the maybe has a value stored.
        /// The none action will be called if an exception is stored.
        /// </summary>
        /// <param name="some">The action/function to run with the passed value</param>
        /// <param name="none">The action to run with the passed exception</param>
        /// <exception cref="NullReferenceException">Throws if both value and exception are null</exception>
        public void Get(Action<TVal> some, Action<Exception> none)
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

        /// <summary>
        /// Pass in one async functions and one action. The some task will be called if the maybe has a value stored.
        /// The none action will be called if an exception is stored.
        /// </summary>
        /// <param name="some">The function to run with the passed value</param>
        /// <param name="none">The action to run with the passed exception</param>
        /// <exception cref="NullReferenceException">Throws if both value and exception are null</exception>
        public async Task GetAsync(Func<TVal, Task> some, Action<Exception> none)
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
        
        /// <summary>
        /// Pass in two async functions. The some task will be called if the maybe has a value stored.
        /// The none function will be called if an exception is stored.
        /// </summary>
        /// <param name="some">The function to run with the passed value</param>
        /// <param name="none">The function to run with the passed exception</param>
        /// <exception cref="NullReferenceException">Throws if both value and exception are null</exception>
        public async Task GetAsync(Func<TVal, Task> some, Func<Exception, Task> none)
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
        
        /// <summary>
        /// Pass in two functions. The some function will be called if the maybe has a value stored.
        /// The none function will be called if an exception is stored.
        /// Both must return the specified type.
        /// </summary>
        /// <param name="some"></param>
        /// <param name="none"></param>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public TReturn Get<TReturn>(Func<TVal, TReturn> some, Func<Exception, TReturn> none)
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
        public Maybe<TVal> Do(Action<TVal> action)
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

    /// <summary>
    /// Static helper class for the Maybe wrapper
    /// </summary>
    public static class Maybe
    {
        /// <summary>
        /// If your initialization must be async then use this function.
        /// </summary>
        /// <param name="initializer">The task that will initialize the maybe</param>
        /// <typeparam name="T">What type of value the maybe shall hold</typeparam>
        /// <returns>The new maybe</returns>
        /// <exception cref="NullReferenceException">If your initializer failed to deliver either a value or an exception</exception>
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

        /// <summary>
        /// Helper function to easily create a Maybe with a value
        /// </summary>
        /// <param name="val">The value to store</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>Newly created maybe</returns>
        public static Maybe<T> FromVal<T>(T val) 
        {
            return new Maybe<T>(val);
        }
        
        /// <summary>
        /// Helper function to easily create a Maybe that stores an error
        /// </summary>
        /// <param name="e">Exception to store</param>
        /// <typeparam name="T">The type of the Maybe</typeparam>
        /// <returns>Newly created Maybe</returns>
        public static Maybe<T> FromErr<T>(Exception e) 
        {
            return new Maybe<T>(e);
        }
        
        /// <summary>
        /// Helper function to easily create a Maybe that stores an error
        /// </summary>
        /// <param name="error">The error message to be passed to the exception</param>
        /// <typeparam name="T">The type of the Maybe</typeparam>
        /// <returns>Newly created Maybe</returns>
        public static Maybe<T> FromErr<T>(string error) 
        {
            return new Maybe<T>(new Exception(error));
        }

        /// <summary>
        /// Helper function to easily create a Maybe wrapped in a Task with a value.
        /// </summary>
        /// <param name="val">The value to store</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>Newly created Maybe wrapped in a Task</returns>
        public static Task<Maybe<T>> FromValTask<T>(T val) 
        {
            return Task.FromResult(new Maybe<T>(val));
        }
        
        /// <summary>
        /// Helper function to easily create a Maybe wrapped in a Task that stores an error
        /// </summary>
        /// <param name="e">Exception to store</param>
        /// <typeparam name="T">The type of the Maybe</typeparam>
        /// <returns>Newly created Maybe in a Task</returns>
        public static Task<Maybe<T>> FromErrTask<T>(Exception e) 
        {
            return Task.FromResult(new Maybe<T>(e));
        }
        
        /// <summary>
        /// Helper function to easily create a Maybe wrapped in a Task that stores an error
        /// </summary>
        /// <param name="err">The error message to be passed to the exception</param>
        /// <typeparam name="T">The type of the Maybe</typeparam>
        /// <returns>Newly created Maybe in a Task</returns>
        public static Task<Maybe<T>> FromErrTask<T>(string err) 
        {
            return Task.FromResult(new Maybe<T>(new Exception(err)));
        }
        
    }
}