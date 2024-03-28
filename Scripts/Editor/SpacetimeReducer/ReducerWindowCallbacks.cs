using System;
using UnityEngine;
using UnityEngine.UIElements;
using static SpacetimeDB.Editor.ReducerMeta;

namespace SpacetimeDB.Editor
{
    /// Handles direct UI callbacks, sending async Tasks to ReducerWindowActions.
    /// Subscribed to @ ReducerWindow.setOnActionEvents.
    /// Set @ setOnActionEvents(), unset at unsetActionEvents().
    /// This is essentially the middleware between UI and logic.
    public partial class ReducerWindow
    {
        #region Init from ReducerWindow.cs CreateGUI()
        /// Curry sync Actions from UI => to async Tasks
        private void setOnActionEvents()
        {
            if (topBannerBtn != null)
            {
                // Launches Module docs website
                topBannerBtn.clicked += onTopBannerBtnClick;
            }
            if (actionsCallReducerBtn != null)
            {
                // Run the reducer via CLI
                actionsCallReducerBtn.clicked += ActionsCallReducerBtnClickAsync;
            }
            if (actionArgsTxt != null)
            {
                // Toggles the Run btn
                actionArgsTxt.RegisterValueChangedCallback(onActionTxtValueChanged);
            }
            if (refreshReducersBtn != null)
            {
                // Refresh reducers tree view live from cli
                refreshReducersBtn.clicked += onRefreshReducersBtnClickAsync;
            }
            if (reducersTreeView != null)
            {
                // No need to unsub - Populates the Adds _entityStructure nickname to element
                reducersTreeView.bindItem = onBindReducersTreeViewItem;
            }
            if (reducersTreeView != null)
            {
                // No need to unsub - Creates a new VisualElement within the tree view on new item
                reducersTreeView.makeItem = onMakeReducersTreeViewItem;
            }
            if (reducersTreeView != null)
            {
                // Selected multiple reducers from tree
                reducersTreeView.selectedIndicesChanged += onReducerTreeViewIndicesChanged;
            }
            if (reducersTreeView != null)
            {
                // Single reducer selected from tree
                reducersTreeView.selectionChanged += onReducerTreeViewSelectionChanged;
            }
        }

        /// Cleanup: This should parity the opposite of setOnActionEvents()
        private void unsetOnActionEvents()
        {
            if (topBannerBtn != null)
            {
                topBannerBtn.clicked -= onTopBannerBtnClick;
            }
            if (actionsCallReducerBtn != null)
            {
                actionsCallReducerBtn.clicked -= ActionsCallReducerBtnClickAsync;
            }
            if (refreshReducersBtn != null)
            {
                refreshReducersBtn.clicked -= onRefreshReducersBtnClickAsync;
            }
            if (reducersTreeView != null)
            {
                reducersTreeView.selectedIndicesChanged -= onReducerTreeViewIndicesChanged;
            }
            if (reducersTreeView != null)
            {
                reducersTreeView.selectionChanged -= onReducerTreeViewSelectionChanged;
            }
        }

        /// Cleanup when the UI is out-of-scope
        private void OnDisable() => unsetOnActionEvents();

        /// When a new item is added to a tree view, assign the VisualElement type
        private VisualElement onMakeReducersTreeViewItem() => new Label();

        /// Populates a tree view item with an label element; also assigns the name
        private void onBindReducersTreeViewItem(VisualElement element, int index)
        {
            Label label = (Label)element;
            label.text = _entityStructure.ReducersInfo[index].GetReducerName();
        }
        #endregion // Init from ReducerWindow.cs CreateGUI()


        #region Direct UI Callbacks
        /// When the action text val changes, toggle the Run button
        /// Considers Entity Arity
        private void onActionTxtValueChanged(ChangeEvent<string> evt)
        {
            bool hasVal = !string.IsNullOrEmpty(evt.newValue);

            if (hasVal)
            {
                actionsCallReducerBtn.SetEnabled(hasVal);
                return;
            }

            // Has no val. First, is anything selected?
            int selectedIndex = reducersTreeView.selectedIndex;

            if (selectedIndex == -1)
            {
                actionsCallReducerBtn.SetEnabled(false); // Nothing selected
                return;
            }

            // We can enable if # of aria is 0
            int numAria = getSelectedReducerArityCount();
            actionsCallReducerBtn.SetEnabled(numAria == 0);
        }

        /// Open link to SpacetimeDB Module docs
        private void onTopBannerBtnClick() => Application.OpenURL(TOP_BANNER_CLICK_LINK);

        private async void onRefreshReducersBtnClickAsync()
        {
            // Sanity check
            if (string.IsNullOrEmpty(moduleNameTxt.value))
            {
                return;
            }

            try
            {
                await setReducersTreeViewAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
        }

        private async void ActionsCallReducerBtnClickAsync()
        {
            try
            {
                await callReducerSetUiAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
            }
        }
        #endregion // Direct UI Callbacks
    }
}
