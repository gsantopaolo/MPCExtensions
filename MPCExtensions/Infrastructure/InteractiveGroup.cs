// -----------------------------------------------------------------------
// <copyright file="InteractiveGroup.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-05-17 @ 16:47
//  edited: 2017-05-19 @ 18:42
// -----------------------------------------------------------------------

#region Using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace MPCExtensions.Infrastructure
{
	public class InteractiveGroup
	{
		private bool isActive;
		private List<IInteractiveElement> items;


		public InteractiveGroupCoordinator Host { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether group is active.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
		/// </value>
		public bool IsActive
		{
			get => this.isActive;
			set
			{
				if (value != this.IsActive)
				{
					this.isActive = value;
					this.OnActiveStateChanged();
				}
			}
		}

		/// <summary>
		/// Gets the items that are part of this group
		/// </summary>
		/// <value>
		/// The items.
		/// </value>
		public List<IInteractiveElement> Items => this.items ?? (this.items = new List<IInteractiveElement>());

		public event EventHandler ActiveStateChanged;

		/// <summary>
		/// Adds a new element to group.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <exception cref="System.ArgumentException">Element already exist</exception>
		public void Add(IInteractiveElement element)
		{
			if (!this.Items.Contains(element))
			{
				element.IsGrouped = true;
				element.LocalGroupTileOperationRequested += this.OnUpdateDependencies;
				element.QueryGroupedTilesStatus += this.OnQueryGroupedTileStatus;
				this.items.Add(element);
			}
			else
			{
				throw new ArgumentException("Element already exist");
			}
		}

		/// <summary>
		/// Clears the group.
		/// </summary>
		/// <param name="forceUngrouped">if set to <c>true</c> force element as ungrouped.</param>
		public void Clear(bool forceUngrouped)
		{
			foreach (IInteractiveElement element in this.Items)
			{
				element.IsGrouped = !forceUngrouped && this.IsGrouped(element);
				element.LocalGroupTileOperationRequested -= this.OnUpdateDependencies;
				element.QueryGroupedTilesStatus -= this.OnQueryGroupedTileStatus;
			}

			this.Items.Clear();
		}

		/// <summary>
		/// Removes an element to group.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <exception cref="ArgumentException">Element doesn't exist</exception>
		public void Remove(IInteractiveElement element)
		{
			if (this.Items.Contains(element))
			{
				element.IsGrouped = this.IsGrouped(element);
				element.LocalGroupTileOperationRequested -= this.OnUpdateDependencies;
				element.QueryGroupedTilesStatus -= this.OnQueryGroupedTileStatus;
				this.items.Remove(element);
			}
			else
			{
				throw new ArgumentException("Element doesn't exist");
			}
		}

		/// <summary>
		/// Called when [active state changed].
		/// </summary>
		protected virtual void OnActiveStateChanged()
		{
			this.ActiveStateChanged?.Invoke(this, EventArgs.Empty);
		}

		private bool IsGrouped(IInteractiveElement element)
		{
			return this.Host.Groups.Where(g => g.Items.Any(t => t == element)).ToList().Count() > 1;
		}

		/// <summary>
		/// Queries tile in a group for its status
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="TileGroupingInfoEventArgs"/> instance containing the event data.</param>
		private void OnQueryGroupedTileStatus(object sender, TileGroupingInfoEventArgs e)
		{
			bool ok = true;
			var gropItemsToProceed = this.Items.Where(t => t != sender);
			if (gropItemsToProceed.Any())
			{
				foreach (IInteractiveElement element in gropItemsToProceed)
				{
					//Inform other items of this group
					bool success = element.QueryTileStatus(e.DeltaX, e.DeltaY);
					if (!success)
					{
						ok = false;
						break;
					}

					//We need to update other groups where this element might be present
					var otherGroups = this.Host.Groups.Where(g => g != this && g.items.Any(t => t == element));

					foreach (InteractiveGroup group in otherGroups.ToList())
					{
						success = group.QueryElement(element, e);
						if (!success)
						{
							ok = false;
							break;
						}
					}
				}

				e.Success = ok;
			}
		}

		/// <summary>
		/// Updates tile that are part of this group
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="TileGroupingInfoEventArgs"/> instance containing the event data.</param>
		private void OnUpdateDependencies(object sender, TileGroupingInfoEventArgs e)
		{
			var gropItemsToProceed = this.Items.Where(t => t != sender);
			if (gropItemsToProceed.Any())
			{
				foreach (IInteractiveElement element in gropItemsToProceed)
				{
					//Inform other items of this group
					element.UpdateFromDependentTile(e.DeltaX, e.DeltaY, e.DeltaRotation, e.IsActive);

					//We need to update other groups where this element might be present
					var otherGroups = this.Host.Groups.Where(g => g != this && g.items.Any(t => t == element));


					foreach (InteractiveGroup group in otherGroups.ToList())
					{
						group.ProcessElement(element, e);
					}
				}
			}
		}

		private void ProcessElement(IInteractiveElement element, TileGroupingInfoEventArgs e)
		{
			this.OnUpdateDependencies(element, e);
		}

		/// <summary>
		/// Queries the element.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="e">The <see cref="TileGroupingInfoEventArgs"/> instance containing the event data.</param>
		/// <returns></returns>
		private bool QueryElement(IInteractiveElement element, TileGroupingInfoEventArgs e)
		{
			this.OnQueryGroupedTileStatus(element, e);
			return e.Success;
		}
	}
}