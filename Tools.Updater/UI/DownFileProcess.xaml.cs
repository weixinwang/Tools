using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using AutoUpdater.Base;
using AutoUpdater.Lib;
using AutoUpdater.Loger;

namespace AutoUpdater.UI
{
    public partial class DownFileProcess : WindowBase
    {
        private string updateFileDir;//更新文件存放的文件夹
        private string callExeName;
        private string appDir;
        private UpdateInfo updateInfo;

        public DownFileProcess(string callExeName, string updateFileDir, string appDir, UpdateInfo updateInfo)
        {
            InitializeComponent();
            this.Loaded += (sl, el) =>
            {
                YesButton.Content = "立即更新";
                NoButton.Content = "暂不更新";

                //YesButton.Content = "Yes";
                //NoButton.Content = "No";

                this.YesButton.Click += (sender, e) =>
                {
                    YesButton.IsEnabled = false;
                    NoButton.IsEnabled = false;
                    Process[] processes = Process.GetProcessesByName(this.callExeName);

                    if (processes.Length > 0)
                    {
                        foreach (var p in processes)
                        {
                            p.Kill();
                        }
                    }

                    DownloadUpdateFile();
                };

                this.NoButton.Click += (sender, e) =>
                {
                    this.Close();
                };

                this.TxtProcess.Text = this.updateInfo.AppName + "发现新的版本(" + this.updateInfo.AppVersion + "),是否更新?";
                //this.TxtProcess.Text = this.updateInfo.AppName + " Find a new version(" + this.updateInfo.AppVersion + "),whether or not to update?";
                TxtDes.Text = this.updateInfo.Description;
                Topmost = true;
            };
            this.callExeName = callExeName;
            this.updateFileDir = updateFileDir;
            this.appDir = appDir;
            this.updateInfo = updateInfo;

            if (updateInfo.Description.ToLower().Equals("null"))
            {
                this.updateInfo.Description = "";
            }
            else
            {
                this.updateInfo.Description = "更新内容如下:\r\n" + updateInfo.Description;
                //this.updateInfo.Description = "Update the content:\r\n" + updateInfo.Description;
            }
        }


