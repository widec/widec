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
using System.Text;
using System.Threading.Tasks;

namespace Widec.Linq
{
	public static class Enumerable
	{
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
			var updates = new List<Tuple<TMaster,TSlave>>();
			
			for (int masterCounter = master.Length - 1; masterCounter > 0; masterCounter--)
			{
				var inSlaveList = false;
				for (int slaveCounter = slave.Count - 1; slaveCounter > 0; slaveCounter--)
				{
					if (compare(master[masterCounter], slave[slaveCounter]))
					{
						// Item is in both lists, add the items to the update list.
						updates.Add(Tuple.Create(master[masterCounter],slave[slaveCounter]));
						
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
	}
}

