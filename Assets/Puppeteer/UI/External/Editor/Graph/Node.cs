using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puppeteer.UI.External.GraphVisualizer
{
	public class Node
	{
		public Node(object _content, NodeType _typeOfNode, float _weight = 1.0f, bool _isActive = false)
		{
			Content = _content;
			Weight = _weight;
			IsActive = _isActive;
			TypeOfNode = _typeOfNode;
			Children = new List<Node>();
		}

		public enum NodeType
		{
			GOAL,
			FINALPATH,
			OPEN,
			CLOSED,
			FINAL,
		}

		public IList<Node> Children { get; private set; }
		public object Content { get; private set; }

		public int Depth
		{
			get { return GetDepthRecursive(this); }
		}

		public bool IsActive { get; set; }
		public Node Parent { get; private set; }
		public NodeType TypeOfNode { get; private set; }
		public float Weight { get; set; }

		public void AddChild(Node _child)
		{
			if (_child == this) throw new Exception("Circular graphs not supported.");
			if (_child.Parent == this) return;

			Children.Add(_child);
			_child.Parent = this;
		}

		public virtual Color GetColor()
		{
			Type type = GetContentType();
			if (type == null)
				return Color.red;

			string shortName = type.ToString().Split('.').Last();
			float h = (float)Math.Abs(shortName.GetHashCode()) / int.MaxValue;
			return Color.HSVToRGB(h, 0.6f, 1.0f);
		}

		public virtual Type GetContentType()
		{
			return Content?.GetType();
		}

		public virtual string GetContentTypeName()
		{
			Type type = GetContentType();
			return type == null ? "Null" : type.ToString();
		}

		public virtual string GetContentTypeShortName()
		{
			return GetContentTypeName().Split('.').Last();
		}

		public override string ToString()
		{
			return "Node content: " + GetContentTypeName();
		}

		private static int GetDepthRecursive(Node _node)
		{
			if (_node.Parent == null) return 1;
			return 1 + GetDepthRecursive(_node.Parent);
		}
	}
}