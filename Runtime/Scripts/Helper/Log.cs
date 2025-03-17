using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace Business
{
    /// <summary>Log</summary>
    public class Log : MonoBehaviour
    {
#if UNITY_EDITOR
        [ContextMenu("保存到一个新文件")]
        public void SaveToOneNewFile()
        {
            var fileName = "Log_" + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".txt";
            Setting(fileName);
            SaveLogToFile(false);
        }

        [ContextMenu("增量保存")]
        public void SaveToFileByAppend()
        {
            Setting();
            SaveLogToFile(true);
        }
#endif


        protected static string strPathLogFile = "Log.txt";
        protected static string strPathFull = "Log.txt";

        protected static List<string> arrLog = new List<string>();

        /// <summary>设置Log文件路径</summary>
        /// <param name="strPathLog"></param>
        public static void Setting(string strPathLog = "Log.txt")
        {
            strPathLogFile = strPathLog;
            if (Application.isEditor)
            {
                strPathFull = Path.Combine(Application.dataPath, strPathLog);
            }
            else
            {
                strPathFull = Path.Combine(Application.persistentDataPath, strPathLog);
            }
        }






        /// <summary>获取Log内容的数组</summary>
        /// <returns></returns>
        public static string[] GetLogContent()
        {
            return arrLog.ToArray();
        }



        /// <summary>
        /// Log
        /// </summary>
        /// <param name="strLog">Log内容</param>
        /// <param name="ErrorType">Log错误类型</param>
        /// <param name="bAddTime">是否加入时间戳</param>
        /// <param name="bSave">是否缓存Log</param>
        /// <param name="logPlatform">Log平台</param>
        public static void LogAndSave(string strLog, LogErrorType ErrorType = LogErrorType.Normal, bool bAddTime = true, bool bSave = true, LogPlatform logPlatform = LogPlatform.UnityDebug)
        {
            strLog = string.Format("Log : [{0}] {1}", strLog, bAddTime ? DateTime.Now.ToString("yy-MM-dd HH:mm:ss:fff") : "");

            if (bSave)
            {
                arrLog.Add(strLog);
            }

            switch (logPlatform)
            {
                case LogPlatform.UnityDebug:
                    if (Application.isEditor)
                    {
                        UnityEngine.Debug.Log(strLog);
                    }
                    break;

                case LogPlatform.Console:
                    Console.WriteLine(strLog);
                    break;

                default:

                    break;
            }
        }


        /// <summary>
        /// 保存Log日志到指定文件
        /// </summary>
        /// <param name="bAppend"></param>
        public static void SaveLogToFile(bool bAppend = true)
        {
            if (arrLog == null || arrLog.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < arrLog.Count; i++)
            {
                sb.AppendLine(arrLog[i]);
            }

            FileTool.WriteStringToFileByFileStream(
                strPathFull, sb.ToString(), FileTool.EncodingUtf8WithOutBom,
                () => { Log.LogAndSave("将Log日志写入文件 " + strPathFull, LogErrorType.Normal, true, false); },

                bAppend);

        }



        /// <summary>仅在编辑器模式下进行Log </summary>
        /// <param name="strLog">Log的内容</param>
        /// <param name="color">颜色</param>
        /// <param name="ErrorType">错误类型</param>
        /// <param name="bAddTime">追加当前时间？</param>
        /// <param name="bSave">写入日志？</param>
        public static void LogAtUnityEditor(string strLog, string color = "white", LogErrorType ErrorType = LogErrorType.Normal, bool bAddTime = true, bool bSave = true, bool appendInvokeStack = false)
        {
            if (!Application.isEditor)
            {
                return;
            }

            Print(strLog, ErrorType, color, bAddTime, bSave, appendInvokeStack);
        }


        /// <summary>仅在编辑器模式下进行Log </summary>
        /// <param name="strLog">Log的内容</param>
        /// <param name="ErrorType">错误类型</param>
        /// <param name="color">颜色</param>
        /// <param name="bAddTime">追加当前时间？</param>
        /// <param name="bSave">写入日志？</param>
        public static void Print(string strLog, LogErrorType ErrorType = LogErrorType.Normal, string color = "white", bool bAddTime = true, bool bSave = true, bool appendInvokeStack = false)
        {
            strLog = string.Format("Log : [{0}] {1}", strLog, bAddTime ? DateTime.Now.ToString("yy-MM-dd HH:mm:ss:fff") : "");

            // 在log 文本后面添加调用堆栈信息
            if (appendInvokeStack)
            {
                StackTrace st = new StackTrace(true);
                strLog += "\r\n" + st.ToString();
            }


            if (bSave)
            {
                arrLog.Add(strLog);
            }

            string str = UnityColorString(strLog, color);

            switch (ErrorType)
            {
                case LogErrorType.Error:
                    UnityEngine.Debug.LogError(str);
                    break;
                case LogErrorType.Waring:
                    UnityEngine.Debug.LogWarning(str);
                    break;
                case LogErrorType.Normal:
                    UnityEngine.Debug.Log(str);
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 直接使用传入的文本打印 , 不再附加富文本!!! <br/>
        /// </summary>
        /// <param name="strLog"></param>
        /// <param name="ErrorType"></param>
        /// <param name="bAddTime"></param>
        /// <param name="bSave"></param>
        /// <param name="appendInvokeStack"></param>
        public static void LogAtUnityEditorWithOutRichText(string strLog, LogErrorType ErrorType = LogErrorType.Normal, bool bAddTime = true, bool bSave = true, bool appendInvokeStack = false)
        {
            if (!Application.isEditor)
            {
                return;
            }

            strLog = string.Format("Log : [{0}] {1}", strLog, bAddTime ? DateTime.Now.ToString("yy-MM-dd HH:mm:ss:fff") : "");

            // 在log 文本后面添加调用堆栈信息
            if (appendInvokeStack)
            {
                StackTrace st = new StackTrace(true);
                strLog += "\r\n" + st.ToString();
            }


            if (bSave)
            {
                arrLog.Add(strLog);
            }

            switch (ErrorType)
            {
                case LogErrorType.Error:
                    UnityEngine.Debug.LogError(strLog);
                    break;
                case LogErrorType.Waring:
                    UnityEngine.Debug.LogWarning(strLog);
                    break;
                case LogErrorType.Normal:
                    UnityEngine.Debug.Log(strLog);
                    break;
                default:
                    break;
            }
        }


        /// <summary>Unity Log [Normal]</summary>
        /// <param name="strLog"></param>
        /// <param name="bAddTime"></param>
        /// <param name="bSave"></param>
        public static void LogAtUnityEditorNormal(string strLog, bool bAddTime = true, bool bSave = true)
        {
            LogAtUnityEditor(strLog, "#aaaaaaff", LogErrorType.Normal, bAddTime, bSave);
        }


        /// <summary>Unity Log [Warning]</summary>
        /// <param name="strLog"></param>
        /// <param name="bAddTime"></param>
        /// <param name="bSave"></param>
        public static void LogAtUnityEditorWarning(string strLog, bool bAddTime = true, bool bSave = true)
        {
            LogAtUnityEditor(strLog, "#ffff00ff", LogErrorType.Waring, bAddTime, bSave);
        }


        /// <summary>Unity Log [Warning]</summary>
        /// <param name="strLog"></param>
        /// <param name="needLogInvokeSource"></param>
        /// <param name="bAddTime"></param>
        /// <param name="bSave"></param>
        public static void LogAtUnityEditorError(string strLog, bool needLogInvokeSource = true, bool bAddTime = true, bool bSave = true)
        {
            if (!needLogInvokeSource)
            {
                LogAtUnityEditor(strLog, "#ff0000ff", LogErrorType.Error, bAddTime, bSave);
                return;
            }

            strLog = UnityColorString(strLog, "#ff0000ff");

            // 还是比较消耗性能的
            StackFrame[] frames = new StackTrace().GetFrames();
            if (frames == null || frames.Length == 0)
                return;
            StringBuilder sb = new StringBuilder(strLog);
            sb.Append("\r\n");
            for (int i = 0; i < frames.Length; i++)
            {
                sb.AppendFormat("调用者[{0}]\r\n", frames[i].GetMethod().ReflectedType.Name);
            }

            LogAtUnityEditorWithOutRichText(sb.ToString(), LogErrorType.Error, bAddTime, bSave);
            return;


            var invokeInfo = StackTraceUtility.ExtractStackTrace();
            LogAtUnityEditor($"{strLog} \n{invokeInfo}", "#ff0000ff", LogErrorType.Error, bAddTime, bSave);
        }


        static string UnityColorString(string strLog, string color)
        {
            if (string.IsNullOrEmpty(strLog))
                return null;
            return string.Format("<color={0}>{1}</color>", color, strLog);
        }


        static string AdjustStrLogAtErrorType(string strLog, LogErrorType LogErrorType, LogPlatform logPlatform = LogPlatform.UnityDebug)
        {
            if (string.IsNullOrEmpty(strLog))
                return null;

            strLog = string.Format("[ErrorType: {0}] {1}", LogErrorType, strLog);


            return strLog;
        }


    }


    /// <summary>Log平台</summary>
    public enum LogPlatform
    {
        /// <summary>使用Unity的Debug</summary>
        UnityDebug,

        /// <summary>使用控制台的打印</summary>
        Console,
    }



    /// <summary>Log错误类型</summary>
    public enum LogErrorType
    {
        /// <summary>错误</summary>
        Error,

        /// <summary>警告</summary>
        Waring,

        /// <summary>正常</summary>
        Normal,
    }

}
