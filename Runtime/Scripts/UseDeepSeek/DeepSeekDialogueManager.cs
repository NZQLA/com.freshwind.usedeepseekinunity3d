using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LitJson;
using MD;
using UnityEngine;
using UnityEngine.Networking;

namespace Business
{
    /// <summary>
    /// Deep Seek 对话管理器
    /// </summary>
    public class DeepSeekDialogueManager : MonoBehaviour
    {
        /// <summary>
        /// 当收到的回复有更新时
        /// </summary>
        public event Action<string> OnRspRefreshedHandler;

        /// <summary>
        /// 当回复完成时
        /// </summary>
        public event Action OnRspCompletedHandler;

        ///// <summary>
        ///// 当接收到 以 stream 方式 收到的数据时
        ///// </summary>
        //public event Action<DeepSeekRspStreamData> OnReceiveStreamDataHandler;

        [SerializeField, Header("DeepSeek的一些数据")]
        protected DeepSeekInfo deepSeekInfo = new DeepSeekInfo();

        [SerializeField, Header("聊天面板")]
        protected ChatWithDeepSeekPanel panelChat;

        [SerializeField, Header("菜单面板")]
        protected DeepSeekChatMenuPanel menuPanel;

        [SerializeField, Header("使用流式获取回复？")]
        protected bool useStream = true;


        [SerializeField, Header("发送问题时 清理上一次的回复内容？")]
        protected bool clearRspOnSendQuestion = true;

        [SerializeField, Header("用于接收回复的内存")]
        protected long bufferSize = 1024 * 20;


        // API配置
        [Header("API Settings")]
        [SerializeField]
        //private string apiKey = "在此处填入你申请的API密钥";  // DeepSeek API密钥
        private string apiKey = "sk-38bd3a635ad2459e85601376800f3a69";  // DeepSeek API密钥

        //[SerializeField, Header("环境变量路径：阿里云的 Deep Seek APIKey")]
        //private string apiKeyPathName = "DeepSeekALiYun";


        public string nameAPIKey = "DeepSeekAPIKey";


        [SerializeField]
        private string modelName = "deepseek-chat";  // 使用的模型名称

        [SerializeField]
        private string apiUrl = "https://api.deepseek.com/v1/chat/completions"; // API请求地址

        [SerializeField]
        protected bool convertRspData = false; // 是否转换响应数据


        [SerializeField]
        protected string rspRawStreamContent = ""; // 响应数据


        [SerializeField]
        protected string rspContent = "";


        [SerializeField, Header("返回的内容")]
        protected DeepSeekRspData rspData = new DeepSeekRspData { };

        //[SerializeField, Header("接收到的流式数据")]
        //protected List<DeepSeekRspStreamData> rspStreamData = new List<DeepSeekRspStreamData>();

        [SerializeField]
        protected byte[] rspBuffer = new byte[1024 * 20];
        private UnityWebRequest requestByStream;
        [SerializeField]
        protected StreamedDownloadHandler streamedDownloadHandler;

        [SerializeField]
        protected JsonData rspJsonObj;


        // 对话参数
        [Header("Dialogue Settings")]
        [Range(0, 2)]
        public float temperature = 0.7f; // 控制生成文本的随机性（0-2，值越高越随机）   
        [Range(1, 2000)]
        public int maxTokens = 800;// 生成的最大令牌数（控制回复长度）


        [SerializeField, Header("样式")]
        protected MdTmpStyle style = new MdTmpStyle();

        // 角色设定
        [Serializable]
        public class NPCCharacter
        {
            public string name;
            [TextArea(3, 10)]
            public string personalityPrompt = "你是虚拟人物Unity-Chan，是个性格活泼，聪明可爱的女生。擅长Unity和C#编程知识。"; // 角色设定提示词
        }
        [SerializeField]
        public NPCCharacter npcCharacter;
        private Coroutine corChatByStream;

