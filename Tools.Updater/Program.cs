using System;
using System.Windows;
using AutoUpdater.Lib;
using AutoUpdater.Loger;
using AutoUpdater.UI;

namespace AutoUpdater
{
    class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            if (args[0] == "update")
            {
                try
                {
                    string callExeName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[1]));
                    string updateFileDir = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[2]));
                    string appDir = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[3]));
                    var updateInfo = ByteConvertHelper.Bytes2Object(Convert.FromBase64String(args[4])) as UpdateInfo;

                    App app = new App();
                    DownFileProcess downUi = new DownFileProcess(callExeName, updateFileDir, appDir, updateInfo) { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                    app.Run(downUi);
                }
                catch (Exception ex)
                {
                    LogerManager.Current.AsyncError("开启升级程序异常：" + ex.Message);
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
