// -----------------------------------------------------------------------
// <copyright file="IDragSource.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-10-14 @ 08:23
//  edited: 2016-10-14 @ 08:23
// -----------------------------------------------------------------------

#region Using

using System;
using System.IO;
using System.Threading.Tasks;

#endregion

namespace MPCExtensions.Infrastructure
{
	public interface IDragSource
	{
		/// <summary>
		/// Gets the drag source thumbnail.
		/// </summary>
		/// <returns></returns>
		Task<Stream> GetThumbnailAsync();
	}
}