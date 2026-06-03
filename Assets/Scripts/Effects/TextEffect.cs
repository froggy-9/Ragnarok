using System.Collections;
using TMPro;
using UnityEngine;

namespace DeadLetterOffice.Effects
{
    public abstract class TextEffect : ScriptableObject
    {
        public abstract IEnumerator Play(TMP_Text target, string text);
        public abstract void ForceComplete(TMP_Text target, string text);
    }
}
