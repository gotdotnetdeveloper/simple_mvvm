using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;

namespace GCTestApp.ErrorProvider
{
	partial class ErrorProvider
	{
		private static class ValidationQueue
		{
			private static HashSet<ErrorProvider> _queue = new HashSet<ErrorProvider>();
			private static readonly object _syncLock = new object();

			public static void Enqueue(ErrorProvider errorProvider)
			{
				lock (_syncLock)
				{
					if (_queue.Add(errorProvider))
						Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, (Action)ProcessQueueOnce);
				}
			}

			private static void ProcessQueueOnce()
			{
				HashSet<ErrorProvider> toProcess;
				lock (_syncLock)
				{
					if (_queue.Count == 0)
						return;
					toProcess = _queue;
					_queue = new HashSet<ErrorProvider>();
				}

				foreach (var provider in toProcess)
				{
					ProcessOneErrorProvider(provider);
				}
			}

			[DebuggerHidden]
			private static void ProcessOneErrorProvider(ErrorProvider provider)
			{
				try
				{
					provider.ProcessValidation();
				}
				// ReSharper disable EmptyGeneralCatchClause
				catch (Exception)
				// ReSharper restore EmptyGeneralCatchClause
				{
				}
			}

			public static void ForceProcessQueue()
			{
				for (int i = 0; i < 2; i++)
				{
					int cnt;
					lock (_queue)
					{
						cnt = _queue.Count;
					}
					if (cnt == 0)
						break;
					ProcessQueueOnce();
				}
			}
		}
	}
}
