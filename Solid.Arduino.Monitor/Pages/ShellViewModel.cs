using System;
using System.Linq;
using Solid.Arduino.Firmata;
using Stylet;
using StyletIoC;
using WpfToolset.Linq;
using WpfToolset.Windows.Data;
using WpfToolset.Windows.Input;

namespace Solid.Arduino.Monitor.Pages
{
    public class ShellViewModel : Screen
    {
        private readonly IWindowManager _windowManager;

        public ShellViewModel(IWindowManager windowManager, ConnectionStateViewModel connectionState, MappedPinsViewModel mappedPins)
        {
            _windowManager = windowManager;

            ConnectionState = connectionState;
            ConnectionState.ConductWith(this);
            MappedPins = mappedPins;
            MappedPins.ConductWith(this);
        }

        public ConnectionStateViewModel ConnectionState { get; }

        public MappedPinsViewModel MappedPins { get; }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();

        }

        #region Commands

        #endregion
    }

    public class ConnectionStateViewModel : Screen
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
            set => SetAndNotify(ref _connectionPort, value);
        }

        public bool IsConnected => _connection?.IsOpen ?? false;

        public IFirmataProtocol Firmata => _firmata;

        public ConnectionStateViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            DisplayName = "Connection state";
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
                OnPropertyChanged(nameof(IsConnected));
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
            return !string.IsNullOrWhiteSpace(ConnectionPort) && _firmata == null && !IsConnected;
        }

        #endregion

        #region DisconnectCommand

        public IViewModelCommand DisconnectCommand =>
            Commands.Get() ?? Commands.CreateCommand(ExecuteDisconnectCommand, CanExecuteDisconnectCommand);

        private void ExecuteDisconnectCommand()
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

            OnPropertyChanged(nameof(IsConnected));
        }

        private bool CanExecuteDisconnectCommand()
        {
            return IsConnected;
        }

        #endregion

        #endregion

    }
}
