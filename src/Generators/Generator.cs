// <#+
#if !T4
namespace UnionTypes.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
#endif

#nullable enable

    public abstract class Generator
    {
        private StringBuilder _builder = new StringBuilder();
        private string _lineStartIndentation = "";
        private const string _indent = "    ";
        private bool _isLineStart = true;

        public string GeneratedText => _builder.ToString();

        protected void Write(string text)
        {
            if (_isLineStart)
            {
                _builder.Append(_lineStartIndentation);
                _isLineStart = false;
            }

            _builder.Append(text);
        }

        protected void WriteLine(string? text = null)
        {
            if (text != null)
                Write(text);
            _builder.AppendLine();
            _isLineStart = true;
        }

        protected void WriteLineNested(string text)
        {
            WriteNested(() => WriteLine(text));
        }

        protected void WriteNested(Action action)
        {
            var oldLineStartIndentation = _lineStartIndentation;
            _lineStartIndentation = _lineStartIndentation + _indent;
            action();
            _lineStartIndentation = oldLineStartIndentation;
        }

        protected void WriteNested(string open, string close, Action action)
        {
            if (!_isLineStart)
                WriteLine();
            WriteLine(open);
            WriteNested(action);
            WriteLine(close);
        }

        protected void WriteBraceNested(Action action)
        {
            WriteNested("{", "}", action);
        }

        /// <summary>
        /// Writes a blank line between each action
        /// </summary>
        protected void WriteLineSeparated(params Action[] actions)
        {
            var oldBuilder = _builder;
            _builder = new StringBuilder();
            var segments = new List<string>();

            for (int i = 0; i < actions.Length; i++)
            {
                _builder.Clear();

                actions[i]();

                if (_builder.Length > 0)
                {
                    segments.Add(_builder.ToString());
                }
            }

            _builder = oldBuilder;
            if (segments.Count > 0)
            {
                _builder.Append(string.Join(Environment.NewLine, segments));
            }
        }

        internal static string LowerName(string name)
        {
            if (!char.IsLower(name[0]))
                return char.ToLower(name[0]) + name.Substring(1);
            return name;
        }

        internal static string UpperName(string name)
        {
            if (!char.IsUpper(name[0]))
                return char.ToUpper(name[0]) + name.Substring(1);
            return name;
        }
    }

#if !T4
}
#endif
// #>