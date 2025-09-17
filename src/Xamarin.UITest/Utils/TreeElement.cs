using System.Linq;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Utils
{
    internal class TreeElement
    {
        readonly string _id;
        readonly string _text;
        readonly string _label;
        string[] _fullTypes;
        TreeElement[] _children;
        readonly bool _visible;

        public TreeElement(string id, string label, string text, string type, TreeElement[] children, bool visible)
        {
            _id = id;
            _text = text;
            _label = label;
            _fullTypes = new[] { type };
            _children = children;
            _visible = visible;
        }

        public string Id
        {
            get { return _id; }
        }
            
        public string Label
        {
            get { return _label; }
        }

        public string Text
        {
            get { return _text; }
        }

        public string[] FullTypes
        {
            get { return _fullTypes; }
        }

        public bool Visible
        {
            get { return _visible; }
        }

        public string SimplifiedType
        {
            get
            {
                var types = FullTypes.Select(x => x.Split('.').LastOrDefault()).ToArray();

                if (types.Length <= 2)
                {
                    return string.Join(" > ", types);
                }

                return string.Join(" > ", new[] { types.First(), "...", types.Last() });
            }
        }

        public TreeElement[] Children
        {
            get { return _children; }
        }

        public bool Condensable
        {
            get
            {
                return Id.IsNullOrWhiteSpace()
                       && Text.IsNullOrWhiteSpace()
                    && Label.IsNullOrWhiteSpace();
            }
        }

        public void Condense()
        {
            foreach (var element in Children)
            {
                element.Condense();
            }

			if (Children.Length == 1) {
				TreeElement child = Children.First ();
				
                if (child.Condensable) {
					_fullTypes = FullTypes.Concat(child.FullTypes).ToArray();
					_children = child.Children;
				}
			}
        }
    }
}