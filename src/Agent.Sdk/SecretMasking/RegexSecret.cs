// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Services.Agent.Util;

namespace Agent.Sdk.SecretMasking;

internal sealed class RegexSecret : ISecret
{
    public RegexSecret(String pattern,
                       ISet<string> sniffLiterals = null,
                       RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.ExplicitCapture)
    {
        ArgUtil.NotNullOrEmpty(pattern, nameof(pattern));

        m_pattern = pattern;
        m_regexOptions = regexOptions;
        m_sniffLiterals = sniffLiterals;
        m_regex = new Regex(pattern, regexOptions);
    }

    public override Boolean Equals(Object obj)
    {
        var item = obj as RegexSecret;
        if (item == null)
        {
            return false;
        }

        if (!String.Equals(m_pattern, item.m_pattern, StringComparison.Ordinal) ||
            m_regexOptions != item.m_regexOptions)
        {
            return false;
        }

        if (m_sniffLiterals == null && item.m_sniffLiterals == null)
        {
            return true;
        }

        m_sniffLiterals.Equals(item.m_sniffLiterals);

        if (m_sniffLiterals.Count !=  item.m_sniffLiterals.Count)
        {
            return false; 
        }

        foreach(string sniffLiteral in m_sniffLiterals) 
        { 
            if (!item.m_sniffLiterals.Contains(sniffLiteral))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        int result = 17;
        unchecked
        {
            result = (result * 31) + m_pattern.GetHashCode();
            result = (result * 31) + m_regexOptions.GetHashCode();

            // Use xor for set values to be order-independent.
            if (m_sniffLiterals != null)
            {
                int xor_0 = 0;
                foreach (var sniffLiteral in m_sniffLiterals)
                {
                    xor_0 ^= sniffLiteral.GetHashCode();
                }
                result = (result * 31) + xor_0;
            }
        }

        return result;
    }

    public IEnumerable<Replacement> GetPositions(String input)
    {
        bool runRegexes = m_sniffLiterals == null ? true : false;

        if (m_sniffLiterals != null)
        {
            foreach (string sniffLiteral in m_sniffLiterals)
            {
                if (input.IndexOf(sniffLiteral, StringComparison.Ordinal) != -1)
                {
                    runRegexes = true;
                    break;
                }
            }
        }

        if (runRegexes)
        {
            Int32 startIndex = 0;
            while (startIndex < input.Length)
            {
                var match = m_regex.Match(input, startIndex);
                if (match.Success)
                {
                    startIndex = match.Index + 1;
                    yield return new Replacement(match.Index, match.Length, "+++");
                }
                else
                {
                    yield break;
                }
            }
        }
    }

    public string Pattern { get { return m_pattern; } }
    private readonly ISet<string> m_sniffLiterals;
    private readonly RegexOptions m_regexOptions;
    private readonly String m_pattern;
    private readonly Regex m_regex;
}