using Google.Api.Ads.AdWords.v201809;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuarterMaster.GoogleAdWords
{
    public class ManagedCustomerTreeNode
    {
        /// <summary>
        /// The parent node.
        /// </summary>
        private ManagedCustomerTreeNode parentNode;

        /// <summary>
        /// The account associated with this node.
        /// </summary>
        private ManagedCustomer account;

        /// <summary>
        /// The list of child accounts.
        /// </summary>
        private readonly List<ManagedCustomerTreeNode> childAccounts = new List<ManagedCustomerTreeNode>();

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        public ManagedCustomerTreeNode ParentNode
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        public ManagedCustomer Account
        {
            get { return account; }
            set { account = value; }
        }

        /// <summary>
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Gets the child accounts.
        /// </summary>
        public List<ManagedCustomerTreeNode> ChildAccounts
        {
            get { return childAccounts; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return String.Format("{0}\t{1}\t\t{2}", account.customerId, account.name, this.Depth);
        }

        /// <summary>
        /// Returns a string representation of the current level of the tree and
        /// recursively returns the string representation of the levels below it.
        /// </summary>
        /// <param name="depth">The depth of the node.</param>
        /// <param name="sb">The String Builder containing the tree
        /// representation.</param>
        /// <returns>The tree string representation.</returns>
        public StringBuilder ToTreeString(int depth, StringBuilder sb)
        {
            sb.Append('-', depth * 2);
            sb.Append(this);
            sb.AppendLine();
            foreach (ManagedCustomerTreeNode childAccount in childAccounts)
            {
                childAccount.ToTreeString(depth + 1, sb);
            }
            return sb;
        }

        public StringBuilder ToTreeString2(StringBuilder sb)
        {
            sb.Append(this);
            sb.AppendLine();
            foreach (ManagedCustomerTreeNode childAccount in childAccounts)
            {
                childAccount.ToTreeString2(sb);
            }
            return sb;
        }

        public StringBuilder ToTreeString3(StringBuilder sb, int index = 0)
        {
            this.Depth = index;
            sb.Append(this);
            sb.AppendLine();
            foreach (ManagedCustomerTreeNode childAccount in childAccounts)
            {
                childAccount.ToTreeString3(sb, index + 1);
            }
            return sb;
        }

        public string ToJSON(StringBuilder sb)
        {
            return string.Empty;
        }
    }
}
