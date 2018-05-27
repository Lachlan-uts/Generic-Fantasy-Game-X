using UnityEngine;
using System.Collections;

namespace AncientCraftGames.Tutorial
{
    /// <summary>
    /// 
    /// </summary>
    public class TextAreaArrayAttribute : PropertyAttribute
    {
        public int Lines;
        public int SpaceBetween;

        public TextAreaArrayAttribute(int lines, int spaceBetween)
        {
            if (lines < 1) lines = 1;
            Lines = lines;
            SpaceBetween = spaceBetween;
        }
    }
}
