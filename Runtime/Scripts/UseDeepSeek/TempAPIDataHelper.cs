using System.Collections;
using UnityEngine;


namespace Business.Temp
{
    /// <summary>
    /// 
    /// </summary>
    public class TempAPIDataHelper : MonoBehaviour
    {

        public string nameKey = "DeepSeekAPIKey";
        public string KeyValue = "APIValue";

        [ContextMenu("WriteAPIToPlayerPrefs")]
        public void WriteAPIToPlayerPrefs()
        {
            PlayerPrefs.SetString(nameKey, KeyValue);
        }

    }
}