        // 回调委托，用于异步处理API响应
        public delegate void DialogueCallback(string response, bool isSuccess);



        public void Start()
        {
            panelChat.OnSendQuestionHandler += SendQuestion;
            panelChat.OnStopHandler += StopReceiveStreamData;

            menuPanel.OnShowModeChangedHandler += (showLikeMd) =>
            {
                if (showLikeMd)
                {
                    ShowRspLikeMarkDown();
                }
                else
                {
                    ShowRspRaw();
                }
            };

            //RefreshAPIKeyFromEnv();
            RefreshAPIKeyFromPlayerPrefs();
        }


        ///// <summary>
        ///// Use the API key from environment variable to refresh the API key. 
        ///// </summary>
        //[ContextMenu("RefreshAPIKeyFromEnv")]
        //public void RefreshAPIKeyFromEnv()
        //{
        //    apiKey = Environment.GetEnvironmentVariable(apiKeyPathName);
        //}


        [ContextMenu("RefreshAPIKeyFromPlayerPrefs")]
        public void RefreshAPIKeyFromPlayerPrefs()
        {
            apiKey = PlayerPrefs.GetString(nameAPIKey);
        }



        /// <summary>
        /// 发送问题
        /// </summary>
        /// <param name="question"></param>
        public void SendQuestion(string question)
        {
            if (question.IsNullOrEmpty())
            {
                Debug.Log("输入内容为空！");
                return;
            }

            if (clearRspOnSendQuestion)
            {
                rspRawStreamContent = "";
                panelChat.SetRspContent(rspRawStreamContent);
            }


            if (useStream)
            {
                //  重新分配内存
                rspBuffer = new byte[bufferSize];
                rspContent = "";
                rspData.Clear();

                TestChatWithDeepSeekWithStream(question);
            }
            else
            {
                TestChatWithDeepSeek(question);
            }

        }

        public void OnRspCompleted()
        {
            OnRspCompletedHandler?.Invoke();
            panelChat.ReadyForNextChat();
        }

        /// <summary>
        /// 刷新来自 stream 方式 收到的数据
        /// </summary>
        public void RefreshStreamRspData()
        {
            rspRawStreamContent = Encoding.UTF8.GetString(rspBuffer);
            bool flowControl = OnGetStreamContent(rspRawStreamContent);
            if (!flowControl)
            {
                return;
            }

        }

