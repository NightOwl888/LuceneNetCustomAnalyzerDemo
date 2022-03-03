using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Icu;
using Lucene.Net.Util;
using System.IO;

namespace LuceneExtensions
{
    public sealed class CustomAnalyzer : Analyzer
    {
        private readonly LuceneVersion matchVersion;

        public CustomAnalyzer(LuceneVersion matchVersion)
        {
            this.matchVersion = matchVersion;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer tokenizer = new WhitespaceTokenizer(matchVersion, reader);
            TokenStream result = tokenizer;
            result = new ICUFoldingFilter(result);
            return new TokenStreamComponents(tokenizer, result);
        }
    }
}
