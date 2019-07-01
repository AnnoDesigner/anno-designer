using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Core.Models
{
    [Serializable]
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute = null;
        readonly Func<T, bool> _canExecute = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        ///<summary>
        ///Defines the method that determines whether the command can execute in its current state.
        ///</summary>
        ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        ///<returns>
        ///true if this command can be executed; otherwise, false.
        ///</returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute((T)parameter);
        }

        ///<summary>
        ///Occurs when changes occur that affect whether or not the command should execute.
        ///</summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        ///<summary>
        ///Defines the method to be called when the command is invoked.
        ///</summary>
        ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion
    }

    /// <summary>
    /// A simple relay Command for easy use of the Command pattern.
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public class RelayCommand : RelayCommand<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        /// <remarks></remarks>
        public RelayCommand(Action<object> execute)
            : base(execute)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <remarks></remarks>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute) : base(execute, canExecute)
        { }
    }

    /// <summary>
    /// An async version of <see cref="ICommand"/>.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Executes the asynchronous command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        Task ExecuteAsync(object parameter);
    }

    public class AsyncDelegateCommand : AsyncDelegateCommand<object>
    {
        public AsyncDelegateCommand(Func<object, Task> asyncExecute, Predicate<object> canExecute = null)
            : base(asyncExecute, canExecute)
        {
        }
    }

    public class AsyncDelegateCommand<TArgType> : IAsyncCommand
    {
        protected readonly Predicate<TArgType> canExecute;
        protected Func<TArgType, Task> asyncExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncDelegateCommand(Func<TArgType, Task> asyncExecute, Predicate<TArgType> canExecute = null)
        {
            this.asyncExecute = asyncExecute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute((TArgType)parameter);
        }

        public async void Execute(object parameter)
        {
            await AsyncRunner(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            await AsyncRunner(parameter);
        }

        protected virtual async Task AsyncRunner(object parameter)
        {
            await asyncExecute((TArgType)parameter);
        }
    }
}
