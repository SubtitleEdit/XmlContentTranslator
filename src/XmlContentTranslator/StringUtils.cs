using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlContentTranslator
{
    public static class StringUtils
    {
        public static string Max50(string text)
        {
            if (text.Length > 50)
                return text.Substring(0, 49).Trim() + "...";
            return text;
        }
    }
}
