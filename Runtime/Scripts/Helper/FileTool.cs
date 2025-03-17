using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;

namespace Business
{

    /// <summary>
    /// 文件操作
    /// </summary>
    public class FileTool
    {

        public static Encoding EncodingUtf8WithBom = new UTF8Encoding(true);
        public static Encoding EncodingUtf8WithOutBom = new UTF8Encoding(false);


        #region 查找文件

        /// <summary>尝试获取从指定路径FileSystemInfo</summary>
        /// <param name="strPathFull"></param>
        /// <returns></returns>
        public static FileSystemInfo GetFileSystemInfo(string strPathFull)
        {
            if (string.IsNullOrEmpty(strPathFull))
                return null;

            if (Directory.Exists(strPathFull))
            {
                return new DirectoryInfo(strPathFull);
            }
            else if (File.Exists(strPathFull))
            {
                return new FileInfo(strPathFull);
            }

            return null;
        }



        /// <summary>
        /// 获取指定文件夹下的文件
        /// </summary>
        /// <param name="strDirectoryPath">指定文件夹的绝对路径</param>
        /// <param name="searchOption"></param>
        /// <param name="strKuoZhanMing">指定扩展名限制(空 表示不限制扩展名)</param>
        /// <returns></returns>
        public static FileInfo[] GetFileInfoAtDirectory(string strDirectoryPath, SearchOption searchOption = SearchOption.TopDirectoryOnly
        , params string[] strKuoZhanMing)
        {
            if (string.IsNullOrEmpty(strDirectoryPath) || !Directory.Exists(strDirectoryPath))
            {
                //该文件夹不存在
                return null;
            }

            DirectoryInfo d = new DirectoryInfo(strDirectoryPath);
            //获取该文件夹下的所有文件
            if (strKuoZhanMing == null || strKuoZhanMing.Length == 0)
            {
                return d.GetFiles("*", searchOption);
            }
            else
            {
                List<FileInfo> fileList = new List<FileInfo>();
                for (int i = 0; i < strKuoZhanMing.Length; i++)
                {
                    fileList.AddRange(d.GetFiles("*" + strKuoZhanMing[i], searchOption));
                }
                return fileList.ToArray();
            }
        }



        /// <summary>
        /// 获取指定文件夹下的文件的路径
        /// </summary>
        /// <param name="strDirectoryPath">指定文件夹的绝对路径</param>
        /// <param name="searchOption"></param>
        /// <param name="strKuoZhanMing">指定扩展名限制(空 表示不限制扩展名)</param>
        /// <returns></returns>
        public static string[] GetFilesPathAtDirectory(string strDirectoryPath, SearchOption searchOption = SearchOption.TopDirectoryOnly
        , params string[] strKuoZhanMing)
        {
            if (string.IsNullOrEmpty(strDirectoryPath) || !Directory.Exists(strDirectoryPath))
            {
                //该文件夹不存在
                return null;
            }

            //获取该文件夹下的所有文件
            if (strKuoZhanMing == null || strKuoZhanMing.Length == 0)
            {
                return Directory.GetFiles(strDirectoryPath, "*", searchOption);
            }
            else
            {
                List<string> fileList = new List<string>();
                for (int i = 0; i < strKuoZhanMing.Length; i++)
                {
                    fileList.AddRange(Directory.GetFiles(strDirectoryPath, "*" + strKuoZhanMing[i], searchOption));
                }
                return fileList.ToArray();
            }
        }


        /// <summary>获取指定路径下的第一层文件信息(包含文件和文件夹)</summary>
        /// <param name="strDirectoryPath"></param>
        /// <returns></returns>
        public static FileSystemInfo[] GetFileSystemInfoAtDirectory(string strDirectoryPath)
        {
            if (!Directory.Exists(strDirectoryPath))
                return null;

            DirectoryInfo di = new DirectoryInfo(strDirectoryPath);
            return di.GetFileSystemInfos();
        }

        /// <summary>判定指定文件夹是否为空文件夹</summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool isEmptyDirectory(DirectoryInfo info)
        {
            if (info == null)
                return true;

            var fs = info.GetFileSystemInfos();
            if (fs == null)
                return true;

            return false;
        }