        public bool OnGetStreamContent(string rawStreamContent)
        {
            rspRawStreamContent = rawStreamContent.Trim();
            //Log.LogAtUnityEditor($"Receive stream data: \n{rspRawContent}");
            // 只要第一个":" 后面的部分 
            var indexStart = rspRawStreamContent.IndexOf(":") + 1;
            // 如果没有找到冒号，说明这不是一个有效的数据
            if (indexStart <= 0)
            {
                return false;
            }

            // this msg is tell you the stream is done
            if (rspRawStreamContent.StartsWith(deepSeekInfo.rspDone))
            {
                return false;
            }

            // 只要第一行的数据
            var indexEnd = rspRawStreamContent.IndexOf("\n");

            // 有效数据的长度为0 说明这不是一个有效的数据
            var length = indexEnd - indexStart;
            if (length <= 0)
            {
                return false;
            }
            // 截取有效的json数据部分
            rspRawStreamContent = rspRawStreamContent.Substring(indexStart, length);

            //var dataStream = new DeepSeekRspStreamData();

            //rspRawContent = $"{{{rspRawContent}}}";
            //rspJsonObj = JsonUtility.FromJson(rspContent, typeof(object));
            Log.LogAtUnityEditor($"Receive stream data: \n{rspRawStreamContent}");
            try
            {
                // 解析json数据
                rspJsonObj = JsonMapper.ToObject(rspRawStreamContent);
                // 只要第一条数据
                var rspChoices0 = rspJsonObj[deepSeekInfo.rspChoices][0];
                //  {"choices":[{"finish_reason":"stop","delta" ... } means the rsp will be over soon!
                if (rspChoices0[deepSeekInfo.rspFinishFlag] != null &&
                    rspChoices0[deepSeekInfo.rspFinishFlag].ToString() == deepSeekInfo.rspFinishFlagValue)
                {
                    //OnRspCompleted();
                    return false;
                }


                var dataFirst = rspChoices0[deepSeekInfo.rspDelta];
                var reasoning_content = dataFirst[deepSeekInfo.rspReasoning_content];
                var content = dataFirst[deepSeekInfo.rspContent];
                //// 获取时间戳
                //dataStream.time = long.Parse(rspJsonObj["created"].ToString());

                if (reasoning_content != null)
                {
                    //dataStream.reasoning_content = reasoning_content.ToString();
                    rspData.AppendReason(reasoning_content.ToString());
                }

                if (content != null)
                {
                    //dataStream.content = content.ToString();
                    rspData.AppendContent(content.ToString());
                }



                //rspStreamData.Add(dataStream);

                //// 对 stream 数据进行排序 ， 排序规则是其时间戳
                //rspStreamData.Sort();

                //// 拼接排好序的 stream 数据
                //rspData.reasoning_content = StringHelper.Join(rspStreamData, (sd) => sd.reasoning_content);
                //rspData.content = StringHelper.Join(rspStreamData, (sd) => sd.content);


                //rspContent = rspData.ToString();

                // 将 markdown 转换为 tmp 格式 再显示
                //ShowRspLikeMarkDown();

                RefreshTheRspPanel();

                //panelChat.SetRspContent(rspContent);
                panelChat.ScrollToBottom();
                OnRspRefreshedHandler?.Invoke(rspRawStreamContent);
            }
            catch (Exception ex)
            {
                Log.LogAtUnityEditor($"{ex.Message}  rawStreamContent:{rawStreamContent}", "red", LogErrorType.Waring);
            }

            return true;
        }

        /// <summary>
        /// Refreshes the RSP panel based on the display mode. It shows the panel in Markdown format if 'ShowAsMd' is
        /// true; otherwise, it shows raw format.
        /// </summary>
        public void RefreshTheRspPanel()
        {
            if (menuPanel.ShowAsMd)
            {
                ShowRspLikeMarkDown();
            }
            else
            {
                ShowRspRaw();
            }
        }

        /// <summary>
        /// 将 markdown 转换为 tmp 格式 再显示
        /// </summary>
        [ContextMenu("ShowRspLikeMarkDown")]
        public void ShowRspLikeMarkDown()
        {
            rspContent = rspData.ToString();
            rspContent = rspContent.ConvertMarkdownToTMP(style);
            panelChat.SetRspContent(rspContent);
        }

        /// <summary>
        /// 显示原始的回复数据
        /// </summary>
        [ContextMenu("ShowRspRaw")]
        public void ShowRspRaw()
        {
            rspContent = rspData.ToString();
            panelChat.SetRspContent(rspContent);
        }

        /// <summary>
        /// 当接收到 以 stream 方式 收到的数据时
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        public void OnReceiveStreamData(byte[] data, int len)
        {
            // 将数据存入 rspBuffer
            System.Buffer.BlockCopy(data, 0, rspBuffer, 0, len);
            RefreshStreamRspData();

        }

        public void TestChatWithDeepSeek(string yourWords)
        {
            if (yourWords.IsNullOrEmpty())
            {
                Debug.Log("输入内容为空！");
                return;
            }

            SendDialogueRequest(yourWords, (response, isSuccess) =>
            {
                if (isSuccess)
                {
                    Log.LogAtUnityEditor($"{gameObject.name}.{this.GetType()} 请求成功!!!  ", "#AAFF00");
                    Log.LogAtUnityEditor($"{gameObject.name}.{this.GetType()} 回复内容：{response}  ", "#EEEEEE");

                }
                else
                {
                    Log.LogAtUnityEditor($"{gameObject.name}.{this.GetType()} 请求失败!!!  ", "#FF0000");
                }
                rspContent = response;
                ShowRspLikeMarkDown();
                //panelChat.SetRspContent(response);
                OnRspCompleted();
            });

        }


