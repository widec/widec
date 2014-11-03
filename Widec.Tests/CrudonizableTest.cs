using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widec.Crudex;
using Widec.Linq;

namespace Widec.Tests
{
	[TestFixture()]
	public class CrudonizableTest
	{
		[TestCase("A", "", "A", "", "")]
		[TestCase("A", "A", "", "A", "")]
		[TestCase("", "A", "", "", "A")]
		[TestCase("A,B", "B", "A", "B", "")]
		[TestCase("A,B", "B,C", "A", "B", "C")]
		[TestCase("A,B", "A,B", "", "A,B", "")]
		[TestCase("", "A,B", "", "", "A,B")]
		public void Crudonize(string master, string slave, string expectedCreates, string expectedUpdates, string expectedDeletes)
		{
			List<string> creates = new List<string>();
			List<string> updates = new List<string>();
			List<string> deletes = new List<string>();

			master.Split(',').
				Crudonize(
					slave.Split(','), 
					(m, s) => m == s).
				Execute(
					(m) => creates.Add(m),
					(m, s) => updates.Add(m),
					(s) => deletes.Add(s));

			Assert.AreEqual(expectedCreates, creates.OrderBy(s => s).UnSplit(","));
			Assert.AreEqual(expectedUpdates, updates.OrderBy(s => s).UnSplit(","));
			Assert.AreEqual(expectedDeletes, deletes.OrderBy(s => s).UnSplit(","));
		}
		
	}
}
