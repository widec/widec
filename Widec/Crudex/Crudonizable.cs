using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Widec.Crudex
{
	public static class Crudonizable
	{	
		#region MasterSlaveCrudonizable

		class MasterSlaveCrudonizable<T> : ICrudonizable<T>
		{
			IEnumerable<T> m_Master;
			IEnumerable<T> m_Slave;
			Func<T,T,bool> m_Compare;
			List<T> m_Creates;
			List<Tuple<T,T>> m_Updates;
			List<T> m_Deletes;

			public MasterSlaveCrudonizable(IEnumerable<T> master, IEnumerable<T> slave, Func<T,T,bool> compare)
			{
				m_Master = master;
				m_Slave = slave;
				m_Compare = compare;	
			}

			public MasterSlaveCrudonizable(IEnumerable<T> master, IEnumerable<T> slave) : 
				this(master, slave, (m,s) => m.GetHashCode() == s.GetHashCode() && m.Equals(s))
			{

			}

			public void Crudonize()
			{
				var master = m_Master.ToArray();
				var slave = m_Slave.ToList();

				m_Creates = new List<T>();
				m_Updates = new List<Tuple<T, T>>();

				for (int masterCounter = master.Length - 1; masterCounter >= 0; masterCounter--)
				{
					var inSlaveList = false;
					for (int slaveCounter = slave.Count - 1; slaveCounter >= 0; slaveCounter--)
					{
						if (m_Compare(master[masterCounter], slave[slaveCounter]))
						{
							// Item is in both lists, add the items to the update list.
							m_Updates.Add(Tuple.Create(master[masterCounter], slave[slaveCounter]));

							// Remove the slave item because it is already found.
							slave.RemoveAt(slaveCounter);

							inSlaveList = true;
							break;
						}
					}
					if (!inSlaveList)
					{
						// Item not in slavelist so add to Create actions
						m_Creates.Add(master[masterCounter]);
					}
				}
				m_Deletes = slave.ToList();
			}

			public IEnumerable<T> Creates
			{
				get 
				{
					if (m_Creates == null)
					{
						Crudonize();
					} 
					return m_Creates.ToArray(); }
			}

			public IEnumerable<T> Deletes
			{
				get 
				{
					if (m_Deletes == null)
					{
						Crudonize();
					}
					return m_Deletes.ToArray(); 
				}
			}

			public IEnumerable<Tuple<T, T>> Updates
			{
				get 
				{
					if (m_Updates == null)
					{
						Crudonize();
					}
					return m_Updates.ToArray(); 
				}
			}
		}

		#endregion

		#region PredicateCrudonizable

		class PredicateCrudonizable<T> : ICrudonizable<T>
		{
			ICrudonizable<T> m_Crudonizable;
			Func<T,bool> m_Predicate;

			public PredicateCrudonizable(ICrudonizable<T> crudonizable, Func<T, bool> predicate)
			{
				m_Crudonizable = crudonizable;
				m_Predicate = predicate;
			}
			
			public IEnumerable<T> Creates
			{
				get { return m_Crudonizable.Creates.Where(m_Predicate); }
			}

			public IEnumerable<T> Deletes
			{
				get { return m_Crudonizable.Deletes.Where(m_Predicate); }
			}

			public IEnumerable<Tuple<T, T>> Updates
			{
				get { return m_Crudonizable.Updates.Where(t => m_Predicate(t.Item1)); }
			}
		}

		#endregion

		public static ICrudonizable<T> Crudonize<T>(this IEnumerable<T> master, IEnumerable<T> slave)
		{
			return new MasterSlaveCrudonizable<T>(master, slave);	
		}

		public static ICrudonizable<T> Crudonize<T>(this IEnumerable<T> master, IEnumerable<T> slave, Func<T,T,bool> compare)
		{
			return new MasterSlaveCrudonizable<T>(master, slave, compare);
		}

		public static void Execute<T>(this ICrudonizable<T> crudonizable, Action<T> create, Action<T,T> update, Action<T> delete)
		{
			foreach(var item in crudonizable.Deletes) { delete(item); }
			foreach (var item in crudonizable.Updates) { update(item.Item1, item.Item2); }
			foreach (var item in crudonizable.Creates) { create(item); }
		}

		public static ICrudonizable<T> Where<T>(this ICrudonizable<T> crudonizable, Func<T, bool> predicate)
		{
			return new PredicateCrudonizable<T>(crudonizable, predicate);		
		}
	}
}
