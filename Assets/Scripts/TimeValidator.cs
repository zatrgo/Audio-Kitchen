using UnityEngine;
using System;

namespace TMPro {
    [Serializable]
    [CreateAssetMenu(fileName = "TimeValidator.asset", menuName = "TextMeshPro/Validators/TimeValidator", order = 100)]
    public class TimeValidator : TMP_InputValidator {
        // Custom text input validation function
        public override char Validate(ref string text, ref int pos, char ch) {
            if (pos > 9) return '\0';

            if ("0123456789.:".Contains(ch)) {
                if (":.".Contains(ch) && text.Contains(ch)) pos = text.IndexOf(ch) + 1;
                else {
                    if (pos >text.Length) pos = text.Length;
                    text = text.Insert(pos, ch.ToString());
                    pos += 1;
                    return ch;
                }
                
            }
            return '\0';
        }
    }
}