using DeadLetterOffice.State;

namespace DeadLetterOffice.Interfaces
{
    public interface IConditional
    {
        FlagSO[] RequiredFlags { get; }
        bool IsConditionMet();
    }
}
