using System.Threading.Tasks;

namespace SuperSafeBank.Worker.Notifications
{
    public interface INotificationsService
    {
        Task DispatchAsync(Notification notification);
    }
}