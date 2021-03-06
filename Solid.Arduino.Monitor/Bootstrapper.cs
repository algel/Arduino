using System;
using Stylet;
using StyletIoC;
using Solid.Arduino.Monitor.Pages;

namespace Solid.Arduino.Monitor
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here

            builder.Bind<ConnectionStateViewModel>().ToSelf().InSingletonScope();
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}
