using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Solid.Arduino.Firmata;
using Stylet;
using WpfToolset.Linq;
using WpfToolset.Windows.Data;
using WpfToolset.Windows.Input;

namespace Solid.Arduino.Monitor.Pages
{
    public class ShellViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private SerialConnection _connection;
        private ArduinoSession _session;
        private IFirmataProtocol _firmata;
        private string _connectionPort;

        public CollectionView<int> BaudRateCollection { get; } = Enum.GetValues(typeof(SerialBaudRate)).Cast<int>().ToArray().ToCollectionView();

        public ViewModelCommandManager Commands { get; } = new ViewModelCommandManager();

        public string ConnectionPort
        {
            get => _connectionPort;
            set
            {
                if (value == _connectionPort) return;
                _connectionPort = value;
                RaisePropertyChanged();
            }
        }

        public ShellViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        [NotifyPropertyChangedInvocator]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }

        #region Commands

        #region ConnectCommand

        public IViewModelCommand ConnectCommand => Commands.Get() ?? Commands.CreateCommand(ExecuteConnectCommand, CanExecuteConnectCommand);

        private void ExecuteConnectCommand()
        {
            try
            {
                _connection = new SerialConnection(ConnectionPort, (SerialBaudRate)BaudRateCollection.CurrentItem);
                _session = new ArduinoSession(_connection, timeOut: 250);
                _firmata = _session;

            }
            catch (Exception e)
            {
                _firmata = null;
                if (_session != null)
                {
                    _session.Dispose();
                    _session = null;
                }
                if (_connection?.IsOpen ?? false)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
                _windowManager.ShowMessageBox(e.Message);
            }
        }

        private bool CanExecuteConnectCommand()
        {
            return !string.IsNullOrWhiteSpace(ConnectionPort) && _firmata == null && !(_connection?.IsOpen ?? false);
        }

        #endregion

        #region DisconnectCommand

        public IViewModelCommand DisconnectCommand =>
            Commands.Get() ?? Commands.CreateCommand(ExecuteDisconnectCommand, CanExecuteDisconnectCommand);

        private void ExecuteDisconnectCommand()
        {

        }

        private bool CanExecuteDisconnectCommand()
        {
            return _connection?.IsOpen ?? false;
        }

        #endregion

        #endregion
    }
}
