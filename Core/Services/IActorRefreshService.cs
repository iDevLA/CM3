﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.Threading.Tasks;

	public delegate void RefreshEvent(Actor actor);

	public interface IActorRefreshService : IService
	{
		event RefreshEvent RefreshBegin;
		event RefreshEvent RefreshComplete;

		bool IsRefreshing
		{
			get;
		}

		void Refresh(Actor offset);
		Task RefreshAsync(Actor offset);
		void PendingRefreshImmediate();
	}
}