        public void TestChatWithDeepSeekWithStream(string yourWords)
        {
            if (yourWords.IsNullOrEmpty())
            {
                Debug.Log("输入内容为空！");
                return;
            }

            SendDialogueRequestWithStream(yourWords, (response, isSuccess) =>
            {
                if (isSuccess)
                {
                    Log.LogAtUnityEditor($"{gameObject.name}.{this.GetType()} 请求成功!!!", "#AAFF00");
                    Log.LogAtUnityEditor($"{gameObject.name}.{this.GetType()} 回复内容：{response}  ", "#EEEEEE");


                }
                else
                {
                    Log.LogAtUnityEditor($"{gameObject.name}.{this.GetType()} 请求失败!!!  ", "#FF0000");
                }
                //panelChat.SetRspContent(response);
                OnRspCompleted();
            });

        }


        /// <summary>    
        /// 发送对话请求    
        /// </summary>    
        /// <param name="userMessage">玩家的输入内容</param> 
        /// <param name="callback">回调函数，用于处理API响应</param>  
        public void SendDialogueRequest(string userMessage, DialogueCallback callback)
        {
            StartCoroutine(ProcessDialogueRequest(userMessage, callback));
        }



        /// <summary>    
        /// 发送对话请求    
        /// </summary>    
        /// <param name="userMessage">玩家的输入内容</param> 
        /// <param name="callback">回调函数，用于处理API响应</param>  
        public void SendDialogueRequestWithStream(string userMessage, DialogueCallback callback)
        {
            corChatByStream = StartCoroutine(ProcessDialogueRequestWithStream(userMessage, callback));
        }


        /// <summary>  
        /// 处理对话请求的协程   
        /// </summary>  
        /// <param name="userInput">玩家的输入内容</param> 
        /// <param name="callback">回调函数，用于处理API响应</param>  
        public IEnumerator ProcessDialogueRequest(string userInput, DialogueCallback callback)
        {
            // 构建消息列表，包含系统提示和用户输入
            List<Message> messages = new List<Message>
        {
            //new Message { role = "system", content = npcCharacter.personalityPrompt },// 系统角色设定
            new Message { role = "user", content = userInput }// 用户输入
        };
            // 构建请求体
            ChatRequest requestBody = new ChatRequest
            {
                model = modelName,// 模型名称
                messages = messages,// 消息列表
                temperature = temperature,// 温度参数
                max_tokens = maxTokens// 最大令牌数
            };
            string jsonBody = JsonUtility.ToJson(requestBody);
            Debug.Log("Sending JSON: " + jsonBody); // 调试用，打印发送的JSON数据
            UnityWebRequest request = CreateWebRequest(jsonBody);
            yield return request.SendWebRequest();
            if (IsRequestError(request))
            {
                Debug.LogError($"API Error: {request.responseCode} --- {request.error}    \n{request.downloadHandler.text}");
                //callback?.Invoke(null, false);
                callback?.Invoke(request.error, false);
                yield break;
            }

            DeepSeekResponse response = ParseResponse(request.downloadHandler.text);
            if (response != null && response.choices.Length > 0)
            {
                string npcReply = response.choices[0].message.content;
                callback?.Invoke(npcReply, true);
            }
            else
            {
                callback?.Invoke(name + "（陷入沉默）", false);
            }
        }



