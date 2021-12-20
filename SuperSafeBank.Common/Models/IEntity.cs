namespace SuperSafeBank.Common.Models
{
    public interface IEntity<out TKey>
    {
        TKey Id { get; }
    }
}