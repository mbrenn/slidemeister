using System;
using System.Windows.Input;

namespace SlideMeister.Helper
{
    public class ActionCommand : ICommand
    {
        private readonly Action _action;

        /// <summary>
        /// Initializes a new instance of the ActionCommand class
        /// </summary>
        /// <param name="action">The action that will be executed upon request commands</param>
        public ActionCommand(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Called, when the user clicks on the button
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _action();
        }
    }
}