using Universalscada.Models;
using Universalscada.Services;
namespace Universalscada.core.Services
{
    public interface IPlcManagerFactory
    {
        IPlcManager Create(Machine machine);
    }
}