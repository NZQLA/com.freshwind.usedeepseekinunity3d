using System;
using System.Text.RegularExpressions;
using Business;


namespace MD
{
    /// <summary>
    /// MarkDown周边
    /// </summary>
    public static class MarkDownHelper
    {
        public const string flagTitle = "#";
        public const string flagTitleEnd = "# ";
        public const string flagCodeLines = "```";
        public const string flagCodeInLine = "`";
        public const string flagLink = "[";
        public const string flagsListWithOutNumber = "-+*";

        // 修改正则表达式，添加对行首空格的捕获
        private static readonly Regex titleRegex = new Regex(@"(^|\n)(\s*)(#+)\s+(.*?)(?=\n|$)", RegexOptions.Compiled);
        private static readonly Regex boldRegex = new Regex(@"\*\*(.*?)\*\*", RegexOptions.Compiled);
        private static readonly Regex italicRegex = new Regex(@"[*_](.*?)[*_]", RegexOptions.Compiled);
        private static readonly Regex linkRegex = new Regex(@"\[(.*?)\]\((.*?)\)", RegexOptions.Compiled);
        private static readonly Regex listRegex = new Regex(@"(^|\n)(\s*)([-+*])\s+(.*?)(?=\n|$)", RegexOptions.Compiled);
        private static readonly Regex strikethroughRegex = new Regex(@"~~(.*?)~~", RegexOptions.Compiled);
        private static readonly Regex highlightRegex = new Regex(@"==(.*?)==", RegexOptions.Compiled);

        // 添加一个正则表达式来检测同时包含标题和列表的行
        private static readonly Regex titleAndListRegex = new Regex(@"(^|\n)(\s*)(#+)\s+(.*?)([-+*])\s+(.*?)(?=\n|$)", RegexOptions.Compiled);


        // 添加正则表达式来检测行内代码和多行代码
        private static readonly Regex inlineCodeRegex = new Regex(@"(?<!`)`([^`]*)`(?!`)", RegexOptions.Compiled);
        private static readonly Regex multilineCodeRegex = new Regex(@"```([\s\S]*?)```", RegexOptions.Compiled);

        /// <summary>
        /// Convert markdown to TextMeshPro format.
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static string ConvertMarkdownToTMP(this string markdown, MdTmpStyle style)
        {
            // 先添加一个换行符，确保最后一行也能被正确处理，最后会去掉多余的<br>
            string workingText = markdown + "\n";

            // 处理多行代码
            workingText = multilineCodeRegex.Replace(workingText, match =>
            {
                string codeContent = match.Groups[1].Value;
                return $"<mark={style.styleCode.colorLinesBg_}><color={style.styleCode.colorLines_}>{codeContent}</color></mark>";
            });

            // 处理行内代码
            workingText = inlineCodeRegex.Replace(workingText, match =>
            {
                string codeContent = match.Groups[1].Value;
                return $"<mark={style.styleCode.colorInLineBg_}><color={style.styleCode.colorInLine_}>{codeContent}</color></mark>";
            });


            // 处理同时包含标题和列表的行（如果存在）
            workingText = titleAndListRegex.Replace(workingText, match =>
            {
                string linePrefix = match.Groups[1].Value; // 换行符
                string indentation = match.Groups[2].Value; // 行首缩进
                int level = match.Groups[3].Value.Length - 1; // 标题级别
                if (level >= style.styleTitle.fontSize.Length)
                    level = style.styleTitle.fontSize.Length - 1;

                string titleContent = match.Groups[4].Value; // 标题内容
                string listMarker = match.Groups[5].Value; // 列表标记
                string listContent = match.Groups[6].Value; // 列表内容

                // 同时应用标题和列表样式
                return $"{linePrefix}{indentation}<size={style.styleTitle.fontSize[level]}><b>{titleContent}</b></size> • {listContent}";
            });

            // 处理标题行
            workingText = titleRegex.Replace(workingText, match =>
            {
                string linePrefix = match.Groups[1].Value; // 换行符
                string indentation = match.Groups[2].Value; // 行首缩进
                int level = match.Groups[3].Value.Length - 1;
                if (level >= style.styleTitle.fontSize.Length)
                    level = style.styleTitle.fontSize.Length - 1;

                string content = match.Groups[4].Value;
                // 保留行首缩进
                return $"{linePrefix}{indentation}<size={style.styleTitle.fontSize[level]}><b>{content}</b></size>";
            });

            // 处理列表项
            workingText = listRegex.Replace(workingText, match =>
            {
                string linePrefix = match.Groups[1].Value; // 换行符
                string indentation = match.Groups[2].Value; // 行首缩进
                string listMarker = match.Groups[3].Value; // 列表标记符号
                string content = match.Groups[4].Value; // 列表内容

                // 保留行首缩进
                return $"{linePrefix}{indentation}• {content}";
            });

            // 处理内联格式
            // 处理高亮
            workingText = highlightRegex.Replace(workingText, $"<mark={style.styleHighLight.color}>$1</mark>");
            // 加粗
            workingText = boldRegex.Replace(workingText, "<b>$1</b>");
            // 倾斜
            workingText = italicRegex.Replace(workingText, "<i>$1</i>");
            // 处理删除线
            workingText = strikethroughRegex.Replace(workingText, "<s>$1</s>");

            workingText = linkRegex.Replace(workingText, match =>
            {
                string text = match.Groups[1].Value;
                string url = match.Groups[2].Value;
                return $"<link=\"{url}\"><color={style.styleLink.color}>{text}</color></link>";
            });




            //// 替换换行符
            //workingText = workingText.Replace("\n", "<br>");

            //// 如果最后有添加的额外换行，去掉最后的<br>
            //if (markdown.EndsWith("\n") == false && workingText.EndsWith("<br>"))
            //{
            //    workingText = workingText.Substring(0, workingText.Length - 4);
            //}

            return workingText.Trim();
        }

