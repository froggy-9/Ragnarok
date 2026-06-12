using System.Collections.Generic;
using DeadLetterOffice.Core;
using DeadLetterOffice.Effects;
using UnityEngine;

namespace DeadLetterOffice.Dialogue
{
    public enum DialogueLineKind
    {
        Speech,
        Narration,
        InnerThought,
        ImagePanel
    }

    public enum CharacterExpression
    {
        Default,
        Surprised,
        Anxious,
        Angry,
        Happy,
        Sad
    }

    [CreateAssetMenu(menuName = "DLO/Dialogue/Line")]
    public class DialogueLineSO : ScriptableObject
    {
        [Header("Speech")]
        [SerializeField] private DialogueLineKind _lineKind = DialogueLineKind.Speech;
        [SerializeField] private string _speaker;
        [SerializeField, TextArea(3, 8)] private string _text;

        [Header("Character Art")]
        [SerializeField] private Sprite _characterSprite;
        [SerializeField] private CharacterExpression _expression;

        [Header("Scene Illustration")]
        [SerializeField] private Sprite _illustrationSprite;
        [SerializeField] private bool _showIllustrationAsPanel;

        [Header("Audio")]
        [SerializeField] private AudioCueSO _audioCue;

        [Header("Text Effects")]
        [SerializeField] private bool _useTypewriter;
        [SerializeField] private bool _useFadeIn;
        [SerializeField] private bool _useShake;
        [SerializeField] private bool _useSlowReveal;

        [Header("Effect Assets")]
        [SerializeField] private TypewriterEffect _typewriterEffect;
        [SerializeField] private TextEffect _fadeInEffect;
        [SerializeField] private TextEffect _shakeEffect;
        [SerializeField] private TextEffect _slowRevealEffect;

        public DialogueLineKind LineKind => _lineKind;
        public string Speaker => _speaker;
        public string Text => _text;
        public Sprite CharacterSprite => _characterSprite;
        public CharacterExpression Expression => _expression;
        public Sprite IllustrationSprite => _illustrationSprite;
        public bool ShowIllustrationAsPanel => _showIllustrationAsPanel || _lineKind == DialogueLineKind.ImagePanel;
        public AudioCueSO AudioCue => _audioCue;
        public bool ShowsSpeakerName => _lineKind == DialogueLineKind.Speech && !string.IsNullOrWhiteSpace(_speaker);

        public IEnumerable<TextEffect> GetActiveEffects()
        {
            if (_useTypewriter && _typewriterEffect != null)
            {
                yield return _typewriterEffect;
            }

            if (_useFadeIn && _fadeInEffect != null)
            {
                yield return _fadeInEffect;
            }

            if (_useShake && _shakeEffect != null)
            {
                yield return _shakeEffect;
            }

            if (_useSlowReveal && _slowRevealEffect != null)
            {
                yield return _slowRevealEffect;
            }
        }
    }
}
