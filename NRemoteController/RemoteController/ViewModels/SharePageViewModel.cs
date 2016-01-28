﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Xaml.Navigation;
using RemoteController.Models;
using RemoteController.Services.DialogService;
using RemoteController.Services.SettingsServiceMyImplementation;
using RemoteController.Views;
using Template10.Mvvm;

namespace RemoteController.ViewModels
{
    public class SharePageViewModel : RemoteController.Mvvm.ViewModelBase
    {
        private DataTransferManager dataTransferManager;
        private readonly ISettingsManager _manager;
        private DialogService _dialog;
        private readonly Services.RemoteController.RemoteController _remoteController;
        private ResourceLoader _loader;
        private TvChannels currentTvChannel;


        public SharePageViewModel()
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += SharePage_DataRequested;

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                _manager = new LocalSettingsManager();
                _remoteController = new Services.RemoteController.RemoteController(IpAddress);
                _loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            }
        }

        private string _shareText;
        public string ShareText
        {
            get { return _shareText; }
            set { Set(ref _shareText, value); }
        }

        public string IpAddress => _manager.Load<string>("IpSetting", String.Empty);

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            currentTvChannel = await _remoteController.GetCurrentChanellInfoAsync();

            var messageTop = _loader.GetString("ShareMessageTop");
            var messageBack = _loader.GetString("ShareMessageBack");

            ShareText = messageTop + currentTvChannel.Name + messageBack;
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested -= SharePage_DataRequested;
            return base.OnNavigatedFromAsync(state, suspending);
        }

        void SharePage_DataRequested(Windows.ApplicationModel.DataTransfer.DataTransferManager sender, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs args)
        {
            if (!string.IsNullOrEmpty(ShareText))
            {
                args.Request.Data.SetText(ShareText);
                args.Request.Data.Properties.Title = Windows.ApplicationModel.Package.Current.DisplayName;
            }
            else
            {
                args.Request.FailWithDisplayText("Nothing to share");
            }
        }

        private DelegateCommand _shareCommand;
        public DelegateCommand ShareCommand
        {
            get
            {
                if (_shareCommand == null)
                {
                    _shareCommand = new DelegateCommand(() => Share());
                }
                return _shareCommand;
            }
        }

        private void Share()
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }



    }
}