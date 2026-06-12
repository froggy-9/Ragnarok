using DeadLetterOffice.State;
using DeadLetterOffice.Letter;
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

        public static bool AreItemsMet(GameStateSO gameState, CollectibleSO[] requiredItems)
        {
            if (requiredItems == null || requiredItems.Length == 0)
            {
                return true;
            }

            if (gameState == null)
            {
                Debug.LogWarning("[ConditionUtility] GameStateSO is required for item conditions.");
                return false;
            }

            foreach (CollectibleSO item in requiredItems)
            {
                if (item == null)
                {
                    Debug.LogWarning("[ConditionUtility] Null CollectibleSO is treated as no condition.");
                    continue;
                }

                if (gameState.CountItem(item) <= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
