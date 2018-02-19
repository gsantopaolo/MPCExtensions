// -----------------------------------------------------------------------
// <copyright file="ConnectableViewItem.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-08-18 @ 16:30
//  edited: 2017-08-18 @ 17:13
// -----------------------------------------------------------------------

//To enable V1 limitation uncomment this define and similar in Scatteview
//#define Diagram_v1

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using AppStudio.Uwp;
using MPCExtensions.Helpers;
using MPCExtensions.Infrastructure;
using MPCExtensions.Infrastructure.Diagram;

#endregion

namespace MPCExtensions.Controls
{
    [TemplatePart(Name = PART_ANCHOR_LEFT, Type = typeof(Grid))]
    [TemplatePart(Name = PART_ANCHOR_TOP, Type = typeof(Grid))]
    [TemplatePart(Name = PART_ANCHOR_RIGHT, Type = typeof(Grid))]
    [TemplatePart(Name = PART_ANCHOR_BOTTOM, Type = typeof(Grid))]
    public abstract class ConnectableViewItem : InteractiveItem
    {
        private const string PART_ANCHOR_LEFT = "PART_ANCHOR_LEFT";
        private const string PART_ANCHOR_TOP = "PART_ANCHOR_TOP";
        private const string PART_ANCHOR_RIGHT = "PART_ANCHOR_RIGHT";
        private const string PART_ANCHOR_BOTTOM = "PART_ANCHOR_BOTTOM";
        private const double AnchorZoomFactor = 1.3;
        private const int AnchorAngleThreshold = 5;

        private Grid anchorBottom;
        private Grid anchorLeft;
        private Grid anchorRight;
        private Grid anchorTop;
        private List<DiagramConnection> diagramConnections;
        private long token1;
        private long token2;
        private long token3;
        private long token4;
        private long token5;
        private long token6;
        private long token7;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectableViewItem"/> class.
        /// </summary>
        /// <param name="scatterView"></param>
        protected ConnectableViewItem(ScatterView scatterView) : base(scatterView)
        {
            this.SubscribeUIChanges();
        }

        /// <summary>
        /// Gets a value indicating whether this tile is a pending connection source.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connection source; otherwise, <c>false</c>.
        /// </value>
        public bool IsPendingConnectionSource { get; private set; }

