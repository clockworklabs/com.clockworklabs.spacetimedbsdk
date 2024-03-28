using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static SpacetimeDB.Editor.PublisherMeta;

namespace SpacetimeDB.Editor
{
    /// Unlike PublisherWindowCallbacks, these are not called *directly* from UI.
    /// Runs an action -> Processes isSuccess -> calls success || fail @ PublisherWindowCallbacks.
    /// PublisherWindowCallbacks should handle try/catch (except for init chains).
    public partial class PublisherWindow
    {
        #region Init from PublisherWindow.CreateGUI
        /// Installs CLI tool, shows identity dropdown, gets identities.
        /// Initially called by PublisherWindow @ CreateGUI.
        private async Task initDynamicEventsFromPublisherWindow()
        {
            await ensureSpacetimeCliInstalledAsync();
            await getServersSetDropdown();
        }
        
        /// Initially called by PublisherWindow @ CreateGUI
        /// - Set to the initial state as if no inputs were set.
        /// - This exists so we can show all ui elements simultaneously in the
        ///   ui builder for convenience.
        /// - (!) If called from CreateGUI, after a couple frames,
        ///       any persistence from `ViewDataKey`s may override this.
        private void resetUi()
        {
            // Hide install CLI
            installCliGroupBox.style.display = DisplayStyle.None;
            installCliProgressBar.style.display = DisplayStyle.None;
            installCliStatusLabel.style.display = DisplayStyle.None;
            
            // Hide all foldouts and labels from Identity+ (show Server)
            toggleFoldoutRipple(startRippleFrom: FoldoutGroupType.Identity, show:false);
            
            // Hide server
            serverAddNewShowUiBtn.style.display = DisplayStyle.None;
            serverNewGroupBox.style.display = DisplayStyle.None;
            resetServerDropdown();
            serverSelectedDropdown.value = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action, "Searching ...");
            
            // Hide identity
            identityAddNewShowUiBtn.style.display = DisplayStyle.None;
            identityNewGroupBox.style.display = DisplayStyle.None;
            resetIdentityDropdown();
            identitySelectedDropdown.value = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action, "Searching ..."); 
            identityAddBtn.SetEnabled(false);
             
            // Hide publish
            publishGroupBox.style.display = DisplayStyle.None;
            publishCancelBtn.style.display = DisplayStyle.None;
            publishInstallProgressBar.style.display = DisplayStyle.None;
            publishStatusLabel.style.display = DisplayStyle.None;

