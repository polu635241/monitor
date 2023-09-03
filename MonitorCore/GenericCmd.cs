using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonitorCore
{
    public class GenericCmd : ICommand
    {
        public GenericCmd (Action callback, Func<bool> clickable = null)
        {
            if (callback != null)
            {
                this.callback = (par) => callback.Invoke ();
            }
            else
            {
                this.callback = (par) =>
                {

                };
            }

            if (clickable != null)
            {
                this.clickable = (par) => clickable.Invoke ();
            }
            else
            {
                //沒有限制就是一直都可以按
                this.clickable = (parameter) => true;
            }
        }

        public GenericCmd (Action<object> callback, Func<object, bool> clickable = null)
        {
            if (callback != null)
            {
                this.callback = callback;
            }
            else
            {
                this.callback = (par) =>
                {

                };
            }

            if (clickable != null)
            {
                this.clickable = clickable;
            }
            else
            {
                //沒有限制就是一直都可以按
                this.clickable = (parameter) => true;
            }
        }

        Action<object> callback;
        Func<object, bool> clickable;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute (object parameter)
        {
            return clickable.Invoke (parameter);
        }

        public void Execute (object parameter)
        {
            callback.Invoke (parameter);
        }
    }
}
