﻿using System;
using System.Windows.Input;

namespace MarketPriceLogger.ViewModel.Commands
{
	public class SimpleCommand : ICommand
	{
		private Action _targetExecuteAction;
		private Func<bool> _targetCanExecuteMethod;

		public SimpleCommand(Action executeAction)
		{
			_targetExecuteAction = executeAction;
		}

		public SimpleCommand(Action executeAction, Func<bool> canExecuteMethod)
		{
			_targetExecuteAction = executeAction;
			_targetCanExecuteMethod = canExecuteMethod;
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			if (_targetCanExecuteMethod != null)
			{
				return _targetCanExecuteMethod();
			}

			return _targetExecuteAction != null;
		}

		public void Execute(object parameter)
		{
			_targetExecuteAction?.Invoke();
		}
	}
}