        /// <summary>  
        /// 处理对话请求的协程   
        /// </summary>  
        /// <param name="userInput">玩家的输入内容</param> 
        /// <param name="callback">回调函数，用于处理API响应</param>  
        public IEnumerator ProcessDialogueRequestWithStream(string userInput, DialogueCallback callback)
        {

            // 构建消息列表，包含系统提示和用户输入
            List<Message> messages = new List<Message>
        {
            //new Message { role = "system", content = npcCharacter.personalityPrompt },// 系统角色设定
            new Message { role = "user", content = userInput }// 用户输入
        };
            // 构建请求体
            ChatRequest requestBody = new ChatRequest
            {
                model = modelName,// 模型名称
                messages = messages,// 消息列表
                temperature = temperature,// 温度参数
                max_tokens = maxTokens,// 最大令牌数
                stream = true
            };
            string jsonBody = JsonUtility.ToJson(requestBody);
            Debug.Log("Sending JSON: " + jsonBody); // 调试用，打印发送的JSON数据
            requestByStream = CreateWebRequest(jsonBody);


            //rspBuffer = new byte[1024 * 4];
            //rspBuffer = new byte[1024 * 20];
            streamedDownloadHandler = new StreamedDownloadHandler(rspBuffer);
            streamedDownloadHandler.OnReceiveDataHandler += OnReceiveStreamData;
            requestByStream.downloadHandler = streamedDownloadHandler;


            convertRspData = true;
            yield return requestByStream.SendWebRequest();

            //request.SendWebRequest();
            //yield break;


            yield return new WaitForEndOfFrame();
            convertRspData = false;


            if (IsRequestError(requestByStream))
            {
                Debug.LogError($"API Error: {requestByStream.responseCode} --- {requestByStream.error}    \n{requestByStream.downloadHandler.text}");
                callback?.Invoke(null, false);
                yield break;
            }

            RefreshStreamRspData();
            if (rspRawStreamContent.IsNullOrEmpty())
            {
                callback?.Invoke(name + "（陷入沉默）", false);
            }
            else
            {
                //callback?.Invoke(rspRawContent, true);
                callback?.Invoke(rspContent, true);
            }

            //DeepSeekResponse response = ParseResponse(request.downloadHandler.text);
            //if (response != null && response.choices.Length > 0)
            //{
            //    string npcReply = response.choices[0].message.content;
            //    callback?.Invoke(npcReply, true);
            //}
            //else
            //{
            //    callback?.Invoke(name + "（陷入沉默）", false);
            //}
        }

        /// <summary>
        /// 停止接收流式数据
        /// </summary>
        [ContextMenu("StopReceiveStreamData")]
        public void StopReceiveStreamData()
        {
            if (streamedDownloadHandler != null)
            {
                streamedDownloadHandler.StopReceive();
            }


            if (requestByStream != null)
            {
                requestByStream.Abort();
            }

            if (corChatByStream != null)
            {
                StopCoroutine(corChatByStream);
                corChatByStream = null;
            }
        }



        /// <summary>    
        /// 创建UnityWebRequest对象   
        /// </summary>  
        /// <param name="jsonBody">请求体的JSON字符串</param>  
        /// <returns>配置好的UnityWebRequest对象</returns>   
        private UnityWebRequest CreateWebRequest(string jsonBody)
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            var request = new UnityWebRequest(apiUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);// 设置上传处理器
            request.downloadHandler = new DownloadHandlerBuffer();// 设置下载处理器
            request.SetRequestHeader("Content-Type", "application/json");// 设置请求头
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");// 设置认证头
            request.SetRequestHeader("Accept", "application/json");// 设置接受类型
            return request;
        }
        /// <summary>   
        /// 检查请求是否出错    
        /// </summary>   
        /// <param name="request">UnityWebRequest对象</param>   
        /// <returns>如果请求出错返回true，否则返回false</returns>   
        private bool IsRequestError(UnityWebRequest request)
        {
            return request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError;
        }
        /// <summary>    
        /// 解析API响应   
        /// </summary> 
        /// <param name="jsonResponse">API响应的JSON字符串</param>  
        /// <returns>解析后的DeepSeekResponse对象</returns>   
        private DeepSeekResponse ParseResponse(string jsonResponse)
        {
            try
            {
                return JsonUtility.FromJson<DeepSeekResponse>(jsonResponse);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON解析失败: {e.Message}\n响应内容：{jsonResponse}");
                return null;
            }
        }
        // 可序列化数据结构
        [System.Serializable]
        private class ChatRequest
        {
            public string model;// 模型名称
            public List<Message> messages;// 消息列表
            public float temperature;// 温度参数
            public int max_tokens;// 最大令牌数
            public bool stream = false;// 是否流式下载

        }
        [System.Serializable]
        public class Message
        {
            public string role;// 角色（system/user/assistant）
            public string content;// 消息内容
        }
        [System.Serializable]
        private class DeepSeekResponse
        {
            public Choice[] choices;//
        }
        [System.Serializable]
        private class Choice
        {
            public Message message;// 生成的消息 
        }


