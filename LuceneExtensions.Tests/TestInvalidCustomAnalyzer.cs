using Lucene.Net.Analysis;
using NUnit.Framework;

namespace LuceneExtensions.Tests
{
    public class TestInvalidCustomAnalyzer : BaseTokenStreamTestCase
    {
        [Test]
        public virtual void TestRemoveAccents()
        {
            Analyzer a = new InvalidCustomAnalyzer(TEST_VERSION_CURRENT);

            // removal of latin accents (composed)
            AssertAnalyzesTo(a, "résumé", new string[] { "resume" });

            // removal of latin accents (decomposed)
            AssertAnalyzesTo(a, "re\u0301sume\u0301", new string[] { "resume" });

            // removal of latin accents (multi-word)
            AssertAnalyzesTo(a, "Carlos Pírez", new string[] { "carlos", "pirez" });
        }
    }
}