            resetPublishResultCache();
        }
        
        /// Check for install => Install if !found -> Throw if err
        private async Task ensureSpacetimeCliInstalledAsync()
        {
            // Check if Spacetime CLI is installed => install, if !found
            SpacetimeCliResult cliResult = await SpacetimeDbCli.GetIsSpacetimeCliInstalledAsync();
            
            // Process result -> Update UI
            bool isSpacetimeCliInstalled = !cliResult.HasCliErr;
            if (isSpacetimeCliInstalled)
            {
                onSpacetimeCliAlreadyInstalled();
                return;
            }
            
            await installSpacetimeDbCliAsync();
        }

        private void setInstallSpacetimeDbCliUi()
        {
            // Command !found: Update status => Install now
            _ = startProgressBarAsync(
                installCliProgressBar, 
                barTitle: "Installing SpacetimeDB CLI ...",
                initVal: 4,
                valIncreasePerSec: 4,
                autoHideOnComplete: true);

            installCliStatusLabel.style.display = DisplayStyle.None;
            installCliGroupBox.style.display = DisplayStyle.Flex;
        }

        private async Task installSpacetimeDbCliAsync()
        {
            setInstallSpacetimeDbCliUi();
            
            // Run CLI cmd
            SpacetimeCliResult installResult = await SpacetimeDbCli.InstallSpacetimeCliAsync();
            
            // Process result -> Update UI
            bool isSpacetimeDbCliInstalled = !installResult.HasCliErr;
            if (isSpacetimeDbCliInstalled)
            {
                installCliGroupBox.style.display = DisplayStyle.None;
                return;
            }
            
            // Critical error: Spacetime CLI !installed and failed install attempt
            onInstallSpacetimeDbCliFail(friendlyFailMsg: "See logs");
        }

        /// Try to get get list of Servers from CLI.
        /// This should be called at init at runtime from PublisherWIndow at CreateGUI time.
        private async Task getServersSetDropdown()
        {
            // Run CLI cmd
            GetServersResult getServersResult = await SpacetimeDbCli.GetServersAsync();
            
            // Process result -> Update UI
            bool isSuccess = getServersResult.HasServer;
            if (!isSuccess)
            {
                onGetSetServersFail(getServersResult);
                return;
            }
            
            // Success
            await onGetServersSetDropdownSuccess(getServersResult);
        }
        #endregion // Init from PublisherWindow.CreateGUI
        
        
        /// Success:
        /// - Get server list and ensure it's default
        /// - Refresh identities, since they are bound per-server
        private async Task onGetServersSetDropdownSuccess(GetServersResult getServersResult)
        {
            await onGetSetServersSuccessEnsureDefaultAsync(getServersResult.Servers);
            await getIdentitiesSetDropdown(); // Process and reveal the next UI group
        }

        /// Try to get get list of Identities from CLI.
        /// (!) Servers must already be set.
        private async Task getIdentitiesSetDropdown()
        {
            Debug.Log($"Gathering identities for selected '{serverSelectedDropdown.value}' server...");
            
            // Sanity check: Is there a selected server?
            bool hasSelectedServer = serverSelectedDropdown.index >= 0;
            if (!hasSelectedServer)
            {
                Debug.LogError("Tried to get identities before server is selected");
                return;
            }
            
            // Run CLI cmd
            GetIdentitiesResult getIdentitiesResult = await SpacetimeDbCli.GetIdentitiesAsync();
            
            // Process result -> Update UI
            bool isSuccess = getIdentitiesResult.HasIdentity;
            if (!isSuccess)
            {
                onGetSetIdentitiesFail();
                return;
            }
            
            // Success
            await onGetSetIdentitiesSuccessEnsureDefault(getIdentitiesResult.Identities);
        }
        
        /// Validates if we at least have a host name before revealing
        /// bug: If you are calling this from CreateGUI, openFoldout will be ignored.
        private void revealPublishResultCacheIfHostExists(bool? openFoldout)
        {
            // Sanity check: Ensure host is set
            bool hasVal = !string.IsNullOrWhiteSpace(publishResultHostTxt.value);
            if (!hasVal)
                return;
            
            // Reveal the publishAsync result info cache
            publishResultFoldout.style.display = DisplayStyle.Flex;
            
            if (openFoldout != null)
                publishResultFoldout.value = (bool)openFoldout;
        }
        
        /// (1) Suggest module name, if empty
        /// (2) Reveal publisher group
        /// (3) Ensure spacetimeDB CLI is installed async
        private void onDirPathSet()
        {
            // We just updated the path - hide old publishAsync result cache
            publishResultFoldout.style.display = DisplayStyle.None;
            
            // Set the tooltip to equal the path, since it's likely cutoff
            publishModulePathTxt.tooltip = publishModulePathTxt.value;
            
            // Since we changed the path, we should wipe stale publishAsync info
            resetPublishResultCache();
            
            // ServerModulePathTxt persists: If previously entered, show the publishAsync group
            bool hasPathSet = !string.IsNullOrEmpty(publishModulePathTxt.value);
            if (hasPathSet)
                revealPublisherGroupUiAsync(); // +Ensures SpacetimeDB CLI is installed async
        }
        
        /// Dynamically sets a dashified-project-name placeholder, if empty
        private void suggestModuleNameIfEmpty()
        {
            // Set the server module name placeholder text dynamically, based on the project name
            // Replace non-alphanumeric chars with dashes
            bool hasName = !string.IsNullOrEmpty(publishModuleNameTxt.value);
            if (hasName)
                return; // Keep whatever the user customized
            
            // Generate dashified-project-name fallback suggestion
            publishModuleNameTxt.value = getSuggestedServerModuleName();
        }
        
        /// (!) bug: If NO servers are found, including the default, we'll regenerate them back.
        private void onGetSetServersFail(GetServersResult getServersResult)
        {
            if (!getServersResult.HasServer && !_isRegeneratingDefaultServers)
            {
                Debug.Log("[BUG] No servers found; defaults were wiped: " +
                    "regenerating, then trying again...");
                _isRegeneratingDefaultServers = true;
                _ = regenerateServers();         
                return;
            }
            
            // Hide dropdown, reveal new ui group
            Debug.Log("No servers found - revealing 'add new server' group");

            // UI: Reset flags, clear cohices, hide selected server dropdown box
            _isRegeneratingDefaultServers = false; // in case we looped around to a fail
            serverSelectedDropdown.choices.Clear();
            serverSelectedDropdown.style.display = DisplayStyle.None;
            
            // Show "add new server" group box, focus nickname
            serverNewGroupBox.style.display = DisplayStyle.Flex;
            serverNicknameTxt.Focus();
            serverNicknameTxt.SelectAll();
        }

        /// When local and testnet are missing, it's 99% due to a bug:
        /// We'll add them back. Assuming default ports (3000) and testnet targets.
        private async Task regenerateServers()
        {
            Debug.Log("Regenerating default servers: [ local, testnet* ] *Becomes default");
            
            // UI
            serverStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action, 
                "<b>Regenerating default servers:</b>\n[ local, testnet* ]");
            serverStatusLabel.style.display = DisplayStyle.Flex;

            AddServerRequest addServerRequest = null;
            
            // Run CLI cmd: Add `local` server (forces `--no-fingerprint` so it doesn't need to be running now)
            addServerRequest = new("local", "http://127.0.0.1:3000");
            _ = await SpacetimeDbPublisherCli.AddServerAsync(addServerRequest);
            
            // Run CLI cmd: Add `testnet` server (becomes default)
            addServerRequest = new("testnet", "https://testnet.spacetimedb.com");
            _ = await SpacetimeDbPublisherCli.AddServerAsync(addServerRequest);
            
            // Success - try again
            _ = getServersSetDropdown();
        }

        private void onGetSetIdentitiesFail()
        {
            // Hide dropdown, reveal new ui group
            Debug.Log("No identities found - revealing 'add new identity' group");
            
            // UI: Reset choices, hide dropdown+new identity btn
            identitySelectedDropdown.choices.Clear();
            identitySelectedDropdown.style.display = DisplayStyle.None;
            identityAddNewShowUiBtn.style.display = DisplayStyle.None;
            
            // UI: Reveal "add new identity" group, reveal foldout
            identityNewGroupBox.style.display = DisplayStyle.Flex;
            identityFoldout.style.display = DisplayStyle.Flex;
            
            // UX: Focus Nickname field
            identityNicknameTxt.Focus();
            identityNicknameTxt.SelectAll();
        }

        /// Works around UI Builder bug on init that will add the literal "string" type to [0]
        private void resetIdentityDropdown()
        {
            identitySelectedDropdown.choices.Clear();
            identitySelectedDropdown.value = "";
            identitySelectedDropdown.index = -1;
        }
        
        /// Works around UI Builder bug on init that will add the literal "string" type to [0]
        private void resetServerDropdown()
        {
            serverSelectedDropdown.choices.Clear();
            serverSelectedDropdown.value = "";
            serverSelectedDropdown.index = -1;
        }
        
        /// Set the selected identity dropdown. If identities found but no default, [0] will be set. 
        private async Task onGetSetIdentitiesSuccessEnsureDefault(List<SpacetimeIdentity> identities)
        {
            // Logs for each found, with default shown
            foreach (SpacetimeIdentity identity in identities)
                Debug.Log($"Found identity: {identity}");
            
            // Setting will trigger the onIdentitySelectedDropdownChangedAsync event @ PublisherWindow
            foreach (SpacetimeIdentity identity in identities)
            {
                identitySelectedDropdown.choices.Add(identity.Nickname);

                if (identity.IsDefault)
                {
                    // Set the index to the most recently-added one
                    int recentlyAddedIndex = identitySelectedDropdown.choices.Count - 1;
                    identitySelectedDropdown.index = recentlyAddedIndex;
                }
            }
            
            // Ensure a default was found
            bool foundIdentity = identities.Count > 0;
            bool foundDefault = identitySelectedDropdown.index >= 0;
            if (foundIdentity && !foundDefault)
            {
                Debug.LogError("Found Identities, but no default " +
                    $"Falling back to [0]:{identities[0].Nickname} and setting via CLI...");
                identitySelectedDropdown.index = 0;
            
                // We need a default identity set
                string nickname = identities[0].Nickname;
                await setDefaultIdentityAsync(nickname);
            }

            // Process result -> Update UI
            onEnsureIdentityDefaultSuccess();
        }
        
        private void onEnsureIdentityDefaultSuccess()
        {
            // Allow selection, show [+] new reveal ui btn
            identitySelectedDropdown.pickingMode = PickingMode.Position;
            identityAddNewShowUiBtn.style.display = DisplayStyle.Flex;
            
            // Hide UI
            identityStatusLabel.style.display = DisplayStyle.None;
            identityNewGroupBox.style.display = DisplayStyle.None;
            
            // Show this identity foldout + dropdown, which may have been hidden
            // if a server was recently changed
            identityFoldout.style.display = DisplayStyle.Flex;
            identitySelectedDropdown.style.display = DisplayStyle.Flex;
            
            // Show the next section + UX: Focus the 1st field
            identityFoldout.style.display = DisplayStyle.Flex;
            publishFoldout.style.display = DisplayStyle.Flex;
            publishModuleNameTxt.Focus();
            publishModuleNameTxt.SelectNone();
            
            // If we have a cached result, show that (minimized)
            revealPublishResultCacheIfHostExists(openFoldout: false);
        }

        /// Set the selected server dropdown. If servers found but no default, [0] will be set.
        /// Also can be called by OnAddServerSuccess by passing a single server
        private async Task onGetSetServersSuccessEnsureDefaultAsync(List<SpacetimeServer> servers)
        {
            // Logs for each found, with default shown
            foreach (SpacetimeServer server in servers)
                Debug.Log($"Found server: {server}");
            
            // Setting will trigger the onIdentitySelectedDropdownChangedAsync event @ PublisherWindow
            for (int i = 0; i < servers.Count; i++)
            {
                SpacetimeServer server = servers[i];
                serverSelectedDropdown.choices.Add(server.Nickname);

                if (server.IsDefault)
                {
                    // Set the index to the most recently-added one
                    int recentlyAddedIndex = serverSelectedDropdown.choices.Count - 1;
                    serverSelectedDropdown.index = recentlyAddedIndex;
                }
            }
            
            // Ensure a default was found
            bool foundServer = servers.Count > 0;
            bool foundDefault = serverSelectedDropdown.index >= 0;
            if (foundServer && !foundDefault)
            {
                Debug.LogError("Found Servers, but no default: " +
                    $"Falling back to [0]:{servers[0].Nickname} and setting via CLI...");
                serverSelectedDropdown.index = 0;
            
                // We need a default server set
                string nickname = servers[0].Nickname;
                await SpacetimeDbPublisherCli.SetDefaultServerAsync(nickname);
            }

            // Process result -> Update UI
            onEnsureServerDefaultSuccess();
        }

        private void onEnsureServerDefaultSuccess()
        {
            // Allow selection, show [+] new reveal ui btn
            serverSelectedDropdown.pickingMode = PickingMode.Position;
            serverAddNewShowUiBtn.style.display = DisplayStyle.Flex;
            
            // Hide UI
            serverStatusLabel.style.display = DisplayStyle.None;
            serverNewGroupBox.style.display = DisplayStyle.None;
            
            // Show the next section
            identityFoldout.style.display = DisplayStyle.Flex;
        }

        /// This will reveal the group and initially check for the spacetime cli tool
        private void revealPublisherGroupUiAsync()
        {
            // Show and enable group, but disable the publishAsync btn
            // to check/install Spacetime CLI tool
            publishGroupBox.SetEnabled(true);
            publishBtn.SetEnabled(false);
            publishStatusLabel.style.display = DisplayStyle.Flex;
            publishGroupBox.style.display = DisplayStyle.Flex;
            setPublishReadyStatus();
        }

        /// Sets status label to "Ready" and enables+shows Publisher btn
        /// +Hides the cancel btn
        private void setPublishReadyStatus()
        {
            publishStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Success, 
                "Ready");
            publishBtn.SetEnabled(true);
            publishBtn.style.display = DisplayStyle.Flex;
            
            publishCancelBtn.style.display = DisplayStyle.None;
        }
        
        /// Be sure to try/catch this with a try/finally to dispose `_cts
        private async Task publishAsync()
        {
            setPublishStartUi();
            resetCancellationTokenSrc();

            PublishRequest publishRequest = new(
                publishModuleNameTxt.value, 
                publishModulePathTxt.value,
                new PublishRequest.AdvancedOpts(
                    publishModuleClearDataToggle.value,
                    publishModuleDebugModeToggle.value
                ));
            
            // Run CLI cmd [can cancel]
            PublishResult publishResult = await SpacetimeDbPublisherCli.PublishAsync(
                publishRequest,
                _cts.Token);

            // Process result -> Update UI
            bool isSuccess = publishResult.IsSuccessfulPublish;
            Debug.Log($"PublishAsync success: {isSuccess}");
            if (isSuccess)
                onPublishSuccess(publishResult);
            else
                onPublishFail(publishResult);
        }
        
        /// Critical err - show label
        private void onPublishFail(PublishResult publishResult)
        {
            _cachedPublishResult = null;
            updatePublishStatus(
                SpacetimeMeta.StringStyle.Error, 
                publishResult.StyledFriendlyErrorMessage 
                    ?? Utils.ClipString(publishResult.CliError, maxLength: 4000));
        }
        
        /// There may be a false-positive wasm-opt err here; in which case, we'd still run success.
        /// Caches the module name into EditorPrefs for other tools to use. 
        private void onPublishSuccess(PublishResult publishResult)
        {
            _cachedPublishResult = publishResult;
            
            // Success - reset UI back to normal
            setPublishReadyStatus();
            setPublishResultGroupUi(publishResult);
            
            // Other editor tools may want to utilize this value,
            // since the CLI has no idea what you're "default" Module is
            EditorPrefs.SetString(
                SpacetimeMeta.EDITOR_PREFS_MODULE_NAME_KEY, 
                publishModuleNameTxt.value);
        }

        private void setPublishResultGroupUi(PublishResult publishResult)
        {
            // Hide old status -> Load the result data
            publishResultStatusLabel.style.display = DisplayStyle.None;
            publishResultDateTimeTxt.value = $"{publishResult.PublishedAt:G} (Local)";
            publishResultHostTxt.value = publishResult.UploadedToHost;
            publishResultDbAddressTxt.value = publishResult.DatabaseAddressHash;
            
            // Set via ValueWithoutNotify since this is a hacky "readonly" Toggle (no official feat for this, yet)
            publishResultIsOptimizedBuildToggle.value = publishResult.IsPublishWasmOptimized;
            
            // Show install pkg button, to optionally optimize next publish
            installWasmOptBtn.style.display = publishResult.IsPublishWasmOptimized
                ? DisplayStyle.None // If it's already installed, no need to show it
                : DisplayStyle.Flex;

            resetGenerateUi();
            
            // Show the result group and expand the foldout
            revealPublishResultCacheIfHostExists(openFoldout: true);
        }

        /// Show progress bar, clamped to 1~100, updating every 1s
        /// Stops when reached 100, or if style display is hidden
        private async Task startProgressBarAsync(
            ProgressBar progressBar,
            string barTitle = "Running CLI ...",
            int initVal = 5, 
            int valIncreasePerSec = 5,
            bool autoHideOnComplete = true)
        {
            progressBar.title = barTitle;
            
            // Prepare the progress bar style and min/max
            const int maxVal = 99;
            progressBar.value = Mathf.Clamp(initVal, 1, maxVal);
            progressBar.style.display = DisplayStyle.Flex;
            
            while (progressBar.value < 100 && 
                   progressBar.style.display == DisplayStyle.Flex)
            {
                // Wait for 1 second, then update the bar
                await Task.Delay(TimeSpan.FromSeconds(1));
                progressBar.value += valIncreasePerSec;
                
                // In case we reach 99%+, we'll add and retract a "." to show progress is continuing
                if (progressBar.value >= maxVal)
                {
                    progressBar.title = progressBar.title.Contains("...")
                        ? progressBar.title.Replace("...", "....")
                        : progressBar.title.Replace("....", "...");    
                }
            }
            
            if (autoHideOnComplete)
                progressBar.style.display = DisplayStyle.None;
        }

        private void onInstallSpacetimeDbCliFail(string friendlyFailMsg)
        {
            installCliStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error,
                $"<b>Failed to Install Spacetime CLI:</b>\n{friendlyFailMsg}");
            
            installCliStatusLabel.style.display = DisplayStyle.Flex;
            installCliGroupBox.style.display = DisplayStyle.Flex;
        }

        /// Hide CLI group
        private void onSpacetimeCliAlreadyInstalled()
        {
            installCliProgressBar.style.display = DisplayStyle.None;
            installCliGroupBox.style.display = DisplayStyle.None;
        }

        /// Show a styled friendly string to UI. Errs will enable publishAsync btn.
        private void updatePublishStatus(SpacetimeMeta.StringStyle style, string friendlyStr)
        {
            publishStatusLabel.text = SpacetimeMeta.GetStyledStr(style, friendlyStr);
            publishStatusLabel.style.display = DisplayStyle.Flex;

            if (style != SpacetimeMeta.StringStyle.Error)
                return; // Not an error
            
            // Error: Hide cancel btn, cancel token, show/enable pub btn
            publishCancelBtn.style.display = DisplayStyle.None;
            _cts?.Dispose();
            
            publishBtn.style.display = DisplayStyle.Flex;
            publishBtn.SetEnabled(true);
        }
        
        /// Yields 1 frame to update UI fast
        private void setPublishStartUi()
        {
            // Reset result cache
            resetPublishResultCache();
            
            // Hide: Publish btn, label, result foldout 
            publishResultFoldout.style.display = DisplayStyle.None;
            publishStatusLabel.style.display = DisplayStyle.None;
            publishBtn.style.display = DisplayStyle.None;
            
            // Show: Cancel btn, show progress bar,
            publishCancelBtn.style.display = DisplayStyle.Flex;
            _ = startProgressBarAsync(
                publishInstallProgressBar,
                barTitle: "Publishing to SpacetimeDB ...",
                autoHideOnComplete: false);
        }

        /// Set 'installing' UI
        private void setinstallWasmOptPackageViaNpmUi()
        {
            // Hide UI
            publishBtn.SetEnabled(false);
            installWasmOptBtn.SetEnabled(false);
            
            // Show UI
            installWasmOptBtn.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action, "Installing ...");
            installCliProgressBar.style.display = DisplayStyle.Flex;
            
            _ = startProgressBarAsync(
                installWasmOptProgressBar,
                barTitle: "Installing `wasm-opt` via npm ...",
                autoHideOnComplete: false);
        }
        
        /// Install `wasm-opt` npm pkg for a "set and forget" publishAsync optimization boost
        private async Task installWasmOptPackageViaNpmAsync()
        {
            setinstallWasmOptPackageViaNpmUi();
            
            // Run CLI cmd
            InstallWasmResult installWasmResult = await SpacetimeDbPublisherCli.InstallWasmOptPkgAsync();

            // Process result -> Update UI
            bool isSuccess = installWasmResult.IsSuccessfulInstall;
            if (isSuccess)
                onInstallWasmOptPackageViaNpmSuccess();
            else
                onInstallWasmOptPackageViaNpmFail(installWasmResult);
        }

        /// UI: Disable btn + show installing status to id label
        private void setAddIdentityUi(string nickname)
        {
            identityAddBtn.SetEnabled(false);
            identityStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action, $"Adding {nickname} ...");
            identityStatusLabel.style.display = DisplayStyle.Flex;
            publishStatusLabel.style.display = DisplayStyle.None;
            publishResultFoldout.style.display = DisplayStyle.None;
        }
        
        private async Task addIdentityAsync(string nickname, string email)
        {
            // Sanity check
            if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(email))
                return;

            setAddIdentityUi(nickname);
            AddIdentityRequest addIdentityRequestRequest = new(nickname, email);
            
            // Run CLI cmd
            AddIdentityResult addIdentityResult = await SpacetimeDbPublisherCli.AddIdentityAsync(addIdentityRequestRequest);
            SpacetimeIdentity identity = new(nickname, isDefault:true);

            // Process result -> Update UI
            if (addIdentityResult.HasCliErr)
                onAddIdentityFail(identity, addIdentityResult);
            else
                onAddIdentitySuccess(identity);
        }

        private void setAddServerUi(string nickname)
        {
            // UI: Disable btn + show installing status to id label
            serverAddBtn.SetEnabled(false);
            serverStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action, $"Adding {nickname} ...");
            serverStatusLabel.style.display = DisplayStyle.Flex;
            
            // Hide the other sections (while clearing out their labels), since we rely on servers
            identityStatusLabel.style.display = DisplayStyle.None;
            identityFoldout.style.display = DisplayStyle.None;
            publishFoldout.style.display = DisplayStyle.None;
            publishStatusLabel.style.display = DisplayStyle.None;
            publishResultFoldout.style.display = DisplayStyle.None;
        }
        
        private async Task addServerAsync(string nickname, string host)
        {
            // Sanity check
            if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(host))
                return;

            setAddServerUi(nickname);
            AddServerRequest request = new(nickname, host);

            // Run the CLI cmd
            AddServerResult addServerResult = await SpacetimeDbPublisherCli.AddServerAsync(request);
            
            // Process result -> Update UI
            SpacetimeServer serverAdded = new(nickname, host, isDefault:true);

            if (addServerResult.HasCliErr)
                onAddServerFail(serverAdded, addServerResult);
            else
                onAddServerSuccess(serverAdded);
        }
        
        private void onAddServerFail(SpacetimeServer serverAdded, AddServerResult addServerResult)
        {
            serverAddBtn.SetEnabled(true);
            serverStatusLabel.text = SpacetimeMeta.GetStyledStr(SpacetimeMeta.StringStyle.Error, 
                $"<b>Failed:</b> Couldn't add `{serverAdded.Nickname}` server</b>\n" +
                addServerResult.StyledFriendlyErrorMessage);
                
            serverStatusLabel.style.display = DisplayStyle.Flex;
        }
        
        /// Success: Add to dropdown + set default + show. Hide the [+] add group.
        /// Don't worry about caching choices; we'll get the new choices via CLI each load
        private void onAddServerSuccess(SpacetimeServer server)
        {
            Debug.Log($"Add new server success: {server.Nickname}");
            _ = onGetSetServersSuccessEnsureDefaultAsync(new List<SpacetimeServer> { server });
        }

        private async Task setDefaultIdentityAsync(string idNicknameOrDbAddress)
        {
            // Sanity check
            if (string.IsNullOrEmpty(idNicknameOrDbAddress))
                return;
            
            // Run CLI cmd
            SpacetimeCliResult cliResult = await SpacetimeDbPublisherCli.SetDefaultIdentityAsync(idNicknameOrDbAddress);

            // Process result -> Update UI
            bool isSuccess = !cliResult.HasCliErr;
            if (isSuccess)
                Debug.Log($"Changed default identity to: {idNicknameOrDbAddress}");
            else
                Debug.LogError($"Failed to set default identity: {cliResult.CliError}");
        }

        private void resetPublishResultCache()
        {
            publishResultFoldout.value = false;
            publishResultDateTimeTxt.value = "";
            publishResultHostTxt.value = "";
            publishResultDbAddressTxt.value = "";
            publishResultIsOptimizedBuildToggle.value = false;
            installWasmOptBtn.style.display = DisplayStyle.None;
            installWasmOptProgressBar.style.display = DisplayStyle.None;
            publishResultStatusLabel.style.display = DisplayStyle.None;
            
            // Hacky readonly Toggle feat workaround
            publishResultIsOptimizedBuildToggle.SetEnabled(false);
            publishResultIsOptimizedBuildToggle.style.opacity = 1;
        }
        
        /// Toggles the group visibility of the foldouts. Labels also hide if !show.
        /// Toggles ripple downwards from top. Checks for nulls
        private void toggleFoldoutRipple(FoldoutGroupType startRippleFrom, bool show)
        {
            // ---------------
            // Server, Identity, Publish, PublishResult
            if (startRippleFrom <= FoldoutGroupType.Server)
            {
                serverFoldout.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                if (!show)
                    serverStatusLabel.style.display = DisplayStyle.None;
            }
            
            // ---------------
            // Identity, Publish, PublishResult
            if (startRippleFrom <= FoldoutGroupType.Identity)
            {
                identityFoldout.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                if (!show)
                    identityStatusLabel.style.display = DisplayStyle.None;    
            }
            else
                return;
            
            // ---------------
            // Publish, PublishResult
            if (startRippleFrom <= FoldoutGroupType.Publish)
            {
                publishFoldout.style.display = DisplayStyle.None;
                if (!show)
                    publishStatusLabel.style.display = DisplayStyle.None;
            }
            else
                return;

            // ---------------
            // PublishResult+
            if (startRippleFrom <= FoldoutGroupType.PublishResult)
                publishResultFoldout.style.display = DisplayStyle.None;
        }

        /// Great for if you just canceled and you want a slight cooldown
        private async Task enableVisualElementInOneSec(VisualElement btn)
        {
            // Sanity check
            if (btn == null)
                return;
            
            await Task.Delay(TimeSpan.FromSeconds(1));
            btn.SetEnabled(true);
        }

        /// Change to a *known* nicknameOrHost
        /// - Changes CLI default server
        /// - Revalidates identities, since they are bound per-server
        private async Task setDefaultServerRefreshIdentitiesAsync(string nicknameOrHost)
        {
            // Sanity check
            if (string.IsNullOrEmpty(nicknameOrHost))
                return;
            
            // UI: This invalidates identities, so we'll hide all Foldouts
            toggleFoldoutRipple(FoldoutGroupType.Identity, show:false);

            // Run CLI cmd
            SpacetimeCliResult cliResult = await SpacetimeDbPublisherCli.SetDefaultServerAsync(nicknameOrHost);
            
            // Process result -> Update UI
            bool isSuccess = !cliResult.HasCliErr;
            if (!isSuccess)
                onChangeDefaultServerFail(cliResult);
            else
                await onChangeDefaultServerSuccessAsync();
        }
        
        private void onChangeDefaultServerFail(SpacetimeCliResult cliResult)
        {
            serverSelectedDropdown.SetEnabled(true);

            string clippedCliErr = Utils.ClipString(cliResult.CliError, maxLength: 4000);
            serverStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error,
                $"<b>Failed to Change Servers:</b>\n{clippedCliErr}");
        }
        
        /// Invalidate identities
        private async Task onChangeDefaultServerSuccessAsync()
        {
            await getIdentitiesSetDropdown(); // Process and reveal the next UI group
            serverSelectedDropdown.SetEnabled(true);
        }

        /// Disable generate btn, show "GGenerating..." label
        private void setGenerateClientFilesUi()
        {
            publishResultStatusLabel.style.display = DisplayStyle.None;
            publishResultGenerateClientFilesBtn.SetEnabled(false);
            publishResultGenerateClientFilesBtn.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Action,
                "Generating ...");
        }

        private async Task generateClientFilesAsync()
        {
            setGenerateClientFilesUi();
            
            // Prioritize result cache, if any - else use the input field
            string serverModulePath = _cachedPublishResult?.Request?.ServerModulePath 
                ?? publishModulePathTxt.value;
            
            Assert.IsTrue(!string.IsNullOrEmpty(serverModulePath),
                $"Expected {nameof(serverModulePath)}");

            if (generatedFilesExist())
            {
                // Wipe old files
                Directory.Delete(PathToAutogenDir, recursive:true);
            }
            
            GenerateRequest request = new(
                serverModulePath,
                PathToAutogenDir,
                deleteOutdatedFiles: true);

            GenerateResult generateResult = await SpacetimeDbPublisherCli
                .GenerateClientFilesAsync(request);

            bool isSuccess = generateResult.IsSuccessfulGenerate;
            if (isSuccess)
            {
                onGenerateClientFilesSuccess(serverModulePath);
            }
            else
            {
                onGenerateClientFilesFail(generateResult);
            }
        }

        private void onGenerateClientFilesFail(SpacetimeCliResult cliResult)
        {
            Debug.LogError($"Failed to generate client files: {cliResult.CliError}");

            resetGenerateUi();
            
            string clippedCliErr = Utils.ClipString(cliResult.CliError, maxLength: 4000);
            publishResultStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error,
                $"<b>Failed to Generate:</b>\n{clippedCliErr}");
            
            publishResultStatusLabel.style.display = DisplayStyle.Flex;
        }

        private void onGenerateClientFilesSuccess(string serverModulePath)
        {
            Debug.Log($"Generated SpacetimeDB client files from:" +
                $"\n`{serverModulePath}`\n\nto:\n`{PathToAutogenDir}`");
         
            resetGenerateUi();
            publishResultStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Success,
                "Generated to dir: <color=white>Assets/Autogen/</color>");
            publishResultStatusLabel.style.display = DisplayStyle.Flex;
        }
        
        bool generatedFilesExist() => Directory.Exists(PathToAutogenDir);

        /// Shared Ui changes after success/fail, or init on ui reset
        private void resetGenerateUi()
        {
            publishResultGenerateClientFilesBtn.text = generatedFilesExist()
                ? "Regenerate Client Typings"
                : "Generate Client Typings";
            
            publishResultStatusLabel.style.display = DisplayStyle.None;
            publishResultGenerateClientFilesBtn.SetEnabled(true);
        }
    }
}