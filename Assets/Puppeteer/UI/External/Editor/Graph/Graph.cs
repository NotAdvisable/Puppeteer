using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Puppeteer.UI.External.GraphVisualizer
{
    public abstract class Graph : IEnumerable<Node>
    {
		public ReadOnlyCollection<Node> Nodes
		{
			get { return m_Nodes.AsReadOnly(); }
		}

		public void AddNode(Node _node)
		{
			m_Nodes.Add(_node);
		}

		public void AddNodeHierarchy(Node _root)
		{
			AddNode(_root);

			IEnumerable<Node> children = GetChildren(_root);
			if (children == null)
				return;

			foreach (Node child in children)
			{
				_root.AddChild(child);
				AddNodeHierarchy(child);
			}
		}

		public void Clear()
		{
			m_Nodes.Clear();
		}

		public IEnumerator<Node> GetEnumerator()
		{
			return m_Nodes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_Nodes.GetEnumerator();
		}

		public bool IsEmpty()
		{
			return m_Nodes.Count == 0;
		}

		public void Refresh()
		{
			// TODO optimize?
			Clear();
			Populate();
		}

		// Derived class should specify the children of a given node.
		protected abstract IEnumerable<Node> GetChildren(Node _node);

		// Derived class should implement how to populate this graph (usually by calling AddNodeHierarchy()).
		protected abstract void Populate();

		private readonly List<Node> m_Nodes = new List<Node>();
		protected class NodeWeight
        {
            public object NodeInstance { get; set; }
            public float Weight { get; set; }
        }
    }
}
