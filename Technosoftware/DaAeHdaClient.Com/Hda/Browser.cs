#region Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
// Web: https://www.technosoftware.com 
// 
// The source code in this file is covered under a dual-license scenario:
//   - Owner of a purchased license: SCLA 1.0
//   - GPL V3: everybody else
//
// SCLA license terms accompanied with this source code.
// See SCLA 1.0: https://technosoftware.com/license/Source_Code_License_Agreement.pdf
//
// GNU General Public License as published by the Free Software Foundation;
// version 3 of the License are accompanied with this source code.
// See https://technosoftware.com/license/GPLv3License.txt
//
// This source code is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Collections;

using Technosoftware.DaAeHdaClient.Hda;
using Technosoftware.OpcRcw.Hda;
using Technosoftware.OpcRcw.Comn;
#endregion

namespace Technosoftware.DaAeHdaClient.Com.Hda
{
    /// <summary>
    /// An in-process wrapper an OPC HDA browser object.
    /// </summary>
    internal class  Browser : ITsCHdaBrowser
    {   
        //======================================================================
        // Construction

        /// <summary>
        /// Initializes the object with the specifed COM server.
        /// </summary>
        internal Browser(Server server, IOPCHDA_Browser browser, TsCHdaBrowseFilter[] filters, OpcResult[] results)
        {
            if (browser == null) throw new ArgumentNullException(nameof(browser));

            // save the server object that created the browser.
            m_server = server;

            // save the COM server (released in Dispose()).
            m_browser = browser;

            // save only the filters that were accepted.
            if (filters != null)
            {
                var validFilters = new ArrayList();

                for (var ii = 0; ii < filters.Length; ii++)
                {
                    if (results[ii].Succeeded())
                    {
                        validFilters.Add(filters[ii]);
                    }
                }

                m_filters = new TsCHdaBrowseFilterCollection(validFilters);
            }
        }

        #region IDisposable Members
        /// <summary>
        /// This must be called explicitly by clients to ensure the COM server is released.
        /// </summary>
        public virtual void Dispose() 
        {
            lock (this)
            {
                m_server = null;
                Utilities.Interop.ReleaseServer(m_browser);
                m_browser = null;
            }
        }
        #endregion

        //======================================================================
        // Filters

        /// <summary>
        /// Returns the set of attribute filters used by the browser. 
        /// </summary>
        public TsCHdaBrowseFilterCollection Filters 
        { 
            get 
            {
                lock (this)
                {
                    return (TsCHdaBrowseFilterCollection)m_filters.Clone();
                }
            }
        }

        //======================================================================
        // Browse
        
        /// <summary>
        /// Browses the server's address space at the specified branch.
        /// </summary>
        /// <param name="itemID">The item id of the branch to search.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        public TsCHdaBrowseElement[] Browse(OpcItem itemID)
        {
            IOpcBrowsePosition position;
            var elements = Browse(itemID, 0, out position);

            if (position != null)
            {
                position.Dispose();
            }

            return elements;
        }
        
