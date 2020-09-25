using System;
using System.Collections.Generic;

namespace Puppeteer.Core.External
{
    public interface IFastPriorityQueueNode
    {
        /// <summary>
        /// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue (ideally just once, in the node's constructor).
        /// Should not be manually edited once the node has been enqueued - use queue.UpdatePriority() instead
        /// </summary>
        float Priority { get; set; }

        /// <summary>
        /// Represents the current position in the queue
        /// </summary>
        int QueueIndex { get; set; }

#if DEBUG
        /// <summary>
        /// The queue this node is tied to. Used only for debug builds.
        /// </summary>
        object Queue { get; set; }
#endif
    }
}