        /// <summary>判定指定文件夹是否为空文件夹</summary>
        /// <param name="strDirectoryPath">文件夹路径</param>
        /// <returns></returns>
        public static bool isEmptyDirectory(string strDirectoryPath)
        {
            if (strDirectoryPath == null)
                return true;

            var fs = GetFileSystemInfoAtDirectory(strDirectoryPath);
            if (fs == null)
                return true;

            return false;
        }

        #endregion




        #region TODO Endcoding 
        // public static Encoding TestGetFileEncodeIs(string strFileFullPath)
        // {
        //     if (strFileFullPath.IsNullOrEmpty() || !FileExist(strFileFullPath))
        //     {
        //         return null;
        //     }

        //     Encoding defaultEncodingIfNoBom = Encoding.UTF8;
        //     using (var reader = new StreamReader(strFileFullPath, true))
        //     {
        //         reader.Peek(); // you need this!
        //         var encoding = reader.CurrentEncoding;
        //         Log.LogAtUnityEditor($"{strFileFullPath}.encoding = {encoding}");
        //         return encoding;
        //     }
        // }

        #endregion




        #region R/W

        #region 写入文件
        /// <summary>
        /// 使用FileStream将指定字符串以指定编码写入指定文件()
        /// </summary>
        /// <param name="strPath">指定文件路径</param>
        /// <param name="data">待写入的数据</param>
        /// <param name="CallBackOnFinsihWrite">写入完毕回调</param>
        /// <param name="encoding">写入时采用的编码格式</param>
        /// <param name="bAppend">是否是续写</param>
        public static void WriteStringToFileByFileStream(string strPath, string data, Encoding encoding, Action CallBackOnFinsihWrite = null, bool bAppend = false)
        {
            if (string.IsNullOrEmpty(strPath))
                return;

            EnsureDirectoryExist(strPath);

            FileStream fs = new FileStream(strPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            if (fs == null)
                return;

            //处理续写
            if (!bAppend)
            {
                fs.SetLength(0);
            }
            else
            {
                fs.Position = fs.Length;
            }


            StreamWriter sw = new StreamWriter(fs, encoding);
            sw.Write(data);
            sw.Flush();
            sw.Close();
            fs.Close();
            if (CallBackOnFinsihWrite != null)
            {
                CallBackOnFinsihWrite.Invoke();
            }
        }




        /// <summary>
        /// 使用StreamWriter将指定字符串以指定编码写入指定文件()
        /// </summary>
        /// <param name="strPath">指定文件路径</param>
        /// <param name="data">待写入的数据</param>
        /// <param name="CallBackOnFinsihWrite">写入完毕回调</param>
        /// <param name="encoding">写入时采用的编码格式</param>
        /// <param name="bAppend">是否是续写</param>
        public static void WriteStringToFileByStreamWriter(string strPath, string data, Encoding encoding, Action CallBackOnFinsihWrite = null, bool bAppend = false)
        {
            if (string.IsNullOrEmpty(strPath))
                return;

            EnsureDirectoryExist(strPath);
            StreamWriter sw = new StreamWriter(strPath, bAppend, encoding);
            sw.Write(data);
            sw.Flush();
            sw.Close();
            if (CallBackOnFinsihWrite != null)
            {
                CallBackOnFinsihWrite.Invoke();
            }
        }



        /// <summary>将字节数据写入指定文件</summary>
        /// <param name="outFilePath"></param>
        /// <param name="data"></param>
        public static void WriteBytesToFileByBinary(string outFilePath, byte[] data, bool withOutExtensionIsDirectory = false)
        {
            EnsureDirectoryExist(outFilePath, withOutExtensionIsDirectory);
            FileStream fs = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(data);
            bw.Flush();
            bw.Close();
            fs.Close();
        }


        /// <summary>
        /// 在指定文件的头部写入字节数据
        /// </summary>
        /// <param name="outFilePath">文件路径</param>
        /// <param name="data">待写入的数据</param>
        /// <param name="withOutExtensionIsDirectory"></param>
        public static void WriteBytesToFileHeadByBinary(string outFilePath, byte[] data, bool withOutExtensionIsDirectory = false)
        {
            EnsureDirectoryExist(outFilePath, withOutExtensionIsDirectory);
            FileStream fs = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            //读取文件原始内容
            BinaryReader reader = new BinaryReader(fs);
            var originData = reader.ReadBytes((int)fs.Length);
            fs.Position = 0;

            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(data);
            bw.Write(originData);
            bw.Flush();
            bw.Close();
            fs.Close();
        }



        #endregion



        #region 读取文件
        /// <summary>读取文件的字符串</summary>
        /// <param name="strPath">指定文件的完整路径</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="CallBackOnFinishRead">读取完毕后的回调</param>
        /// <param name="detectEncodingFromByteOrderMarks"></param>
        /// <returns></returns>
        public static string ReadFile(string strPath, Encoding encoding, Action CallBackOnFinishRead, bool detectEncodingFromByteOrderMarks = true)
        {
            if (!File.Exists(strPath))
                return null;

            StreamReader sr = new StreamReader(strPath, encoding, detectEncodingFromByteOrderMarks);
            string data = sr.ReadToEnd();
            sr.Close();

            if (CallBackOnFinishRead != null)
                CallBackOnFinishRead.Invoke();

            return data;
        }


        /// <summary>读取文件的字符串</summary>
        /// <param name="strPath">指定文件的完整路径</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="CallBackOnFinishRead">读取完毕后的回调</param>
        /// <param name="detectEncodingFromByteOrderMarks"></param>
        /// <returns></returns>
        public static List<string> ReadFileAllLines(string strPath, Encoding encoding, Action CallBackOnFinishRead, bool detectEncodingFromByteOrderMarks = true)
        {
            if (!File.Exists(strPath))
                return null;

            StreamReader sr = new StreamReader(strPath, encoding, detectEncodingFromByteOrderMarks);
            List<string> data = new List<string>(5);
            while (true)
            {
                string strLine = sr.ReadLine();
                if (string.IsNullOrEmpty(strLine))
                {
                    continue;
                }

                using (StreamReader reader = new StreamReader(strPath, encoding, detectEncodingFromByteOrderMarks))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        data.Add(line);
                    }
                }

                if (CallBackOnFinishRead != null)
                {
                    CallBackOnFinishRead.Invoke();
                }

                return data;
            }
        }


