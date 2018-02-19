// -----------------------------------------------------------------------
// <copyright file="DiagramOperation.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-07-21 @ 07:27
//  edited: 2017-07-21 @ 07:28
// -----------------------------------------------------------------------

namespace MPCExtensions.Infrastructure.Diagram
{
    /// <summary>
    /// Contains info about an operation on a diagram connection
    /// </summary>
    public class DiagramConnectionEditOperation
    {
        /// <summary>
        /// Gets or sets the connection to edit.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public TileConnection Connection { get; internal set; }
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public double X { get; internal set; }
        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public double Y { get; internal set; }
    }
}