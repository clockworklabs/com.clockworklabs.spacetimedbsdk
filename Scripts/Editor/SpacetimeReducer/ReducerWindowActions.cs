using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpacetimeDB.Editor
{
    /// Unlike ReducerWindowCallbacks, these are not called *directly* from UI.
    /// Runs an action -> Processes isSuccess -> calls success || fail @ ReducerWindowCallbacks.
    /// ReducerWindowCallbacks should handle try/catch (except for init chains).
    public partial class ReducerWindow
    {
        #region Init from ReducerWindow.CreateGUI
        /// Gets selected server + identity. On err, refers to PublisherWindow
        /// Initially called by ReducerWindow @ CreateGUI.
        private async Task initDynamicEventsFromReducerWindow()
        {
            Debug.Log("initDynamicEventsFromReducerWindow");
            
            await ensureCliInstalledAsync();
            await setSelectedServerTxtAsync();
            await setSelectedIdentityTxtAsync();
            
            //// TODO: If `spacetime list` ever returns db names (not just addresses),
            //// TODO: Auto list them in dropdown
            setSelectedModuleTxtAsync();
            
            // At this point, sanity  check an existing module name to continue
            if (string.IsNullOrEmpty(moduleNameTxt.value))
            {
                return;
            }
            
            await setReducersTreeViewAsync();
        }

        /// Pulls from publisher, if any
        private void setSelectedModuleTxtAsync()
        {
            // Other editor tools may want to utilize this value,
            // since the CLI has no idea what you're "default" Module is
            moduleNameTxt.value = EditorPrefs.GetString(
                SpacetimeMeta.EDITOR_PREFS_MODULE_NAME_KEY, 
                defaultValue: "");

            moduleNameTxt.Focus();
            moduleNameTxt.SelectAll();
        }

        /// [Slow] Loads reducer names into #reducersTreeView -> Enable
        /// Doc | https://docs.unity3d.com/2022.3/Documentation/Manual/UIE-uxml-element-TreeView.html
        private async Task setReducersTreeViewAsync()
        {
            toggleRefreshReducersBtn(isRefreshing: true);
            
            // TODO: Cache the vals using ViewDataKey -> add a refresh btn
            string moduleName = moduleNameTxt.value;
            GetEntityStructureResult entityStructureResult = await SpacetimeDbCli.GetEntityStructure(moduleName);
            
            bool isSuccess = entityStructureResult is { HasEntityStructure: true };
            if (!isSuccess)
            { 
                Debug.Log("Warning: Searched for reducers; found none");
                return;
            }
            
            // Success: Load entity names into reducer tree view - cache _entityStructure state
            onSetReducersTreeViewSuccess(entityStructureResult);
        }

        /// Success: Load entity names into reducer tree view - cache _entityStructure state
        private void onSetReducersTreeViewSuccess(GetEntityStructureResult entityStructureResult)
        {
            // TODO: +with friendly styled syntax hint children
            _entityStructure = entityStructureResult.EntityStructure;

            reducersTreeView.Clear();
            List<TreeViewItemData<string>> treeViewItems = new();

            for (int i = 0; i < _entityStructure.ReducersInfo.Count; i++)
            {
                ReducerInfo reducerInfo = _entityStructure.ReducersInfo[i];
                
                // TODO: Subitems, eg: treeViewSubItemsData.Add(new TreeViewItemData<string>(subItem.Id, subItem.Name));
                List<TreeViewItemData<string>> treeViewSubItemsData = new(); // Children

                TreeViewItemData<string> treeViewItemData = new(
                    id: i,
                    reducerInfo.GetReducerName(),
                    treeViewSubItemsData);

                treeViewItems.Add(treeViewItemData);
            }

            reducersTreeView.SetRootItems(treeViewItems);
            reducersTreeView.Rebuild();

            // Enable the TreeView, hide refreshing status
            reducersTreeView.SetEnabled(true); // Possibly unnecessary, but for sanity
            reducersTreeView.style.display = DisplayStyle.Flex;
            
            toggleRefreshReducersBtn(isRefreshing: false);
        }

        /// Show the actions foldout + syntax hint
        private void setAction(int index)
        {
            int argsCount = _entityStructure.ReducersInfo[index].ReducerEntity.Arity;
            List<string> styledSyntaxHints = _entityStructure.ReducersInfo[index].GetNormalizedStyledSyntaxHints();

            if (argsCount > 0)
            {
                // Has args: Set txt + txt label -> enable
                actionArgsTxt.value = "";
                actionArgsTxt.style.display = DisplayStyle.Flex;
                actionArgsTxt.SetEnabled(true);
                
                // Set syntax hint label -> show
                actionsSyntaxHintLabel.text = string.Join("  ", styledSyntaxHints);
                actionsSyntaxHintLabel.style.display = DisplayStyle.Flex;    
            }
            else
            {
                // Disable txt, set label to sanity check no args
                actionArgsTxt.SetEnabled(false);
                actionsSyntaxHintLabel.text = ""; // Just empty so we don't shift the UI
            }
            
            actionsFoldout.style.display = DisplayStyle.Flex;
        }

        /// We only expect a single index changed
        /// (!) Looking for name? See onReducerTreeViewSelectionChanged()
        private void onReducerTreeViewIndicesChanged(IEnumerable<int> selectedIndices)
        {
            // Get selected index, or fallback to -1
            int selectedIndex = selectedIndices != null && selectedIndices.Any() 
                ? selectedIndices.First()
                : -1;

            if (selectedIndex == -1)
            {
                // User pressed ESC
                actionsFoldout.style.display = DisplayStyle.None;
                return;
            }

            // Since we have a real selection, show the actions foldout + syntax hint
            setAction(selectedIndex);
        }
        
        /// We only expect a single element changed
        /// (!) Looking for index? See onReducerTreeViewIndicesChanged()
        private void onReducerTreeViewSelectionChanged(IEnumerable<object> obj)
        {
            actionArgsTxt.value = "";

            // The first element should be the string name of the element Label.
            // Fallback to null if obj count is null or 0
            bool isNullOrEmpty = obj == null || !obj.Any();
            if (isNullOrEmpty)
            {
                actionsCallReducerBtn.SetEnabled(false);
                return;
            }

            // We have a new selection - when we run, we'll use this name
            // string selectedReducerName = obj.First().ToString();
            toggleActionCallBtnIfArityOk();
            // actionArgsTxt.Focus(); // If someone wants to scroll down list, this will interrupt the UX
        }

        /// 0 args? Enable! Else, ensure some input. Else, disable.
        private void toggleActionCallBtnIfArityOk()
        {
            int argsCount = getSelectedReducerArityCount();
            if (argsCount == 0)
            {
                // No args: Enable right away
                actionsCallReducerBtn.SetEnabled(true);
                return;
            }

            // Ensure some input
            bool hasInput = !string.IsNullOrWhiteSpace(actionArgsTxt.value);
            actionsCallReducerBtn.SetEnabled(hasInput);
            
            // TODO: Cache a map of reducer to arg field to persist the previous test
        }

        /// An alternative to `_entityStructure.ReducersInfo[reducersTreeView.selectedIndex].GetReducerName()`
        /// getting the info, instead, from the selected tree view element
        private string getSelectedReducerName() => reducersTreeView.selectedItem as string;
        
        /// How many args does this reducer handle?
        private int getSelectedReducerArityCount() =>
            _entityStructure.ReducersInfo[reducersTreeView.selectedIndex]
                .ReducerEntity.Arity;
        
        private async Task setSelectedServerTxtAsync() 
        {
            GetServersResult getServersResult = await SpacetimeDbCli.GetServersAsync();
            
            bool isSuccess = getServersResult.HasServer && !getServersResult.HasServersButNoDefault;
            if (!isSuccess)
            {
                showErrorWrapper("<b>Failed to get servers:</b>\n" +
                    "Setup via top menu `Window/SpacetimeDB/Publisher`");
                return;
            }
            
            // Success
            SpacetimeServer defaultServer = getServersResult.Servers
                .First(server => server.IsDefault);
            serverNameTxt.value = defaultServer.Nickname;
        }

        /// Load selected identities => set readonly identity txt
        private async Task setSelectedIdentityTxtAsync()
        {
            GetIdentitiesResult getIdentitiesResult = await SpacetimeDbCli.GetIdentitiesAsync();

            bool isSuccess = getIdentitiesResult.HasIdentity && !getIdentitiesResult.HasIdentitiesButNoDefault;
            if (!isSuccess)
            {
                showErrorWrapper("<b>Failed to get identities:</b>\n" +
                    "Setup via top menu `Window/SpacetimeDB/Publisher`");
                return;
            }

            // Success
            SpacetimeIdentity defaultIdentity = getIdentitiesResult.Identities
                .First(id => id.IsDefault);
            identityNameTxt.value = defaultIdentity.Nickname;
        }

        private async Task ensureCliInstalledAsync()
        {
            // Ensure CLI installed -> Show err (refer to PublisherWindow), if not
            SpacetimeCliResult isSpacetimeDbCliInstalledResult = await SpacetimeDbCli.GetIsSpacetimeCliInstalledAsync();

            bool isCliInstalled = !isSpacetimeDbCliInstalledResult.HasCliErr;
            if (!isCliInstalled)
            {
                showErrorWrapper("<b>SpacetimeDB CLI is not installed:</b>\n" +
                    "Setup via top menu `Window/SpacetimeDB/Publisher`");
                return;
            }
            
            // Success: Do nothing!
        }

        /// Initially called by ReducerWindow @ CreateGUI
        /// - Set to the initial state as if no inputs were set.
        /// - This exists so we can show all ui elements simultaneously in the
        ///   ui builder for convenience.
        /// - (!) If called from CreateGUI, after a couple frames,
        ///       any persistence from `ViewDataKey`s may override this.
        private void resetUi()
        {
            serverNameTxt.value = "";
            identityNameTxt.value = "";

            toggleRefreshReducersBtn(isRefreshing: true);
            reducersTreeView.Clear();
            reducersTreeView.style.display = DisplayStyle.None;
            
            resetActionsFoldoutUi();
            resetActionResultFoldoutUi();
        }
        
        /// Refreshing + disables + sets action txt +
        /// hides/clears reducer tree + actions/results
        private void toggleRefreshReducersBtn(bool isRefreshing)
        {
            refreshReducersBtn.SetEnabled(!isRefreshing);
            refreshReducersBtn.text = isRefreshing 
                ? SpacetimeMeta.GetStyledStr(SpacetimeMeta.StringStyle.Action, "Refreshing ...") 
                : "<b>Refresh</b>"; // TODO: Mv this to meta

            if (!isRefreshing)
                return;
            
            // Refreshing
            reducersTreeView.selectedIndex = -1;
            reducersTreeView.style.display = DisplayStyle.None;
            actionsFoldout.style.display = DisplayStyle.None;
            actionsResultFoldout.style.display = DisplayStyle.None;
        }

        private void resetActionResultFoldoutUi()
        {
            actionsResultFoldout.value = false;
            actionsResultFoldout.style.display = DisplayStyle.None;
            actionsResultLabel.text = "";
        }

        private void resetActionsFoldoutUi()
        {
            actionsFoldout.style.display = DisplayStyle.None;
            actionsSyntaxHintLabel.style.display = DisplayStyle.None;
            actionsCallReducerBtn.SetEnabled(false);
        }
        #endregion // Init from ReducerWindow.CreateGUI


        /// Wraps the entire body in an error message, generally when there's
        /// a cli/server/identity error that should be configured @ PublisherWindow (not here).
        /// Wraps text in error style color.
        /// Throws.
        private void showErrorWrapper(string friendlyError)
        {
            errorCoverLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                $"<b>Error:</b> {friendlyError}");
            errorCover.style.display = DisplayStyle.Flex;

            throw new Exception(friendlyError);
        }

        /// Toggle module|args txt, reducers tree based on is[Not]Calling
        /// Intended for start/done from callReducerSetUiAsync
        /// If calling, set `callingReducerName`
        private void toggleCallReducerUi(string callingReducerName = null)
        {
            bool isCalling = callingReducerName != null;
            
            moduleNameTxt.SetEnabled(!isCalling);
            reducersTreeView.SetEnabled(!isCalling);
            actionsSyntaxHintLabel.SetEnabled(!isCalling); // For consistency

            if (isCalling)
            {
                actionArgsTxt.SetEnabled(false);
            }
            else
            {
                // We need to verify arity to enable
                int arityCount = getSelectedReducerArityCount();
                actionArgsTxt.SetEnabled(arityCount > 0);
            }
            
            actionsCallReducerBtn.SetEnabled(!isCalling);
            actionsCallReducerBtn.text = isCalling 
                ? SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Action, 
                    $"Calling {callingReducerName} ...") 
                : "<b>Call Reducer</b>"; // TODO: Mv this to meta
            
            actionsResultFoldout.style.display = isCalling 
                ? DisplayStyle.None 
                : DisplayStyle.Flex;
            actionsResultFoldout.value = !isCalling; // Expand
        }

        private async Task callReducerSetUiAsync()
        {
            string reducerName = getSelectedReducerName();
            toggleCallReducerUi(reducerName);
            
            CallReducerRequest request = new CallReducerRequest(
                moduleNameTxt.value,
                reducerName,
                actionCallAsIdentityTxt.value,
                actionArgsTxt.value);
            
            SpacetimeCliResult cliResult = await SpacetimeDbReducerCli.CallReducerAsync(request);
            bool isSuccess = string.IsNullOrEmpty(cliResult.CliError);
            if (!isSuccess)
                onCallReducerFail(request, cliResult);
            else
                onCallReducerSuccess(request,cliResult);
            
            toggleCallReducerUi(callingReducerName: null);
        }

        private void onCallReducerSuccess(
            CallReducerRequest request, 
            SpacetimeCliResult cliResult)
        {
            // Module
            string module = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action, 
                $"<b>Module:</b> {request.ModuleName} |");
            
            // Args
            bool hasArgs = !string.IsNullOrWhiteSpace(request.Args);
            string args = hasArgs ? $"`{request.Args}`" : "(None)";
            string inputArgs = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action, 
                $"<b>Args:</b> {args}");
            
            // Result
            string parsedOutput = string.IsNullOrWhiteSpace(cliResult.CliOutput)
                ? SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Success,
                    "<b>Success:</b> Done (200)")
                : cliResult.CliOutput;

            // Combined
            actionsResultLabel.text = $"{module} {inputArgs}\n{parsedOutput}";
        }

        /// Show result error
        private void onCallReducerFail(
            CallReducerRequest request, 
            SpacetimeCliResult cliResult)
        {
            string clippedParsedError = string.IsNullOrWhiteSpace(cliResult.CliError)
                ? "(No error message)"
                : Utils.ClipString(cliResult.CliError, 4000);
            
            actionsResultLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error,
                $"<b>Error:</b> {clippedParsedError}");
        }
    }
}