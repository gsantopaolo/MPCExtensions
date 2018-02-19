// -----------------------------------------------------------------------
// <copyright file="IDropTarget.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-09-16 @ 16:23
//  edited: 2016-10-17 @ 17:14
// -----------------------------------------------------------------------

#region Using

using System;
using System.Threading.Tasks;

#endregion

namespace MPCExtensions.Infrastructure
{
	public interface IDropTarget
	{
		/// <summary>
		/// Gets or sets a value indicating whether a tile can dropped on this container.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns>
		/// <c>true</c> if this instance can drop the specified source; otherwise, <c>false</c>.
		/// </returns>
		/// <value>
		///   <c>true</c> if this instance can drop; otherwise, <c>false</c>.
		/// </value>
		bool CanDrop(IInteractiveElement source);

		/// <summary>
		/// Determines whether drop target contains the specified element.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>
		///   <c>true</c> if [contains] [the specified element]; otherwise, <c>false</c>.
		/// </returns>
		bool Contains(IInteractiveElement element);

        /// <summary>
        /// Invoke when an item is dropped over a drop target.
        /// </summary>
        /// <param name="interactiveItem">The interactive item.</param>
        /// <param name="confirmCallback">The confirm callback to invoke if user confirms droppind</param>
        /// <param name="notify">if set to <c>true</c> notifies clients.</param>
        /// <returns>
        /// True when item has been properly dropped
        /// </returns>
        void Drop(IInteractiveElement interactiveItem, Action confirmCallback, bool notify = true);

	    /// <summary>
	    /// Invoked when drop operation is completed
	    /// </summary>
	    /// <param name="droppedElement">The dropped element.</param>
	    void DropCompleted(IInteractiveElement droppedElement);
	}
}