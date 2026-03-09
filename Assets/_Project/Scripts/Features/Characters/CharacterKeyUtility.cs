using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shogun.Features.Characters
{
    public static class CharacterKeyUtility
    {
        public static string NormalizeCharacterId(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            StringBuilder builder = new StringBuilder(rawValue.Length);
            bool lastWasSeparator = false;

            foreach (char rawChar in rawValue.Trim().ToLowerInvariant())
            {
                if ((rawChar >= 'a' && rawChar <= 'z') || (rawChar >= '0' && rawChar <= '9'))
                {
                    builder.Append(rawChar);
                    lastWasSeparator = false;
                    continue;
                }

                if (rawChar == ' ' || rawChar == '_' || rawChar == '-')
                {
                    if (!lastWasSeparator && builder.Length > 0)
                    {
                        builder.Append('-');
                        lastWasSeparator = true;
                    }
                }
            }

            return builder.ToString().Trim('-');
        }

        public static string NormalizeLookupKey(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            StringBuilder builder = new StringBuilder(rawValue.Length);
            foreach (char rawChar in rawValue.Trim().ToLowerInvariant())
            {
                if ((rawChar >= 'a' && rawChar <= 'z') || (rawChar >= '0' && rawChar <= '9'))
                    builder.Append(rawChar);
            }

            return builder.ToString();
        }

        public static string[] NormalizeAliases(IEnumerable<string> aliases)
        {
            if (aliases == null)
                return System.Array.Empty<string>();

            return aliases
                .Select(NormalizeCharacterId)
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Distinct()
                .ToArray();
        }
    }
}