        public static string[] ReadAllLinesByFile(string strPath, Encoding encoding, Action<string[]> onFinish)
        {
            if (!File.Exists(strPath))
                return null;

            var data = File.ReadAllLines(strPath, encoding);

            onFinish?.Invoke(data);
            return data;
        }


        /// <summary>读取文件指定行的字符串</summary>
        /// <param name="strPath">指定文件的完整路径</param>
        /// <param name="Line">指定要读取的行(从1开始)</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="CallBackOnFinishRead">读取完毕后的回调</param>
        /// <param name="detectEncodingFromByteOrderMarks"></param>
        /// <returns></returns>
        public static string ReadFileLine(string strPath, int Line, Encoding encoding, Action CallBackOnFinishRead, bool detectEncodingFromByteOrderMarks = true)
        {
            if (!File.Exists(strPath) || Line < 0)
                return null;

            StreamReader sr = new StreamReader(strPath, encoding, detectEncodingFromByteOrderMarks);
            List<string> data = new List<string>(5);
            int row = 1;
            string strLine = null;
            while (true)
            {
                strLine = sr.ReadLine();
                if (string.IsNullOrEmpty(strLine))
                    break;

                if (row == Line)
                    return strLine;

                row++;
            }

            sr.Close();

            if (CallBackOnFinishRead != null)
                CallBackOnFinishRead.Invoke();

            return strLine;
        }


        /// <summary>读取指定文件的字节</summary>
        /// <param name="strPath">指定文件的路径</param>
        /// <returns></returns>
        public static byte[] ReadFileBytes(string strPath)
        {
            if (!FileExist(strPath))
                return null;

            FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();
            return data;
        }


        #endregion

        #endregion



