using System;
using WixSharp.UI;

namespace Monitoring.Setup.Wpf
{
    public class MyProductSetup : GenericSetup
    {
        public bool InitialCanInstall { get; set; }

        public bool InitialCanUnInstall { get; set; }

        public bool InitialCanRepair { get; set; }

        private bool _setupBegan;

        public bool SetupBegan
        {
            get => _setupBegan;
            set
            {
                _setupBegan = value;
                OnPropertyChanged(nameof (SetupBegan));
                OnPropertyChanged(nameof (SetupNotYetBegan));
            }
        }
        public bool SetupNotYetBegan => !SetupBegan;
        
        public void StartRepair()
        {
            //The MSI will abort any attempt to start unless CUSTOM_UI is set. This  a feature for preventing starting the MSI without this custom GUI.
            base.StartRepair("CUSTOM_UI=true");
            SetupBegan = true;
        }

        public void StartChange()
        {
            base.StartRepair("CUSTOM_UI=true");
        }

        public void StartInstall()
        {
            base.StartInstall("CUSTOM_UI=true");
        }

        public MyProductSetup(string msiFile, bool enableLoging = true)
            : base(msiFile, enableLoging)
        {
            InitialCanInstall = CanInstall;
            InitialCanUnInstall = CanUnInstall;
            InitialCanRepair = CanRepair;

            SetupStarted += MyProductSetup_SetupStarted;

            //Uncomment if you want to see current action name changes. Otherwise it is too quick.
            ProgressStepDelay = 10;
        }

        private void MyProductSetup_SetupStarted()
        {
            SetupBegan = true;
        }
    }
}