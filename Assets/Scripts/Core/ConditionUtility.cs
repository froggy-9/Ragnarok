using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Core
{
    public static class ConditionUtility
    {
        public static bool AreFlagsMet(FlagSO[] requiredFlags)
        {
            if (requiredFlags == null || requiredFlags.Length == 0)
            {
                return true;
            }

            foreach (FlagSO flag in requiredFlags)
            {
                if (flag == null)
                {
                    Debug.LogWarning("[ConditionUtility] Null FlagSO is treated as no condition.");
                    continue;
                }

                if (!flag.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
