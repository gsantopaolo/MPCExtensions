// -----------------------------------------------------------------------
// <copyright file="IDiagramElement.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-06-09 @ 14:43
//  edited: 2017-06-09 @ 14:43
// -----------------------------------------------------------------------

using Windows.Foundation;

namespace MPCExtensions.Infrastructure.Diagram
{
    public interface IInteractiveDiagramElement
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is diagram enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is diagram enabled; otherwise, <c>false</c>.
        /// </value>
        bool IsDiagramEnabled { get;}

        /// <summary>
        /// Determines whether this tile is a valid connection target.
        /// </summary>
        /// <param name="sourceConnectionId">The source connection identifier.</param>
        /// <param name="originAnchor">The origin anchor where connection originates.</param>
        /// <param name="destinationAnchor">The desired destination anchor.</param>
        /// <returns>
        ///   <c>true</c> if [is valid connection target]; otherwise, <c>false</c>.
        /// </returns>
        bool IsValidConnectionTarget(string sourceConnectionId, ConnectorOrientation originAnchor, ConnectorOrientation destinationAnchor);
    }
}