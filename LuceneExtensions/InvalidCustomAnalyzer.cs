using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Icu;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System.IO;

namespace LuceneExtensions
{
    public class InvalidCustomAnalyzer : Analyzer
    {
        private readonly LuceneVersion matchVersion;

        public InvalidCustomAnalyzer(LuceneVersion matchVersion)
        {
            this.matchVersion = matchVersion;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            Tokenizer tokenizer = new KeywordTokenizer(reader);
            TokenStream result = new StandardTokenizer(matchVersion, reader);
            result = new LowerCaseFilter(matchVersion, result);
            result = new ICUFoldingFilter(result);
            result = new StandardFilter(matchVersion, result);
            result = new ASCIIFoldingFilter(result);
            return new TokenStreamComponents(tokenizer, result);
        }
    }
}
