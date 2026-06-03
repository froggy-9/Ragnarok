namespace DeadLetterOffice.Interfaces
{
    public interface ICollectible
    {
        string ItemId { get; }
        void OnCollect();
    }
}
