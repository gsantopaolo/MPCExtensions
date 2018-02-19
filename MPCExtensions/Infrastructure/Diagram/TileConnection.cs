// -----------------------------------------------------------------------
// <copyright file="TileConnection.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-07-25 @ 14:48
//  edited: 2017-08-09 @ 14:27
// -----------------------------------------------------------------------

#region Using

using System;

#endregion

namespace MPCExtensions.Infrastructure.Diagram
{
    /// <summary>
    /// Represents a diagnam connection entity
    /// </summary>
    public class TileConnection
    {
        private DiagramConnection diagramConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileConnection"/> class.
        /// </summary>
        public TileConnection()
        {
            this.Id = Guid.NewGuid().ToString("N");
            this.SyncId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>
        /// The type of the connection.
        /// </value>
        public ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets from identifier.
        /// </summary>
        /// <value>
        /// From identifier.
        /// </value>
        public string FromId { get; set; }

        /// <summary>
        /// Gets or sets from orientation.
        /// </summary>
        /// <value>
        /// From orientation.
        /// </value>
        public ConnectorOrientation FromOrientation { get; set; }

        /// <summary>
        /// Gets or sets the connection identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets a value indicating whether this connection is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected => this.DiagramConnection != null && this.DiagramConnection.IsSelected;

        /// <summary>
        /// Gets or sets the connection opacity.
        /// </summary>
        /// <value>
        /// The opacity.
        /// </value>
        public double Opacity { get; set; } = 1;


        /// <summary>
        /// Gets or sets the routing mode.
        /// </summary>
        /// <value>
        /// The routing mode.
        /// </value>
        public RoutingMode RoutingMode { get; set; } = RoutingMode.Bezier;

        /// <summary>
        /// Gets or sets the synchronization identifier.
        /// </summary>
        /// <value>
        /// The synchronize identifier.
        /// </value>
        public string SyncId { get; set; }

        /// <summary>
        /// Gets or sets the thickness.
        /// </summary>
        /// <value>
        /// The thickness.
        /// </value>
        public int Thickness { get; set; }

        /// <summary>
        /// Gets or sets to identifier.
        /// </summary>
        /// <value>
        /// To identifier.
        /// </value>
        public string ToId { get; set; }

        /// <summary>
        /// Gets or sets to orientation.
        /// </summary>
        /// <value>
        /// To orientation.
        /// </value>
        public ConnectorOrientation ToOrientation { get; set; }

        /// <summary>
        /// Gets or sets the diagram connection.
        /// </summary>
        /// <value>
        /// The diagram connection.
        /// </value>
        internal DiagramConnection DiagramConnection
        {
            get { return this.diagramConnection; }
            set
            {
                this.diagramConnection = value;
            }
        }

        /// <summary>
        /// Updates the synchronization identifier of this connection
        /// </summary>
        /// <returns></returns>
        public Guid UpdateSyncId()
        {
            Guid syncId = Guid.NewGuid();
            this.SyncId = syncId.ToString("N");
            return syncId;
        }

        /// <summary>
        /// Gets a values indicating whether provided connection matches this instance
        /// </summary>
        /// <param name="connectionToMatch">The connection to match.</param>
        /// <returns></returns>
        public bool Matches(TileConnection connectionToMatch)
        {
            return this.FromId == connectionToMatch.FromId &&
                   this.ToId == connectionToMatch.ToId && 
                   this.FromOrientation == connectionToMatch.FromOrientation &&
                   this.ToOrientation == connectionToMatch.ToOrientation;

        }
    }
}