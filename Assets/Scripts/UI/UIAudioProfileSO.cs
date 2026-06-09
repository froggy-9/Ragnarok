using DeadLetterOffice.Core;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    [CreateAssetMenu(menuName = "DLO/UI/Audio Profile")]
    public class UIAudioProfileSO : ScriptableObject
    {
        [SerializeField] private AudioCueSO _genericOpen;
        [SerializeField] private AudioCueSO _genericClose;
        [SerializeField] private AudioCueSO _mapOpen;
        [SerializeField] private AudioCueSO _mapClose;
        [SerializeField] private AudioCueSO _buttonClick;
        [SerializeField] private AudioCueSO _select;
        [SerializeField] private AudioCueSO _tab;

        public AudioCueSO GetCue(UIAudioKind kind)
        {
            return kind switch
            {
                UIAudioKind.GenericOpen => _genericOpen,
                UIAudioKind.GenericClose => _genericClose,
                UIAudioKind.MapOpen => _mapOpen,
                UIAudioKind.MapClose => _mapClose,
                UIAudioKind.ButtonClick => _buttonClick,
                UIAudioKind.Select => _select,
                UIAudioKind.Tab => _tab,
                _ => null
            };
        }
    }
}
