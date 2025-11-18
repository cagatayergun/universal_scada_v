// File: Universalscada.core/Services/PlcManagerFactory.cs
using System;
using Universalscada.core.Services;
using Universalscada.Models;
using Universalscada.Module.Textile.Services;
using Universalscada.Models;     // 'Machine' tipini bulmak için
using Universalscada.Services;

namespace Universalscada.Module.Textile.Services
{
    /// <summary>
    /// Factory class that creates the appropriate IPlcManager object based on the given machine type.
    /// </summary>
    public class PlcManagerFactory : IPlcManagerFactory
    {
        public IPlcManager Create(Machine machine)
        {
            switch (machine.MachineType)
            {
                case "BYMakinesi":
                    return new BYMakinesiManager(machine.IpAddress, machine.Port);

                case "Kurutma Makinesi":
                    return new KurutmaMakinesiManager(machine.IpAddress, machine.Port);

                default:
                    // Throw an exception for an unknown machine type to prevent the program from crashing.
                    throw new ArgumentException($"Unknown machine type: '{machine.MachineType}'. Please check machine settings.");
            }
        }
    }
}