        /// <summary>
        /// Begins a browsing the server's address space at the specified branch.
        /// </summary>
        /// <param name="itemID">The item id of the branch to search.</param>
        /// <param name="maxElements">The maximum number of elements to return.</param>
        /// <param name="position">The position object used to continue a browse operation.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        public TsCHdaBrowseElement[] Browse(OpcItem itemID, int maxElements, out IOpcBrowsePosition position)
        {
            position = null;

            // interpret invalid values as 'no limit'.
            if (maxElements <= 0)
            {
                maxElements = int.MaxValue;
            }

            lock (this)
            {
                var branchPath = (itemID != null && itemID.ItemName != null)?itemID.ItemName:"";

                // move to the correct position in the server's address space.
                try
                {                   
                    m_browser.ChangeBrowsePosition(OPCHDA_BROWSEDIRECTION.OPCHDA_BROWSE_DIRECT, branchPath);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Browser.ChangeBrowsePosition", e);
                }

                // browse for branches
                var enumerator = GetEnumerator(true);

                var elements = FetchElements(enumerator, maxElements, true);

                // check if max element count reached.
                if (elements.Count >= maxElements)
                {
                    position = new BrowsePosition(branchPath, enumerator, false);
                    return (TsCHdaBrowseElement[])elements.ToArray(typeof(TsCHdaBrowseElement));
                }

                // release enumerator.
                enumerator.Dispose();

                // browse for items
                enumerator = GetEnumerator(false);

                var items = FetchElements(enumerator, maxElements-elements.Count, false);

                if (items != null)
                {
                    elements.AddRange(items);
                }

                // check if max element count reached.
                if (elements.Count >= maxElements)
                {
                    position = new BrowsePosition(branchPath, enumerator, true);
                    return (TsCHdaBrowseElement[])elements.ToArray(typeof(TsCHdaBrowseElement));
                }

                // release enumerator.
                enumerator.Dispose();

                return (TsCHdaBrowseElement[])elements.ToArray(typeof(TsCHdaBrowseElement));
            }
        }

        //======================================================================
        // BrowseNext

        /// <summary>
        /// Continues browsing the server's address space at the specified position.
        /// </summary>
        /// <param name="maxElements">The maximum number of elements to return.</param>
        /// <param name="position">The position object used to continue a browse operation.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        public TsCHdaBrowseElement[] BrowseNext(int maxElements, ref IOpcBrowsePosition position)
        {
            // check arguments.
            if (position == null || position.GetType() != typeof(BrowsePosition))
            {
                throw new ArgumentException("Not a valid browse position object.", nameof(position));
            }

            // interpret invalid values as 'no limit'.
            if (maxElements <= 0)
            {
                maxElements = int.MaxValue;
            }

            lock (this)
            {
                var pos = (BrowsePosition)position;

                var elements = new ArrayList();

                if (!pos.FetchingItems)
                {
                    elements = FetchElements(pos.Enumerator, maxElements, true);

                    // check if max element count reached.
                    if (elements.Count >= maxElements)
                    {
                        return (TsCHdaBrowseElement[])elements.ToArray(typeof(TsCHdaBrowseElement));
                    }

                    // release enumerator.
                    pos.Enumerator.Dispose();
                    
                    pos.Enumerator = null;
                    pos.FetchingItems = true;

                    // move to the correct position in the server's address space.
                    try
                    {                   
                        m_browser.ChangeBrowsePosition(OPCHDA_BROWSEDIRECTION.OPCHDA_BROWSE_DIRECT, pos.BranchPath);
                    }
                    catch (Exception e)
                    {
                        throw Utilities.Interop.CreateException("IOPCHDA_Browser.ChangeBrowsePosition", e);
                    }

                    // create enumerator for items.
                    pos.Enumerator = GetEnumerator(false);
                }

                // fetch next set of items.
                var items = FetchElements(pos.Enumerator, maxElements-elements.Count, false);

                if (items != null)
                {
                    elements.AddRange(items);
                }

                // check if max element count reached.
                if (elements.Count >= maxElements)
                {
                    return (TsCHdaBrowseElement[])elements.ToArray(typeof(TsCHdaBrowseElement));
                }

                // release position object.
                position.Dispose();
                position = null;

                // return elements.
                return (TsCHdaBrowseElement[])elements.ToArray(typeof(TsCHdaBrowseElement));
            }
        }

        #region Private Methods
        /// <summary>
        /// Creates an enumerator for the elements contained with the current branch.
        /// </summary>
        private EnumString GetEnumerator(bool isBranch)
        {
            try
            {
                var browseType = (isBranch)?OPCHDA_BROWSETYPE.OPCHDA_BRANCH:OPCHDA_BROWSETYPE.OPCHDA_LEAF;

                IEnumString pEnumerator = null;
                m_browser.GetEnum(browseType, out pEnumerator);

                return new EnumString(pEnumerator);
            }
            catch (Exception e)
            {
                throw Utilities.Interop.CreateException("IOPCHDA_Browser.GetEnum", e);
            }
        }

