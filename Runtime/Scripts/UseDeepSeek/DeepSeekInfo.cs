using System;
using System.Collections;
using UnityEngine;


namespace Business
{
    /// <summary>
    /// some data for deep seek 
    /// </summary>
    [Serializable]
    public class DeepSeekInfo
    {
        public string rspChoices = "choices";
        public string rspReasoning_content = "reasoning_content";
        public string rspContent = "content";

        public string rspFinishFlag = "finish_reason";
        public string rspFinishFlagValue = "stop";
        public string rspDone = "[DONE]";
        internal string rspDelta = "delta";
    }
}