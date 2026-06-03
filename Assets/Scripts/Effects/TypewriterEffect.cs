using System.Collections;
using TMPro;
using UnityEngine;

namespace DeadLetterOffice.Effects
{
    [CreateAssetMenu(menuName = "DLO/Effects/Typewriter")]
    public class TypewriterEffect : TextEffect
    {
        [SerializeField, Min(0.001f)] private float _secondsPerCharacter = 0.035f;

        public override IEnumerator Play(TMP_Text target, string text)
        {
            if (target == null)
            {
                yield break;
            }

            target.text = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                target.text = text.Substring(0, i + 1);
                yield return new WaitForSeconds(_secondsPerCharacter);
            }
        }

        public override void ForceComplete(TMP_Text target, string text)
        {
            if (target != null)
            {
                target.text = text;
            }
        }
    }
}
