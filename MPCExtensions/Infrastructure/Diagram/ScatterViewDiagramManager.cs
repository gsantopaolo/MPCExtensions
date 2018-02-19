// -----------------------------------------------------------------------
// <copyright file="ScatterViewDiagramManager.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-08-25 @ 16:46
//  edited: 2017-10-20 @ 15:14
// -----------------------------------------------------------------------

#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using MPCExtensions.Controls;

#endregion

namespace MPCExtensions.Infrastructure.Diagram
{
    /// <summary>
    /// Manages all public diagram related stuff
    /// </summary>
    internal class ScatterViewDiagramManager
    {
        private readonly ScatterView scatterView;
        private readonly ObservableCollection<TileConnection> tilesCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterViewDiagramManager"/> class.
        /// </summary>
        /// <param name="scatterView">The scatter view.</param>
        public ScatterViewDiagramManager(ScatterView scatterView)
        {
            this.scatterView = scatterView;
            //this.tilesCollection = this.scatterView.TileConnections;
            this.tilesCollection.CollectionChanged += this.OnConnectionChanged;
        }

        /// <summary>
        /// Cleanup the manager
        /// </summary>
        public void Dispose()
        {
            //this.scatterView.TileConnections.CollectionChanged -= this.OnConnectionChanged;
        }

        /// <summary>
        /// Processes a hit testing operation to check if a connection is involved, if so manages related action.
        /// </summary>
        /// <param name="point">The hit testing point.</param>
        /// <param name="showUi">if set to <c>true</c> show any related processing related UI.</param>
        /// <returns>
        /// True when an operation has been processed, otherwise false
        /// </returns>
        public bool ProcessHitTestOperation(Point point, bool showUi)
        {
            //if (!this.scatterView.TileConnections.Any()) return false;

            bool processed = false;

            //We nee to temporarily enable hit testing for hit test to work
            //foreach (TileConnection connection in this.scatterView.TileConnections)
            //{
            //    connection.DiagramConnection.IsHitTestVisible = true;
            //}

            //Adjust positioning for propert hit testing considering zoom and panning
            //point.X = (point.X * this.scatterView.ScrollHost.ZoomFactor) - this.scatterView.ScrollHost.HorizontalOffset;
            //point.Y = (point.Y * this.scatterView.ScrollHost.ZoomFactor) - this.scatterView.ScrollHost.VerticalOffset;

            IEnumerable<UIElement> elems = VisualTreeHelper.FindElementsInHostCoordinates(point, this.scatterView);
            foreach (UIElement elem in elems)
            {
                if (elem is Polygon || elem is Path)
                {
                    //We hit a diagram connection element, show edit UI
                    if ((elem as FrameworkElement)?.DataContext is DiagramConnection diagramConnection)
                    {
                        diagramConnection.IsSelected = !diagramConnection.IsSelected;
                        if (showUi) diagramConnection.ShowConnectionEditUi(point);
                        processed = true;
                        break;
                    }
                }
            }

            //Hit testing is disabled by default to allow inking close to diagram connection
            //foreach (TileConnection connection in this.scatterView.TileConnections)
            //{
            //    connection.DiagramConnection.IsHitTestVisible = false;
            //}

            return processed;
        }

        /// <summary>
        /// Resets the connections zoom factor.
        /// </summary>
        /// <param name="controlTileScale">The control tile scale.</param>
        public void ResetConnectionsZoom(double controlTileScale)
        {
            if (!this.tilesCollection.Any()) return;

            foreach (TileConnection connection in this.tilesCollection)
            {
                connection.DiagramConnection?.ResetZoom(controlTileScale);
            }
        }

        /// <summary>
        /// Updates the provide tile connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void UpdateConnection(TileConnection connection)
        {
            //connection.DiagramConnection?.Update(connection, this.scatterView.ControlTileScale);
        }

