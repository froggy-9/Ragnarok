using System;
using System.Collections.Generic;

namespace DeadLetterOffice.State
{
    [Serializable]
    public class SaveData
    {
        public List<FlagSaveEntry> unlockedFlags = new();
        public List<string> collectedLetterIds = new();
        public List<string> collectedItemIds = new();
        public List<string> completedConnectionIds = new();
        public int currentChapter = 1;
        public int currentBeat;
        public string currentScene = "Scene_PostOffice_1F";
        public string saveTime;
        public float playTime;

        public bool TryGetFlagValue(string flagId, out bool value)
        {
            foreach (FlagSaveEntry entry in unlockedFlags)
            {
                if (entry.flagId == flagId)
                {
                    value = entry.value;
                    return true;
                }
            }

            value = false;
            return false;
        }
    }

    [Serializable]
    public class FlagSaveEntry
    {
        public string flagId;
        public bool value;

        public FlagSaveEntry(string flagId, bool value)
        {
            this.flagId = flagId;
            this.value = value;
        }
    }
}
