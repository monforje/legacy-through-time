using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LegacyThroughTime.Narrative.Json
{
    /// <summary>
    /// Minimal allocation-conscious JSON reader producing a plain object tree:
    /// <see cref="Dictionary{TKey,TValue}"/> (string → object), <see cref="List{T}"/> of object,
    /// string, int, float, bool and null. Numbers without '.', 'e' or 'E' are ints — the
    /// same rule the ink format relies on to tell int values from float values.
    /// </summary>
    static class JsonReader
    {
        public static object Parse(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var p = new Parser(text);
            p.SkipWhitespace();
            var value = p.ReadValue();
            p.SkipWhitespace();
            if (!p.AtEnd) throw p.Fail("unexpected trailing content");
            return value;
        }

        struct Parser
        {
            readonly string _s;
            int _i;
            StringBuilder _sb;

            public Parser(string s)
            {
                _s = s;
                // Tolerate a UTF-8 BOM that survived decoding (inklecate writes one).
                _i = s.Length > 0 && s[0] == '﻿' ? 1 : 0;
                _sb = null;
            }

            public bool AtEnd => _i >= _s.Length;

            public FormatException Fail(string message) =>
                new FormatException($"JSON: {message} at offset {_i}");

            public void SkipWhitespace()
            {
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c != ' ' && c != '\t' && c != '\n' && c != '\r') return;
                    _i++;
                }
            }

            public object ReadValue()
            {
                if (AtEnd) throw Fail("unexpected end");
                char c = _s[_i];
                switch (c)
                {
                    case '{': return ReadObject();
                    case '[': return ReadArray();
                    case '"': return ReadString();
                    case 't': Expect("true"); return BoxedTrue;
                    case 'f': Expect("false"); return BoxedFalse;
                    case 'n': Expect("null"); return null;
                    default:
                        if (c == '-' || (c >= '0' && c <= '9')) return ReadNumber();
                        throw Fail($"unexpected character '{c}'");
                }
            }

            static readonly object BoxedTrue = true;
            static readonly object BoxedFalse = false;

            void Expect(string word)
            {
                if (string.CompareOrdinal(_s, _i, word, 0, word.Length) != 0) throw Fail($"expected '{word}'");
                _i += word.Length;
            }

            Dictionary<string, object> ReadObject()
            {
                _i++; // {
                var dict = new Dictionary<string, object>();
                SkipWhitespace();
                if (!AtEnd && _s[_i] == '}') { _i++; return dict; }
                while (true)
                {
                    SkipWhitespace();
                    if (AtEnd || _s[_i] != '"') throw Fail("expected property name");
                    string key = ReadString();
                    SkipWhitespace();
                    if (AtEnd || _s[_i] != ':') throw Fail("expected ':'");
                    _i++;
                    SkipWhitespace();
                    dict[key] = ReadValue();
                    SkipWhitespace();
                    if (AtEnd) throw Fail("unterminated object");
                    char c = _s[_i++];
                    if (c == '}') return dict;
                    if (c != ',') throw Fail("expected ',' or '}'");
                }
            }

            List<object> ReadArray()
            {
                _i++; // [
                var list = new List<object>();
                SkipWhitespace();
                if (!AtEnd && _s[_i] == ']') { _i++; return list; }
                while (true)
                {
                    SkipWhitespace();
                    list.Add(ReadValue());
                    SkipWhitespace();
                    if (AtEnd) throw Fail("unterminated array");
                    char c = _s[_i++];
                    if (c == ']') return list;
                    if (c != ',') throw Fail("expected ',' or ']'");
                }
            }

            string ReadString()
            {
                int start = ++_i; // opening quote
                // Fast path: no escapes → one Substring, no builder.
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c == '"') { _i++; return _s.Substring(start, _i - 1 - start); }
                    if (c == '\\') break;
                    _i++;
                }
                if (AtEnd) throw Fail("unterminated string");

                var sb = _sb ??= new StringBuilder(64);
                sb.Clear();
                sb.Append(_s, start, _i - start);
                while (_i < _s.Length)
                {
                    char c = _s[_i++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    if (AtEnd) break;
                    char e = _s[_i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length) throw Fail("bad \\u escape");
                            sb.Append((char)int.Parse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            _i += 4;
                            break;
                        default: throw Fail($"bad escape '\\{e}'");
                    }
                }
                throw Fail("unterminated string");
            }

            object ReadNumber()
            {
                int start = _i;
                bool isFloat = false;
                if (_s[_i] == '-') _i++;
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c >= '0' && c <= '9') { _i++; continue; }
                    if (c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-') { isFloat = true; _i++; continue; }
                    break;
                }
                var span = _s.Substring(start, _i - start);
                if (!isFloat && int.TryParse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int n))
                    return n;
                if (float.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                    return f;
                throw Fail($"bad number '{span}'");
            }
        }
    }
}
