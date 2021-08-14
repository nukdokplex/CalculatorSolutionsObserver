using System;
using System.Collections.Generic;
using System.Text;

namespace CalculatorSolutionsObserver.Utils
{
    delegate void NTreeExplorer<T, V>(T value, V linkValue);

    class NTree<T, V>
    {
        public T Value;
        public V LinkValue;
        public List<NTree<T, V>> Children = new List<NTree<T, V>>();

        public int AddChild(NTree<T, V> node)
        {
            Children.Add(node);
            return Children.Count - 1;
        }

        public NTree<T, V> GetChild(int index)
        {
            return Children[index];
        }

        public static void Traverse<T, V>(NTree<T, V> tree, NTreeExplorer<T, V> explorer)
        {
            explorer(tree.Value, tree.LinkValue);
            foreach (NTree<T, V> child in tree.Children)
            {
                Traverse<T, V>(child, explorer);
            }
        }
    }
}