        /// <summary>
        /// Fetches the element names and item ids for each element.
        /// </summary>
        private ArrayList FetchElements(EnumString enumerator, int maxElements, bool isBranch)
        {
            var elements = new ArrayList();

            while (elements.Count < maxElements)
            {           
                // fetch next batch of element names.
                var count = BLOCK_SIZE;

                if (elements.Count + count > maxElements)
                {
                    count = maxElements - elements.Count; 
                }
                var names = enumerator.Next(count);

                // check if no more elements found.
                if (names == null || names.Length == 0)
                {
                    break;
                }

                // create new element objects.
                foreach (var name in names)
                {           
                    var element = new TsCHdaBrowseElement();

                    element.Name   = name;
                    // lookup item id for element.
                    try
                    {
                        string itemID = null;
                        m_browser.GetItemID(name, out itemID);
                        
                        element.ItemName    = itemID;
                        element.ItemPath    = null;
                        element.HasChildren = isBranch;
                    }
                    catch
                    {
                        // ignore errors.
                    }

                    elements.Add(element);
                }
            }

            // validate items - this is necessary to set the IsItem flag correctly.
            var results = m_server.ValidateItems((OpcItem[])elements.ToArray(typeof(OpcItem)));

            if (results != null)
            {
                for (var ii = 0; ii < results.Length; ii++)
                {
                    if (results[ii].Result.Succeeded())
                    {
                        ((TsCHdaBrowseElement)elements[ii]).IsItem = true;
                    }
                }
            }

            // return results.
            return elements;
        }
        #endregion

        #region Private Members
        private Server m_server = null;
        private IOPCHDA_Browser m_browser = null;
        private TsCHdaBrowseFilterCollection m_filters = new TsCHdaBrowseFilterCollection();
        private const int BLOCK_SIZE = 10;
        #endregion
    }

    /// <summary>
    /// Stores the state of a browse operation that was halted.
    /// </summary>
    internal class BrowsePosition : TsCHdaBrowsePosition
    {
        /// <summary>
        /// Initializes a the object with the browse operation state information.
        /// </summary>
        /// <param name="branchPath">The item id of branch used in the browse operation.</param>
        /// <param name="enumerator">The enumerator used for the browse operation.</param>
        /// <param name="fetchingItems">Whether the enumerator is return branches or items.</param>
        internal BrowsePosition(string branchPath, EnumString enumerator, bool fetchingItems)
        {
            m_branchPath    = branchPath;
            m_enumerator    = enumerator;
            m_fetchingItems = fetchingItems;
        }

        /// <summary>
        /// The item id of the branch being browsed.
        /// </summary>
        internal string BranchPath
        {
            get => m_branchPath;
            set => m_branchPath = value;
        }

        /// <summary>
        /// The enumerator that was in use when the browse halted.
        /// </summary>
        internal EnumString Enumerator
        {
            get => m_enumerator;
            set => m_enumerator = value;
        }

        /// <summary>
        /// Whether the browse halted while fetching items.
        /// </summary>
        internal bool FetchingItems
        {
            get => m_fetchingItems;
            set => m_fetchingItems = value;
        }

        #region IDisposable Members
        /// <summary>
        /// Releases any unmanaged resources held by the object.
        /// </summary>
        public override void Dispose()
        {
            if (m_enumerator != null)
            {
                m_enumerator.Dispose();
                m_enumerator = null;
            }

            base.Dispose();
        }
        #endregion

        #region Private Members
        private string m_branchPath = null;
        private EnumString m_enumerator = null;
        private bool m_fetchingItems = false;
        #endregion
    }
}
