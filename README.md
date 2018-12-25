# Tools
一、升级的过程很，大致如下：
1、检测服务器上的版本号—>比较本地程序的版本号和服务器上的版本号—>如果不相同则下载升级的压缩包—>
    下载完成后解压升级包—>解压后的文件覆盖到应用程序文件目录—>升级完成
2、有两点需要注意：
因为升级的过程就是用新文件覆盖旧文件的过程，所以要防止老文件被占用后无法覆盖的情况，因而升级之前应该关闭运用程序。
升级程序本身也可能需要升级，而升级程序启动后如问题1所说，就不可能被覆盖了，因而应该想办法避免这种情况。

二、检测的详细步骤，如下：
1、异步下载update.xml到本地
2、分析xml文件信息，存储到自定义的类UpdateInfo中
3、判断升级需要的最低版本号，如果满足，启动升级程序。这里就碰到了上面提到的问题，文件被占用的问题。
因为如果直接启动AutoUpdater.exe，升级包中的AutoUpdater.exe是无法覆盖这个文件的，所以采取的办法是将AutoUpdater.exe拷贝到缓存temp文件夹中，然后启动缓存文件夹temp中的AutoUpdater.exe文件来完成升级的过程。

三、升级步骤应该如下：
1、关闭应用程序进程
2、下载升级包到缓存文件夹
3、解压升级包到缓存文件夹
4、从缓存文件夹复制解压后的文件和文件夹到应用程序目录
5、提醒用户升级成功

四、使用说明：
服务器上升级包的目录层次应该如下(假如要升级的运用程序为Test.exe):
　　Test(与exe的名字相同)
　　----update.xml
　　----update.zip   (也可以自定义名称，需要确保自定义名称和update.xml中的  <Package>update.zip</Package> 保持一致)
　　update.zip包用如下方式生成：
　　新建一个目录APP,将所用升级的文件拷贝到APP目录下，然后压缩APP文件夹为update.zip文件


注意：1、升级服务器的路径配置写到Constants.cs类中。
	  2、使用方法如下，在要升级的运用程序项目的Main函数中，加上一行语句:AutoUpdater.Updater.CheckUpdateStatus();

到此，一款简单的自动升级组件就完成了!

开发流程：
1、引用AutoUpdater.exe
2、启动时或登录之前添加如下代码
            var updateUrl = System.Configuration.ConfigurationManager.AppSettings["UpdateUrl"];
            AutoUpdater.Lib.Updater.CheckUpdateStatus(updateUrl);
3、在app.config 添加
 <appSettings>
    <!--升级文件所在路径-->
    <add key="UpdateUrl" value="http://192.168.250.191:8090/Uploads" />
  </appSettings>

安装包制作：
服务器上升级包的目录层次应该如下(假如要升级的运用程序为Test.exe):
　　Test(与exe的名字相同)
　　----update.xml
　　----update.zip   (也可以自定义名称，需要确保自定义名称和update.xml中的  <Package>update.zip</Package> 保持一致)
　　update.zip包用如下方式生成：
　　新建一个目录APP,将所用升级的文件拷贝到APP目录下，然后压缩APP文件夹为update.zip文件
发布流程：
