// -----------------------------------------------------------------------
// <copyright file="HitTestInfo.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-07-18 @ 12:00
//  edited: 2017-07-18 @ 12:01
// -----------------------------------------------------------------------

namespace MPCExtensions.Infrastructure
{
    /// <summary>
    /// Area of a tile that has been tested
    /// </summary>
    internal enum HitTestInfo
    {
        None,
        Content,
        AnchorLeft,
        AnchorTop,
        AnchorRight,
        AnchorBottom
    }
}