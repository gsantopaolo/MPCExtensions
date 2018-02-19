// -----------------------------------------------------------------------
// <copyright file="IInteractiveElement.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-11-06 @ 15:23
//  edited: 2017-11-07 @ 16:40
// -----------------------------------------------------------------------

#region Using

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using MPCExtensions.Infrastructure.Entities;

#endregion

namespace MPCExtensions.Infrastructure
{
    /// <summary>
    /// Interface implemented by Viewmodels representing interactive UI elements
    /// </summary>
    public interface IInteractiveElement
    {
        /// <summary>
        /// Gets a value indicating whether this element can be dropped inside a library.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be dropped; otherwise, <c>false</c>.
        /// </value>
        bool CanBeDropped { get; }

        /// <summary>
        /// Gets a value indicating whether this element can be un dropped.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be un dropped; otherwise, <c>false</c>.
        /// </value>
        bool CanBeUnDropped { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether menu can appear over this element .
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can menu appear; otherwise, <c>false</c>.
        /// </value>
        bool CanMenuAppear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can resize.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can resize; otherwise, <c>false</c>.
        /// </value>
        bool CanResize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tile can rotate.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can rotate; otherwise, <c>false</c>.
        /// </value>
        bool CanRotate { get; set; }

        /// <summary>
        /// Gets or sets the height of the element.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        double Height { get; set; }

        /// <summary>
        /// Gets the tile identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Gets or sets a value indicating whether tile is frozen.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is frozen; otherwise, <c>false</c>.
        /// </value>
        bool IsFrozen { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this tile is full screen.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is full screen; otherwise, <c>false</c>.
        /// </value>
        bool IsFullScreen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this tile is grouped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is grouped; otherwise, <c>false</c>.
        /// </value>
        bool IsGrouped { get; set; }

        /// <summary>
        /// Gets a value indicating whether tile is pinned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is pinned; otherwise, <c>false</c>.
        /// </value>
        bool IsPinned { get; }

        /// <summary>
        /// Gets or sets a value indicating whether user is touching this element.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is touching; otherwise, <c>false</c>.
        /// </value>
        bool IsTouching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether element is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets the minimum height.
        /// </summary>
        /// <value>
        /// The minimum height.
        /// </value>
        double MinHeight { get; }

        /// <summary>
        /// Gets the minimum width.
        /// </summary>
        /// <value>
        /// The minimum width.
        /// </value>
        double MinWidth { get; }

        /// <summary>
        /// Gets or sets the optional Id of the tile hosting this tile.
        /// </summary>
        /// <value>
        /// The hosting tile identifier.
        /// </value>
        /// <remarks>When ParentTileId is not null it means that this tile is a tile hosted inside a library</remarks>
        string ParentTileId { get; set; }

        /// <summary>
        /// Gets or sets the id of the the tile who is in some way connected to this one.
        /// </summary>
        /// <value>
        /// The related tile identifier.
        /// </value>
        string RelatedTileId { get; set; }

        /// <summary>
        /// Gets or sets the item rotation.
        /// </summary>
        /// <value>
        /// The rotation.
        /// </value>
        double Rotation { get; set; }

        /// <summary>
        /// Gets or sets the scaling factor.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        double Scale { get; set; }

        /// <summary>
        /// Gets or sets the width of the element.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        double Width { get; set; }

        /// <summary>
        /// Gets or sets the value of X position
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        double X { get; set; }

        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        double Y { get; set; }

        /// <summary>
        ///  Gets or sets the element ZIndex.
        /// </summary>
        /// <value>
        /// The index of the z.
        /// </value>
        int ZIndex { get; set; }

        /// <summary>
        /// Prompts a request to the user, invoking related callback depending on user choice
        /// </summary>
        /// <param name="relatedTileId">The id of the tile who generated confirmation</param>
        /// <param name="message">The message.</param>
        /// <param name="confirmCallback">The confirm callback.</param>
        /// <param name="cancelCallback">The cancel callback.</param>
        void AskConfirm(string relatedTileId,string message, AskConfirmContext context, Action<int> onSelectionCallback);

        /// <summary>
        /// Determines whether the specified point is contained into this item.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified area contains area; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsPoint(Point point);

        /// <summary>
        /// Occurs when this element is going to be deleted.
        /// </summary>
        event EventHandler<EventArgs> Deleting;

        event EventHandler<EventArgs> FrozenStateChanged;

        /// <summary>
        /// Gets the size of the proper tile.
        /// </summary>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newHeight">The new height.</param>
        /// <returns>Allows a single to tile to properly manage how it should be resized depending on its content</returns>
        Size GetProperTileSize(double newWidth, double newHeight);

        /// <summary>
        /// Gets the tile thumbnail .
        /// </summary>
        /// <returns></returns>
        Task<Stream> GetThumbnailAsync();

        /// <summary>
        /// Occurs when tile must sync with another tile that is part of this group.
        /// </summary>
        event EventHandler<TileGroupingInfoEventArgs> GroupedTileOperationRequested;

        /// <summary>
        /// Occurs when local tile, part of a group changes its info.
        /// </summary>
        event EventHandler<TileGroupingInfoEventArgs> LocalGroupTileOperationRequested;

        /// <summary>
        /// Query dependent tiles if is ok to move the tile for specified amount
        /// </summary>
        /// <param name="deltaX">The delta x.</param>
        /// <param name="deltaY">The delta y.</param>
        /// <returns>True if dependent tiles confirmed the operation</returns>
        bool QueryDependentTiles(double deltaX, double deltaY);

        /// <summary>
        /// Occurs when a tile queries other tiles in a group.
        /// </summary>
        event EventHandler<TileGroupingInfoEventArgs> QueryGroupedTilesStatus;

        event EventHandler<HostContext> QueryHostContext;

        /// <summary>
        /// Queries the tile status
        /// </summary>
        /// <param name="deltaX">The delta x.</param>
        /// <param name="deltaY">The delta y.</param>
        /// <returns>True if status is ok</returns>
        bool QueryTileStatus(double deltaX, double deltaY);

        /// <summary>
        /// Indicates that element is active.
        /// </summary>
        void SetActive();

        event EventHandler<EventArgs> UnDropped;

        /// <summary>
        /// Updates the dependent tiles.
        /// </summary>
        /// <param name="deltaX">The delta x.</param>
        /// <param name="deltaY">The delta y.</param>
        /// <param name="deltaRotation">The delta rotation.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        void UpdateDependentTiles(double deltaX, double deltaY, double deltaRotation, bool isActive);


        /// <summary>
        /// Updates this tile with another active tile that is part of same group
        /// </summary>
        /// <param name="deltaX">The delta x.</param>
        /// <param name="deltaY">The delta y.</param>
        /// <param name="deltaRotation">The delta rotation.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        void UpdateFromDependentTile(double deltaX, double deltaY, double deltaRotation, bool isActive);

        /// <summary>
        /// Updates the preview.
        /// </summary>
        void UpdatePreview();

        /// <summary>
        /// Updates the remote clients UI.
        /// </summary>
        Task UpdateRemoteClients();

        /// <summary>
        /// Occurs when a UI updated is.
        /// </summary>
        event EventHandler<UpdateUiInfo> UpdateUiRequested;

        /// <summary>
        /// Occurs when z index changes.
        /// </summary>
        event EventHandler<EventArgs> ZIndexChanged;
    }
}