using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater.Loger
{
    public static class Loger
    {
        public static void Info(this object instance,string log)
        {
            LogerManager.Current.Info(log);
        }

        public static void Debug(this object instance, string log)
        {
            LogerManager.Current.Debug(log);
        }

        public static void Error(this object instance, string log)
        {
            LogerManager.Current.Error(log);
        }

        public static void Warning(this object instance, string log)
        {
            LogerManager.Current.Warning(log);
        }

        /// <summary>
        /// 异步log
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="log"></param>
        public static void AsyncInfo(this object instance, string log)
        {
            new Task((object state) =>
            {
                LogerManager.Current.Info(string.Format("{0}", state));

            }, log).Start();
        }
        public static void AsyncWarning(this object instance, string log)
        {
            new Task((object state) =>
            {
                LogerManager.Current.Warning(string.Format("{0}", state));

            }, log).Start();
        }
        public static void AsyncDebug(this object instance, string log)
        {
            new Task((object state) =>
            {
                LogerManager.Current.Debug(string.Format("{0}", state));

            }, log).Start();
        }

        public static void AsyncError(this object instance, string log)
        {
            new Task((object state) =>
            {
                LogerManager.Current.Error(string.Format("{0}", state));

            },log).Start();
        }
    }
}
