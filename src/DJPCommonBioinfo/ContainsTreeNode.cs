/*
 * Daniel J Parente
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * Date: 12/20/2011
 * Time: 14:09 PM
 * 
 */
using System;
using System.Collections.Generic;

namespace DJPCommonBioinfo
{
	/// <summary>
	/// Description of EqualityTreeNode.
	/// </summary>
	public class ContainsTreeNode
	{
		public List<int> terminalRows;
		public Dictionary<char, ContainsTreeNode> children;
		public ContainsTreeNode parent;
		
		public ContainsTreeNode(bool isTerminal, ContainsTreeNode parent)
		{
			this.parent = parent;
			this.children = new Dictionary<char, ContainsTreeNode>();
			this.terminalRows = new List<int>();
		}
	}
}
