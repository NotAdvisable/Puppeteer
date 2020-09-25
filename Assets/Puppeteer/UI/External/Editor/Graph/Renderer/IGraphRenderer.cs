using UnityEngine;

namespace Puppeteer.UI.External.GraphVisualizer
{
    // Interface for rendering a tree layout to screen.
    public interface IGraphRenderer
    {
        void Draw(IGraphLayout _graphLayout, Rect _drawingArea);
        void Draw(IGraphLayout _graphLayout, Rect _drawingArea, GraphSettings _graphSettings);
    }

    // Customization of how the graph will be displayed:
    // - size, distances and proportions of nodes
    // - legend
    public struct GraphSettings
    {
        // In layout units. If 1, node will be drawn as large as possible to avoid overlapping, and to respect aspect ratio
        public float MaximumNormalizedNodeSize;

        // when the graph is very simple, the nodes can seem disproportionate relative to the graph. This limits their size
        public float MaximumNodeSizeInPixels;

        // width / height; 1 represents a square node
        public float NodeAspectRatio;

        // Control the display of the legend.
        public bool ShowInspector;
    }
}
