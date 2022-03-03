# Lucene.NET Custom Analyzer Demo

This is an example of how to (and how not to) build and test a custom analyzer, based on the [Analysis Overview](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/core/Lucene.Net.Analysis.html) and the [TokenStream consumer workflow](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/core/Lucene.Net.Analysis.TokenStream.html). See https://github.com/apache/lucenenet/issues/618 for background information.

## Properly Testing an Analyzer

As with testing any component, it is always best to do unit testing to ensure there are no problems with a custom  analyzer before attempting to use it with other components.

[Lucene.Net.TestFramework](https://www.nuget.org/packages/Lucene.Net.TestFramework) has several base classes and mocks that can be used to test custom extensions to Lucene.NET, including custom analyzers. [`BaseTokenStreamTestCase.AssertAnalyzesTo()` overloads](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/test-framework/Lucene.Net.Analysis.BaseTokenStreamTestCase.html#Lucene_Net_Analysis_BaseTokenStreamTestCase_AssertAnalyzesTo_Lucene_Net_Analysis_Analyzer_System_String_System_String___) (as in this example) ensure the consuming contract for `TokenStream` is adhered to.

In addition, they use random text, random cultures, and test both single and multi-threaded scenarios to ensure the analyzer doesn't have bugs that can cause hard-to-find problems when plugged into other components. While multi-threaded problems happen truly at random and cannot be reliably repeated, tests with random conditions can be fully repeated simply by adding attributes with the random seed and culture to the test assembly or by configuring the corresponding .runsettings (see the test failure message below).

> **NOTE**: It is important to always use an instance of `J2N.Randomizer` (a subclass of `System.Random`) because `System.Random` does not guarantee the same results across operating systems. Also, either use the `LuceneTestCase.Random` property or a new instance of `J2N.Randomizer` that is seeded by `LuceneTestCase.Random` (or the previous `J2N.Randomizer` instance in the chain) in order to make the tests repeatable. Always create a new instance of `J2N.Randomizer` for each parallel task that is tested that is seeded from a `J2N.Randomizer` instance from the calling thread.

## Scenario

Here, we demonstrate how to build and test an analyzer to normalize text using a [WhitespaceTokenizer](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/analysis-common/Lucene.Net.Analysis.Core.WhitespaceTokenizer.html) as well as an [ICUFoldingFilter](https://lucenenet.apache.org/docs/4.8.0-beta00016/api/icu/Lucene.Net.Analysis.Icu.ICUFoldingFilter.html). `ICUFoldingFilter` implements the [UTR #30](https://unicode.org/reports/tr30/) (now withdrawn) draft Unicode Technical Report, which does several common text transformations, such as removal of diacritics (accent characters) from Latin text. This is extremely useful in Western European languages which commonly use variations of accent characters for certain words.

For example, "resume" may be spelled:

1. "résume"
2. "resumè"
3. "résumé"
4. "resume"

So, it is advantageous when setting up automated search to remove the accent characters and normalize the text to "resume" in order to match all spellings.

We also demonstrate an invalid analyzer that uses 2 `Tokenizers`, which violates the consuming contract, resulting in the test failure.

```console
Message: 
System.InvalidOperationException : TokenStream contract violation: Reset()/Dispose() call missing, Reset() called multiple times, or subclass does not call base.Reset(). Please see the documentation of TokenStream class for more information about the correct consuming workflow.

To reproduce this test result:

Option 1:

 Apply the following assembly-level attributes:

[assembly: Lucene.Net.Util.RandomSeed("0xda5906119cfa75ef")]
[assembly: NUnit.Framework.SetCulture("en-AU")]

Option 2:

 Use the following .runsettings file:

<RunSettings>
  <TestRunParameters>
    <Parameter name="tests:seed" value="0xda5906119cfa75ef" />
    <Parameter name="tests:culture" value="en-AU" />
  </TestRunParameters>
</RunSettings>

See the .runsettings documentation at: https://docs.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file.

  Stack Trace: 
ReaderAnonymousClass.Read(Char[] cbuf, Int32 off, Int32 len)
StandardTokenizerImpl.ZzRefill()
StandardTokenizerImpl.GetNextToken()
StandardTokenizer.IncrementToken()
LowerCaseFilter.IncrementToken()
ICUNormalizer2Filter.IncrementToken()
StandardFilter.IncrementToken()
ASCIIFoldingFilter.IncrementToken()
BaseTokenStreamTestCase.CheckResetException(Analyzer a, String input)
BaseTokenStreamTestCase.AssertAnalyzesTo(Analyzer a, String input, String[] output, Int32[] startOffsets, Int32[] endOffsets, String[] types, Int32[] posIncrements, Int32[] posLengths)
BaseTokenStreamTestCase.AssertAnalyzesTo(Analyzer a, String input, String[] output)
TestInvalidCustomAnalyzer.TestRemoveAccents() line 14

  Standard Output: 
RandomSeed: 0xda5906119cfa75ef
Culture: en-AU
Time Zone: (UTC-03:00) Araguaina
Default Codec: Lucene41 (Lucene41RWCodec)
Default Similarity: DefaultSimilarity
Nightly: False
Weekly: False
Slow: True
Awaits Fix: True
Directory: random
Verbose: False
Random Multiplier: 1
```