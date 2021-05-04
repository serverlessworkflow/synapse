using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Synapse
{

    /// <summary>
    /// Defines extensions for <see cref="string"/>s
    /// </summary>
    public static class StringExtensions
    {

        private const string SubstitutionBlock = "§§";
        private static Regex ReduceSpaces = new Regex(@"\s+", RegexOptions.Compiled);
        private static Regex InvalidChars = new Regex(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled);
        private static Regex ReplaceSubstitutionBlock = new Regex(SubstitutionBlock, RegexOptions.Compiled);
        private static Regex MatchCurlyBracedWords = new Regex(@"\{([^}]+)\}", RegexOptions.Compiled);

        private static readonly string LowerCaseAlphabeticCharacters = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string UpperCaseAlphabeticCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string AlphabeticCharacters = LowerCaseAlphabeticCharacters + UpperCaseAlphabeticCharacters;
        private static readonly string NumericCharacters = "0123456789";
        private static readonly string AlphaNumericCharacters = AlphabeticCharacters + NumericCharacters;

        /// <summary>
        /// Generates a random alphabetic string of the specified length
        /// </summary>
        /// <param name="length">The length of the string to generate</param>
        /// <param name="stringCase">The case of the resulting string</param>
        /// <returns>A new random string of the specified length</returns>
        public static string GenerateRandomAlphabeticString(int length, Case stringCase = Case.Lower | Case.Upper)
        {
            char[] characters;
            switch (stringCase)
            {
                case Case.Lower | Case.Upper:
                    characters = LowerCaseAlphabeticCharacters.ToArray();
                    break;
                case Case.Lower:
                    characters = LowerCaseAlphabeticCharacters.ToArray();
                    break;
                case Case.Upper:
                    characters = UpperCaseAlphabeticCharacters.ToArray();
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(Case)} '{stringCase}' is not supported");
            }
            byte[] data = new byte[4 * length];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                uint rnd = BitConverter.ToUInt32(data, i * 4);
                long index = rnd % characters.Length;
                result.Append(characters[index]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Generates a random alphanumeric string of the specified length
        /// </summary>
        /// <param name="length">The length of the string to generate</param>
        /// <param name="stringCase">The case of the resulting string</param>
        /// <returns>A new random string of the specified length</returns>
        public static string GenerateRandomAlphanumericString(int length, Case stringCase = Case.Lower | Case.Upper)
        {
            char[] characters;
            switch (stringCase)
            {
                case Case.Lower | Case.Upper:
                    characters = (AlphabeticCharacters + NumericCharacters).ToArray();
                    break;
                case Case.Lower:
                    characters = (LowerCaseAlphabeticCharacters + NumericCharacters).ToArray();
                    break;
                case Case.Upper:
                    characters = (UpperCaseAlphabeticCharacters + NumericCharacters).ToArray();
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(Case)} '{stringCase}' is not supported");
            }
            byte[] data = new byte[4 * length];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                uint rnd = BitConverter.ToUInt32(data, i * 4);
                long index = rnd % characters.Length;
                result.Append(characters[index]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Generates a random numeric string of the specified length
        /// </summary>
        /// <param name="length">The length of the string to generate</param>
        /// <param name="stringCase">The case of the resulting string</param>
        /// <returns>A new random string of the specified length</returns>
        public static string GenerateRandomNumericString(int length, Case stringCase = Case.Lower | Case.Upper)
        {
            char[] characters = NumericCharacters.ToCharArray();
            byte[] data = new byte[4 * length];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                uint rnd = BitConverter.ToUInt32(data, i * 4);
                long index = rnd % characters.Length;
                result.Append(characters[index]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Checks that the string is alphabetic
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>A boolean indicating whether or not the string is alphabetic</returns>
        public static bool IsAlphabetic(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));
            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks that the string is numeric
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>A boolean indicating whether or not the string is numeric</returns>
        public static bool IsNumeric(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks that the string is alphanumeric
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>A boolean indicating whether or not the string is alphanumeric</returns>
        public static bool IsAlphanumeric(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));
            foreach (char c in text)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks that the string represents a numerical value
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>A boolean indicating whether or not the string represents a numerical value</returns>
        public static bool IsNumericalValue(this string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c) && c != '.')
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether or not the string is a valid email address
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>A boolean indicating whether or not the string is a valid email address</returns>
        public static bool IsValidEmail(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));
            try
            {
                text = Regex.Replace(text, @"(@)(.+)$", MapEmailDomain, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            try
            {
                return Regex.IsMatch(text, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether or not the string is a valid two-letter ISO 3166 country code
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>A boolean indicating whether or not the string is a valid two-letter ISO 3166 country code</returns>
        public static bool IsValidCountryCode(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));
            if (text.Length != 2 || !text.IsAlphabetic())
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether or not the string is a valid two-letter ISO 6391 language code
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>A boolean indicating whether or not the string is a valid two-letter ISO 6391 language code</returns>
        public static bool IsValidLanguageCode(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));
            if (text.Length != 2 || !text.IsAlphabetic())
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether or not the string is a valid three-letter ISO 4217 currency code
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <returns>A boolean indicating whether or not the string is a valid three-letter ISO 4217 currency code</returns>
        public static bool IsValidCurrencyCode(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            if (text.Length != 3 || !text.IsAlphabetic())
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether or not the string is a valid time zone id
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <returns>A boolean indicating whether or not the string is a valid time zone id</returns>
        public static bool IsValidTimeZoneId(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));
            TimeZoneInfo timeZone = null;
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(text);
            }
            catch
            {
                return false;
            }
            return timeZone != null;
        }

        /// <summary>
        /// Determines whether or not the string is a valid uri
        /// </summary>
        /// <param name="text">The string to check</param>
        /// <returns>A boolean indicating whether or not the string is a valid uri</returns>
        public static bool IsValidUri(this string text)
        {
            return Uri.IsWellFormedUriString(text, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Replaces the upper case characters by their lowercase counterpart and prepend them with a whitespace character
        /// </summary>
        /// <param name="text">The string to split</param>
        /// <param name="toLowerCase">A boolean indicating whether or not to lowercase the first character of each resulting word</param>
        /// <param name="keepFirstLetterInUpercase">A boolean indicating whether or not to keep the first letter in upper case</param>
        /// <returns>The resulting string</returns>
        public static string SplitCamelCase(this string text, bool toLowerCase = true, bool keepFirstLetterInUpercase = true)
        {
            string result = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                if (i == 0 && keepFirstLetterInUpercase)
                {
                    result += currentChar;
                    continue;
                }
                if (char.IsUpper(currentChar))
                {
                    if (i != 0 && (!char.IsUpper(text[i - 1]) || (i != text.Length - 1 && !char.IsUpper(text[i + 1]))))
                        result += " ";
                    if (toLowerCase && i != text.Length - 1 && !char.IsUpper(text[i + 1]))
                        result += char.ToLower(currentChar);
                    else
                        result += currentChar;
                }
                else
                {
                    result += currentChar;
                }
            }
            return result;
        }

        /// <summary>
        /// Removes diacritics from the string
        /// </summary>
        /// <param name="text">The string to remove diacritics from</param>
        /// <returns>The resulting string</returns>
        public static string RemoveDiacritics(this string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString()
                .Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Slugifies the string by replacing whitespace by the specified delimiter character, removing diacritics, making it lowercase and restricting it to the specified max length, if any
        /// </summary>
        /// <param name="text">The string to slugify</param>
        /// <param name="delimiter">The delimiter character. Defaults to '_'</param>
        /// <param name="splitCamelCase">A boolean indicating whether or not to split camel cases</param>
        /// <param name="maxLength">The resulting string's maximum length</param>
        /// <returns>The resulting string</returns>
        public static string Slugify(this string text, string delimiter = "_", bool splitCamelCase = false, int? maxLength = null)
        {
            string result;
            result = text.RemoveDiacritics();
            if (splitCamelCase)
                result = result.SplitCamelCase(true, false);
            else
                result = result.ToLower();
            result = InvalidChars.Replace(result, SubstitutionBlock).Trim();
            result = ReduceSpaces.Replace(result, " ");
            if (maxLength.HasValue)
                result = result.Substring(0, result.Length <= maxLength.Value ? result.Length : maxLength.Value).Trim();
            result = ReplaceSubstitutionBlock.Replace(result, delimiter);
            return result;
        }

        /// <summary>
        /// Formats the string
        /// </summary>
        /// <param name="text">The string to format</param>
        /// <param name="args">The arguments to format the string with</param>
        /// <remarks>Accepts named arguments, which will be replaced in sequence by the specified values</remarks>
        /// <returns>The resulting string</returns>
        public static string Format(this string text, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            string formattedText = text;
            List<string> matches = MatchCurlyBracedWords.Matches(text)
                .Select(m => m.Value)
                .Distinct()
                .ToList();
            for (int i = 0; i < matches.Count && i < args.Length; i++)
            {
                formattedText = formattedText.Replace(matches[i], args[i].ToString());
            }
            return formattedText;
        }

        /// <summary>
        /// Converts the specified text to a camel-cased string
        /// </summary>
        /// <param name="text">The text to convert</param>
        /// <returns>The camel-cased text</returns>
        public static string ToCamelCase(this string text)
        {
            return string.IsNullOrEmpty(text) || text.Length < 2 ? text.ToLowerInvariant() : char.ToLowerInvariant(text[0]) + text.Substring(1);
        }

        /// <summary>
        /// Converts the specified text to a dash-cased string
        /// </summary>
        /// <param name="text">The text to convert</param>
        /// <returns>The dash-cased text</returns>
        public static string ToDashCase(this string text)
        {
            return string
                .Concat(text.Select((x, i) => char.IsUpper(x) && i != 0 ? $"-{x}" : x.ToString()))
                .ToLower();
        }

        private static string MapEmailDomain(Match match)
        {
            IdnMapping idn = new IdnMapping();
            string domainName = idn.GetAscii(match.Groups[2].Value);
            return match.Groups[1].Value + domainName;
        }

    }

}
