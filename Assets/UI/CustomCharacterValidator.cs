using UnityEngine;
using System;
using System.Collections.Generic;

namespace TMPro
{
    [Serializable]
    [CreateAssetMenu(menuName = "TextMeshPro/Input Validators/Invalid Characters", order = 100)]
    public class CustomCharacterValidator : TMP_InputValidator
    {
        [SerializeField] private string disallowedCharacters;
        
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (disallowedCharacters.Contains(ch)) return (char)0;
            
            text += ch;
            pos += 1;
            return ch;

        }
    }
}