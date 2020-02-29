namespace SuperSafeBank.Core.Models
{
    public interface IEntity<out TKey>
    {
        TKey Id { get; }
    }
}