// -----------------------------------------------------------------------
// <copyright file="UpdateUiOperation.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-10-17 @ 10:07
//  edited: 2016-10-17 @ 10:07
// -----------------------------------------------------------------------

namespace MPCExtensions.Infrastructure
{
	/// <summary>
	/// Describes the type of operation we are going to perform on Interactive element
	/// </summary>
	public enum UpdateUiOperation
	{
		Update,
		SetRotationEnabled,
		RemoteLockMode,
		UpdatePreview,
        RemoveDropHighlight
	}
}