using System.Collections.Generic;
using UnityEngine;
using o2.Editor.Tools.HierarchyHighlighter;

namespace o2.EditorTools {
    [System.Serializable]
    public class HierarchyHighlighterAsset : DataAssetBase {
        public List<TagHighlightPreferences> _tagColorPairs = new();

        [System.Serializable]
        public class TagHighlightPreferences {
            [Tooltip("If you don't see the tag you are looking for click the regenerate button down below.")]
            public TagEnumAutoGenerated Tag;

            public int fontSize = 13;
            public Color Color = Color.white;
            public TextAnchor alignment = TextAnchor.MiddleLeft;
            public FontStyle FontStyle = FontStyle.Normal;
        }
    }
}