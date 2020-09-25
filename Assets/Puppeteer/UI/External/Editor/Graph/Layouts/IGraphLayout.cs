using System.Collections.Generic;
using UnityEngine;

namespace Puppeteer.UI.External.GraphVisualizer
{
	// Interface for a generic graph layout.
	public interface IGraphLayout
	{
		IEnumerable<Vertex> Vertices { get; }
		IEnumerable<Edge> Edges { get; }

		bool LeftToRight { get; }

		void CalculateLayout(Graph _graph);
	}

	public class Edge
	{
		// Indices in the vertex array of the layout algorithm.
		public Edge(Vertex _src, Vertex _dest)
		{
			Source = _src;
			Destination = _dest;
		}

		public Vertex Source { get; private set; }

		public Vertex Destination { get; private set; }
	}

	// One vertex is associated to each node in the graph.
	public class Vertex
	{
		// Center of the node in the graph layout.
		public Vector2 Position { get; set; }

		// The Node represented by the vertex.
		public Node Node { get; private set; }

		public Vertex(Node _node)
		{
			Node = _node;
		}
	}
}
