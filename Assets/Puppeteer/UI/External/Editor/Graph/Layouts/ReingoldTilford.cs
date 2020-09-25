using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puppeteer.UI.External.GraphVisualizer
{
	// Implementation of Reingold and Tilford algorithm for graph layout
	// "Tidier Drawings of Trees", IEEE Transactions on Software Engineering Vol SE-7 No.2, March 1981
	// The implementation has been customized to support graphs with multiple roots and unattached nodes.
	public class ReingoldTilford : IGraphLayout
	{
		public ReingoldTilford(bool _leftToRight = true)
		{
			this.LeftToRight = _leftToRight;
		}

		public IEnumerable<Edge> Edges
		{
			get
			{
				var edgesList = new List<Edge>();
				foreach (var node in m_NodeVertexLookup)
				{
					Vertex v = node.Value;
					foreach (Node child in v.Node.Children)
					{
						edgesList.Add(new Edge(m_NodeVertexLookup[child], v));
					}
				}
				return edgesList;
			}
		}

		public bool LeftToRight { get; private set; }

		public IEnumerable<Vertex> Vertices
		{
			get { return m_NodeVertexLookup.Values; }
		}

		// Main entry point of the algorithm
		public void CalculateLayout(Graph _graph)
		{
			m_NodeVertexLookup.Clear();
			foreach (Node node in _graph)
			{
				if (node == null)
				{
					continue;
				}
				m_NodeVertexLookup.Add(node, new Vertex(node));
			}

			if (m_NodeVertexLookup.Count == 0) return;

			IList<float> horizontalPositions = ComputeHorizontalPositionForEachLevel();

			List<Node> roots = m_NodeVertexLookup.Keys.Where(_n => _n.Parent == null).ToList();

			for (int i = 0; i < roots.Count; ++i)
			{
				RecursiveLayout(roots[i], 0, horizontalPositions);

				if (i > 0)
				{
					Vector2 previousRootRange = ComputeRangeRecursive(roots[i - 1]);
					RecursiveMoveSubtree(roots[i], previousRootRange.y + VERTICAL_DISTANCE_BETWEEN_TREES + DISTANCE_BETWEEN_NODES);
				}
			}
		}

		// After adjusting a subtree, compute its new boundary positions
		private Dictionary<int, Vector2> CombineBoundaryPositions(Dictionary<int, Vector2> _upperTree, Dictionary<int, Vector2> _lowerTree)
		{
			var combined = new Dictionary<int, Vector2>();
			int minDepth = _upperTree.Keys.Min();
			int maxDepth = System.Math.Max(_upperTree.Keys.Max(), _lowerTree.Keys.Max());

			for (int d = minDepth; d <= maxDepth; d++)
			{
				float upperBoundary = _upperTree.ContainsKey(d) ? _upperTree[d].x : _lowerTree[d].x;
				float lowerBoundary = _lowerTree.ContainsKey(d) ? _lowerTree[d].y : _upperTree[d].y;
				combined[d] = new Vector2(upperBoundary, lowerBoundary);
			}
			return combined;
		}

		// Pre-compute the horizontal position for each level.
		// Levels with few wires (as measured by the maximum number of children for one node) are placed closer
		// apart; very cluttered levels are placed further apart.
		private float[] ComputeHorizontalPositionForEachLevel()
		{
			// Gather information about depths.
			var maxDepth = int.MinValue;
			var nodeDepths = new Dictionary<int, List<Node>>();
			foreach (Node node in m_NodeVertexLookup.Keys)
			{
				int d = node.Depth;
				if (!nodeDepths.TryGetValue(d, out List<Node> nodes))
				{
					nodeDepths[d] = nodes = new List<Node>();
				}
				nodes.Add(node);
				maxDepth = Mathf.Max(d, maxDepth);
			}

			// Bake the left to right horizontal positions.
			var horizontalPositionForDepth = new float[maxDepth];
			horizontalPositionForDepth[0] = 0;
			for (int d = 1; d < maxDepth; ++d)
			{
				IEnumerable<Node> nodesOnThisLevel = nodeDepths[d + 1];

				int maxChildren = nodesOnThisLevel.Max(_entry => _entry.Children.Count);

				float wireLengthHeuristic = Mathf.Lerp(1, WIRE_LENGTH_FACTOR_FOR_LARGE_SPANNING_TREES,
						Mathf.Min(1, maxChildren / MAX_CHILDREN_THRESHOLD));

				horizontalPositionForDepth[d] = horizontalPositionForDepth[d - 1] +
					DISTANCE_BETWEEN_NODES * wireLengthHeuristic;
			}

			return LeftToRight ? horizontalPositionForDepth : horizontalPositionForDepth.Reverse().ToArray();
		}

		private Vector2 ComputeRangeRecursive(Node _node)
		{
			var range = Vector2.one * m_NodeVertexLookup[_node].Position.y;
			foreach (Node child in _node.Children)
			{
				Vector2 childRange = ComputeRangeRecursive(child);
				range.x = Mathf.Min(range.x, childRange.x);
				range.y = Mathf.Max(range.y, childRange.y);
			}
			return range;
		}

		// Determine parent's vertical position based on its children
		private Vector2 GetAveragePosition(ICollection<Node> _children)
		{
			Vector2 centroid = new Vector2();

			centroid = _children.Aggregate(centroid, (_current, _n) => _current + m_NodeVertexLookup[_n].Position);

			if (_children.Count > 0)
				centroid /= _children.Count;

			return centroid;
		}

		// Using a Vector2 at each depth to hold the extrema vertical positions
		private Dictionary<int, Vector2> GetBoundaryPositions(Node _subTreeRoot)
		{
			var extremePositions = new Dictionary<int, Vector2>();

			IEnumerable<Node> descendants = GetSubtreeNodes(_subTreeRoot);

			foreach (var node in descendants)
			{
				int depth = m_NodeVertexLookup[node].Node.Depth;
				float pos = m_NodeVertexLookup[node].Position.y;
				if (extremePositions.ContainsKey(depth))
					extremePositions[depth] = new Vector2(Mathf.Min(extremePositions[depth].x, pos),
							Mathf.Max(extremePositions[depth].y, pos));
				else
					extremePositions[depth] = new Vector2(pos, pos);
			}

			return extremePositions;
		}

		// Includes all descendants and the subtree root itself
		private IEnumerable<Node> GetSubtreeNodes(Node _root)
		{
			var allDescendants = new List<Node> { _root };
			foreach (Node child in _root.Children)
			{
				allDescendants.AddRange(GetSubtreeNodes(child));
			}
			return allDescendants;
		}

		// Traverse the graph and place all nodes according to the algorithm
		private void RecursiveLayout(Node _node, int _depth, IList<float> _horizontalPositions)
		{
			IList<Node> children = _node.Children;
			foreach (Node child in children)
			{
				RecursiveLayout(child, _depth + 1, _horizontalPositions);
			}

			var yPos = 0.0f;
			if (children.Count > 0)
			{
				SeparateSubtrees(children);
				yPos = GetAveragePosition(children).y;
			}

			var pos = new Vector2(_horizontalPositions[_depth], yPos);
			m_NodeVertexLookup[_node].Position = pos;
		}

		// Apply a vertical delta to all nodes in a subtree
		private void RecursiveMoveSubtree(Node _subtreeRoot, float _yDelta)
		{
			Vector2 pos = m_NodeVertexLookup[_subtreeRoot].Position;
			m_NodeVertexLookup[_subtreeRoot].Position = new Vector2(pos.x, pos.y + _yDelta);

			foreach (Node child in _subtreeRoot.Children)
			{
				RecursiveMoveSubtree(child, _yDelta);
			}
		}

		// Separate the given subtrees so they do not overlap
		private void SeparateSubtrees(IList<Node> _subroots)
		{
			if (_subroots.Count < 2)
				return;

			Node upperNode = _subroots[0];

			Dictionary<int, Vector2> upperTreeBoundaries = GetBoundaryPositions(upperNode);
			for (int s = 0; s < _subroots.Count - 1; s++)
			{
				Node lowerNode = _subroots[s + 1];
				Dictionary<int, Vector2> lowerTreeBoundaries = GetBoundaryPositions(lowerNode);

				int minDepth = upperTreeBoundaries.Keys.Min();
				if (minDepth != lowerTreeBoundaries.Keys.Min())
					Debug.LogError("Cannot separate subtrees which do not start at the same root depth");

				int lowerMaxDepth = lowerTreeBoundaries.Keys.Max();
				int upperMaxDepth = upperTreeBoundaries.Keys.Max();
				int maxDepth = System.Math.Min(upperMaxDepth, lowerMaxDepth);

				for (int depth = minDepth; depth <= maxDepth; depth++)
				{
					float delta = DISTANCE_BETWEEN_NODES - (lowerTreeBoundaries[depth].x - upperTreeBoundaries[depth].y);
					delta = System.Math.Max(delta, 0);
					RecursiveMoveSubtree(lowerNode, delta);
					for (int i = minDepth; i <= lowerMaxDepth; i++)
						lowerTreeBoundaries[i] += new Vector2(delta, delta);
				}
				upperTreeBoundaries = CombineBoundaryPositions(upperTreeBoundaries, lowerTreeBoundaries);
			}
		}

		// By convention, all graph layout algorithms should have a minimum distance of 1 unit between nodes
		private static readonly float DISTANCE_BETWEEN_NODES = 1.0f;

		private static readonly float MAX_CHILDREN_THRESHOLD = 6.0f;

		// Used to specify the vertical distance two non-attached trees in the graph.
		private static readonly float VERTICAL_DISTANCE_BETWEEN_TREES = 3.0f;

		// Used to lengthen the wire when lots of children are connected. If 1, all levels will be evenly separated
		private static readonly float WIRE_LENGTH_FACTOR_FOR_LARGE_SPANNING_TREES = 3.0f;

		// Helper structure to easily find the vertex associated to a given Node.
		private readonly Dictionary<Node, Vertex> m_NodeVertexLookup = new Dictionary<Node, Vertex>();
	}
}