        /// <summary>准备一个文件流</summary>
        /// <param name="strFilePath">指定路径</param>
        /// <param name="FileMode">文件打开模式</param>
        /// <param name="FileAccess">文件操作类型</param>
        /// <param name="bAppend">在尾部添加内容/将已存在文件内容清空</param>
        /// <returns></returns>
        public static Stream ReadyStream(string strFilePath, FileMode FileMode = FileMode.OpenOrCreate, FileAccess FileAccess = FileAccess.ReadWrite, bool bAppend = false)
        {
            if (IsErrorPath(strFilePath))
                return null;

            FileStream fs = new FileStream(strFilePath, FileMode, FileAccess);

            //处理续写
            if (FileAccess.Write == FileAccess && !bAppend)
            {
                if (bAppend)
                {
                    fs.Position = fs.Length;
                }
                else
                {
                    fs.SetLength(0);
                }
            }
            return fs;
        }

        /// <summary>》》》基于Stream同步执行指定的Action 执行Action后会执行 flush()、close()</summary>
        /// <param name="stream"></param>
        /// <param name="action"></param>
        /// <param name="arg"></param>
        //[UnFinishedMethod]
        public static void SteamAction(Stream stream, Action<Stream, object> action, object arg = null)
        {
            if (null == stream || null == action)
                return;

            action(stream, arg);
            stream.Flush();
            stream.Close();
        }


        #region FileSafe

        /// <summary>》》》判断指定路径是否是不可用的路径(空路径、非法路径、路径起始错误......)</summary>
        /// <param name="strPath">待检测的路径</param>
        /// <returns></returns>
        public static bool IsErrorPath(string strPath)
        {
            return string.IsNullOrEmpty(strPath);
        }


        /// <summary>
        /// 确保指定路径所在的文件夹存在,这里可以直接指定到文件的Fullpath，该文件的所在的文件夹以及上层的文件夹不存在，都会进行创建
        /// </summary>
        /// <param name="strPath">指定路径</param>
        /// <param name="withOutExtensionIsDirectory">没有扩展名的视为文件夹进行处理,避免"Fold/2/3/4"中的4被当做文件处理</param>
        public static void EnsureDirectoryExist(string strPath, bool withOutExtensionIsDirectory = false)
        {
            if (string.IsNullOrEmpty(strPath))
                return;

            if (Path.HasExtension(strPath) || !withOutExtensionIsDirectory)
            {
                if (!Directory.Exists(Path.GetDirectoryName(strPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(strPath));
                }
            }
            else if (withOutExtensionIsDirectory)
            {
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }
            }
        }



        /// <summary>更安全更和谐的判定文件是否存在》》》待深入开发</summary>
        /// <param name="strPath">待检查的路径</param>
        /// <param name="RecsPathType">资源路径类型</param>
        /// <returns></returns>
        public static bool FileExist(string strPath, RecsPathType RecsPathType = RecsPathType.PhysicalFullPath)
        {
            return !(string.IsNullOrEmpty(strPath) || !File.Exists(strPath));
        }
        #endregion



        #region 文件扩展名 FileExtension

        /// <summary>移除指定路径的扩展名称</summary>
        /// <param name="strPath">指定路径</param>
        /// <returns></returns>
        public static string RemovePathExtension(string strPath)
        {
            return string.IsNullOrEmpty(strPath) ? null : strPath.Substring(0, strPath.LastIndexOf("."));
        }




        /// <summary>确保扩展名正确</summary>
        /// <param name="strPath">待处理的路径</param>
        /// <param name="strExtension">扩展名(带'.')</param>
        /// <returns></returns>
        public static string SetPathExtension(string strPath, string strExtension)
        {
            if (!Path.HasExtension(strPath))
            {
                strPath += strExtension;
            }
            else if (!Path.GetExtension(strPath).Equals(strExtension))
            {
                strPath = string.Concat(strPath.Substring(0, strPath.LastIndexOf('.')), strExtension);
            }

            return strPath;
        }

        #endregion




