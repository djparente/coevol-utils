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
	/// Description of EqualityTree.
	/// </summary>
	public class ContainsTree
	{
		private ContainsTreeNode root;
		
		public ContainsTree(MSA msa)
		{
			this.root = new ContainsTreeNode(false, null);
			
			int curRes = -1;
			int nextRes = -1;
			
			ContainsTreeNode curNode;
			for(int r = 0; r < msa.Rows; r++)
			{
				//Set up pointers
				curRes = -1;
				nextResidue(msa, r, ref curRes);
				nextRes = curRes;
				nextResidue(msa, r, ref nextRes);
				
				curNode = this.root;
				while( curRes < msa.Cols )
				{
					ContainsTreeNode child;
					bool alreadyDefined = curNode.children.TryGetValue(msa[r, curRes], out child);
					if( !alreadyDefined )
					{
						child = new ContainsTreeNode(false, curNode);
						curNode.children.Add(msa[r, curRes], child);
					}
					
					if( nextRes == msa.Cols )
						child.terminalRows.Add(r);
					
					curRes = nextRes;
					nextResidue(msa, r, ref nextRes);
					
					curNode = child;
				}
			}
		}
		
		public List<int> contains(MSA msa, int row)
		{
			//Set up pointers
			int curRes = -1;
			nextResidue(msa, row, ref curRes);
			int nextRes = curRes;
			nextResidue(msa, row, ref nextRes);
			
			ContainsTreeNode curNode = this.root;
			while( curRes < msa.Cols )
			{
				//If there are no more residues, return result
				if( nextRes == msa.Cols )
					return curNode.terminalRows;
				
				//Otherwise, move to the next residue
				ContainsTreeNode child;
				bool exists = curNode.children.TryGetValue(msa[row, curRes], out child);
				if( !exists )
					return null;
				
				curRes = nextRes;
				nextResidue(msa, row, ref nextRes);
				
				curNode = child;
			}
			
			throw new InvalidOperationException("Could not determine if row is contained within ContainsTree");
		}
		
		//Increment the pointer to the next residue in the specified row of the MSA
		static void nextResidue(MSA msa, int row, ref int colPtr)
		{
			colPtr++;
			while( colPtr < msa.Cols && msa[row, colPtr] == '-' )
				colPtr++;
		}
	}
}
