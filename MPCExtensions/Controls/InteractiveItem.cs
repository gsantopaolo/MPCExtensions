// -----------------------------------------------------------------------
// <copyright file="InteractiveItem.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-07-17 @ 20:35
//  edited: 2017-07-19 @ 14:01
// -----------------------------------------------------------------------

#define UseNoPushinManipulation
#define DropOnReleaseMode //Uncomment to prevent tiles to go into a library when "thrown over it"

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using AppStudio.Uwp;
using MPCExtensions.Common;
using MPCExtensions.Infrastructure;
using MPCExtensions.Infrastructure.Diagram;

#endregion

namespace MPCExtensions.Controls
{
    public abstract class InteractiveItem : ContentControl
    {
        private const float TargetMinSize = 200F;
        private const float TargetMaxSize = 1000F;
        private const float TargetMinInside = 250F;
        private readonly List<uint> contactPoints = new List<uint>();
        private readonly bool preciseBouncing = true; //NOTE: set it to false to have tile precisely bounding at the edge
        private bool addInertia;
        private bool canForciblyMovePinnedTile;
        private bool canRotate;
        private bool canScale;
        private bool canTranslate;
        private GestureRecognizer gestureRecognizer;
        private Panel host;
        private bool inertiaStarting;

        private IInteractiveElement interactiveElement;
        private bool isCtrlKeyPressed;
        private bool isDragging;
        private bool isMultiFingerMode;
        private ManipulationFilterType manipulationFilterType;

        private CompositeTransform transform;

        private ZoomType zoomType;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveItem"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        protected InteractiveItem(ScatterView scatterView)
        {
            this.ScatterView = scatterView;
            this.Unloaded += this.OnUnLoaded;
            this.InitRecognizer();
            this.InitTransforms();
        }

        /// <summary>
        /// Gets or sets a value indicating whether item can rotate.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can rotate; otherwise, <c>false</c>.
        /// </value>
        public bool CanRotate
        {
            get { return this.canRotate; }
            set
            {
                this.canRotate = value;
                this.gestureRecognizer.GestureSettings = this.InitGestureSettings();
            }
        }

        /// <summary>
        /// Gets the tile identifier.
        /// </summary>
        /// <value>
        /// The tile identifier.
        /// </value>
        public string Id => this.InteractiveElement?.Id;


        /// <summary>
        /// Gets the interactive diagram element or null if item is not a digram item
        /// </summary>
        /// <value>
        /// The interactive diagram element.
        /// </value>
        public IInteractiveDiagramElement InteractiveDiagramElement => this.InteractiveElement as IInteractiveDiagramElement;

