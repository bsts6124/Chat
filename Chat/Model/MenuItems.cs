using Command;
using System;

namespace Client
{
    class MenuItems
    {
        public string Header { get; private set; }
        public DelegateCommand Command { get; private set; }

        public MenuItems(string header, Action<Object> execute, Predicate<Object> canExecute = null)
        {
            this.Header = header;
            this.Command = new DelegateCommand(execute, canExecute);
        }
    }
}
