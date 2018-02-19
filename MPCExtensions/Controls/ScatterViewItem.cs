// -----------------------------------------------------------------------
// <copyright file="ScatterviewItem.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-08-25 @ 16:46
//  edited: 2017-11-07 @ 17:46
// -----------------------------------------------------------------------

#region Using

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using MPCExtensions.Infrastructure;

#endregion

namespace MPCExtensions.Controls
{
    [TemplatePart(Name = PART_FOCUS, Type = typeof(Rectangle))]
    [TemplatePart(Name = PART_DROP_HIGHLIGHT, Type = typeof(Rectangle))]
    [TemplatePart(Name = PART_REMOTE_LOCK, Type = typeof(Rectangle))]
    [TemplatePart(Name = PART_GROUP_HIGHLIGHT, Type = typeof(Rectangle))]
    public class ScatterViewItem : ConnectableViewItem
    {
        private const string PART_FOCUS = "PART_FOCUS";
        private const string PART_DROP_HIGHLIGHT = "PART_DROP_HIGHLIGHT";
        private const string PART_REMOTE_LOCK = "PART_REMOTE_LOCK";
        private const string PART_GROUP_HIGHLIGHT = "PART_GROUP_HIGHLIGHT";


        private Rectangle dropHighlightRectangle;
        private Rectangle focusRectangle;
        private Rectangle groupHighlightRectangle;
        private Rectangle remoteLockRectangle;

        public ScatterViewItem(ScatterView scatterView) : base(scatterView)
        {
            this.DefaultStyleKey = typeof(ScatterViewItem);
        }

        /// <summary>
        /// Gets or sets the opacity value to use when pointer down event occurs.
        /// </summary>
        /// <value>
        /// The selection opacity.
        /// </value>
        internal double SelectionOpacity { get; set; }

        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call ApplyTemplate. In simplest terms, this means the method is called just before a UI element displays in your app. Override this method to influence the default post-template logic of a class.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.focusRectangle = (Rectangle) this.GetTemplateChild(PART_FOCUS);
            this.dropHighlightRectangle = (Rectangle) this.GetTemplateChild(PART_DROP_HIGHLIGHT);
            this.remoteLockRectangle = (Rectangle) this.GetTemplateChild(PART_REMOTE_LOCK);
            this.groupHighlightRectangle = (Rectangle) this.GetTemplateChild(PART_GROUP_HIGHLIGHT);
        }

        /// <summary>
        /// Called when item is in grouped mode.
        /// </summary>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        protected override void OnGroupedMode(bool isActive)
        {
            base.OnGroupedMode(isActive);
            this.groupHighlightRectangle.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Called when pointer status changes.
        /// </summary>
        /// <param name="isPointerDown">if set to <c>true</c> [is pointer down].</param>
        protected override void OnPointerStatusChanged(bool isPointerDown)
        {
            base.OnPointerStatusChanged(isPointerDown);
            if (isPointerDown)
            {
                this.Opacity = this.SelectionOpacity;
                this.ToggleBorder(true);
            }
            else
            {
                this.Opacity = 1;
                this.ToggleBorder(false);
            }
        }

        /// <summary>
        /// Called when a tile is remotely locked
        /// </summary>
        /// <param name="isLocked">if set to <c>true</c> tile is locked.</param>
        protected override void OnRemotelyLocked(bool isLocked)
        {
            base.OnRemotelyLocked(isLocked);
            this.remoteLockRectangle.Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Called when UI must be updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="info">The information.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected override void OnUpdateUi(object sender, UpdateUiInfo info)
        {
            switch (info.Operation)
            {
                case UpdateUiOperation.RemoveDropHighlight:
                    this.ToggleDropHighlight(false);
                    break;
                default:
                    base.OnUpdateUi(sender, info);
                    break;
            }
        }

        /// <summary>
        /// Toggles the drop highlight.
        /// </summary>
        /// <param name="state">if set to <c>true</c> [state].</param>
        internal void ToggleDropHighlight(bool state)
        {
            this.dropHighlightRectangle.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ToggleBorder(bool state)
        {
#if ShowFocusRectangle
			this.focusRectangle.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
#endif
        }
    }
}