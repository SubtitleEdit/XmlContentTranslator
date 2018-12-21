using System.Collections.Generic;
using System.Text;

namespace XmlContentTranslator.Translator
{
    public interface ITranslator
    {
        List<TranslationPair> GetTranslationPairs();
        string GetName();
        string GetUrl();
        List<string> Translate(string sourceLanguage, string targetLanguage, List<string> paragraphs, StringBuilder log);
    }
}
