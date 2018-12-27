using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace wigc
{
    public class ParameterInterpolator {
        Regex token = new Regex(@"^\#\{(\S+)}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private IDictionary<string, string> dict;

        public ParameterInterpolator(IDictionary<string, string> dict)
        {
            this.dict = dict;
        }

        public string Substitute(string what) {
            Match m = token.Match(what);
            if (!m.Success)
                return what;

            var key = m.Groups[1].Value;
            if (dict.ContainsKey(key))
                return dict[key];

            // no replacement
            return what;
        }
    }
}