        /// <summary>
        /// Gets the optional associated diagram connections.
        /// </summary>
        /// <value>
        /// The diagram connections.
        /// </value>
        internal List<DiagramConnection> DiagramConnections
        {
            get
            {
                if (this.diagramConnections == null) this.diagramConnections = new List<DiagramConnection>();
                return this.diagramConnections;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has anchors.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has anchors; otherwise, <c>false</c>.
        /// </value>
        internal bool HasAnchors => this.anchorLeft != null && this.anchorLeft.Visibility == Visibility.Visible;

        /// <summary>
        /// Gets a value indicating whether this tile has some diagram connections.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has connections; otherwise, <c>false</c>.
        /// </value>
        internal bool HasConnections => this.diagramConnections != null && this.diagramConnections.Any();

#if Diagram_v1
        /// <summary>
        /// Gets a value indicating whether this instance is diagram enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is diagram enabled; otherwise, <c>false</c>.
        /// </value>
        internal bool IsDiagramEnabled => !(this.InteractiveElement.Rotation < -AnchorAngleThreshold || this.InteractiveElement.Rotation > AnchorAngleThreshold);
#endif

        /// <summary>
        /// Resets the anchors zoom.
        /// </summary>
        public void ResetAnchorsZoom()
        {
            //(this.anchorLeft.RenderTransform as CompositeTransform).ScaleX = this.ScatterView.ControlTileScale;
            //(this.anchorLeft.RenderTransform as CompositeTransform).ScaleY = this.ScatterView.ControlTileScale;
            //(this.anchorTop.RenderTransform as CompositeTransform).ScaleX = this.ScatterView.ControlTileScale;
            //(this.anchorTop.RenderTransform as CompositeTransform).ScaleY = this.ScatterView.ControlTileScale;
            //(this.anchorBottom.RenderTransform as CompositeTransform).ScaleX = this.ScatterView.ControlTileScale;
            //(this.anchorBottom.RenderTransform as CompositeTransform).ScaleY = this.ScatterView.ControlTileScale;
            //(this.anchorRight.RenderTransform as CompositeTransform).ScaleX = this.ScatterView.ControlTileScale;
            //(this.anchorRight.RenderTransform as CompositeTransform).ScaleY = this.ScatterView.ControlTileScale;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.anchorLeft = (Grid)this.GetTemplateChild(PART_ANCHOR_LEFT);
            this.anchorTop = (Grid)this.GetTemplateChild(PART_ANCHOR_TOP);
            this.anchorRight = (Grid)this.GetTemplateChild(PART_ANCHOR_RIGHT);
            this.anchorBottom = (Grid)this.GetTemplateChild(PART_ANCHOR_BOTTOM);
        }

        /// <summary>
        /// Called when tile is going to be deleted.
        /// </summary>
        protected override void OnDeleting()
        {
            base.OnDeleting();
            this.UnSubscribeUIChanges();
        }

        /// <summary>
        /// Called before the Windows.UI.Xaml.UIElement.DoubleTapped event occurs.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            base.OnDoubleTapped(e);

            //Show/Hide anchors
            //if (!this.ScatterView.IsDiagramEnabled) return;
            if (!this.IsDiagramElement) return;
            if (!this.InteractiveDiagramElement.IsDiagramEnabled) return;

#if Diagram_v1
            //Tile rotated with angle greater than threshold can't be connected
            if (!this.IsDiagramEnabled) return;
#endif

            if (this.anchorLeft.Visibility == Visibility.Collapsed)
            {
                this.IsPendingConnectionSource = true;
                this.ActivateAnchors();
            }
            else
            {
                this.IsPendingConnectionSource = false;
                this.DeActivateAnchors();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Called when UI must be updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="info">The information.</param>
        protected override async void OnUpdateUi(object sender, UpdateUiInfo info)
        {
            base.OnUpdateUi(sender, info);
            if (info.Operation == UpdateUiOperation.Update)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, this.UpdateDiagramConnections);
            }
        }

#if Diagram_v1
        protected override void OnManipulationUpdatedNoPushin(ManipulationUpdatedEventArgs e)
        {
            base.OnManipulationUpdatedNoPushin(e);
            if (this.HasConnections) this.Tranform.Rotation = 0;
        }
#endif

        /// <summary>
        /// Activates the anchors.
        /// </summary>
        internal void ActivateAnchors()
        {
            this.ResetAnchorsZoom();
            this.anchorLeft.Visibility = Visibility.Visible;
            this.anchorTop.Visibility = Visibility.Visible;
            this.anchorRight.Visibility = Visibility.Visible;
            this.anchorBottom.Visibility = Visibility.Visible;

            this.anchorLeft.PointerPressed += this.OnBeginConnection;
            //this.anchorLeft.PointerEntered += this.OnBeginHovering;
            //this.anchorLeft.PointerExited += this.OnEndHovering;
            this.anchorTop.PointerPressed += this.OnBeginConnection;
            this.anchorRight.PointerPressed += this.OnBeginConnection;
            this.anchorBottom.PointerPressed += this.OnBeginConnection;
        }

        /// <summary>
        /// Creates a new diagram connection for provided pointer id
        /// </summary>
        /// <param name="hitTestInfo">The hit test result.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">anchorInfo - null</exception>
        internal ConnectorInfo CreateDiagramConnection(HitTestInfo hitTestInfo)
        {
            ConnectorInfo connectionInfo = new ConnectorInfo()
            {
                HostLeft = this.Tranform.TranslateX,
                HostTop = this.Tranform.TranslateY,
                HostSize = this.GetSize(),
                HitTestInfo = hitTestInfo
            };

            switch (hitTestInfo)
            {
                case HitTestInfo.AnchorLeft:
                    connectionInfo.Position = new Point(this.Tranform.TranslateX, this.Tranform.TranslateY + connectionInfo.HostSize.Height / 2);
                    connectionInfo.Orientation = ConnectorOrientation.Left;
                    break;
                case HitTestInfo.AnchorTop:
                    connectionInfo.Position = new Point(this.Tranform.TranslateX + connectionInfo.HostSize.Width / 2, this.Tranform.TranslateY);
                    connectionInfo.Orientation = ConnectorOrientation.Top;
                    break;
                case HitTestInfo.AnchorRight:
                    connectionInfo.Position = new Point(this.Tranform.TranslateX + connectionInfo.HostSize.Width, this.Tranform.TranslateY + connectionInfo.HostSize.Height / 2);
                    connectionInfo.Orientation = ConnectorOrientation.Right;
                    break;
                case HitTestInfo.AnchorBottom:
                    connectionInfo.Position = new Point(this.Tranform.TranslateX + connectionInfo.HostSize.Width / 2, this.Tranform.TranslateY + connectionInfo.HostSize.Height);
                    connectionInfo.Orientation = ConnectorOrientation.Bottom;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hitTestInfo), hitTestInfo, null);
            }

