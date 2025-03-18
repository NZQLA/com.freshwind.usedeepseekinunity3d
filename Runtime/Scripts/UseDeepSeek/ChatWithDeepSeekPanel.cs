using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Business
{
    /// <summary>
    /// Panel: Chat with deep seek .<br/>
    /// </summary>
    public class ChatWithDeepSeekPanel : MonoBehaviour
    {
        /// <summary>
        /// 事件：发送问题
        /// </summary>
        public event Action<string> OnSendQuestionHandler;

        public event Action<Vector3> OnSelectContentEndHandler;

        public event Action OnStopHandler;

        [SerializeField, Header("聊天状态")]
        protected ChatState chatState = ChatState.Free;

        [SerializeField, Header("回复内容滚动控制")]
        protected ScrollRect rspScroll;

        [SerializeField, Header("回复内容")]
        //protected TextMeshProUGUI rsp;
        protected TMP_InputField rsp;

        [SerializeField, Header("输入框")]
        protected InputField inputQuestion;


        [SerializeField, Header("按钮 发送/停止")]
        protected Button btnSendOrStop;

        [SerializeField, Header("按钮 的图片")]
        protected Image imgBtn;

        [SerializeField, Header("素材 发送/停止")]
        protected Sprite spriteSend;

        [SerializeField, Header("素材 停止")]
        protected Sprite spriteStop;


        public void Awake()
        {
            rsp.readOnly = true;

            // 监听输入框的输入事件
            inputQuestion.onValueChanged.AddListener(OnInputting);

            //监听输入框的回车事件
            inputQuestion.onEndEdit.AddListener(TrySendQuestion);
            GUIUtility.systemCopyBuffer = "";

            btnSendOrStop.onClick.AddListener(onBtnSendOrStop);

            if (imgBtn == null)
            {
                imgBtn = btnSendOrStop.targetGraphic as Image;
            }

            // 监听文本选择结束事件
            rsp.onEndTextSelection.AddListener((str, v1, v2) =>
            {
                if (rsp.selectionStringAnchorPosition != rsp.selectionStringFocusPosition)
                {
                    Vector3 buttonPosition = CalculateButtonPosition();
                    OnSelectContentEndHandler?.Invoke(buttonPosition);
                    //actionButton.transform.position = buttonPosition;
                    //actionButton.gameObject.SetActive(true);
                }
                else
                {
                    //actionButton.gameObject.SetActive(false);
                }

            });
        }

        public void Start()
        {
            ReadyForNextChat();
        }



        public Vector3 CalculateButtonPosition()
        {
            int startIndex = Mathf.Min(rsp.selectionStringAnchorPosition, rsp.selectionStringFocusPosition);
            int endIndex = Mathf.Max(rsp.selectionStringAnchorPosition, rsp.selectionStringFocusPosition);

            TMP_Text textComponent = rsp.textComponent;
            Vector3 startPosition = textComponent.textInfo.characterInfo[startIndex].bottomLeft;
            Vector3 endPosition = textComponent.textInfo.characterInfo[endIndex].topRight;

            Vector3 buttonPosition = (startPosition + endPosition) / 2;
            buttonPosition = textComponent.transform.TransformPoint(buttonPosition);

            return buttonPosition;
        }


        /// <summary>
        /// 当点击了 停止/发送 按钮
        /// </summary>
        private void onBtnSendOrStop()
        {
            if (chatState == ChatState.WaitingRsp)
            {
                OnStopHandler?.Invoke();
                chatState = ChatState.Free;
                RefreshBtnState();
            }
            else
            {
                TrySendQuestion(inputQuestion.text);
            }
        }


        /// <summary>
        /// 刷新按钮的状态
        /// </summary>
        public void RefreshBtnState()
        {
            switch (chatState)
            {
                case ChatState.Free:
                    imgBtn.sprite = spriteSend;
                    btnSendOrStop.interactable = false;
                    //btnSendOrStop.targetGraphic
                    break;
                case ChatState.InputtingQuestion:
                    imgBtn.sprite = spriteSend;
                    btnSendOrStop.interactable = inputQuestion.text.Length > 0;
                    break;
                case ChatState.WaitingRsp:
                    imgBtn.sprite = spriteStop;
                    btnSendOrStop.interactable = true;
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// 输入框输入中
        /// </summary>
        /// <param name="input"></param>
        public void OnInputting(string input)
        {
            if (chatState == ChatState.WaitingRsp)
            {
                return;
            }

            chatState = ChatState.InputtingQuestion;
            RefreshBtnState();
        }

        /// <summary>
        /// 尝试发送问题
        /// </summary>
        /// <param name="question"></param>
        public void TrySendQuestion(string question)
        {
            if (chatState == ChatState.WaitingRsp)
            {
                return;
            }

            chatState = question.IsNullOrEmpty() ? ChatState.Free : ChatState.WaitingRsp;
            OnSendQuestionHandler?.Invoke(question);
            RefreshBtnState();
        }


        public void SetRspContent(string content)
        {
            rsp.text = content;
        }


        public void ClearRspContent()
        {
            rsp.text = "";
        }

        public void ClearInput()
        {
            inputQuestion.text = "";
        }

        public void ScrollToBottom()
        {
            //rspScroll.ScrollToBottom();
            rspScroll.verticalNormalizedPosition = 0;
        }

        public void ReadyForNextChat()
        {
            chatState = ChatState.Free;
            ClearInput();
            RefreshBtnState();
            ScrollToTop();
        }

        public void ScrollToTop()
        {
            rspScroll.verticalNormalizedPosition = 1;
        }

    }


    /// <summary>
    /// 聊天状态
    /// </summary>
    public enum ChatState
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        Free,

        /// <summary>
        /// 输入问题中
        /// </summary>
        InputtingQuestion,

        /// <summary>
        /// 等待回复中
        /// </summary>
        WaitingRsp,
    }
}