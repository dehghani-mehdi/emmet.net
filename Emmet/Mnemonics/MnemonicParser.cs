﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Emmet.Mnemonics
{
    /// <summary>
    /// Parses mnemonics and provides its string representation.
    /// </summary>
    public static class MnemonicParser
    {
        private static readonly Regex _mnemonicTemplate = new Regex(
            @"(_|p|P|pi|i)([csvar]?)(\w{1,2})\??([pmf])",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Dictionary<string, string> _accessibilityLevels =
            new Dictionary<string, string>
        {
            { "_", "private" },
            { "p", "public" },
            { "P", "protected" },
            { "i", "internal" },
            { "pi", "protected internal" }
        };

        private static readonly Dictionary<string, string> _modifiers =
            new Dictionary<string, string>
        {
            { "c", "const" },
            { "s", "static" },
            { "v", "virtual" },
            { "a", "abstract" },
            { "r", "readonly" }
        };

        private static readonly Dictionary<string, string> _types =
            new Dictionary<string, string>
        {
            { "b", "bool" },
            { "by", "byte" },
            { "d", "double" },
            { "de", "decimal" },
            { "dt", "DateTime" },
            { "g", "Guid" },
            { "i", "int" },
            { "l", "long" },
            { "s", "string" },
            { "sh", "short" },
            { "u", "uint" },
            { "v", "void" }
        };

        public static bool TryParse(
            string mnemonic,
            string indent,
            out string memberDeclaration,
            out int cursorPosition)
        {
            memberDeclaration = null;
            cursorPosition = 0;

            Match match = _mnemonicTemplate.Match(mnemonic);
            if (!match.Success)
                return false;

            try
            {
                StringBuilder retVal = new StringBuilder(64);
                cursorPosition = BuildSnippet(retVal, match, mnemonic, indent);

                memberDeclaration = retVal.ToString();
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public static void MergeConfiguration(string configurationFile)
        {
            string[] lines = File.ReadAllLines(configurationFile);

            foreach (string line in lines)
            {
                string[] pair = line.Split('=');
                _types[pair[0]] = pair[1];
            }
        }

        private static int BuildSnippet(StringBuilder snippet, Match match, string mnemonic, string indent)
        {
            int caretPos = 0;

            string accessibilityLevel = _accessibilityLevels[match.Groups[1].Value];
            string modifier = match.Groups[2].Value.Length > 0 ? _modifiers[match.Groups[2].Value] : null;
            char memberType = match.Groups[4].Value[0];
            string returnValue = _types[match.Groups[3].Value];

            // Check whether we have a nullable type
            if (mnemonic.IndexOf('?') > 0)
                returnValue += '?';

            snippet.Append(accessibilityLevel);
            snippet.Append(' ');

            if (null != modifier)
            {
                snippet.Append(modifier);
                snippet.Append(' ');
            }

            snippet.Append(returnValue);

            switch (memberType)
            {
                case 'f':
                    caretPos = snippet.Length + 2 - mnemonic.Length;
                    snippet.Append(" _;");
                    break;
                case 'm':
                    caretPos = snippet.Length + 1 - mnemonic.Length;
                    snippet.AppendLine(" ()");
                    snippet.AppendLine(indent + "{");
                    snippet.Append(indent + "}");
                    break;
                case 'p':
                    caretPos = snippet.Length + 1 - mnemonic.Length;
                    snippet.Append("  { get; set; }");
                    break;
            }

            return caretPos;
        }
    }
}