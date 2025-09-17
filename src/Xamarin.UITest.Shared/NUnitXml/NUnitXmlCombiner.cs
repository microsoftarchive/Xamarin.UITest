using System;
using System.Xml.Linq;
using System.Linq;

namespace Xamarin.UITest.Shared.NUnitXml
{
    public class NUnitXmlCombiner
    {
        public XDocument Combine(XDocument doc1, XDocument doc2)
        {
            var doc = new XDocument(doc1);

            CombineTestResultsAttribute(doc, doc2, "total");
            CombineTestResultsAttribute(doc, doc2, "errors");
            CombineTestResultsAttribute(doc, doc2, "failures");
            CombineTestResultsAttribute(doc, doc2, "not-run");
            CombineTestResultsAttribute(doc, doc2, "inconclusive");
            CombineTestResultsAttribute(doc, doc2, "ignored");
            CombineTestResultsAttribute(doc, doc2, "skipped");
            CombineTestResultsAttribute(doc, doc2, "invalid");
           
            foreach (var rootSuite in doc2.Root.Elements("test-suite"))
            {
                doc.Root.Add(rootSuite);
            }

            return doc;
        }

        void CombineTestResultsAttribute(XDocument targetDoc, XDocument otherDoc, string attributeName)
        {
            AddTestResultsAttribute(targetDoc, attributeName, GetTestResultsAttribute(otherDoc, attributeName));
        }

        int GetTestResultsAttribute(XDocument doc, string attributeName)
        {
            var attributeValue = doc.Descendants("test-results")
                .First()
                .Attribute(attributeName)
                .Value;

            int val;
            if (Int32.TryParse(attributeValue, out val))
            {
                return val;
            }

            return 0;
        }

        void AddTestResultsAttribute(XDocument doc, string attributeName, int value)
        {
            int current = GetTestResultsAttribute(doc, attributeName);

            doc.Descendants("test-results")
                .First()
                .Attribute(attributeName)
                .SetValue(current + value);
        }
    }
}