using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Chapter
{
    public class BeatManager : MonoBehaviour
    {
        [SerializeField] private ChapterSO _chapter;
        [SerializeField] private int _currentBeatIndex;

        public BeatSO CurrentBeat { get; private set; }

        private void OnEnable()
        {
            GameEventBus.Subscribe<FlagChangedEvent>(OnFlagChanged);
            GameEventBus.Subscribe<ConnectionMadeEvent>(OnConnectionMade);
            EvaluateBeats();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<FlagChangedEvent>(OnFlagChanged);
            GameEventBus.Unsubscribe<ConnectionMadeEvent>(OnConnectionMade);
        }

        public void EvaluateBeats()
        {
            if (_chapter == null || _chapter.Beats == null || _chapter.Beats.Length == 0)
            {
                return;
            }

            for (int i = _currentBeatIndex; i < _chapter.Beats.Length; i++)
            {
                BeatSO beat = _chapter.Beats[i];
                if (beat == null || !beat.CanEnter())
                {
                    continue;
                }

                if (CurrentBeat != beat)
                {
                    CurrentBeat = beat;
                    _currentBeatIndex = i;
                    GameEventBus.Publish(new BeatAdvancedEvent(beat));
                }
            }
        }

        private void OnFlagChanged(FlagChangedEvent evt)
        {
            EvaluateBeats();
        }

        private void OnConnectionMade(ConnectionMadeEvent evt)
        {
            EvaluateBeats();
        }
    }
}
