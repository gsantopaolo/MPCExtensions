// -----------------------------------------------------------------------
// <copyright file="InteractiveGroupCoordinator.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-05-19 @ 19:10
//  edited: 2017-05-22 @ 15:22
// -----------------------------------------------------------------------

#region Using

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace MPCExtensions.Infrastructure
{
	/// <summary>
	/// Manages relations between tiles groups
	/// </summary>
	public class InteractiveGroupCoordinator
	{
		public List<InteractiveGroup> Groups { get; } = new List<InteractiveGroup>();

		/// <summary>
		/// Adds a new group.
		/// </summary>
		/// <param name="group">The group.</param>
		public void Add(InteractiveGroup group)
		{
			group.Host = this;
			this.Groups.Add(group);
		}


#if DEBUG
		public void Dump()
		{
			int i = 0;
			foreach (InteractiveGroup interactiveGroup in this.Groups)
			{
				foreach (IInteractiveElement element in interactiveGroup.Items)
				{
					Debug.WriteLine($"Element:{element.ZIndex}");
				}
				i++;
			}
		}
#endif

		/// <summary>
		/// Gets the tiles that are interconnected to provide tile.
		/// </summary>
		/// <param name="tile">The tile.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public List<IInteractiveElement> GetGroupedTiles(IInteractiveElement origin)
		{
			List<IInteractiveElement> result = new List<IInteractiveElement>();
			//Find the group, if any, containing provided tile
			var hostGroups = this.Groups.Where(g => g.Items.Any(t => t == origin)).ToList();
			if (hostGroups.Any())
			{
				//Get the unique tiles from containing groups
				var tiles = hostGroups.SelectMany(g => g.Items).Distinct();
				result.AddRange(tiles);
				if (this.Groups.Count > 1)
				{
					//Search other groups for linked tiles
					foreach (var otherTile in tiles.Where(t => t != origin))
					{
						hostGroups = this.Groups.Where(g => !hostGroups.Contains(g) && g.Items.Any(t => t == otherTile)).ToList();
						if (hostGroups.Any())
						{
							tiles = hostGroups.SelectMany(g => g.Items).Distinct().Except(result);
							result.AddRange(tiles);
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Removes a group.
		/// </summary>
		/// <param name="group">The group.</param>
		public void Remove(InteractiveGroup group)
		{
			group.Host = null;
			this.Groups.Remove(group);
		}

		/// <summary>
		/// Resets the unit.
		/// </summary>
		public void Reset()
		{
			foreach (InteractiveGroup group in this.Groups)
			{
				group.Clear(true);
			}
		}

		/// <summary>
		/// Removes a tile from the groups it is part of
		/// </summary>
		/// <param name="tile">The tile.</param>
		public void UnGroup(IInteractiveElement tile)
		{
			var affectedGroups = this.Groups.Where(g => g.Items.Any(t => t == tile));
			if (affectedGroups.Any())
			{
				foreach (InteractiveGroup group in affectedGroups)
				{
					//If deleted element is the first of the group is the one containing all others aka 'the glue' of the group
					bool isGroupGlue = group.Items.First() == tile;
					group.Remove(tile);
					//Check if tile is not pinned to any other tile, if so ungroups it
					if (group.Items.Count == 1 || isGroupGlue)
					{
						group.Clear(true);
					}

				}
			}
		}
	}
}