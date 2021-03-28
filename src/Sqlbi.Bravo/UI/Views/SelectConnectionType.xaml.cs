﻿using Dax.Vpax.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Sqlbi.Bravo.Core.Logging;
using Sqlbi.Bravo.Core.Services.Interfaces;
using Sqlbi.Bravo.UI.DataModel;
using Sqlbi.Bravo.UI.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace Sqlbi.Bravo.UI.Views
{
    public partial class SelectConnectionType : UserControl
    {
        private readonly ILogger _logger;

        public SelectConnectionType()
        {
            InitializeComponent();
            
            _logger = App.ServiceProvider.GetRequiredService<ILogger<SelectConnectionType>>();
        }

        private void RequestNavigateHyperlink(object sender, RequestNavigateEventArgs e)
        {
            _logger.Information(LogEvents.NavigateHyperlink, "{@Details}", new object[] { new
            {
                Uri = e.Uri.AbsoluteUri
            }});

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void HowToUseClicked(object sender, RoutedEventArgs e) => ShellView.Instance.ShowMediaDialog(new HowToUseBravoHelp());

        private void AttachToWindowClicked(object sender, RoutedEventArgs e)
        {
            _logger.Trace();

            var instances = App.ServiceProvider.GetRequiredService<IPowerBIDesktopService>().GetInstances();

            foreach (var instance in instances)
            {
                _ = MessageBox.Show($"{ instance.Name } @ { instance.LocalEndPoint }", "TODO", MessageBoxButton.OK);                
            }

            // TODO REQUIREMENTS: need to know how to connect here
            _ = MessageBox.Show(
                "Need to attach to an active window",
                "TODO",
                MessageBoxButton.OK,
                MessageBoxImage.Question);

            _logger.Information(LogEvents.StartConnectionAction, "{@Details}", new object[] { new
            {
                Action = "AttachPowerBIDesktop"
            }});
        }

        private async void ConnectToDatasetClicked(object sender, RoutedEventArgs e)
        {
            _logger.Trace();

            #region Test

            var service = App.ServiceProvider.GetRequiredService<IPowerBICloudService>();
 
            var succeed = await service.LoginAsync();
            if (succeed == false)
            {
                _ = MessageBox.Show("Login failed, no response message received within expected timeframe.", "TODO", MessageBoxButton.OK);
                return;
            }

            _ = MessageBox.Show($"{ service.Account.Username } - { service.Account.Environment } @ TenantId { service.Account.HomeAccountId.TenantId } ", "TODO", MessageBoxButton.OK);
            
            var datasets = await service.GetSharedDatasetsAsync();

            foreach (var dataset in datasets)
            {
                _ = MessageBox.Show($"{ dataset.WorkspaceName }({ dataset.WorkspaceType }) - { dataset.Model.DisplayName } ", "TODO", MessageBoxButton.OK);
            }

            await service.LogoutAsync();

            #endregion

            // TODO REQUIREMENTS: need to know how to connect here
            _ = MessageBox.Show(
                "Need to sign-in and connect to a dataset",
                "TODO",
                MessageBoxButton.OK,
                MessageBoxImage.Question);

            _logger.Information(LogEvents.StartConnectionAction, "{@Details}", new object[] { new
            {
                Action = "ConnectPowerBIDataset"
            }});
        }

        private void OpenVertipaqFileClicked(object sender, RoutedEventArgs e)
        {
            _logger.Trace();

            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "Vertipaq files (*.vpax)|*.vpax",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _logger.Information(LogEvents.StartConnectionAction, "{@Details}", new object[] { new
                {
                    Action = "OpenVertipaqFile"
                }});

                var fileContent = VpaxTools.ImportVpax(openFileDialog.FileName);

                var vm = DataContext as TabItemViewModel;
                vm.ConnectionType = BiConnectionType.VertipaqAnalyzerFile;
                vm.ConnectionName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                vm.AnalyzeModelVm.OnPropertyChanged(nameof(AnalyzeModelViewModel.ConnectionName));
                vm.ShowAnalysisOfLoadedModel(fileContent.DaxModel);
            }
        }
    }
}
