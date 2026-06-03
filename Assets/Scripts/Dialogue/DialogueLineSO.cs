using System.Collections.Generic;
using DeadLetterOffice.Effects;
using UnityEngine;

namespace DeadLetterOffice.Dialogue
{
    public enum CharacterExpression
    {
        Default,
        Surprised,
        Anxious
    }

    [CreateAssetMenu(menuName = "DLO/Dialogue/Line")]
    public class DialogueLineSO : ScriptableObject
    {
        [Header("Speech")]
        [SerializeField] private string _speaker;
        [SerializeField, TextArea(3, 8)] private string _text;

        [Header("Character Art")]
        [SerializeField] private Sprite _characterSprite;
        [SerializeField] private CharacterExpression _expression;

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

        public string Speaker => _speaker;
        public string Text => _text;
        public Sprite CharacterSprite => _characterSprite;
        public CharacterExpression Expression => _expression;

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
