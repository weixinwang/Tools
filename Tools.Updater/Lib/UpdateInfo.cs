using System;

namespace AutoUpdater.Lib
{
    /// <summary>
    /// 升级信息的具体包装
    /// </summary>
    [Serializable]
    public class UpdateInfo
    {
        /// <summary>
        /// 应用程序名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 升级包名称
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// 应用程序版本
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// 升级需要的最低版本
        /// </summary>
        public string RequiredMinVersion { get; set; }

        /// <summary>
        /// 下载路径
        /// </summary>
        public string DownloadUrl { get; set; }

        public Guid MD5 { get; set; }

        ///<summary>
        /// 前脚本名称
        ///</summary>
        public string FileExecuteBefore { get; set; }

        ///<summary>
        /// 前脚本参数
        ///</summary>
        public string ExecuteArgumentBefore { get; set; }

        ///<summary>
        /// 后脚本名称
        ///</summary>
        public string FileExecuteAfter { get; set; }

        ///<summary>
        /// 后脚本参数
        ///</summary>
        public string ExecuteArgumentAfter { get; set; }

        private string description;

        /// <summary>
        /// 更新描述
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    description = string.Join(Environment.NewLine,
                        value.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }

        public override string ToString()
        {
            return "AppName: " + AppName + " PackageName: " + PackageName +
                    " AppVersion: " + AppVersion + " RequiredMinVersion: " + RequiredMinVersion +
                    " DownloadUrl: " + DownloadUrl + " MD5: " + MD5 +
                    " FileExecuteBefore: " + FileExecuteBefore + " ExecuteArgumentBefore: " + ExecuteArgumentBefore +
                    " FileExecuteAfter: " + FileExecuteAfter + " ExecuteArgumentAfter: " + ExecuteArgumentAfter;
        }
    }
}