        #region UnityPath
        /// <summary>获取不带扩展名的文件名称/去除指定路径的扩展名</summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtension(string strPath)
        {
            if (string.IsNullOrEmpty(strPath))
                return null;

            return Path.GetFileNameWithoutExtension(strPath);
        }


        /// <summary>获取不带扩展名的文件路径/去除指定路径的扩展名</summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string GetFilePathWithoutExtension(string strPath)
        {
            if (string.IsNullOrEmpty(strPath))
                return null;
            int index = strPath.LastIndexOf('.');
            if (strPath.Length > index && index > 0)
                return strPath.Remove(strPath.LastIndexOf('.'));
            else
                return strPath;
        }



        /// <summary>"Assets/..."→"完整路径"</summary>
        /// <param name="strAssetPath"></param>
        /// <returns></returns>
        public static string AssetPathToFullPath(string strAssetPath)
        {
            return string.IsNullOrEmpty(strAssetPath) ? null : Application.dataPath + strAssetPath.Substring("Assets".Length);
        }


        /// <summary>"Asset/Recs/..."       →      "Recs/..."</summary>
        /// <param name="strAssetPath"></param>
        /// <returns></returns>
        public static string AssetPathToAssetCutPath(string strAssetPath)
        {
            if (string.IsNullOrEmpty(strAssetPath))
                return null;

            if (strAssetPath.Length < 7)
                return strAssetPath;

            return strAssetPath.Substring(7);
        }


        /// <summary>"(Assets下不包含'Asset/')..."      →     "完整路径"</summary>
        /// <param name="strAssetCutPath"></param>
        /// <returns></returns>
        public static string AssetCutPathToFullPath(string strAssetCutPath)
        {
            return string.IsNullOrEmpty(strAssetCutPath) ? null : string.Format("{0}/{1}", Application.dataPath, strAssetCutPath);
        }


        /// <summary>"完整路径" → "(Assets/)Recs/..."</summary>
        /// <param name="strFullPath"></param>
        /// <returns></returns>
        public static string FullPathToAssetCutPath(string strFullPath)
        {
            if (string.IsNullOrEmpty(strFullPath))
                return null;

            if (strFullPath.Length < Application.dataPath.Length + 1)
                return strFullPath;

            string str = strFullPath.Substring(Application.dataPath.Length + 1);
            return str;
        }

        /// <summary></summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string ToResourcesPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath))
                return null;

            //剔除扩展名
            strPath = GetFilePathWithoutExtension(strPath);