        public void DownloadUpdateFile()
        {
            string url = "";
            if (!string.IsNullOrWhiteSpace(updateInfo.DownloadUrl))
            {
                url = updateInfo.DownloadUrl;
            }

            //string url = Constants.RemoteUrl + callExeName + "/update.zip";
            url = url + "/" + callExeName + "/" + updateInfo.PackageName;

            LogerManager.Current.AsyncDebug("下载升级包：" + url);

            var client = new System.Net.WebClient();
            client.DownloadProgressChanged += (sender, e) =>
            {
                UpdateProcess(e.BytesReceived, e.TotalBytesToReceive);
            };
            client.DownloadDataCompleted += (sender, e) =>
            {
                //string zipFilePath = System.IO.Path.Combine(updateFileDir, "update.zip");
                string zipFilePath = Path.Combine(updateFileDir, updateInfo.PackageName);
                byte[] data = e.Result;
                var writer = new BinaryWriter(new FileStream(zipFilePath, FileMode.OpenOrCreate));
                writer.Write(data);
                writer.Flush();
                writer.Close();

                System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                {
                    Action f = () =>
                    {
                        TxtProcess.Text = "开始更新程序...";
                        //TxtProcess.Text = "Start updating ...";
                    };
                    this.Dispatcher.Invoke(f);

                    string tempDir = Path.Combine(updateFileDir, "temp");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    UnZipFile(zipFilePath, tempDir);

                    //移动文件
                    //App
                    var newAppDir = Path.Combine(tempDir, "app");
                    if (Directory.Exists(newAppDir))
                    {
                        //启动升级前脚本
                        if (!string.IsNullOrWhiteSpace(updateInfo.FileExecuteBefore))
                        {
                            var scriptPath = Path.Combine(newAppDir, updateInfo.FileExecuteBefore + ".exe");
                            StartApp(scriptPath, updateInfo.ExecuteArgumentBefore);
                        }


                        // 备份 将当前版本的目录文件复制到bak
                        CopyDirectory(appDir, Path.Combine(updateFileDir, "bak"));
                        // 升级 将app文件夹中的内容复制到程序运行目标
                        try
                        {
                            //启动升级后脚本
                            if (!string.IsNullOrWhiteSpace(updateInfo.FileExecuteAfter))
                            {
                                var scriptPath = Path.Combine(newAppDir, updateInfo.FileExecuteAfter + ".exe");
                                StartApp(scriptPath, updateInfo.ExecuteArgumentAfter);
                            }

                            CopyDirectory(newAppDir, appDir);
                        }
                        catch (Exception ex)
                        {
                            LogerManager.Current.AsyncError("更新错误：" + ex.Message);
                            // 失败回滚
                            CopyDirectory(Path.Combine(updateFileDir, "bak"), appDir);
                            return;
                        }
                    }

                    f = () =>
                    {
                        TxtProcess.Text = "更新完成!";
                        //TxtProcess.Text = "Update completed";

                        try
                        {
                            //清空缓存文件夹
                            string rootUpdateDir = updateFileDir.Substring(0,
                                updateFileDir.LastIndexOf(Path.DirectorySeparatorChar));
                            foreach (string p in Directory.EnumerateDirectories(rootUpdateDir))
                            {
                                if (!p.ToLower().Equals(updateFileDir.ToLower()))
                                {
                                    Directory.Delete(p, true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        LogerManager.Current.AsyncError("更新后清空缓存文件错误：" + ex.Message);
                            //MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            YesButton.IsEnabled = true;
                            NoButton.IsEnabled = true;
                        }
                    };
                    this.Dispatcher.Invoke(f);

                    try
                    {
                        f = () =>
                        {
                            AlertWin alert = new AlertWin("更新完成,是否启动软件?") { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
                            alert.Title = "更新完成";
                            //AlertWin alert = new AlertWin("The application has been updated to start it?") { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };

                            //alert.Title = "Update completed";
                            alert.Loaded += (ss, ee) =>
                            {
                                alert.YesButton.Width = 50;
                                alert.NoButton.Width = 50;
                            };
                            alert.Width = 300;
                            alert.Height = 200;
                            alert.ShowDialog();
                            if (alert.YesBtnSelected)
                            {
                                //启动软件
                                string exePath = Path.Combine(appDir, callExeName + ".exe");
                                StartApp(exePath);
                            }
                            this.Close();
                        };
                        this.Dispatcher.Invoke(f);
                    }
                    catch (Exception ex)
                    {
                        LogerManager.Current.AsyncError("下载更新错误：" + ex.Message);
                        //MessageBox.Show(ex.Message);
                    }
                });

            };
            client.DownloadDataAsync(new Uri(url));
        }

        private void StartApp(string exePath, string args = "")
        {
            if (!File.Exists(exePath))
            {
                return;
            }

            var info = new ProcessStartInfo(exePath)
            {
                Arguments = args,
                UseShellExecute = true,
                WorkingDirectory = exePath.Substring(0, exePath.LastIndexOf(Path.DirectorySeparatorChar))
            };

            System.Diagnostics.Process.Start(info);
        }

        private static void UnZipFile(string zipFilePath, string targetDir)
        {
            ICCEmbedded.SharpZipLib.Zip.FastZipEvents evt = new ICCEmbedded.SharpZipLib.Zip.FastZipEvents();
            ICCEmbedded.SharpZipLib.Zip.FastZip fz = new ICCEmbedded.SharpZipLib.Zip.FastZip(evt);
            fz.ExtractZip(zipFilePath, targetDir, "");
        }

        public void UpdateProcess(long current, long total)
        {
            string status = (int)((float)current * 100 / (float)total) + "%";
            this.TxtProcess.Text = status;
            RectProcess.Width = ((float)current / (float)total) * BProcess.ActualWidth;
        }

        public void CopyDirectory(string sourceDirName, string destDirName)
        {
            try
            {
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                    File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
                }
                if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                    destDirName = destDirName + Path.DirectorySeparatorChar;
                string[] files = Directory.GetFiles(sourceDirName);
                foreach (string file in files)
                {
                    File.Copy(file, destDirName + Path.GetFileName(file), true);
                    File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
                }

                string rootUpdateDir = updateFileDir.Substring(0, updateFileDir.LastIndexOf(Path.DirectorySeparatorChar));
                string[] dirs = Directory.GetDirectories(sourceDirName);
                foreach (string dir in dirs)
                {
                    if (!dir.ToLower().Equals(rootUpdateDir.ToLower()))
                    {
                        CopyDirectory(dir, destDirName + Path.GetFileName(dir));
                    }
                }
            }
            catch (Exception ex)
            {
                LogerManager.Current.AsyncError("复制文件错误：" + ex.Message);
                throw new Exception("复制文件错误");
                //throw new Exception("An error occurred while copying the file.");
            }
        }
    }
}