            return connectionInfo;
        }

        /// <summary>
        /// Des the activate anchors.
        /// </summary>
        internal void DeActivateAnchors()
        {
            this.anchorLeft.Visibility = Visibility.Collapsed;
            this.anchorTop.Visibility = Visibility.Collapsed;
            this.anchorRight.Visibility = Visibility.Collapsed;
            this.anchorBottom.Visibility = Visibility.Collapsed;

            this.anchorLeft.PointerPressed -= this.OnBeginConnection;
            this.anchorTop.PointerPressed -= this.OnBeginConnection;
            this.anchorRight.PointerPressed -= this.OnBeginConnection;
            this.anchorBottom.PointerPressed -= this.OnBeginConnection;
        }

        /// <summary>
        /// Returns a specific area of a tile
        /// </summary>
        /// <param name="position">The hit point position</param>
        /// <returns>Tile area</returns>
        internal HitTestInfo HitTest(Point position)
        {
            //double x = (position.X * this.ScatterView.ScrollHost.ZoomFactor) - this.ScatterView.ScrollHost.HorizontalOffset;
            //double y = (position.Y * this.ScatterView.ScrollHost.ZoomFactor) - this.ScatterView.ScrollHost.VerticalOffset;
            //Point canvasPoint = new Point(x, y);

            //List<UIElement> elements = VisualTreeHelper.FindElementsInHostCoordinates(canvasPoint, this).ToList();

            //if (elements.Any())
            //{
            //    var anchor = (Grid)elements.FirstOrDefault(e => e is Grid);
            //    switch (anchor.Tag)
            //    {
            //        case "AnchorLeft":
            //            return HitTestInfo.AnchorLeft;
            //        case "AnchorTop":
            //            return HitTestInfo.AnchorTop;
            //        case "AnchorRight":
            //            return HitTestInfo.AnchorRight;
            //        case "AnchorBottom":
            //            return HitTestInfo.AnchorBottom;
            //    }

            //    return HitTestInfo.Content;
            //}

            return HitTestInfo.None;
        }

        /// <summary>
        /// Determines whether provided anchor is a valid diagram connection target .
        /// </summary>
        /// <param name="sourceConnectionId">The source connection identifier.</param>
        /// <param name="anchorInfo">The hit test information.</param>
        /// <returns>
        ///   <c>true</c> if [is valid target] [the specified source connection identifier]; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsValidTarget(DiagramConnection sourceConnection, HitTestInfo anchorInfo)
        {
            ConnectorOrientation sourceOrientation = sourceConnection.OriginOrientation;
            ConnectorOrientation targetOrientation = anchorInfo.ToConnectorOrientation();


            return this.InteractiveDiagramElement.IsValidConnectionTarget(sourceConnection.Origin.Id, sourceOrientation, targetOrientation);
        }