            return strPath.Substring(strPath.LastIndexOf(strResourcesHead) + strResourcesHead.Length + 1);
        }


        /// <summary>将一个字符串</summary>
        /// <param name="strPath">
        /// 将目标字符串在指定字符串后截断
        /// 示例：("Assets/Resources/Audio/Resources/123.mp3","Resources",false,false) → "/123.mp3"
        /// </param>
        /// <param name="strCut">截断参考的字符串</param>
        /// <param name="bCutAtCutStringHead">在截断参考字符串的头部/尾部进行截断</param>
        /// <param name="bCutAtHead">在真个字符串的头部/尾部进行截断</param>
        /// <returns></returns>
        public static string PathCutString(string strPath, string strCut, bool bCutAtCutStringHead = true, bool bCutAtHead = true)
        {
            if (string.IsNullOrEmpty(strPath))
                return null;

            //参考字符串的合法性检查
            if (string.IsNullOrEmpty(strCut))
                return strPath;

            //参考字符串长度 > 源字符串
            if (strPath.Length < strCut.Length)
                return strPath;

            int Index = 0;
            if (bCutAtHead)
                Index = strPath.IndexOf(strCut);
            else
                Index = strPath.LastIndexOf(strCut);

            return strPath.Substring(Index + (bCutAtCutStringHead ? 0 : strCut.Length));
        }


        /// <summary>完整路径→"Assets/..."</summary>
        /// <param name="strFullPath"></param>
        /// <returns></returns>
        public static string PhysicalFullPathToAssetPath(string strFullPath)
        {
            if (string.IsNullOrEmpty(strFullPath))
                return null;

            if (strFullPath.Length < Application.dataPath.Length)
                return strFullPath;

            string str = strFullPath.Substring(Application.dataPath.Length - 6);
            return str.Substring(str.IndexOf("Assets"));
        }

        /// <summary>完整路径→"(StreamingAssets/)Recs/..."</summary>
        /// <param name="strFullPath"></param>
        /// <returns></returns>
        public static string PhysicalFullPathToStreamingPath(string strFullPath)
        {
            if (string.IsNullOrEmpty(strFullPath))
                return null;

            if (strFullPath.Length < Application.streamingAssetsPath.Length)
                return strFullPath;

            string str = strFullPath.Substring(Application.streamingAssetsPath.Length + 1);
            return str;
        }


        /// <summary>"(StreamingAssets/)Recs/..." → 完整路径</summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string StreamingPathToPhysicalFullPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath))
                return null;

            return string.Format("{0}/{1}", Application.streamingAssetsPath, strPath);
        }





        /// <summary>"D:/Peojects/.../Asset/......"  →  "file://D:/Peojects/.../Asset/......"</summary>
        /// <param name="strFullPath"></param>
        /// <returns></returns>
        public static string FullPhysicalPathToPhysicalUrl(string strFullPath)
        {
            return string.Format("file://{0}", strFullPath);
        }


        /// <summary>"D:/Projects/.../Asset/......"  →  "file://D:/Peojects/.../Asset/......"</summary>
        /// <param name="strFullPath"></param>
        /// <returns></returns>
        public static string PhysicalUrlToFullPhysicalPath(string strFullPath)
        {
            return string.IsNullOrEmpty(strFullPath) ? null : (strFullPath.StartsWith(strFileUrlHead) ? strFullPath.Substring(strFileUrlHead.Length) : strFullPath);
        }


        /// <summary>将指定类型的路径转换为物理完整路径</summary>
        /// <param name="strPath">指定路径</param>
        /// <param name="from">指定路径类型</param>
        /// <returns></returns>
        public static string RecsPathToPhySicalFullPath(string strPath, RecsPathType from)
        {
            if (string.IsNullOrEmpty(strPath))
                return null;

            switch (from)
            {
                case RecsPathType.None:
                    break;
                case RecsPathType.PhysicalFullPath:
                    return strPath;

                case RecsPathType.AssetPath:
                    return AssetPathToFullPath(strPath);

                case RecsPathType.AssetCutPath:
                    return AssetCutPathToFullPath(strPath);

                case RecsPathType.ResourcesPath:

                    break;

                case RecsPathType.StreamingPath:
                    return StreamingPathToPhysicalFullPath(strPath);

                case RecsPathType.NetUrl:
                    break;

                case RecsPathType.PhysicalFullUrl:
                    return PhysicalUrlToFullPhysicalPath(strPath);

                case RecsPathType.Url:
                    break;
                default:
                    break;
            }

            return strPath;
        }



        /// <summary>将指定类型的路径转换为物理完整路径</summary>
        /// <param name="strPath">指定路径</param>
        /// <param name="to">指定路径类型</param>
        /// <returns></returns>
        public static string PhysicalFullPathToRecsPath(string strPath, RecsPathType to)
        {
            if (string.IsNullOrEmpty(strPath))
                return null;

            switch (to)
            {
                case RecsPathType.None:
                    break;
                case RecsPathType.PhysicalFullPath:
                    return strPath;

                case RecsPathType.AssetPath:
                    return PhysicalFullPathToAssetPath(strPath);

                case RecsPathType.AssetCutPath:
                    return FullPathToAssetCutPath(strPath);

                case RecsPathType.ResourcesPath:
                    return ToResourcesPath(strPath);


                case RecsPathType.StreamingPath:
                    return PhysicalFullPathToStreamingPath(strPath);

                case RecsPathType.NetUrl:
                    break;

                case RecsPathType.PhysicalFullUrl:
                    return FullPhysicalPathToPhysicalUrl(strPath);

                case RecsPathType.Url:
                    break;
                default:
                    break;
            }

            return strPath;
        }



        /// <summary>资源路径转换</summary>
        /// <param name="strPath">资源路径</param>
        /// <param name="from">资源路径类型</param>
        /// <param name="to">目标资源路径类型</param>
        /// <returns></returns>
        public static string RecsPathSwitch(string strPath, RecsPathType from, RecsPathType to)
        {
            if (string.IsNullOrEmpty(strPath))
                return null;

            string strFullPath = RecsPathToPhySicalFullPath(strPath, from);

            return PhysicalFullPathToRecsPath(strFullPath, to);
        }

        #endregion



        #region 路径判断

        /// <summary>判定指定路径是否是物理路径</summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static bool IsPhyscisPath(string strPath)
        {
            return (string.IsNullOrEmpty(strPath) || strPath.Length < 2) ? false : strPath[1] == ':';
        }


        /// <summary>判定指定路径是完整路径</summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static bool IsFullPath(string strPath)
        {
            return File.Exists(strPath);
        }


        /// <summary>判定指定路径是否是合法的Url(支持"File://","https|http|ftp|rtsp|mms")</summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static bool isUrlPath(string strPath)
        {
            return isFileUrl(strPath) || IsNetPath(strPath);
        }

        /// <summary></summary>
        public const string strResourcesHead = "Resources";

        /// <summary></summary>
        public const string strFileUrlHead = "file://";
        /// <summary>判定指定路径是否是合法的文件型Url"file://"</summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static bool isFileUrl(string strPath)
        {
            return (string.IsNullOrEmpty(strPath) || !strPath.StartsWith(strFileUrlHead)) ? false : File.Exists(strPath.Substring(strFileUrlHead.Length));
            //return (string.IsNullOrEmpty(strPath) || strPath.Length < 9) ? false : strPath.StartsWith(strFileUrlHead) && strPath[8] == ':';
        }

        /// <summary>判定指定路径是完整路径(支持"https|http|ftp|rtsp|mms")</summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static bool IsNetPath(string strPath)
        {
            return Regex.IsMatch(strPath, urlRegex);
        }

        /// <summary>Url合法性判断</summary>
        public const string urlRegex = "^((https|http|ftp|rtsp|mms)?://)"
                + "?(([0-9a-zA-Z_!~*'().&=+$%-]+: )?[0-9a-zA-Z_!~*'().&=+$%-]+@)?" //ftp的user@     
                + "(([0-9]{1,3}\\.){3}[0-9]{1,3}"                                 // IP形式的URL- 199.194.52.184     
                + "|"                                                         // 允许IP和DOMAIN（域名）     
                + "([0-9a-zA-Z_!~*'()-]+\\.)*"                                 // 域名- www.     
                + "([0-9a-zA-Z][0-9a-zA-Z-]{0,61})?[0-9a-zA-Z]\\."                     // 二级域名     
                + "[a-zA-Z]{2,6})"                                         // first level domain- .com or .museum     
                + "(:[0-9]{1,4})?"                                                     // 端口- :80     
                + "((/?)|"
                + "(/[0-9a-zA-Z_!~*'().;?:@&=+$,%#-]+)+/?)$";


        #endregion



    }


    /// <summary>资源路径类型</summary>
    public enum RecsPathType
    {
        /// <summary></summary>
        None = 0,

        /// <summary>物理完整路径"D:/Recs/..."</summary>
        PhysicalFullPath = 1 << 1,

        /// <summary>"Asset/Recs/abc/ad/..."</summary>
        AssetPath = 1 << 2,

        /// <summary>"Recs/abc/ad/..."</summary>
        AssetCutPath = 1 << 3,

        /// <summary>"[Resources]下没有后缀的路径"</summary>
        ResourcesPath = 1 << 4,

        /// <summary>"StreamingAssets/..."</summary>
        StreamingPath = 1 << 5,

        /// <summary>"www.13.123.4354.2354.Recs/dasd..."</summary>
        NetUrl = 1 << 6,

        /// <summary>物理完整路径"file://D:/Recs/..."</summary>
        PhysicalFullUrl = 1 << 7,

        /// <summary>"file://D:/Recs/..."或"www.13.123.4354.2354.Recs/dasd..."</summary>
        Url = 1 << 8,
    }

}
