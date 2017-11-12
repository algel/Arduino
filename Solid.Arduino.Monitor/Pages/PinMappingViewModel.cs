using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Solid.Arduino.Firmata;
using Stylet;
using WpfToolset.Linq;
using WpfToolset.Windows.Data;
using WpfToolset.Windows.Input;

namespace Solid.Arduino.Monitor.Pages
{
    public class PinMappingViewModel : Screen
    {
        protected ViewModelCommandManager Commands { get; } = new ViewModelCommandManager();

        public PinMappingViewModel(PinSetting pinSetting)
        {

        }
    }

    //public class DigitalPinMappingViewModel : PinMappingViewModel
    //{

    //}

    //public class AnalogPinMappingViewModel : PinMappingViewModel
    //{

    //}

    public class MappedPinsViewModel : Conductor<PinMappingViewModel>.Collection.AllActive
    {
        private readonly IWindowManager _windowManager;
        private readonly ConnectionStateViewModel _connectionStateViewModel;

        protected ViewModelCommandManager Commands { get; } = new ViewModelCommandManager();

        public MappedPinsViewModel(IWindowManager windowManager, ConnectionStateViewModel connectionStateViewModel)
        {
            _windowManager = windowManager;
            _connectionStateViewModel = connectionStateViewModel;
        }

        private void ObserveTaskException(Task task, Exception exception)
        {
            if (exception is TaskCanceledException)
                return;

            _windowManager.ShowMessageBox(exception.Message, "Error", icon: MessageBoxImage.Error);
        }
        #region Commands

        #region AddDigitalPortMappingCommand

        public IViewModelCommand AddDigitalPortMappingCommand =>
            Commands.Get() ?? Commands.CreateCommand(ExecuteAddDigitalPortMappingCommand, CanExecuteAddDigitalPortMappingCommand, ObserveTaskException);

        private async Task ExecuteAddDigitalPortMappingCommand()
        {
            var cap = await _connectionStateViewModel.Firmata.GetBoardCapabilityAsync();
            var pins = cap.PinCapabilities.Where(e => e.DigitalInput || e.DigitalOutput || e.Pwm || e.Analog).Distinct();
            var settingsVm = new PinMappingSettingsViewModel(pins);
            _windowManager.ShowDialog(settingsVm);
            var setting = await settingsVm.CompletionTask;

            ActivateItem(new PinMappingViewModel(setting));
        }

        private bool CanExecuteAddDigitalPortMappingCommand()
        {
            return _connectionStateViewModel.IsConnected;
        }

        #endregion

        #endregion
    }

    public class PinMappingSettingsViewModel : Screen
    {
        private static readonly PinModeViewModel[] PinModeSource =
        {
            new PinModeViewModel("Input", PinMode.DigitalInput, true),
            new PinModeViewModel("Output", PinMode.DigitalOutput, true),
            new PinModeViewModel("PWM", PinMode.PwmOutput, true),
            new PinModeViewModel("Undefined", PinMode.Undefined, false),
            new PinModeViewModel("Input", PinMode.AnalogInput, false),
        };

        private TaskCompletionSource<PinSetting> _tcs;

        public PinMappingSettingsViewModel(IEnumerable<PinCapability> pinCaps)
        {
            PinCollection = pinCaps.OrderBy(e => e.PinNumber).ToCollectionView();
            PinModeCollection = GetPinModes().ToCollectionView();
            _tcs = new TaskCompletionSource<PinSetting>();
        }

        private IEnumerable<PinMode> GetPinModes()
        {
            if (PinCollection.IsCurrentBeforeFirst || PinCollection.IsCurrentAfterLast)
                yield break;

            var currentPin = PinCollection.CurrentItem;
            if (currentPin.Analog) yield return PinMode.AnalogInput;
            if (currentPin.DigitalInput) yield return PinMode.DigitalInput;
            if (currentPin.DigitalOutput) yield return PinMode.DigitalOutput;
            if (currentPin.Pwm) yield return PinMode.PwmOutput;

        }

        public Task<PinSetting> CompletionTask => _tcs.Task;

        private ViewModelCommandManager Commands { get; } = new ViewModelCommandManager();

        public CollectionView<PinMode> PinModeCollection { get; }

        public CollectionView<PinCapability> PinCollection { get; }

        public class PinModeViewModel : LabelledValue<PinMode>
        {
            public bool IsDigital { get; }

            public PinModeViewModel(string label, PinMode value, bool isDigital) : base(label, value)
            {
                IsDigital = isDigital;
            }
        }



        #region Commands

        #region AcceptCommand

        public IViewModelCommand AcceptCommand =>
            Commands.Get() ?? Commands.CreateCommand(ExecuteAcceptCommand, CanExecuteAcceptCommand);

        private void ExecuteAcceptCommand()
        {
            _tcs.SetResult(new PinSetting { Number = PinCollection.CurrentItem.PinNumber, Mode = PinModeCollection.CurrentItem });

            RequestClose(true);
        }

        private bool CanExecuteAcceptCommand()
        {
            return !PinCollection.IsCurrentBeforeFirst && !PinModeCollection.IsCurrentBeforeFirst;
        }

        #endregion

        #region CancelCommand

        public IViewModelCommand CancelCommand =>
            Commands.Get() ?? Commands.CreateCommand(ExecuteCancelCommand);

        private void ExecuteCancelCommand()
        {
            _tcs.SetCanceled();
            RequestClose(false);
        }

        #endregion

        #endregion
    }

    public struct PinSetting
    {
        public int Number { get; set; }
        public PinMode Mode { get; set; }
    }
}