        /// <summary>
        /// Gets or sets the interactive element.
        /// </summary>
        /// <value>
        /// The interactive element.
        /// </value>
        public IInteractiveElement InteractiveElement
        {
            get { return this.interactiveElement; }
            set
            {
                if (this.interactiveElement != null)
                {
                    this.interactiveElement.UpdateUiRequested -= this.OnUpdateUi;
                    this.interactiveElement.FrozenStateChanged -= this.OnFrozenStateChanged;
                    this.interactiveElement.Deleting -= this.OnDeleting;
                    this.interactiveElement.ZIndexChanged -= this.OnZIndexChanged;
                    this.interactiveElement.UnDropped -= this.OnUnDropped;
                    this.interactiveElement.GroupedTileOperationRequested -= this.OnGroupedTileOperationRequested;
                    //This prevents radial menu to appear when acting over a tile
                    this.RightTapped -= this.OnRightTapped;
                    this.Holding -= this.OnHolding;
                }

                this.interactiveElement = value;
                this.IsDiagramElement = false;

                if (value != null)
                {
                    this.IsDiagramElement = value is IInteractiveDiagramElement;
                    this.IsNotZoomable = value is INotZoomable;
                    this.interactiveElement.Deleting += this.OnDeleting;
                    this.interactiveElement.ZIndexChanged += this.OnZIndexChanged;
                    //if (!this.ScatterView.IsMiniMapHosted)
                    //{
                    //    this.interactiveElement.CanRotate = this.CanRotate;
                    //    if (!this.ScatterView.IsLibrary) this.interactiveElement.UnDropped += this.OnUnDropped;
                    //}

                    //Handle drop targets elements
                    IDropTarget dropTarget = value as IDropTarget;
                    //if (dropTarget != null) this.ScatterView.DropTargets.Add(this);

                    this.RightTapped += this.OnRightTapped;
                    this.Holding += this.OnHolding;

                    this.interactiveElement.UpdateUiRequested += this.OnUpdateUi;
                    this.interactiveElement.FrozenStateChanged += this.OnFrozenStateChanged;
                    this.interactiveElement.GroupedTileOperationRequested += this.OnGroupedTileOperationRequested;
                    Canvas.SetZIndex(this, this.interactiveElement.ZIndex);

                    if (this.IsNotZoomable)
                    {
                        this.ResetZoom();
                    }
                }
                else
                {
                    this.OnDeleting();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item is not zoomable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is not zoomable; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotZoomable { get; set; }


        /// <summary>
        /// Gets a value indicating whether this element is diagram element.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is diagram element; otherwise, <c>false</c>.
        /// </value>
        protected bool IsDiagramElement { get; private set; }

        protected ScatterView ScatterView { get; private set; }

        /// <summary>
        /// Gets or sets the optional drop target.
        /// </summary>
        /// <value>
        /// The drop target.
        /// </value>
        internal InteractiveItem DropTarget { get; set; }

        /// <summary>
        /// Gets a value indicating whether pointer is down.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is pointer down; otherwise, <c>false</c>.
        /// </value>
        internal bool IsPointerDown { get; private set; }

        /// <summary>
        /// Gets or sets the zoom behavior.
        /// </summary>
        /// <value>
        /// The zoom behavior.
        /// </value>
        internal ZoomType ZoomBehavior
        {
            get { return this.zoomType; }
            set { this.zoomType = value; }
        }

        /// <summary>
        /// Resets the item zoom.
        /// </summary>
        public void ResetZoom()
        {
            //This prevents control tiles to slight left when canvas zoom changes
            this.RenderTransformOrigin = new Point(0, 0);

            //this.transform.ScaleX = this.ScatterView.ControlTileScale;
            //this.transform.ScaleY = this.ScatterView.ControlTileScale;
        }

        /// <summary>
        /// Moves the specified animated.
        /// </summary>
        /// <param name="animated">if set to <c>true</c> [animated].</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="rotation">The rotation.</param>
        private void UpdateView(bool animated, double? x, double? y, double? width, double? height, double? rotation)
        {
            //Ensure that tile is always displayed withing scatterview bounds
            //if (x.HasValue)
            //{
            //    if (x < 0) x = 0;
            //    double maxWidth = this.ScatterView.CanAreaZoom ? this.ScatterView.Width : this.ScatterView.ScrollHost.ActualWidth;
            //    if (x + this.interactiveElement.Width > maxWidth)
            //    {
            //        x = (maxWidth - this.interactiveElement.Width);
            //    }
            //}
            //if (y.HasValue)
            //{
            //    if (y < 0) y = 0;
            //    double maxHeight = this.ScatterView.CanAreaZoom ? this.ScatterView.Height : this.ScatterView.ScrollHost.ActualHeight;
            //    if (y + this.interactiveElement.Height > maxHeight)
            //    {
            //        y = (maxHeight - this.interactiveElement.Height);
            //    }
            //}

            //if (animated)
            //{
            //    if (x.HasValue && x != this.transform.TranslateX)
            //    {
            //        this.transform.AnimateDoublePropertyAsync("TranslateX", this.transform.TranslateX, x.Value, this.ScatterView.AnimationDuration, new CubicEase());
            //    }

            //    if (y.HasValue && y != this.transform.TranslateY)
            //    {
            //        this.transform.AnimateDoublePropertyAsync("TranslateY", this.transform.TranslateY, y.Value, this.ScatterView.AnimationDuration, new CubicEase());
            //    }

            //    if (rotation.HasValue && rotation != this.transform.Rotation)
            //    {
            //        this.transform.AnimateDoublePropertyAsync("Rotation", this.transform.Rotation, rotation.Value, this.ScatterView.AnimationDuration, new CubicEase());
            //    }

            //    if (height.HasValue && height != this.ActualHeight)
            //    {
            //        this.AnimateDoublePropertyAsync("Height", this.ActualHeight, height.Value, this.ScatterView.AnimationDuration, new CubicEase());
            //    }
            //    if (width.HasValue && width != this.ActualWidth)
            //    {
            //        this.AnimateDoublePropertyAsync("Width", this.ActualWidth, width.Value, this.ScatterView.AnimationDuration, new CubicEase());
            //    }
            //}
            //else
            //{
            //    if (rotation.HasValue) this.transform.Rotation = rotation.Value;
            //    if (x.HasValue) this.transform.TranslateX = x.Value;
            //    if (y.HasValue) this.transform.TranslateY = y.Value;
            //    if (height.HasValue) this.Height = height.Value;
            //    if (width.HasValue) this.Width = width.Value;
            //}
        }

        /// <summary>
        /// Called when tile is going to be deleted.
        /// </summary>
        protected virtual void OnDeleting()
        {
        }

        /// <summary>
        /// Called when item is in grouped mode.
        /// </summary>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        protected virtual void OnGroupedMode(bool isActive)
        {
        }

        /// <summary>
        /// Called before the KeyDown event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            this.isCtrlKeyPressed = (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) != 0;
        }

        /// <summary>
        /// Called before the KeyUp event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            this.isCtrlKeyPressed = (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) != 0;
        }

        /// <summary>
        /// Called when inertia process is starting.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ManipulationInertiaStartingEventArgs"/> instance containing the event data.</param>
        protected virtual void OnManipulationInertiaStarting(GestureRecognizer sender, ManipulationInertiaStartingEventArgs args)
        {
            this.inertiaStarting = true;
        }

        /// <summary>
        /// Called during manipulation events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ManipulationUpdatedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs e)
        {
#if UseNoPushinManipulation
            this.OnManipulationUpdatedNoPushin(e);
#else
			this.OnManipulationUpdated(e);
#endif
        }

        /// <summary>
        /// Handles manipulation keeping container within host bordersHandles manipulation keeping container within host borders
        /// </summary>
        /// <param name="e">The <see cref="ManipulationUpdatedEventArgs"/> instance containing the event data.</param>
        protected virtual async void OnManipulationUpdatedNoPushin(ManipulationUpdatedEventArgs e)
        {
            if (this.InteractiveElement == null) return;

            //Ignores manipulation if element is frozen and not pinned
            if (this.InteractiveElement.IsFrozen && !this.interactiveElement.IsPinned || this.interactiveElement.IsFullScreen) return;


            GeneralTransform hostTransform = this.TransformToVisual(this.host);
            Rect itemRect = hostTransform.TransformBounds(new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            double deltaX = 0;
            double deltaY = 0;
            double remoteDeltaX = 0;
            double remoteDeltaY = 0;
            double remoteDeltaRotation = 0;
            bool canGroupMove = true;

            //Constraints X
            if (e.Cumulative.Translation.X < 0)
            {
                deltaX = (itemRect.Left < 0 && e.Cumulative.Translation.X < 0) ? 0 : e.Delta.Translation.X;
                if (this.preciseBouncing && deltaX < 0 && Math.Abs(deltaX) > itemRect.Left) deltaX = -itemRect.Left;
            }
            else
            {
                deltaX = itemRect.Right > this.ScatterView.Width ? 0D : e.Delta.Translation.X;
                double diff = this.ScatterView.Width - itemRect.Right;
                if (this.preciseBouncing && deltaX > diff) deltaX = diff;
            }
            //Constraints Y
            if (e.Cumulative.Translation.Y < 0)
            {
                deltaY = (itemRect.Top < 0) ? 0 : e.Delta.Translation.Y;
                if (this.preciseBouncing && deltaY < 0 && Math.Abs(deltaY) > itemRect.Top) deltaY = -itemRect.Top;
            }
            else
            {
                deltaY = itemRect.Bottom > this.ScatterView.Height ? 0D : e.Delta.Translation.Y;
                double diff = this.ScatterView.Height - itemRect.Bottom;
                if (this.preciseBouncing && deltaY > diff) deltaY = diff;
            }

            //Pinned tiles can't move, unless multifinger/ctrl mode is in use
            this.canForciblyMovePinnedTile = this.InteractiveElement.IsGrouped && ((this.interactiveElement.IsPinned && this.isMultiFingerMode) || (this.interactiveElement.IsPinned && this.isCtrlKeyPressed));

            if (!this.interactiveElement.IsPinned || this.canForciblyMovePinnedTile)
            {
                //If tile is grouped, we need to query tiles in the group is it's ok for every tile to move by specified amount
                if (this.InteractiveElement.IsGrouped) canGroupMove = this.InteractiveElement.QueryDependentTiles(deltaX, deltaY);
                if (canGroupMove)
                {
                    this.transform.TranslateX += deltaX;
                    this.transform.TranslateY += deltaY;
                    remoteDeltaX = deltaX;
                    remoteDeltaY = deltaY;
                    if (this.transform.TranslateX < 0)
                    {
                        this.transform.TranslateX = 0;
                        remoteDeltaX = 0;
                    }
                    if (this.transform.TranslateY < 0)
                    {
                        this.transform.TranslateY = 0;
                        remoteDeltaY = 0;
                    }
                }
            }

            if (this.interactiveElement != null && this.interactiveElement.CanRotate)
            {
                //Rotation not allowed on pinned tiles that can forcibly moved (to ovoid rotation side-effect during drag)
                if (!this.InteractiveElement.IsPinned && !this.canForciblyMovePinnedTile)
                {
                    this.transform.Rotation += e.Delta.Rotation;
                    remoteDeltaRotation = e.Delta.Rotation;
                }
            }
            if (this.zoomType == ZoomType.Resize)
            {
                var newHeight = this.ActualHeight * e.Delta.Scale;
                var newWidth = this.ActualWidth * e.Delta.Scale;
                if (this.interactiveElement != null)
                {
                    if (this.interactiveElement.CanResize)
                    {
                        if (newWidth >= this.InteractiveElement.MinWidth && newHeight >= this.InteractiveElement.MinHeight)
                        {
                            Size properSize = this.interactiveElement.GetProperTileSize(newWidth, newHeight);
                            if (properSize.Width > 0 & properSize.Height > 0)
                            {
                                this.Width = properSize.Width;
                                this.Height = properSize.Height;
                            }
                        }
                    }
                }
            }
            else if (this.zoomType == ZoomType.Scale)
            {
                this.transform.ScaleX = this.transform.ScaleY = e.Delta.Scale;
            }

            if (this.InteractiveElement != null)
            {
                if (!this.interactiveElement.IsPinned || this.canForciblyMovePinnedTile)
                {
                    if (canGroupMove)
                    {
                        this.InteractiveElement.X = this.transform.TranslateX;
                        this.InteractiveElement.Y = this.transform.TranslateY;

                        //Notify tiles in the (optional) group that they need to follow this tile
                        if (this.InteractiveElement.IsGrouped) this.InteractiveElement.UpdateDependentTiles(remoteDeltaX, remoteDeltaY, remoteDeltaRotation, this.IsPointerDown);
                    }
                }
                this.InteractiveElement.Scale = this.transform.ScaleX;
                if (this.interactiveElement.CanResize)
                {
                    this.InteractiveElement.Width = this.Width;
                    this.interactiveElement.Height = this.Height;
                }
                if (this.interactiveElement.CanRotate) this.interactiveElement.Rotation = this.transform.Rotation;

                if (this.isDragging && !this.inertiaStarting)
                {
                    //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => { this.ScatterView.UpdateDropTargets(this); });
                }
            }
        }

        protected virtual void OnPointerCaptureLostBase(object sender, PointerRoutedEventArgs e)
        {
            if (this.inertiaStarting) return;
            try
            {
                this.OnPointerReleasedBase(null, e);
            }
            catch (Exception)
            {
                this.gestureRecognizer.CompleteGesture();
            }

            this.gestureRecognizer.CompleteGesture();
            e.Handled = true;
        }

        /// <summary>
        /// Called when a point move event occurs
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PointerRoutedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPointerMovedBase(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                this.gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(this.host));
            }
            catch
            {
                //see https://connect.microsoft.com/VisualStudio/feedback/details/895979/exceptions-thrown-by-gesturerecognizer
            }

            e.Handled = true;
        }