        /// <summary>
        /// Adds a new connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private void AddConnection(TileConnection connection)
        {
            ConnectableViewItem connectableFrom = this.GetConnectable(connection.FromId);
            if (connectableFrom == null) return;
            ConnectableViewItem connectableTo = this.GetConnectable(connection.ToId);
            if (connectableTo == null) return;


            //From
            HitTestInfo hitTestInfo = this.GetHitTestInfo(connection.FromOrientation);
            ConnectorInfo originConnectorInfo = connectableFrom.CreateDiagramConnection(hitTestInfo);
            DiagramConnection newConnection = new DiagramConnection(connectableFrom, originConnectorInfo, 0)
            {
                Id = connection.Id,
                ConnectionType = connection.ConnectionType,
                RoutingMode = connection.RoutingMode
            };
            newConnection.EditOperationRequested += this.OnConnectionEditRequested;
            //newConnection.CreateConnection(connection.Color, this.scatterView.DiagramSelectedConnectionColor, this.scatterView.DiagramConnectionHighlightColor, connection.Thickness, connection.Opacity);

            //To
            hitTestInfo = this.GetHitTestInfo(connection.ToOrientation);
            ConnectorInfo targetConnectorInfo = connectableTo.CreateDiagramConnection(hitTestInfo);
            //newConnection.CompletePendingConnection(connectableTo, targetConnectorInfo, this.scatterView.ControlTileScale);

            //Adds connection to related tiles
            newConnection.Origin.DiagramConnections.Add(newConnection);
            newConnection.Destination.DiagramConnections.Add(newConnection);

            connection.DiagramConnection = newConnection;
            
            //this.scatterView.AddVisualConnection(newConnection, false);
        }

        /// <summary>
        /// Clears all diagram connections.
        /// </summary>
        private void ClearConnections()
        {
            List<IInteractiveDiagramElement> diagramItems = this.scatterView.Items.OfType<IInteractiveDiagramElement>().ToList();
            foreach (IInteractiveDiagramElement item in diagramItems)
            {
                var connectableItem = (ConnectableViewItem)this.scatterView.ContainerFromItem(item);
                if (connectableItem.HasConnections)
                {
                    foreach (DiagramConnection connection in connectableItem.DiagramConnections)
                    {
                        //this.scatterView.RemoveVisualConnection(connection, false);
                    }

                    connectableItem.DiagramConnections.Clear();
                }
            }
        }

        private ConnectableViewItem GetConnectable(string tileId)
        {
            IInteractiveElement tile = this.scatterView.Items.OfType<IInteractiveElement>().FirstOrDefault(t => t.Id == tileId);
            if (tile == null) return null;
            return (ConnectableViewItem)this.scatterView.ContainerFromItem(tile);
        }

        /// <summary>
        /// Gets the hit test information for provided orientation
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <returns></returns>
        private HitTestInfo GetHitTestInfo(ConnectorOrientation orientation)
        {
            switch (orientation)
            {
                case ConnectorOrientation.Left:
                    return HitTestInfo.AnchorLeft;
                case ConnectorOrientation.Top:
                    return HitTestInfo.AnchorTop;
                case ConnectorOrientation.Right:
                    return HitTestInfo.AnchorRight;
                case ConnectorOrientation.Bottom:
                    return HitTestInfo.AnchorBottom;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
            }
        }

        private void OnConnectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                //New connection added
                case NotifyCollectionChangedAction.Add:
                    TileConnection addConnection = (TileConnection)e.NewItems[0];
                    this.AddConnection(addConnection);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    TileConnection deleteConnection = (TileConnection)e.OldItems[0];
                    this.RemoveConnection(deleteConnection);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.ClearConnections();
                    break;
            }
        }

        /// <summary>
        /// Called when an edit operation on a connection is requested
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="point">The point.</param>
        private void OnConnectionEditRequested(object sender, Point point)
        {
            DiagramConnection connection = (DiagramConnection)sender;

            TileConnection tileConnection = connection.ToTileConnection();
            tileConnection.DiagramConnection = connection;

            DiagramConnectionEditOperation operation = new DiagramConnectionEditOperation()
            {
                Connection = tileConnection,
                X = point.X,
                Y = point.Y
            };

            //this.scatterView.DiagramConnectionEditCommand?.Execute(operation);
        }

        /// <summary>
        /// Removes a connection.
        /// </summary>
        /// <param name="connection">The connection to remove.</param>
        private void RemoveConnection(TileConnection connection)
        {
            //Guard against temporary server issue, can be removed later
            if (connection.DiagramConnection == null) return;

            connection.DiagramConnection.Origin.DiagramConnections.Remove(connection.DiagramConnection);
            connection.DiagramConnection.Destination.DiagramConnections.Remove(connection.DiagramConnection);
            //this.scatterView.RemoveVisualConnection(connection.DiagramConnection, false);

            connection.DiagramConnection.EditOperationRequested -= this.OnConnectionEditRequested;
            connection.DiagramConnection = null;
        }
    }
}