using UnityEngine;

namespace GlitchHunter.Data
{
    [System.Serializable]
    public class UIData
    {
        public string Messages;
        public AudioClip AudioClip;
    }

    [CreateAssetMenu(fileName = "UIText", menuName = "GlitchHunter/UITextData", order = 0)]
    public class UITextData : ScriptableObject
    {
        public UIData[] UIData;
    }
}
