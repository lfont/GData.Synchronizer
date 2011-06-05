using System;
using System.Threading;

namespace GData.Synchronizer.Tasks
{
	public interface IGDataTask<TItem>
	{
        string Id { get; set; }
        Predicate<TItem> Prerequisites { get; }
		void Do(TItem item, CancellationToken cancellationToken);
	}
}
