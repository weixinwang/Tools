using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater.Loger
{
    public class LogerManager
    {
        private static ILoger _loger;

        private static object _lockflag = new object();

        public static ILoger Current
        {
            get {
                //线程同步
                lock (_lockflag)
                {
                    if (_loger == null)
                    {
                        _loger = new FileLoger();
                    }
                    return _loger;
                }
            }
            set
            {
                lock (_lockflag)
                {
                    _loger = value;
                }
            }
        }
    }
}
