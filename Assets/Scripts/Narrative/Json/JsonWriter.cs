using System.Globalization;
using System.Text;

namespace LegacyThroughTime.Narrative.Json
{
    /// <summary>
    /// Streaming JSON writer over a <see cref="StringBuilder"/>. Tracks comma placement
    /// itself, so callers only describe structure. Floats always carry a '.' or exponent
    /// so that <see cref="JsonReader"/> reads them back as floats, never as ints.
    /// </summary>
    sealed class JsonWriter
    {
        readonly StringBuilder _sb = new StringBuilder(1024);
        bool _needComma;

        public override string ToString() => _sb.ToString();

        public JsonWriter BeginObject() { Separate(); _sb.Append('{'); _needComma = false; return this; }
        public JsonWriter EndObject() { _sb.Append('}'); _needComma = true; return this; }
        public JsonWriter BeginArray() { Separate(); _sb.Append('['); _needComma = false; return this; }
        public JsonWriter EndArray() { _sb.Append(']'); _needComma = true; return this; }

        public JsonWriter Key(string name)
        {
            Separate();
            WriteString(name);
            _sb.Append(':');
            _needComma = false;
            return this;
        }

        public JsonWriter Null() { Separate(); _sb.Append("null"); _needComma = true; return this; }
        public JsonWriter Bool(bool v) { Separate(); _sb.Append(v ? "true" : "false"); _needComma = true; return this; }
        public JsonWriter Int(int v) { Separate(); _sb.Append(v.ToString(CultureInfo.InvariantCulture)); _needComma = true; return this; }

        public JsonWriter Float(float v)
        {
            Separate();
            var s = v.ToString("R", CultureInfo.InvariantCulture);
            _sb.Append(s);
            if (s.IndexOf('.') < 0 && s.IndexOf('E') < 0 && s.IndexOf('e') < 0) _sb.Append(".0");
            _needComma = true;
            return this;
        }

        public JsonWriter String(string v)
        {
            Separate();
            if (v == null) _sb.Append("null"); else WriteString(v);
            _needComma = true;
            return this;
        }

        /// <summary>Embeds an already-serialised JSON value verbatim.</summary>
        public JsonWriter Raw(string json)
        {
            Separate();
            _sb.Append(json);
            _needComma = true;
            return this;
        }

        public JsonWriter Property(string key, int v) => Key(key).Int(v);
        public JsonWriter Property(string key, bool v) => Key(key).Bool(v);
        public JsonWriter Property(string key, string v) => Key(key).String(v);

        void Separate()
        {
            if (_needComma) _sb.Append(',');
        }

        void WriteString(string s)
        {
            _sb.Append('"');
            int run = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                string esc = c switch
                {
                    '"' => "\\\"",
                    '\\' => "\\\\",
                    '\n' => "\\n",
                    '\r' => "\\r",
                    '\t' => "\\t",
                    _ => c < ' ' ? "\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture) : null,
                };
                if (esc == null) { run++; continue; }
                _sb.Append(s, i - run, run).Append(esc);
                run = 0;
            }
            _sb.Append(s, s.Length - run, run).Append('"');
        }
    }
}
