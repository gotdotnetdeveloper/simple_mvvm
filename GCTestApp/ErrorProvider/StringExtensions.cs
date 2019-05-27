using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTestApp.ErrorProvider
{
    /// <summary>
    /// Defines extension methods for the <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        #region Fields

        private static readonly string[] _roman;
        private static readonly int[] _arabic;
        private static readonly Dictionary<string, bool> ExcludedWordsDict;
        private static readonly char[] FioSeparators;

        #endregion

        #region .ctor

        static StringExtensions()
        {
            string[] excludedWords = {
                "ван", "д", "да", "де", "дер", "ла", "фон", "эль", "аль",
                "эд", "аш", "зуль", "паша", "бей",
                "чеди", "фу", "хуа"
            };

            string[] excludedWordsОтчество = {
                "оглы", "заде", "кызы"
            };

            ExcludedWordsDict =
                excludedWords.Select(e => new KeyValuePair<string, bool>(e, false))
                    .Concat(excludedWordsОтчество.Select(e => new KeyValuePair<string, bool>(e, true)))
                    .ToDictionary(e => e.Key, e => e.Value, StringComparer.CurrentCultureIgnoreCase);

            FioSeparators = " -.'`".OrderBy(e => e).ToArray();

            _roman = new[] { "I", "IV", "V", "IX", "X", "XL", "L", "XC", "C", "CD", "D", "CM", "M" };

            _arabic = new[] { 1, 4, 5, 9, 10, 40, 50, 90, 100, 400, 500, 900, 1000 };
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

  

     

        /// <summary>
        /// Extension shortcut to String.IsNullOrEmpty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Extension shortcut to String.IsNullOrEmpty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string value, int length)
        {
            if (length < 0)
                throw new ArgumentException(@"Length Must Be > 0.", "length");

            return value == null || value.Length < length ? value : value.Substring(0, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string value, int length)
        {
            if (length < 0)
                throw new ArgumentException(@"Length Must Be > 0.", "length");

            return value == null || value.Length < length ? value : value.Substring(value.Length - length, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Mid(this string value, int startIndex, int length)
        {
            if (startIndex < 0)
                throw new ArgumentException(@"StartIndex Must Be > 0.", "startIndex");

            if (length < 0)
                throw new ArgumentException(@"Length Must Be > 0.", "length");

            return value.Substring(startIndex, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string Mid(this string value, int startIndex)
        {
            if (startIndex < 0)
                throw new ArgumentException(@"StartIndex Must Be > 0.", "startIndex");

            return value.Substring(startIndex);
        }

        /// <summary>
        /// Усекает строку до указанной длины. При усечении дополняет многоточиями
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string TrimWithPeriods(this string s, int length)
        {
            if (length < 3)
                throw new ArgumentException(@"Length Must Be > 3.", "length");

            if (s == null || s.Length < length)
                return s;

            return s.Left(length - 3) + "...";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delimiter"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string JoinNotEmpty(this string delimiter, params string[] values)
        {
            return delimiter.JoinNotEmpty((IEnumerable<string>)values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delimiter"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string JoinNotEmpty(this string delimiter, IEnumerable<string> values)
        {
            var sb = new StringBuilder();

            foreach (var s in values.Where(s => !s.IsNullOrEmpty()))
            {
                (sb.Length == 0 ? sb : sb.Append(delimiter)).Append(s);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Возвращает null, если строка пустая или пробельная
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string NullIfWhiteSpace(this string s)
        {
            return (string.IsNullOrWhiteSpace(s)) ? null : s;
        }

        /// <summary>
        /// Get string with first letter in upper case.
        /// </summary>
        /// <param name="value">
        /// The original string.
        /// </param>
        /// <returns>
        /// String with first letter in upper case.
        /// </returns>
        public static string FirstLetterInUpperCase(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return string.Concat(value.Substring(0, 1).ToUpper(), value.Substring(1, value.Length - 1));
            return value;
        }

  

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EachLetterInUpperCase(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.ToUpper();
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ReplaceЁ(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Replace('ё', 'е').Replace('Ё', 'Е');
            return value;
        }

      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SafeToNullIfEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            return value;
        }

        /// <summary>
        /// Удаляет повторения символа
        /// </summary>
        /// <param name="value"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public static string SafeRemoveRepeatingCharacter(this string value, char character)
        {
            if (value == null)
                return null;

            var sb = new StringBuilder();

            Char? previousChar = null;
            foreach (var currentChar in value)
            {
                if (previousChar == character && currentChar == character)
                    continue;
                sb.Append(currentChar);
                previousChar = currentChar;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Безопасное преобразование в <see cref="char"/>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static char? SafeChar(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? default(char?) : str[0];
        }

        /// <summary>
        /// Безопасное получение длины строки
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int SafeLength(this string str)
        {
            return str == null ? 0 : str.Length;
        }

        /// <summary>
        /// Безопасное получение левых символов
        /// </summary>
        /// <param name="str"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string SafeLeft(this string str, int i)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            str = str.Trim();
            return str.Substring(0, Math.Min(str.Length, i));
        }

        /// <summary>
        /// Безопасное получение правых символов
        /// </summary>
        /// <param name="str"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string SafeRight(this string str, int i)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            str = str.Trim();
            return str.Remove(0, str.Length - Math.Min(str.Length, i));
        }

        /// <summary>
        /// Безопасное получение подстроки
        /// </summary>
        /// <param name="str"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string SafeSubstring(this string str, int start, int count)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            str = str.Trim();
            if (str.Length < start)
                return null;
            if (str.Length < (start + count))
                return str.Substring(start, str.Length - start);
            return str.Substring(start, count);
        }

        /// <summary>
        /// Безопасный разбор строки в <see cref="long"/>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long? SafeParseToLong(this string str)
        {
            long res;
            if (
                !string.IsNullOrWhiteSpace(str)
                && long.TryParse(str, out res)
                )
            {
                return res;
            }
            return default(long?);
        }

        /// <summary>
        /// Безопасный разбор строки в <see cref="int"/>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int? SafeParseToInt(this string str)
        {
            int res;
            if (
                !string.IsNullOrWhiteSpace(str)
                && int.TryParse(str, out res)
                )
            {
                return res;
            }
            return default(int?);
        }
        /// <summary>
        /// Безопасное удаление пробельных символов
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SafeTrim(this string str)
        {
            return str == null ? null : str.Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SafeToUpper(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str.ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SafeToLower(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str.ToLower();
        }

    

        /// <summary>
        /// Аналог <see cref="string"/>.<see cref="string.PadLeft(int, char)"/>. Если строка длиннее <paramref name="totalWidth"/>, она будет усечена до указанной длины. Если <paramref name="str"/> == <c>null</c>, то вернется строка из <paramref name="paddingChar"/> длиной <paramref name="totalWidth"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="totalWidth"></param>
        /// <param name="paddingChar"></param>
        public static string PadLeftTruncate(this string str, int totalWidth, char paddingChar)
        {
            if (str == null)
                return new string(paddingChar, totalWidth);
            if (str.Length > totalWidth)
                return str.Left(totalWidth);
            return str.PadLeft(totalWidth, paddingChar);
        }

        /// <summary>
        /// Аналог <see cref="string"/>.<see cref="string.PadRight(int, char)"/>. Если строка длиннее <paramref name="totalWidth"/>, она будет усечена до указанной длины. Если <paramref name="str"/> == <c>null</c>, то вернется строка из <paramref name="paddingChar"/> длиной <paramref name="totalWidth"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="totalWidth"></param>
        /// <param name="paddingChar"></param>
        public static string PadRightTruncate(this string str, int totalWidth, char paddingChar)
        {
            if (str == null)
                return new string(paddingChar, totalWidth);
            if (str.Length > totalWidth)
                return str.Left(totalWidth);
            return str.PadRight(totalWidth, paddingChar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitLines(this string value)
        {
            if (value == null)
                yield break;
            var l = value.Length;
            var sb = new StringBuilder();
            for (var i = 0; i < l; i++)
            {
                var ch = value[i];
                switch (ch)
                {
                    case '\r':
                        yield return sb.ToString();
                        sb.Clear();
                        if (i < (l - 1) && value[i + 1] == '\n')
                        {
                            i++;
                        }
                        break;
                    case '\n':
                        yield return sb.ToString();
                        sb.Clear();
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            yield return sb.ToString();
        }

        /// <summary>
        /// Переводит римские цифры в арабские
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int? FromRomanToArabic(this string s)
        {
            if (s.IsNullOrWhiteSpace())
                return null;
            var arabic = 0;
            var p = 0;
            while (p < s.Length)
            {
                var i = _roman.Length - 1;
                while ((s + " ").Substring(p, _roman[i].Length) != _roman[i])
                {
                    i--;
                    if (i < 0) return arabic;
                }
                arabic += _arabic[i];
                p += _roman[i].Length;

            }
            return arabic;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string StringJoin<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="replaceValue"></param>
        /// <returns></returns>
        public static string IfNullOrEmpty(this string source, string replaceValue)
        {
            if (string.IsNullOrEmpty(source))
                return replaceValue;
            return source;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetLast(this string source, int length)
        {
            if (source.IsNullOrEmpty()) return String.Empty;

            if (length >= source.Length)
                return source;

            return source.Substring(source.Length - length);
        }


        /// <summary>
        /// вычисляет длину строки исключая символы в exceptCharList в начале и конце
        /// </summary>
        /// <param name="str"></param>
        /// <param name="exceptCharList"> Unicode character array except List</param>
        /// <returns></returns>
        public static int SafeTrimLength(this string str, string exceptCharList)
        {
            if (str == null)
                return 0;

            var exceptChar = exceptCharList.ToCharArray();
            var стартоваяПозиция = 0;
            var конечнаяПозиция = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (!exceptChar.Contains(str[i]))
                {
                    стартоваяПозиция = i;
                    break;
                }
            }

            for (int i = str.Length - 1; i >= 0; i--)
            {
                if (!exceptChar.Contains(str[i]))
                {
                    конечнаяПозиция = i;
                    break;
                }
            }

            return конечнаяПозиция - стартоваяПозиция + 1;
        }

        #endregion

        #region Nested types

   

        #endregion
    }
}