        /// <summary>
        /// Called when a new connection between tiles is started
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Windows.UI.Xaml.Input.PointerRoutedEventArgs" /> instance containing the event data.</param>
        private void OnBeginConnection(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void OnBeginHovering(object sender, PointerRoutedEventArgs e)
        {
            Ellipse target = (Ellipse)sender;
            CompositeTransform tranform = (CompositeTransform)target.RenderTransform;
            tranform.ScaleX = AnchorZoomFactor;
            tranform.ScaleY = AnchorZoomFactor;
            target.Fill = Application.Current.Resources["ScatterViewItemDiagramAnchorBrushHover"] as Brush;
            target.Stroke = Application.Current.Resources["ScatterViewItemDiagramAnchorStrokeBrushHover"] as Brush;
        }

        private void OnEndHovering(object sender, PointerRoutedEventArgs e)
        {
            Ellipse target = (Ellipse)sender;
            CompositeTransform tranform = (CompositeTransform)target.RenderTransform;
            tranform.ScaleX = 1;
            tranform.ScaleY = 1;
        }


        /// <summary>
        /// Called when Transform register a value change.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="dp">The dp.</param>
        private void OnTileUIChanged(DependencyObject sender, DependencyProperty dp)
        {
            this.UpdateDiagramConnections();
#if Diagram_v1
            if (this.HasAnchors && this.IsDiagramEnabled)
            {
                this.DeActivateAnchors();
            }
#endif
        }

        /// <summary>
        /// Subscribes for UI property changes.
        /// </summary>
        private void SubscribeUIChanges()
        {
            this.token1 = this.Tranform.RegisterPropertyChangedCallback(CompositeTransform.TranslateXProperty, this.OnTileUIChanged);
            this.token2 = this.Tranform.RegisterPropertyChangedCallback(CompositeTransform.TranslateYProperty, this.OnTileUIChanged);
            this.token3 = this.Tranform.RegisterPropertyChangedCallback(CompositeTransform.ScaleXProperty, this.OnTileUIChanged);
            this.token4 = this.Tranform.RegisterPropertyChangedCallback(CompositeTransform.ScaleYProperty, this.OnTileUIChanged);
            this.token5 = this.Tranform.RegisterPropertyChangedCallback(CompositeTransform.RotationProperty, this.OnTileUIChanged);
            this.token6 = this.RegisterPropertyChangedCallback(FrameworkElement.WidthProperty, this.OnTileUIChanged);
            this.token7 = this.RegisterPropertyChangedCallback(FrameworkElement.HeightProperty, this.OnTileUIChanged);
        }

        private void UnSubscribeUIChanges()
        {
            this.Tranform.UnregisterPropertyChangedCallback(CompositeTransform.TranslateXProperty, this.token1);
            this.Tranform.UnregisterPropertyChangedCallback(CompositeTransform.TranslateYProperty, this.token2);
            this.Tranform.UnregisterPropertyChangedCallback(CompositeTransform.ScaleXProperty, this.token3);
            this.Tranform.UnregisterPropertyChangedCallback(CompositeTransform.ScaleYProperty, this.token4);
            this.Tranform.UnregisterPropertyChangedCallback(CompositeTransform.RotationProperty, this.token5);
            this.UnregisterPropertyChangedCallback(FrameworkElement.WidthProperty, this.token6);
            this.UnregisterPropertyChangedCallback(FrameworkElement.HeightProperty, this.token7);
        }

        /// <summary>
        /// Updates the diagram connections, if any
        /// </summary>
        private void UpdateDiagramConnections()
        {
            //if (this.ScatterView.IsDiagramEnabled && this.InteractiveElement is IInteractiveDiagramElement)
            //{
            //    if (this.HasConnections)
            //    {
            //        foreach (DiagramConnection diagramConnection in this.DiagramConnections)
            //        {
            //            diagramConnection.Update(this, this.ScatterView.ControlTileScale);
            //        }
            //    }
            //}
        }
    }
}