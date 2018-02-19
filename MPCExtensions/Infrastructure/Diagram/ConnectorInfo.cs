// -----------------------------------------------------------------------
// <copyright file="ConnectorInfo.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-06-08 @ 15:41
//  edited: 2017-06-09 @ 13:45
// -----------------------------------------------------------------------

#region Using

using Windows.Foundation;

#endregion

namespace MPCExtensions.Infrastructure.Diagram
{
    public struct ConnectorInfo
    {
        public double HostLeft { get; set; }
        public double HostTop { get; set; }
        public Size HostSize { get; set; }
        public Point Position { get; set; }
        public ConnectorOrientation Orientation { get; set; }

        internal HitTestInfo HitTestInfo { get; set; }
    }
}