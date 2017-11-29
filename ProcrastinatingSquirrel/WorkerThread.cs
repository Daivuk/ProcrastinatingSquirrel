using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProcrastinatingSquirrel
{
	class WorkerThread
	{
		//------------------------------------------------------------------------------------
		// Consts
		//------------------------------------------------------------------------------------
		public static WorkerThread Instance;

		//------------------------------------------------------------------------------------
		// Privates
		//------------------------------------------------------------------------------------
		Thread m_thread;
		List<ThreadStart> m_workQueue;

		//------------------------------------------------------------------------------------
		// Accessors
		//------------------------------------------------------------------------------------

		//------------------------------------------------------------------------------------
		// Functions
		//------------------------------------------------------------------------------------
		public WorkerThread()
		{
			Instance = this;
			m_workQueue = new List<ThreadStart>();
			m_thread = new Thread(WorkerFunc);
			m_thread.IsBackground = true;
	//		m_thread.Priority = ThreadPriority.BelowNormal;
			m_thread.Start();
		}

		public void AddWork(ThreadStart in_workItem)
		{
			lock (m_workQueue)
			{
				foreach (ThreadStart workItem in m_workQueue)
				{
					if (workItem == in_workItem) return; // Already in queue!
				}
				m_workQueue.Add(in_workItem);
			}
		}

		public void Stop()
		{
			m_thread.Abort();
		}

		void WorkerFunc()
		{
#if XBOX
			int[] affinity = new int[]{4};
			Thread.CurrentThread.SetProcessorAffinity(affinity);
#endif
			ThreadStart nextWorkItem = null;
			while (true)
			{
				nextWorkItem = null;
				lock (m_workQueue)
				{
					if (m_workQueue.Count() > 0)
					{
						nextWorkItem = m_workQueue.First();
						m_workQueue.RemoveAt(0);
					}
				}
				if (nextWorkItem != null)
				{
					nextWorkItem();
				}
				Thread.Sleep(100);
			}
		}
	}
}