        /// <summary>
        /// 用于处理流式下载的DownloadHandler
        /// </summary>
        public class StreamedDownloadHandler : DownloadHandlerScript
        {
            public event Action<byte[], int> OnReceiveDataHandler;
            public event Action OnCompleteHandler;
            public StreamedDownloadHandler(byte[] buffer) : base(buffer) { }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (data == null || data.Length < 1)
                {
                    Debug.LogError("No data received");
                    return false;
                }

                OnReceiveDataHandler?.Invoke(data, dataLength);
                //// 确保不会超出缓冲区大小
                //if (bufferOffset + dataLength > buffer.Length)
                //{
                //    Debug.LogError("Buffer overflow");
                //    return false;
                //}

                //// 将接收到的数据写入缓冲区
                //System.Buffer.BlockCopy(data, 0, buffer, bufferOffset, dataLength);
                //bufferOffset += dataLength;

                //// 处理接收到的数据
                //Debug.Log($"Received {dataLength} bytes");
                return true;
            }

            protected override void CompleteContent()
            {
                OnCompleteHandler?.Invoke();
                Debug.Log("Download complete");
            }

            protected override void ReceiveContentLength(int contentLength)
            {
                Debug.Log($"Content length: {contentLength}");
            }


            /// <summary>
            /// 停止接收数据
            /// </summary>
            public void StopReceive()
            {
                OnReceiveDataHandler = null;
                OnCompleteHandler = null;
            }
        }


        /// <summary>
        /// DeepSeek返回的数据
        /// </summary>
        [Serializable]
        public class DeepSeekRspData
        {
            public string reasoning_content = "";
            public string content = "";

            public void AppendReason(string _reason)
            {
                reasoning_content += _reason;
            }

            public void AppendContent(string _content)
            {
                content += _content;
            }

            public void Append(DeepSeekRspStreamData data)
            {
                if (data == null)
                {
                    return;
                }

                if (data.reasoning_content != null)
                {
                    reasoning_content += data.reasoning_content;
                }

                if (data.content != null)
                {
                    content += data.content;
                }
            }

            public void Clear()
            {
                reasoning_content = "";
                content = "";
            }

            public override string ToString()
            {
                return $"<color=#CCCCCC><b>deepSeek思路</b>\n<i>{reasoning_content}</i></color>\n <b>正式回复:</b>\n{content}";
            }
        }


        /// <summary>
        /// deep seek 返回的流式数据
        /// </summary>
        [Serializable]
        public class DeepSeekRspStreamData : IComparable<DeepSeekRspStreamData>
        {
            public string reasoning_content = "";
            public string content = "";
            /// <summary>
            /// 时间戳
            /// </summary>
            public long time;

            public int CompareTo(DeepSeekRspStreamData other)
            {
                if (other == null)
                {
                    return 1;
                }
                return time.CompareTo(other.time);
            }
        }




    }

}