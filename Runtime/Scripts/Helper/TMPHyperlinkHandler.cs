using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Business
{
    public class TMPHyperlinkHandler : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// 当点击链接<br/>
        /// 参数： 链接信息(根据这个链接的信息做你想做的事)<br/>
        /// </summary>
        public event Action<TMP_LinkInfo> OnClickLinkHandler;
        public class LinkClickedEvent : UnityEvent<string> { }
        public LinkClickedEvent OnLinkClicked = new LinkClickedEvent();


        [SerializeField]
        private TextMeshProUGUI self;

        // 链接点击事件

        [SerializeField]
        protected bool enableLog = true;


        public void Reset()
        {
            GetSelfAuto();
        }

        public void GetSelfAuto()
        {
            if (self == null)
            {
                self = GetComponent<TextMeshProUGUI>();
            }
        }

        private void Awake()
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 从点击位置获取鼠标点击的链接
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(self, eventData.position, eventData.pressEventCamera);

            // 如果点击了链接
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = self.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();

                // 触发链接点击事件
                OnLinkClicked.Invoke(linkId);
                OnClickLinkHandler?.Invoke(linkInfo);

                if (enableLog)
                {
                    Log.LogAtUnityEditor($"{gameObject.name}.{this.GetType()} 点击了[<color=blue>{linkId}</color>]({linkInfo.GetLinkText()})", "orange");
                }
            }
        }
    }

}