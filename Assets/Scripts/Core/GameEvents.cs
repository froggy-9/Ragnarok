using DeadLetterOffice.Board;
using DeadLetterOffice.Chapter;
using DeadLetterOffice.Dialogue;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using DeadLetterOffice.UI;
using UnityEngine;

namespace DeadLetterOffice.Core
{
    public enum GameMode
    {
        Exploration,
        Dialogue,
        UI,
        Cinematic
    }

    public readonly struct FlagChangedEvent
    {
        public readonly FlagSO Flag;
        public readonly bool Value;

        public FlagChangedEvent(FlagSO flag, bool value)
        {
            Flag = flag;
            Value = value;
        }
    }

    public readonly struct LetterFoundEvent
    {
        public readonly LetterSO Letter;

        public LetterFoundEvent(LetterSO letter)
        {
            Letter = letter;
        }
    }

    public readonly struct ItemCollectedEvent
    {
        public readonly CollectibleSO Item;
        public readonly int Amount;

        public ItemCollectedEvent(CollectibleSO item, int amount = 1)
        {
            Item = item;
            Amount = amount;
        }
    }

    public readonly struct ConnectionMadeEvent
    {
        public readonly BoardConnectionSO Connection;

        public ConnectionMadeEvent(BoardConnectionSO connection)
        {
            Connection = connection;
        }
    }

    public readonly struct BeatAdvancedEvent
    {
        public readonly BeatSO Beat;

        public BeatAdvancedEvent(BeatSO beat)
        {
            Beat = beat;
        }
    }

    public readonly struct AudioPlayEvent
    {
        public readonly AudioCueSO Cue;

        public AudioPlayEvent(AudioCueSO cue)
        {
            Cue = cue;
        }
    }

    public readonly struct ArchiveRefreshedEvent
    {
    }

    public readonly struct InteractionPromptChangedEvent
    {
        public readonly bool Visible;
        public readonly string Text;

        public InteractionPromptChangedEvent(bool visible, string text)
        {
            Visible = visible;
            Text = text;
        }
    }

    public readonly struct NarrationRequestedEvent
    {
        public readonly string Text;

        public NarrationRequestedEvent(string text)
        {
            Text = text;
        }
    }

    public readonly struct DialogueRequestedEvent
    {
        public readonly DialogueSO Dialogue;
        public readonly Transform FocusTarget;

        public DialogueRequestedEvent(DialogueSO dialogue, Transform focusTarget = null)
        {
            Dialogue = dialogue;
            FocusTarget = focusTarget;
        }
    }

    public readonly struct BoardCardUnlockedEvent
    {
        public readonly BoardCardSO Card;

        public BoardCardUnlockedEvent(BoardCardSO card)
        {
            Card = card;
        }
    }

    public readonly struct GameModeChangedEvent
    {
        public readonly GameMode Mode;

        public GameModeChangedEvent(GameMode mode)
        {
            Mode = mode;
        }
    }

    public readonly struct CameraShotRequestedEvent
    {
        public readonly Transform ShotTransform;
        public readonly float Duration;
        public readonly bool LockPlayer;

        public CameraShotRequestedEvent(Transform shotTransform, float duration, bool lockPlayer)
        {
            ShotTransform = shotTransform;
            Duration = duration;
            LockPlayer = lockPlayer;
        }
    }

    public readonly struct QuestObjectiveChangedEvent
    {
        public readonly string QuestTitle;
        public readonly string ObjectiveText;
        public readonly string LocationText;
        public readonly Transform Target;
        public readonly int StaticDistanceMeters;
        public readonly string ProgressText;

        public QuestObjectiveChangedEvent(string objectiveText, Transform target = null, int staticDistanceMeters = -1, string progressText = "", string questTitle = "", string locationText = "")
        {
            QuestTitle = questTitle;
            ObjectiveText = objectiveText;
            LocationText = locationText;
            Target = target;
            StaticDistanceMeters = staticDistanceMeters;
            ProgressText = progressText;
        }
    }

    public readonly struct QuestUnlockedEvent
    {
        public readonly StoryQuestSO Quest;

        public QuestUnlockedEvent(StoryQuestSO quest)
        {
            Quest = quest;
        }
    }

    public readonly struct QuestLogChangedEvent
    {
    }

    public readonly struct QuestAcceptedEvent
    {
        public readonly StoryQuestSO Quest;

        public QuestAcceptedEvent(StoryQuestSO quest)
        {
            Quest = quest;
        }
    }
}
