// -----------------------------------------------------------------------
// <copyright file="ConnectionType.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-07-17 @ 16:02
//  edited: 2017-07-17 @ 16:02
// -----------------------------------------------------------------------

namespace MPCExtensions.Infrastructure.Diagram
{
    /// <summary>
    /// Indentifies the type of a connection
    /// </summary>
    public enum ConnectionType
    {
        None,
        ArrowTo,
        ArrowFrom,
        ArrowToAndFrom
    }
}