        /// <summary>
        /// Called when a pointer pressed event occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PointerRoutedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPointerPressedBase(object sender, PointerRoutedEventArgs e)
        {
            if (!e.Pointer.IsInContact)
            {
                return;
            }

            //this.ScatterView.SetActive(this);
            this.IsPointerDown = true;
            this.Focus(FocusState.Programmatic);
            this.inertiaStarting = false;
            this.OnPointerStatusChanged(true);

            if (this.interactiveElement != null && this.InteractiveElement.IsGrouped)
            {
                this.interactiveElement.UpdateDependentTiles(0, 0, 0, true);
                this.OnGroupedMode(true);
            }


            this.CapturePointer(e.Pointer);
            PointerPoint newPoint = e.GetCurrentPoint(this.host);
            try
            {
                this.gestureRecognizer.ProcessDownEvent(newPoint);
            }
            catch (Exception ex)
            {
                //see https://connect.microsoft.com/VisualStudio/feedback/details/895979/exceptions-thrown-by-gesturerecognizer
            }
            finally
            {
                if (!this.contactPoints.Contains(e.Pointer.PointerId)) this.contactPoints.Add(e.Pointer.PointerId);
                this.isMultiFingerMode = this.contactPoints.Count > 1;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Called when pointer is released.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PointerRoutedEventArgs"/> instance containing the event data.</param>
        protected virtual async void OnPointerReleasedBase(object sender, PointerRoutedEventArgs e)
        {
            if (!e.Pointer.IsInContact)
            {
                try
                {
                    this.gestureRecognizer.ProcessUpEvent(e.GetCurrentPoint(this.host));
                    this.ReleasePointerCapture(e.Pointer);
                    if (sender != null)
                    {
#if DropOnReleaseMode
                        if (this.DropTarget != null)
                        {
                            await this.DropTileIntoLibraryAsync();
                        }
#endif

                        this.IsPointerDown = false;
                        this.OnPointerStatusChanged(false);
                        if (this.InteractiveElement != null && this.InteractiveElement.IsGrouped)
                        {
                            this.interactiveElement.UpdateDependentTiles(0, 0, 0, false);
                            this.OnGroupedMode(false);
                        }
                    }
                }

                catch (Exception ex)
                {
                }
                finally
                {
                    if (this.contactPoints.Contains(e.Pointer.PointerId)) this.contactPoints.Remove(e.Pointer.PointerId);

                    if (!this.isDragging)
                    {
                        this.isMultiFingerMode = this.contactPoints.Count > 1;
                    }
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Drops the tile into library.
        /// </summary>
        /// <returns></returns>
        private async Task DropTileIntoLibraryAsync()
        {
            if (this.DropTarget != null)
            {
                IDropTarget dropTarget = (IDropTarget)this.DropTarget.InteractiveElement;
                dropTarget?.Drop(this.InteractiveElement, () => this.ZoomOut(() => dropTarget.DropCompleted(this.InteractiveElement)));;

                ScatterViewItem dt = (ScatterViewItem)this.DropTarget;
                dt?.ToggleDropHighlight(false);
                this.DropTarget = null;
            }
        }

        /// <summary>
        /// Called when pointer status changes.
        /// </summary>
        /// <param name="isPointerDown">if set to <c>true</c> [is pointer down].</param>
        protected virtual void OnPointerStatusChanged(bool isPointerDown)
        {
            if (this.InteractiveElement != null)
            {
                this.InteractiveElement.IsTouching = isPointerDown;
            }
        }

        /// <summary>
        /// Called when pointer wheel event occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PointerRoutedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPointerWheelChangedBase(object sender, PointerRoutedEventArgs e)
        {
            bool shift = (e.KeyModifiers & VirtualKeyModifiers.Shift) == VirtualKeyModifiers.Shift;
            bool ctrl = (e.KeyModifiers & VirtualKeyModifiers.Control) == VirtualKeyModifiers.Control;
            if (ctrl)
            {
                try
                {
                    this.gestureRecognizer.ProcessMouseWheelEvent(e.GetCurrentPoint(this.host), shift, ctrl);
                }
                catch
                {
                    this.gestureRecognizer.CompleteGesture();
                }
                finally
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Called when a tile is remotely locked
        /// </summary>
        /// <param name="isLocked">if set to <c>true</c> tile is locked.</param>
        protected virtual void OnRemotelyLocked(bool isLocked)
        {
        }

        /// <summary>
        /// Configures the interactive element.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="canScale">if set to <c>true</c> [canScale].</param>
        /// <param name="canRotate">if set to <c>true</c> [canRotate].</param>
        /// <param name="translate">if set to <c>true</c> [translate].</param>
        /// <param name="addInertia">if set to <c>true</c> [add inertia].</param>
        /// <param name="zoomType">Type of the zoom.</param>
        /// <param name="isMiniMapHosted">if set to <c>true</c> [is mini map hosted].</param>
        /// <param name="manipulationFilterType">Type of the manipulation filter.</param>
        internal void Configure(Panel host, bool canScale, bool canRotate, bool translate, bool addInertia, ZoomType zoomType,
            ManipulationFilterType manipulationFilterType = ManipulationFilterType.Clamp)
        {
            this.host = host;
            this.zoomType = zoomType;
            this.manipulationFilterType = manipulationFilterType;

            this.canScale = canScale;
            this.canRotate = canRotate;
            this.canTranslate = translate;
            this.addInertia = addInertia;
            this.gestureRecognizer.GestureSettings = this.InitGestureSettings();
            this.gestureRecognizer.ShowGestureFeedback = true;

            CrossSlideThresholds cst = new CrossSlideThresholds
            {
                SelectionStart = 2,
                SpeedBumpStart = 3,
                SpeedBumpEnd = 4,
                RearrangeStart = 5
            };
            this.gestureRecognizer.CrossSlideHorizontally = true;
            this.gestureRecognizer.CrossSlideThresholds = cst;

            if (canScale | canRotate | translate)
            {
                this.gestureRecognizer.ManipulationStarted += this.OnManipulationStarted;
                this.gestureRecognizer.ManipulationUpdated += this.OnManipulationUpdated;
                this.gestureRecognizer.ManipulationCompleted += this.OnManipulationCompleted;
                this.gestureRecognizer.ManipulationInertiaStarting += this.OnManipulationInertiaStarting;
            }
        }

        /// <summary>
        /// Zooms this element in.
        /// </summary>
        internal void ZoomIn()
        {
            //this.transform.AnimateDoubleProperty("ScaleX", this.transform.ScaleX, 1, this.ScatterView.AnimationDuration, new CubicEase());
            //this.transform.AnimateDoubleProperty("ScaleY", this.transform.ScaleY, 1, this.ScatterView.AnimationDuration, new CubicEase());
            //this.AnimateDoubleProperty("Opacity", this.Opacity, 1, this.ScatterView.AnimationDuration, new CubicEase());
        }

        /// <summary>
        /// Zooms this element out.
        /// </summary>
        /// <param name="noAnimation">if set to <c>true</c> no animation is applied</param>
        internal void ZoomOut(Action callback, bool noAnimation = false)
        {
            if (noAnimation)
            {
                this.transform.ScaleX = 0;
                this.transform.ScaleY = 0;
                this.Opacity = 0;
                return;
            }

            //this.transform.AnimateDoubleProperty("ScaleX", this.transform.ScaleX, 0, this.ScatterView.AnimationDuration, new CubicEase());
            //this.transform.AnimateDoubleProperty("ScaleY", this.transform.ScaleY, 0, this.ScatterView.AnimationDuration, new CubicEase());
            //var sb = this.AnimateDoubleProperty("Opacity", this.Opacity, 0, this.ScatterView.AnimationDuration, new CubicEase());
            //if (callback != null)
            //{
            //    sb.Completed += (s, e) => callback();
            //}
        }

        /// <summary>
        /// Implementation of <see cref="FilterManipulation"/> that forces at least <see cref="ManipulationFilter.TargetMinSize"/>
        /// pixels of the manipulation target to remain inside its container.
        /// This filter also makes sure the manipulation target does not become too small or too big.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private FilterManipulationEventArgs Clamp(ManipulationUpdatedEventArgs initialArgs)
        {
            FilterManipulationEventArgs args = new FilterManipulationEventArgs(initialArgs);
            // Get the bounding box of the manipulation target, expressed in the coordinate system of its container
            var rect = this.RenderTransform.TransformBounds(new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            // Make sure the manipulation target does not go completely outside the boundaries of its container
            var translate = new Point
            {
                X = args.Delta.Translation.X,
                Y = args.Delta.Translation.Y
            };


            double areaWidth =  this.ScatterView.Width ;
            double areaHeight = this.ScatterView.Height ;

            if ((args.Delta.Translation.X > 0 && args.Delta.Translation.X > areaWidth - rect.Left - TargetMinInside) ||
                (args.Delta.Translation.X < 0 && args.Delta.Translation.X < TargetMinInside - rect.Right) ||
                (args.Delta.Translation.Y > 0 && args.Delta.Translation.Y > areaHeight - rect.Top - TargetMinInside) ||
                (args.Delta.Translation.Y < 0 && args.Delta.Translation.Y < TargetMinInside - rect.Bottom))
            {
                translate.X = 0;
                translate.Y = 0;
            }

            // Make sure the manipulation target does not become too small, or too big
            float scale = args.Delta.Scale < 1F
                ? (float)Math.Max(TargetMinSize / Math.Min(rect.Width, rect.Height), args.Delta.Scale)
                : (float)Math.Min(TargetMaxSize / Math.Max(rect.Width, rect.Height), args.Delta.Scale);

            scale = args.Delta.Scale;

            args.Delta = new ManipulationDelta
            {
                Expansion = args.Delta.Expansion,
                Rotation = args.Delta.Rotation,
                Scale = scale,
                Translation = translate
            };

            return args;
        }

        /// <summary>
        /// Implementation that forces the center of mass of the
        /// manipulation target to remain inside its container.
        /// </summary>
        /// <param name="initialArgs">The <see cref="ManipulationUpdatedEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private FilterManipulationEventArgs ClampCenterOfMass(ManipulationUpdatedEventArgs initialArgs)
        {
            FilterManipulationEventArgs args = new FilterManipulationEventArgs(initialArgs);

            var rect = this.RenderTransform.TransformBounds(new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            var centerOfMass = new Point
            {
                X = rect.Left + (rect.Width / 2),
                Y = rect.Top + (rect.Height / 2)
            };

            // Apply delta transform to the center of mass
            var transform = new CompositeTransform
            {
                CenterX = args.Pivot.X,
                CenterY = args.Pivot.Y,
                Rotation = args.Delta.Rotation,
                ScaleX = args.Delta.Scale,
                ScaleY = args.Delta.Scale,
                TranslateX = args.Delta.Translation.X,
                TranslateY = args.Delta.Translation.Y
            };

            var transformedCenterOfMass = transform.TransformPoint(centerOfMass);

            // Reset the transformation if the transformed center of mass falls outside the container
            if (transformedCenterOfMass.X < 0 || transformedCenterOfMass.X > this.ScatterView.Width ||
                transformedCenterOfMass.Y < 0 || transformedCenterOfMass.Y > this.ScatterView.Height)
            {
                args.Delta = new ManipulationDelta
                {
                    Rotation = 0F,
                    Scale = 1F,
                    Translation = new Point(0, 0)
                };
            }

            return args;
        }

        /// <summary>
        /// Initializes the gesture settings.
        /// </summary>
        /// <returns></returns>
        private GestureSettings InitGestureSettings()
        {
            //Minimap is not interactive
            //if (this.ScatterView.IsMiniMapHosted) return GestureSettings.None;

            //Element becomes interactive after hold
            //if (this.ScatterView.IsLibrary) return GestureSettings.None;

            GestureSettings settings = GestureSettings.ManipulationMultipleFingerPanning;

            if (this.canScale)
            {
                settings |= GestureSettings.ManipulationScale;
                if (this.addInertia) settings |= GestureSettings.ManipulationScaleInertia;
            }

            if (this.canRotate)
            {
                settings |= GestureSettings.ManipulationRotate;
                if (this.addInertia) settings |= GestureSettings.ManipulationRotateInertia;
            }
            if (this.canTranslate)
            {
                settings |= GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY;
                if (this.addInertia) settings |= GestureSettings.ManipulationTranslateInertia;
            }

            return settings;
        }

        /// <summary>
        /// Initilizes interaction events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void InitRecognizer()
        {
            this.gestureRecognizer = new GestureRecognizer { GestureSettings = GestureSettings.None };
            //if (!this.ScatterView.IsLibrary)
            //{
            //    this.ManipulationMode = ManipulationModes.None;
            //    this.PointerPressed += this.OnPointerPressedCore;
            //    this.PointerReleased += this.OnPointerReleasedCore;
            //    this.PointerMoved += this.OnPointerMovedCore;
            //    this.PointerWheelChanged += this.OnPointerWheelChangedCore;
            //    this.PointerCaptureLost += this.OnPointerCaptureLostBase;
            //}
            //else
            //{
            //    //This is mandatory to allow hosted items to scroll inside LibraryTile
            //    this.ManipulationMode = ManipulationModes.System;
            //}
        }

        /// <summary>
        /// Initializes the transforms.
        /// </summary>
        private void InitTransforms()
        {
            this.transform = new CompositeTransform();
            this.RenderTransformOrigin = new Point(0.5, 0.5);
            this.RenderTransform = this.transform;
        }

        /// <summary>
        /// Gets the interactive item tranform.
        /// </summary>
        /// <value>
        /// The tranform.
        /// </value>
        protected CompositeTransform Tranform => this.transform;

        /// <summary>
        /// Called when item is going to be deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnDeleting(object sender, EventArgs e)
        {
            //IDropTarget is gonna be delete, remove it from available targets...
            IDropTarget dropTarget = sender as IDropTarget;
            if (dropTarget != null)
            {
                //InteractiveItem itemToRemove = this.ScatterView.DropTargets.First(dt => dt.InteractiveElement == dropTarget);
                //this.ScatterView.DropTargets.Remove(itemToRemove);
            }

            this.PointerPressed -= this.OnPointerPressedCore;
            this.PointerCaptureLost -= this.OnPointerCaptureLostCore;
            this.PointerMoved -= this.OnPointerMovedCore;
            this.PointerReleased -= this.OnPointerReleasedCore;
            this.PointerWheelChanged -= this.OnPointerWheelChangedCore;

            this.InteractiveElement = null;
            this.ScatterView = null;
        }

        private void OnFrozenStateChanged(object sender, EventArgs e)
        {
            this.IsHitTestVisible = !this.InteractiveElement.IsFrozen;
        }

        /// <summary>
        /// Called when another tile (part of a group) want to acts on this tile
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TileGroupingInfoEventArgs"/> instance containing the event data.</param>
        private void OnGroupedTileOperationRequested(object sender, TileGroupingInfoEventArgs e)
        {
            //QueryMode indicates that a tile is asking whether this tile is within container bounds
            if (e.IsQueryMode)
            {
                GeneralTransform hostTransform = this.TransformToVisual(this.host);
                Rect itemRect = hostTransform.TransformBounds(new Rect(0, 0, this.ActualWidth, this.ActualHeight));

                if (itemRect.X + e.DeltaX < 0 || itemRect.Right + e.DeltaX > this.ScatterView.Width)
                {
                    e.Success = false;
                    return;
                }
                if (itemRect.Y + e.DeltaY < 0 || itemRect.Bottom + e.DeltaY > this.ScatterView.Height)
                {
                    e.Success = false;
                    return;
                }

                e.Success = true;
            }
        }

        /// <summary>
        /// Called when user is holding over this element
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="HoldingRoutedEventArgs"/> instance containing the event data.</param>
        private void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            e.Handled = !this.InteractiveElement.CanMenuAppear;
        }

        /// <summary>
        /// Called when a manipulation is completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ManipulationCompletedEventArgs"/> instance containing the event data.</param>
        private async void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            //Ignores manipulation if element is frozen and not pinned
            if (this.InteractiveElement != null && this.InteractiveElement.IsFrozen && !this.interactiveElement.IsPinned) return;

            if (this.isDragging)
            {
#if !DropOnReleaseMode
                await this.DropTileIntoLibraryAsync();
#endif
                this.isDragging = false;
            }

            //Informs previews that that they must sync
            //if (!this.ScatterView.IsMiniMapHosted && this.interactiveElement != null)
            //{
            //    this.InteractiveElement.UpdatePreview();
            //    await this.interactiveElement.UpdateRemoteClients();
            //}


            this.isMultiFingerMode = this.contactPoints.Count > 1;
            this.canForciblyMovePinnedTile = false;
        }

        /// <summary>
        /// Called when a manipulation starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ManipulationStartedEventArgs"/> instance containing the event data.</param>
        private void OnManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            this.isDragging = true;
        }

        /// <summary>
        /// Handles manipulation the old way
        /// </summary>
        /// <param name="e">The <see cref="ManipulationUpdatedEventArgs"/> instance containing the event data.</param>
        private async void OnManipulationUpdated(ManipulationUpdatedEventArgs e)
        {
            //Ignores manipulation if element is frozen
            if (this.InteractiveElement != null && this.InteractiveElement.IsFrozen) return;

            //Filter arguments
            FilterManipulationEventArgs filteredArgs;
            switch (this.manipulationFilterType)
            {
                case ManipulationFilterType.Clamp:
                    filteredArgs = this.Clamp(e);
                    break;
                case ManipulationFilterType.RotateAroundCenter:
                    filteredArgs = this.RotateAroundCenter(e);
                    break;
                case ManipulationFilterType.ClampCenterOfMass:
                    filteredArgs = this.ClampCenterOfMass(e);
                    break;
                default:
                    filteredArgs = new FilterManipulationEventArgs(e);
                    break;
            }

            this.transform.TranslateX += filteredArgs.Delta.Translation.X;
            this.transform.TranslateY += filteredArgs.Delta.Translation.Y;

            if (this.interactiveElement != null && this.interactiveElement.CanRotate) this.transform.Rotation += filteredArgs.Delta.Rotation;
            if (this.zoomType == ZoomType.Resize)
            {
                var newHeight = this.ActualHeight * filteredArgs.Delta.Scale;
                var newWidth = this.ActualWidth * filteredArgs.Delta.Scale;
                if (this.interactiveElement != null)
                {
                    if (this.interactiveElement.CanResize)
                    {
                        if (newWidth >= this.InteractiveElement.MinWidth && newHeight >= this.InteractiveElement.MinHeight)
                        {
                            Size properSize = this.interactiveElement.GetProperTileSize(newWidth, newHeight);
                            this.Width = properSize.Width;
                            this.Height = properSize.Height;
                        }
                    }
                }
            }
            else if (this.zoomType == ZoomType.Scale)
            {
                this.transform.ScaleX = this.transform.ScaleY = filteredArgs.Delta.Scale;
            }

            if (this.InteractiveElement != null)
            {
                this.InteractiveElement.X = this.transform.TranslateX;
                this.InteractiveElement.Y = this.transform.TranslateY;
                this.InteractiveElement.Scale = this.transform.ScaleX;
                if (this.interactiveElement.CanResize)
                {
                    this.InteractiveElement.Width = this.Width;
                    this.interactiveElement.Height = this.Height;
                }
                if (this.interactiveElement.CanRotate) this.interactiveElement.Rotation = this.transform.Rotation;

                if (this.isDragging && !this.inertiaStarting)
                {
                    //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => { this.ScatterView.UpdateDropTargets(this); });
                }
            }
        }

        private void OnPointerCaptureLostCore(object sender, PointerRoutedEventArgs e)
        {
            this.OnPointerCaptureLostBase(sender, e);
        }

        private void OnPointerMovedCore(object sender, PointerRoutedEventArgs e)
        {
            this.OnPointerMovedBase(sender, e);
        }

        private void OnPointerPressedCore(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                this.OnPointerPressedBase(sender, e);
            }
            catch
            {
            }
        }

        private void OnPointerReleasedCore(object sender, PointerRoutedEventArgs e)
        {
            this.OnPointerReleasedBase(sender, e);
        }

        private void OnPointerWheelChangedCore(object sender, PointerRoutedEventArgs e)
        {
            this.OnPointerWheelChangedBase(sender, e);
        }

        /// <summary>
        /// Called when user right tapps over this element
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RightTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = !this.InteractiveElement.CanMenuAppear;
        }

        /// <summary>
        /// Called when user un drops a tile.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnUnDropped(object sender, EventArgs e)
        {
            //bool isUnDropped = this.ScatterView.UnDrop(this);
            //if (isUnDropped) this.InteractiveElement.UnDropped -= this.OnUnDropped;
        }

        private void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= this.OnUnLoaded;
            this.InteractiveElement = null;
        }

        /// <summary>
        /// Called when UI must be updated.
        /// </summary>
        /// <param name="info">The information.</param>
        protected virtual void OnUpdateUi(object sender, UpdateUiInfo info)
        {
            switch (info.Operation)
            {
                case UpdateUiOperation.Update:
                case UpdateUiOperation.UpdatePreview:
                    this.OnGroupedMode(info.IsGroupFrameVisible);

                    //if (info.Operation == UpdateUiOperation.UpdatePreview && !this.ScatterView.IsMiniMapHosted) return;

                    this.UpdateView(info.Animated, info.X, info.Y, info.Width, info.Height, info.Rotation);
                    //if (!this.ScatterView.IsMiniMapHosted)
                    //{
                    //    if (info.X.HasValue) this.InteractiveElement.X = info.X.Value;
                    //    if (info.Y.HasValue) this.InteractiveElement.Y = info.Y.Value;
                    //    if (info.Width.HasValue) this.InteractiveElement.Width = info.Width.Value;
                    //    if (info.Height.HasValue) this.InteractiveElement.Height = info.Height.Value;
                    //    if (info.Rotation.HasValue) this.InteractiveElement.Rotation = info.Rotation.Value;
                    //}
                    break;
                case UpdateUiOperation.SetRotationEnabled:
                    this.CanRotate = info.IsRotationEnabled;
                    //double rotation = !info.IsRotationEnabled ? 0 : (info.Rotation ?? 0);
                    //this.UpdateView(true, null, null, null, null, rotation);
                    //if (!this.ScatterView.IsMiniMapHosted) this.InteractiveElement.Rotation = rotation;
                    break;
                case UpdateUiOperation.RemoteLockMode:
                    this.OnRemotelyLocked(info.IsRemotelyLocked);
                    break;
            }
        }

        /// <summary>
        /// Sets the ZIndex of this ite
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void OnZIndexChanged(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (this.InteractiveElement != null) Canvas.SetZIndex(this, this.InteractiveElement.ZIndex);
                });
        }

        /// <summary>
        /// Implementation that forces the rotation to be about
        /// the center of the manipulation target.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private FilterManipulationEventArgs RotateAroundCenter(ManipulationUpdatedEventArgs initialArgs)
        {
            FilterManipulationEventArgs args = new FilterManipulationEventArgs(initialArgs);

            // Get the bounding box of the manipulation target, expressed in the coordinate system of its container
            var rect = this.RenderTransform.TransformBounds(new Rect(0, 0, this.ScatterView.Width, this.ScatterView.Height));

            args.Pivot = new Point
            {
                X = rect.Left + (rect.Width / 2),
                Y = rect.Top + (rect.Height / 2)
            };

            return args;
        }
    }
}