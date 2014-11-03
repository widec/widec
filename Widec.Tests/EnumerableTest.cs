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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widec.Linq;

namespace Widec.Tests
{
	[TestFixture]
	public class EnumerableTest
	{
		[TestCase("A","","A","","")]
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

			master.Split(',').Crudonize(
				slave.Split(','), 
				(m,s) => m == s, 
				(m) => creates.Add(m),
				(m,s) => updates.Add(m),
				(s) => deletes.Add(s));

			Assert.AreEqual(expectedCreates, creates.OrderBy(s=>s).UnSplit(","));
			Assert.AreEqual(expectedUpdates, updates.OrderBy(s => s).UnSplit(","));
			Assert.AreEqual(expectedDeletes, deletes.OrderBy(s => s).UnSplit(","));
		}

		[TestCase("A,B")]
		[TestCase("A")]
		[TestCase("")]
		[TestCase("A,B,C")]
		public void UnSplit(string expected)
		{
			Assert.AreEqual(expected, expected.Split(',').UnSplit(","));
		}

	}
}
