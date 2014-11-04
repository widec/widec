//Copyright (c) 2014 Wim De Cleen

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Widec.Linq
{
	public static class Enumerable
	{
		#region SequencedItem

		class SequencedItem<T> : ISequencedItem<T>
		{
			public SequencedItem(T item, int sequence)
			{
				Item = item;
				Sequence = sequence;
			}

			public T Item { get; private set; }
			public int Sequence { get; private set; }
		}

		#endregion

		#region ClosureEnumerable

		class ClosureEnumerable<T> : IEnumerable<T>
		{
			Func<IEnumerator<T>> m_GetEnumerator;

			public ClosureEnumerable(Func<IEnumerator<T>> getEnumerator)
			{
				m_GetEnumerator = getEnumerator;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return m_GetEnumerator();
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return m_GetEnumerator();
			}
		}

		#endregion

		#region SequencedEnumerator

		class SequencedEnumerator<T> : IEnumerator<ISequencedItem<T>>
		{
			IEnumerator<T> m_Original;
			ISequencedItem<T> m_Current;
			int m_Sequence;
			int m_StartSequence;

			public SequencedEnumerator(IEnumerator<T> original, int startSequence)
			{
				m_Original = original;
				m_StartSequence = startSequence;
				m_Sequence = startSequence;
				m_Current = null;
			}

			public ISequencedItem<T> Current
			{
				get
				{
					return m_Current;
				}
			}

			public void Dispose()
			{
				m_Original.Dispose();
			}

			object System.Collections.IEnumerator.Current
			{
				get { return m_Current; }
			}

			public bool MoveNext()
			{
				var result = m_Original.MoveNext();
				if (result)
				{
					m_Current = new SequencedItem<T>(m_Original.Current, m_Sequence);
					m_Sequence++;
				}
				return result;
			}

			public void Reset()
			{
				m_Original.Reset();
				m_Sequence = m_StartSequence;
				m_Current = null;
			}
		}

		#endregion

		#region Support

		static IEnumerable<T> GetEnumerable<T>(Func<IEnumerator<T>> getEnumerator)
		{
			return new ClosureEnumerable<T>(getEnumerator);
		}


		#endregion

		/// <summary>
		/// Crudonize the difference between 2 enumerables
		/// </summary>
		/// <typeparam name="TMaster">The type of the master enumerable</typeparam>
		/// <typeparam name="TSlave">The type of the slave enumerable</typeparam>
		/// <param name="masterList">The master enumerable</param>
		/// <param name="slaveList">The slave enumerable</param>
		/// <param name="compare">The compare delegate</param>
		/// <param name="create">The create delegate</param>
		/// <param name="update">The update delegate</param>
		/// <param name="delete">The delete delegate</param>
		public static void Crudonize<TMaster, TSlave>(
			this IEnumerable<TMaster> masterList,
			IEnumerable<TSlave> slaveList,
			Func<TMaster, TSlave, bool> compare,
			Action<TMaster> create,
			Action<TMaster, TSlave> update,
			Action<TSlave> delete)
		{
			var master = masterList.ToArray();
			var slave = slaveList.ToList();

			var creates = new List<TMaster>();
			var updates = new List<Tuple<TMaster, TSlave>>();

			for (int masterCounter = master.Length - 1; masterCounter >= 0; masterCounter--)
			{
				var inSlaveList = false;
				for (int slaveCounter = slave.Count - 1; slaveCounter >= 0; slaveCounter--)
				{
					if (compare(master[masterCounter], slave[slaveCounter]))
					{
						// Item is in both lists, add the items to the update list.
						updates.Add(Tuple.Create(master[masterCounter], slave[slaveCounter]));

						// Remove the slave item because it is already found.
						slave.RemoveAt(slaveCounter);

						inSlaveList = true;
						break;
					}
				}
				if (!inSlaveList)
				{
					// Item not in slavelist so add to Create actions
					creates.Add(master[masterCounter]);
				}
			}

			slave.ForEach(s => delete(s));
			updates.ForEach(ms => update(ms.Item1, ms.Item2));
			creates.ForEach(m => create(m));
		}

		public static string UnSplit(this IEnumerable<string> items, string seperator)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var item in items)
			{
				if (sb.Length == 0)
				{
					sb.Append(item);
				}
				else
				{
					sb.AppendFormat("{0}{1}", seperator, item);
				}
			}
			return sb.ToString();
		}

		public static IEnumerable<ISequencedItem<T>> Sequence<T>(this IEnumerable<T> items)
		{
			return Sequence(items, 0);
		}

		public static IEnumerable<ISequencedItem<T>> Sequence<T>(this IEnumerable<T> items, int startIndex)
		{
			return GetEnumerable(() => new SequencedEnumerator<T>(items.GetEnumerator(), startIndex));
		}

		public static IEnumerable<byte> ToMD5Hash(this IEnumerable<string> items)
		{
			return GetEnumerable(
				() =>
				{
					var sb = new StringBuilder();
					foreach (var s in items)
					{
						sb.Append(s);
					}

					var encoding = new ASCIIEncoding();
					var md5CryptoServiceProvider = new MD5CryptoServiceProvider();
					var md5 = md5CryptoServiceProvider.ComputeHash(encoding.GetBytes(sb.ToString()));

					sb = new StringBuilder();

					return md5.AsEnumerable().GetEnumerator();
				});
		}
	}
}

