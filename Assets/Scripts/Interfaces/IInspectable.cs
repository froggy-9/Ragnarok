using DeadLetterOffice.State;

namespace DeadLetterOffice.Interfaces
{
    public interface IInspectable
    {
        string GetNarration();
        FlagSO RequiredFlag { get; }
    }
}
