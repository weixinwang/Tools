using AutoUpdater.Base;

namespace AutoUpdater.UI
{
    /// <summary>
    /// Interaction logic for DownFileProcess.xaml
    /// </summary>
    public partial class AlertWin : WindowBase
    {
        public bool YesBtnSelected = false;
        public AlertWin(string msg)
        {
            InitializeComponent();

            this.Loaded += (sl, el) =>
            {
                YesButton.Content = "是";
                NoButton.Content = "否";
                //YesButton.Content = "Yes";
                //NoButton.Content = "No";
                this.TxtMsg.Text = msg;
                this.YesButton.Click += (sender, e) =>
                {
                    YesBtnSelected = true;
                    this.Close();
                };

                this.NoButton.Click += (sender, e) =>
                {
                    YesBtnSelected = false;
                    this.Close();
                };
            };


        }
    }
}
