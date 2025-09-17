using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.UITest.Utils
{
    /// <summary>
    /// Class representing element properties.
    /// </summary>
    public class UIElement
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// has_focus property.
        /// </summary>
        public bool HasFocus { get; set; }

        /// <summary>
        /// label property.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// type property.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// title property.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// value property.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// hit_point property.
        /// </summary>
        public Dictionary<string, int> HitPoint { get; set; }

        /// <summary>
        /// enabled property.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<UIElement> Children { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Hitable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool HasKeyboardFocus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, int> Rect { get; set; }

        internal TreeElement ToTreeElement()
        {
            if (Children != null)
            {
                TreeElement[] childrenElements = Children.Select(x => x.ToTreeElement())
                    .Where(x => x != null).ToArray();
                return new TreeElement(Id, Label, Value, Type, childrenElements, Hitable);
            }
            else
            {
                return new TreeElement(Id, Label, Value, Type, Array.Empty<TreeElement>(), Hitable);
            }
        }

        /// <summary>
        /// Recursively searches through descendants of <see cref="UIElement"/>
        /// </summary>
        /// <param name="type">Type of descendants to search for.</param>
        /// <returns>Descendants with requested type.</returns>
        private IEnumerable<UIElement> GetDescendants(string type)
        {
            if (Children != null)
            {
                foreach (UIElement child in Children)
                {
                    if (child.Type == type)
                    {
                        yield return child;
                    }

                    foreach (UIElement childOfChild in child.GetDescendants(type: type))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        /// Searches for descendants of <see cref="UIElement"/> with specified type.
        /// </summary>
        /// <param name="type">Type of descendants to search for.</param>
        /// <returns>List with found descendants.</returns>
        public List<UIElement> Descendants(string type)
        {
            return GetDescendants(type: type).ToList();
        }
    }
}