        /// <summary>
        /// Is the line content a title line?
        /// </summary>
        /// <param name="lineContent">"## title\n"</param>
        /// <returns></returns>
        public static bool IsTitleLine(this string lineContent)
        {
            if (lineContent.IsNullOrEmpty())
            {
                return false;
            }
            return lineContent.StartsWith(flagTitle) && lineContent.IndexOf(flagTitle) < lineContent.Length;
        }


    }


    /// <summary>
    /// markdown 在 tmp 中的样式
    /// </summary>
    [Serializable]
    public class MdTmpStyle
    {
        public MdTmpStyleTitle styleTitle = new MdTmpStyleTitle();
        public MdTmpStyleCode styleCode = new MdTmpStyleCode();
        public MdTmpStyleLink styleLink = new MdTmpStyleLink();
        public MdTmpStyleHighLight styleHighLight = new MdTmpStyleHighLight();
    }


    /// <summary>
    /// markdown 在 tmp 中的样式(用于标题)
    /// </summary>
    [Serializable]
    public class MdTmpStyleTitle
    {
        //public int fontSizeH1 = 24;
        //public int fontSizeH2 = 20;
        //public int fontSizeH3 = 18;
        //public int fontSizeH4 = 16;
        //public int fontSizeH5 = 14;
        //public int fontSizeH6 = 12;
        public int[] fontSize = new int[] { 24, 20, 18, 16, 14, 12 };
        public bool bold = true;
        public string colorCode = "";

        /// <summary>
        /// Generate the md raw title line content to tmp , let it show like  md in TextMeshPro.
        /// </summary>
        /// <param name="titleOrigin">"### title content \n"</param>
        /// <returns></returns>
        public string GenerateTitle(string titleOrigin)
        {
            titleOrigin = titleOrigin.Trim();
            var titleStart = titleOrigin.IndexOf(" ");
            if (titleStart < 0)
            {
                return titleOrigin;
            }
            var titleContentPure = titleOrigin.Substring(titleStart);
            var titleType = titleStart - 1;


            if (!bold && colorCode.IsNullOrEmpty())
            {
                return $"<size={titleType}>{titleContentPure}</size>";
            }
            else if (bold && colorCode.IsNullOrEmpty())
            {
                return $"<size={titleType}><b>{titleContentPure}</b></size>";
            }
            else if (!bold && !colorCode.IsNullOrEmpty())
            {
                return $"<size={titleType}><color={colorCode}>{titleContentPure}</color></size>";
            }
            else
            {
                return $"<size={titleType}><b><color={colorCode}>{titleContentPure}</color></b></size>";
            }

        }

    }

    public enum TitleType
    {
        H1 = 1,
        H2 = 1 << 1,
        H3 = 1 << 2,
        H4 = 1 << 3,
        H5 = 1 << 4,
        H6 = 1 << 5,
    }

    /// <summary>
    /// markdown 在 tmp 中的样式(用于代码)
    /// </summary>
    [Serializable]
    public class MdTmpStyleCode
    {
        public string colorLinesBg_ = "#AAAAAA22";
        public string colorLines_ = "#4ec9b0";
        public string colorInLineBg_ = "#66666622";
        public string colorInLine_ = "#f48617";
        //public string format = "<color={0}>{1}</color>";
    }


    /// <summary>
    /// markdown 在 tmp 中的样式(用于代码)
    /// </summary>
    [Serializable]
    public class MdTmpStyleLink
    {
        public string color = "#00aacc";
        public bool underline = true;
        public string format = "<link=\"{0}\"><color={1}>{2}</color></link>";
    }


    [Serializable]
    public class MdTmpStyleHighLight
    {
        public string color = "#AAAA0022";
    }



}