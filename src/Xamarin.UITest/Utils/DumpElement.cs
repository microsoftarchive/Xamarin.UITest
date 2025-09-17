using System.Collections.Generic;
using System.Linq;

namespace Xamarin.UITest.Utils
{
    internal class DumpElement
    {
        public bool enabled { get; set; }
        public bool visible { get; set; }
        public DumpElement[] children { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public string el { get; set; }
        public string name { get; set; }
        public string value { get; set; }

        public TreeElement ToTreeElement(bool trimInvisibleLeaves)
        {
            var childElements = children.Select(x => x.ToTreeElement(trimInvisibleLeaves))
                .Where(x => x != null)
                .ToArray();

            if (trimInvisibleLeaves && !visible && !childElements.Any())
            {
                return null;
            }

            return new TreeElement(id, label, value, type, childElements, visible);
        }

        public IEnumerable<DumpElement> GetAllElements()
        {
            yield return this;

            foreach (var child in children)
            {
                foreach (var allChildren in child.GetAllElements())
                {
                    yield return allChildren;
                }
            }
        }
    }
}