﻿using System;
using System.Diagnostics;
using System.Windows.Input;

namespace SubtitlesRunner
{
    // taken from http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action<object> _execute; private readonly Predicate<object> _canExecute;

        #endregion Fields

        #region Constructors

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute"); _execute = execute; _canExecute = canExecute;
        }

        #endregion Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter) { return _canExecute == null || _canExecute(parameter); }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion ICommand Members
    }
}