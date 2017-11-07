using System;
using System.Collections.Generic;
using System.Linq;
using Stylet;

namespace Solid.Arduino.Monitor.Pages
{
    public class ShellViewModel : Screen
    {
        public IList<int> BaudRateCollection { get; } = Enum.GetValues(typeof(SerialBaudRate)).Cast<int>().ToArray();
    }
}
