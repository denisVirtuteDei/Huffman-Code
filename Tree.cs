using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTree
{
    class Node
    {
        public string value;
        public char key;

        public int bit;
        public double probability;

        public Node left;
        public Node right;

        public Node()
        {
            left = null;
            right = null;
            bit = 0;
            probability = 0;
            key = default;
            value = "";
        }
    }

    class Tree
    {
        private Node root;
        private int leaveCount = 0;

        public Tree(Dictionary<char, double> map)
        {
            List<Node> nodes = new List<Node>();

            foreach (var elem in map)
            {
                Node _root = new Node()
                {
                    key = elem.Key,
                    probability = elem.Value
                };

                nodes.Add(_root);
            }

            CreateTree(nodes);
            root = nodes.First();
        }

        public void InitDictionary(Dictionary<char, string> dictionary)
        {
            if(root != null)
            {
                string oof = "";
                SetBitsToDictionary(root, oof, dictionary);
            }
        }

        private void CreateTree(List<Node> nodes)
        {
            // Create _root with 2 branch, 
            // first and second elem in ListCollection.
            // Find index to insert this _root.
            // Remove duplicated elem in ListCollection.  
            if (nodes.Count > 1)
            {
                var newElem = AddBranch(nodes.ElementAt(0), nodes.ElementAt(1));

                var index = nodes.FindIndex(2, item => newElem.probability <= item.probability);

                if (index < 0)
                    nodes.Add(newElem);
                else
                    nodes.Insert(index, newElem);

                nodes.RemoveAt(0);
                nodes.RemoveAt(0);

                CreateTree(nodes);
            }
        }

        private Node AddBranch(Node _left, Node _right)
        {
            Node _root = new Node
            {
                left = _left,
                right = _right
            };

            _root.left.bit = 0;
            _root.right.bit = 1;
            _root.probability = _root.left.probability + _root.right.probability;

            return _root;
        }

        public int LeaveCount()
        {
            return leaveCount;
        }

        private void SetBitsToDictionary(Node _root, string bigOOF, Dictionary<char, string> dictionary)
        {
            if (_root == null)
            {
                return;
            }

            SetBitsToDictionary(_root.left, bigOOF + "0", dictionary);

            if (_root.key != default)
            {
                leaveCount++;
                _root.value = bigOOF;
                dictionary.Add(_root.key, _root.value);
            }

            SetBitsToDictionary(_root.right, bigOOF + "1", dictionary);
        }
    }
}
