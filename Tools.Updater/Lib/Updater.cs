using AutoUpdater.Loger;
using System;
using System.IO;
using System.Xml.Linq;

namespace AutoUpdater.Lib
{
    public class Updater
    {
        private static Updater _instance;
        public static Updater Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Updater();
                }
                return _instance;
            }
        }

        public static void CheckUpdateStatus(string remoteUrl)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                string url = "";
                if (!string.IsNullOrWhiteSpace(remoteUrl))
                {
                    url = remoteUrl;
                }
                url = url + "/" + Updater.Instance.CallExeName + "/update.xml";

                LogerManager.Current.AsyncDebug("下载：" + url);
                var client = new System.Net.WebClient();
                client.DownloadDataCompleted += (x, y) =>
                {
                    try
                    {
                        MemoryStream stream = new MemoryStream(y.Result);

                        XDocument xDoc = XDocument.Load(stream);
                        UpdateInfo updateInfo = new UpdateInfo();
                        XElement root = xDoc.Element("UpdateInfo");
                        updateInfo.AppName = root.Element("AppName").Value;
                        updateInfo.PackageName = root.Element("PackageName").Value;
                        updateInfo.AppVersion = root.Element("AppVersion") == null || string.IsNullOrEmpty(root.Element("AppVersion").Value) ?
                                                "" : root.Element("AppVersion").Value;
                        updateInfo.RequiredMinVersion = root.Element("RequiredMinVersion") == null || string.IsNullOrEmpty(root.Element("RequiredMinVersion").Value) ?
                                                "" : root.Element("RequiredMinVersion").Value;
                        updateInfo.DownloadUrl = root.Element("DownloadUrl").Value;
                        updateInfo.MD5 = root.Element("MD5") == null || string.IsNullOrEmpty(root.Element("MD5").Value) ?
                            Guid.NewGuid() : new Guid(root.Element("MD5").Value);
                        updateInfo.FileExecuteBefore = root.Element("FileExecuteBefore") == null ? "" : root.Element("FileExecuteBefore").Value;
                        updateInfo.ExecuteArgumentBefore = root.Element("ExecuteArgumentBefore") == null ? "" : root.Element("ExecuteArgumentBefore").Value;
                        updateInfo.FileExecuteAfter = root.Element("FileExecuteAfter") == null ? "" : root.Element("FileExecuteAfter").Value;
                        updateInfo.ExecuteArgumentAfter = root.Element("ExecuteArgumentAfter") == null ? "" : root.Element("ExecuteArgumentAfter").Value;
                        updateInfo.Description = root.Element("Description") == null ? "" : root.Element("Description").Value;
                        if (updateInfo.MD5 == Guid.Empty)
                        {
                            updateInfo.MD5 = Guid.NewGuid();
                        }

                        stream.Close();

                        Updater.Instance.StartUpdate(updateInfo);
                    }
                    catch (Exception ex)
                    {
                        LogerManager.Current.AsyncError("检查版本更新异常：" + ex.Message);
                    }
                };
                client.DownloadDataAsync(new Uri(url));
            });

        }

        public void StartUpdate(UpdateInfo updateInfo)
        {
            if (!string.IsNullOrWhiteSpace(updateInfo.RequiredMinVersion))
            {
                var minVersion = updateInfo.RequiredMinVersion.Trim('V', 'v', ' ');
                if (Updater.Instance.CurrentVersion < new Version(minVersion))//当前版本比需要的版本小，不更新
                {
                    return;
                }
            }

            var appVersion = updateInfo.AppVersion;
            if (!string.IsNullOrWhiteSpace(appVersion))
            {
                appVersion = appVersion.Trim('V', 'v', ' ');
            }

            if (Updater.Instance.CurrentVersion >= new Version(appVersion))
            {
                //当前版本是最新的，不更新
                return;
            }

            //更新程序复制到缓存文件夹
            string appDir = Path.Combine(System.Reflection.Assembly.GetEntryAssembly().Location.Substring(0, System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(Path.DirectorySeparatorChar)));
            //string updateFileDir = Path.Combine(Path.Combine(appDir.Substring(0, appDir.LastIndexOf(Path.DirectorySeparatorChar))), "Update");
            string updateFileDir = Path.Combine(appDir, "Update");
            if (!Directory.Exists(updateFileDir))
            {
                Directory.CreateDirectory(updateFileDir);
            }
            updateFileDir = Path.Combine(updateFileDir, updateInfo.MD5.ToString());
            if (!Directory.Exists(updateFileDir))
            {
                Directory.CreateDirectory(updateFileDir);
            }

            string exePath = Path.Combine(updateFileDir, "AutoUpdater.exe");
            File.Copy(Path.Combine(appDir, "AutoUpdater.exe"), exePath, true);

            var info = new System.Diagnostics.ProcessStartInfo(exePath);
            info.UseShellExecute = true;
            info.WorkingDirectory = exePath.Substring(0, exePath.LastIndexOf(Path.DirectorySeparatorChar));
            if (string.IsNullOrWhiteSpace(updateInfo.Description))
            {
                updateInfo.Description = "Non description content";
            }

            info.Arguments = "update " +
                             Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CallExeName)) + " " +
                             Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateFileDir)) + " " +
                             Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(appDir)) + " " +
                             Convert.ToBase64String(ByteConvertHelper.Object2Bytes(updateInfo));

            System.Diagnostics.Process.Start(info);
        }

        public bool UpdateFinished = false;

        private string _callExeName;
        public string CallExeName
        {
            get
            {
                if (string.IsNullOrEmpty(_callExeName))
                {
                    _callExeName = System.Reflection.Assembly.GetEntryAssembly().Location.Substring(System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1).Replace(".exe", "");
                }
                return _callExeName;
            }
        }

        /// <summary>
        /// 获得当前应用软件的版本
        /// </summary>
        public virtual Version CurrentVersion
        {
            get { return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductVersion); }
        }

        /// <summary>
        /// 获得当前应用程序的根目录
        /// </summary>
        public virtual string CurrentApplicationDirectory
        {
            get { return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); }
        }
    }
}
