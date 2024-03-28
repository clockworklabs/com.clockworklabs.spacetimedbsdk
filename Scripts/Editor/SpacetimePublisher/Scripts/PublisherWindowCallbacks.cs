using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static SpacetimeDB.Editor.PublisherMeta;

namespace SpacetimeDB.Editor
{
    /// Handles direct UI callbacks, sending async Tasks to PublisherWindowActions.
    /// Subscribed to @ PublisherWindow.setOnActionEvents.
    /// Set @ setOnActionEvents(), unset at unsetActionEvents().
    /// This is essentially the middleware between UI and logic.
    public partial class PublisherWindow
    {
        #region Init from PublisherWindow.cs CreateGUI()
        /// Curry sync Actions from UI => to async Tasks
        private void setOnActionEvents()
        {
            if (topBannerBtn != null)
            {
                topBannerBtn.clicked += onTopBannerBtnClick;
            }
            if (serverSelectedDropdown != null)
            {
                // Show if !null
                serverSelectedDropdown.RegisterValueChangedCallback(
                    onServerSelectedDropdownChangedAsync);
            }
            if (serverAddNewShowUiBtn != null)
            {
                // Toggle reveals the "new server" groupbox UI
                serverAddNewShowUiBtn.clicked += onServerAddNewShowUiBtnClick;
            }
            if (serverNicknameTxt != null)
            {
                // Replace spaces with dashes
                serverNicknameTxt.RegisterValueChangedCallback(
                    onServerNicknameTxtChanged);
            }
            if (serverNicknameTxt != null)
            {
                serverNicknameTxt.RegisterCallback<FocusOutEvent>(
                    onServerNicknameFocusOut);
            }
            if (serverHostTxt != null)
            {
                // If valid, enable Add New Server btn
                serverHostTxt.RegisterCallback<FocusOutEvent>(
                    onServerHostTxtFocusOut);
            }
            if (serverAddBtn != null)
            {
                // Add new newServer
                serverAddBtn.clicked += onServerAddBtnClickAsync;
            }
            if (identitySelectedDropdown != null)
            {
                // Show if !null
                identitySelectedDropdown.RegisterValueChangedCallback(
                    onIdentitySelectedDropdownChangedAsync);
            }
            if (identityAddNewShowUiBtn != null)
            {
                // Toggle reveals the "new identity" groupbox UI
                identityAddNewShowUiBtn.clicked += onIdentityAddNewShowUiBtnClick;
            }
            if (identityNicknameTxt != null)
            {
                // Replace spaces with dashes
                identityNicknameTxt.RegisterValueChangedCallback(
                    onIdentityNicknameTxtChanged);
            }
            if (identityNicknameTxt != null)
            {
                identityNicknameTxt.RegisterCallback<FocusOutEvent>(
                    onIdentityNicknameFocusOut);
            }
            if (identityEmailTxt != null)
            {
                // Normalize email chars
                identityEmailTxt.RegisterValueChangedCallback(
                    onIdentityEmailTxtChanged);
            }
            if (identityEmailTxt != null)
            {
                // If valid, enable Add New Identity btn
                identityEmailTxt.RegisterCallback<FocusOutEvent>(
                    onIdentityEmailTxtFocusOut);
            }
            if (identityAddBtn != null)
            {
                // Add new newIdentity
                identityAddBtn.clicked += onIdentityAddBtnClickAsync;
            }
            
            if (publishModulePathTxt != null)
            {
                // For init only
                publishModulePathTxt.RegisterValueChangedCallback(
                    onPublishModulePathTxtInitChanged);
            }
            if (publishModulePathTxt != null)
            {
                // If !empty, Reveal next UI grou
                publishModulePathTxt.RegisterCallback<FocusOutEvent>(
                    onPublishModulePathTxtFocusOut);
            }
            if (publishPathSetDirectoryBtn != null)
            {
                // Show folder dialog -> Set path label
                publishPathSetDirectoryBtn.clicked += OnPublishPathSetDirectoryBtnClick;
            }
            if (publishModuleNameTxt != null)
            {
                // Suggest module name if empty
                publishModuleNameTxt.RegisterCallback<FocusOutEvent>(
                    onPublishModuleNameTxtFocusOut);
            }
            if (publishModuleNameTxt != null)
            {
                // Replace spaces with dashes
                publishModuleNameTxt.RegisterValueChangedCallback(
                    onPublishModuleNameTxtChanged);
            }
            if (publishBtn != null)
            {
                // Start publishAsync chain
                publishBtn.clicked += onPublishBtnClickAsync;
            }
            if (publishCancelBtn != null)
            {
                // Cancel publishAsync chain
                publishCancelBtn.clicked += onCancelPublishBtnClick;
            }
            
            if (publishResultIsOptimizedBuildToggle != null)
            {
                // Show [Install Package] btn if !optimized
                publishResultIsOptimizedBuildToggle.RegisterValueChangedCallback(
                    onPublishResultIsOptimizedBuildToggleChanged);
            }
            if (installWasmOptBtn != null)
            {
                // Curry to an async Task => install `wasm-opt` npm pkg
                installWasmOptBtn.clicked += onInstallWasmOptBtnClick;
            }
            if (publishResultGenerateClientFilesBtn != null)
            {
                // Generate SDK via CLI `spacetime generate`
                publishResultGenerateClientFilesBtn.clicked += onPublishResultGenerateClientFilesBtnClick;
            }
        }

        /// Cleanup: This should parity the opposite of setOnActionEvents()
        private void unsetOnActionEvents()
        {
            if (topBannerBtn != null)
            {
                topBannerBtn.clicked -= onTopBannerBtnClick;
            }
            if (serverSelectedDropdown != null) 
            {
                serverSelectedDropdown.UnregisterValueChangedCallback(
                    onServerSelectedDropdownChangedAsync);
            }
            if (serverAddNewShowUiBtn != null) 
            {
                serverAddNewShowUiBtn.clicked -= onServerAddNewShowUiBtnClick;
            }
            if (serverNicknameTxt != null) 
            {
                serverNicknameTxt.UnregisterValueChangedCallback(
                    onServerNicknameTxtChanged);
            }
            if (serverNicknameTxt != null) 
            {
                serverNicknameTxt.UnregisterCallback<FocusOutEvent>(
                    onServerNicknameFocusOut);
            }
            if (serverHostTxt != null)
            {
                serverHostTxt.UnregisterCallback<FocusOutEvent>(
                    onServerHostTxtFocusOut);
            }
            if (serverAddBtn != null)
            {
                serverAddBtn.clicked -= onServerAddBtnClickAsync;
            }
            if (identitySelectedDropdown != null)
            {
                identitySelectedDropdown.RegisterValueChangedCallback(
                    onIdentitySelectedDropdownChangedAsync);
            }
            if (identityNicknameTxt != null)
            {
                identityNicknameTxt.UnregisterValueChangedCallback(
                    onIdentityNicknameTxtChanged);
            }
            if (identityNicknameTxt != null)
            {
                identityNicknameTxt.UnregisterCallback<FocusOutEvent>(
                    onIdentityNicknameFocusOut);
            }
            if (identityEmailTxt != null)
            {
                identityEmailTxt.UnregisterValueChangedCallback(
                    onIdentityEmailTxtChanged);
            }
            if (identityEmailTxt != null)
            {
                identityEmailTxt.UnregisterCallback<FocusOutEvent>(
                    onIdentityEmailTxtFocusOut);
            }
            if (identityAddBtn != null)
            {
                identityAddBtn.clicked -= onIdentityAddBtnClickAsync;
            }
            if (publishModulePathTxt != null)
            {
                // For init only; likely already unsub'd itself
                publishModulePathTxt.UnregisterValueChangedCallback(
                    onPublishModulePathTxtInitChanged);
            }
            if (publishModulePathTxt != null)
            {
                publishModulePathTxt.UnregisterCallback<FocusOutEvent>(
                    onPublishModulePathTxtFocusOut);
            }
            if (publishPathSetDirectoryBtn != null)
            {
                publishPathSetDirectoryBtn.clicked -= OnPublishPathSetDirectoryBtnClick;
            }
            if (publishModuleNameTxt != null)
            {
                publishModuleNameTxt.UnregisterCallback<FocusOutEvent>(
                    onPublishModuleNameTxtFocusOut);
            }
            if (publishModuleNameTxt != null)
            {
                publishModuleNameTxt.UnregisterValueChangedCallback(onPublishModuleNameTxtChanged);
            }
            if (publishBtn != null)
            {
                publishBtn.clicked -= onPublishBtnClickAsync;
            }
            if (publishResultIsOptimizedBuildToggle != null)
            {
                publishResultIsOptimizedBuildToggle.UnregisterValueChangedCallback(
                    onPublishResultIsOptimizedBuildToggleChanged);
            }
            if (installWasmOptBtn != null)
            {
                installWasmOptBtn.clicked -= onInstallWasmOptBtnClick;
            }
        }

        /// Cleanup when the UI is out-of-scope
        private void OnDisable() => unsetOnActionEvents();
        #endregion // Init from PublisherWindow.cs CreateGUI()
        
        
        #region Direct UI Callbacks
        /// Open link to SpacetimeDB Module docs
        private void onTopBannerBtnClick() =>
            Application.OpenURL(TOP_BANNER_CLICK_LINK);
        
        /// Normalize with no spacing
        private void onIdentityNicknameTxtChanged(ChangeEvent<string> evt) =>
            identityNicknameTxt.SetValueWithoutNotify(replaceSpacesWithDashes(evt.newValue));

        private void onServerNicknameTxtChanged(ChangeEvent<string> evt) =>
            serverNicknameTxt.SetValueWithoutNotify(replaceSpacesWithDashes(evt.newValue));

        /// Change spaces to dashes
        private void onPublishModuleNameTxtChanged(ChangeEvent<string> evt) =>
            publishModuleNameTxt.SetValueWithoutNotify(replaceSpacesWithDashes(evt.newValue));
        
        /// Normalize with email formatting
        private void onIdentityEmailTxtChanged(ChangeEvent<string> evt)
        {
            if (string.IsNullOrWhiteSpace(evt.newValue))
                return;
            
            bool isEmailFormat = tryFormatAsEmail(evt.newValue, out string email);
            if (isEmailFormat)
                identityEmailTxt.SetValueWithoutNotify(email);
            else
                identityEmailTxt.SetValueWithoutNotify(evt.previousValue); // Revert non-email attempt
        }
        
        private async void onServerSelectedDropdownChangedAsync(ChangeEvent<string> evt)
        {
            bool selectedAnything = serverSelectedDropdown.index >= 0;
            
            // The old val could've beeen a placeholder "<color=yellow>Searching ...</color>" val
            bool oldValIsPlaceholderStr = selectedAnything && evt.previousValue.Contains("<"); 
            bool isHidden = serverSelectedDropdown.style.display == DisplayStyle.None;
            
            // We have "some" server loaded by runtime code; show this dropdown
            if (!selectedAnything || oldValIsPlaceholderStr)
                return;
            
            if (isHidden)
                serverSelectedDropdown.style.display = DisplayStyle.Flex;
            
            // We changed from a known server to another known one.
            // We should change the CLI default.
            string serverNickname = evt.newValue;
            Debug.Log($"Selected server changed to {serverNickname} (from {evt.previousValue})");
            
            // Process via CLI => Set default, revalidate identities
            try
            {
                await setDefaultServerRefreshIdentitiesAsync(serverNickname);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
        }
        
        /// This is hidden, by default, until a first newIdentity is added
        private async void onIdentitySelectedDropdownChangedAsync(ChangeEvent<string> evt)
        {
            bool selectedAnything = identitySelectedDropdown.index >= 0;
            bool isHidden = identitySelectedDropdown.style.display == DisplayStyle.None;
            
            // We have "some" newIdentity loaded by runtime code; show this dropdown
            if (!selectedAnything)
                return;
            
            if (isHidden)
                identitySelectedDropdown.style.display = DisplayStyle.Flex;
            
            // We changed from a known identity to another known one.
            // We should change the CLI default.
            try
            {
                await setDefaultIdentityAsync(evt.newValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
        }
        
        /// Used for init only, for when the persistent ViewDataKey
        private void onPublishModulePathTxtInitChanged(ChangeEvent<string> evt)
        {
            onDirPathSet();
            revealPublishResultCacheIfHostExists(openFoldout: null);
            publishModulePathTxt.UnregisterValueChangedCallback(onPublishModulePathTxtInitChanged);
        }
        
        /// Toggle newIdentity btn enabled based on email + nickname being valid
        private void onIdentityNicknameFocusOut(FocusOutEvent evt) =>
            checkIdentityReqsToggleIdentityBtn();
        
        /// Toggle newServer btn enabled based on email + nickname being valid
        private void onServerNicknameFocusOut(FocusOutEvent evt) =>
            checkServerReqsToggleServerBtn();
        
        /// Toggle newIdentity btn enabled based on nickname + email being valid
        private void onIdentityEmailTxtFocusOut(FocusOutEvent evt) =>
            checkIdentityReqsToggleIdentityBtn();
        
        /// Toggle newServer btn enabled based on nickname + host being valid
        private void onServerHostTxtFocusOut(FocusOutEvent evt) =>
            checkServerReqsToggleServerBtn();
        
        /// Toggle next section if !null
        private void onPublishModulePathTxtFocusOut(FocusOutEvent evt)
        {
            // Prevent inadvertent UI showing too early, frozen on modal file picking
            if (_isFilePicking)
                return;
            
            bool hasPathSet = !string.IsNullOrEmpty(publishModulePathTxt.value);
            if (hasPathSet)
            {
                // Since we just changed the path, wipe old publishAsync info cache
                resetPublishResultCache();
                
                // Normalize, then reveal the next UI group
                publishModulePathTxt.value = superTrim(publishModulePathTxt.value);
                revealPublisherGroupUiAsync();
            }
            else
                publishGroupBox.style.display = DisplayStyle.None;
        }
        
        /// Explicitly declared and curried so we can unsubscribe
        /// There will *always* be a value for nameTxt
        private void onPublishModuleNameTxtFocusOut(FocusOutEvent evt) =>
            suggestModuleNameIfEmpty();

        /// Curry to an async Task to install `wasm-opt` npm pkg
        private async void onInstallWasmOptBtnClick()
        {
            try
            {
                await installWasmOptPackageViaNpmAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e.Message}");
                throw;
            }
            finally
            {
                installWasmOptProgressBar.style.display = DisplayStyle.None;
                publishBtn.SetEnabled(true);
            }
        }
        
        /// Run CLI cmd `spacetime generate`
        private async void onPublishResultGenerateClientFilesBtnClick()
        {
            try
            {
                await generateClientFilesAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
        }
        
        /// Toggles the "new server" group UI
        private void onServerAddNewShowUiBtnClick()
        {
            bool isHidden = serverNewGroupBox.style.display == DisplayStyle.None;
            if (isHidden)
            {
                // Show + UX: Focus the 1st field
                serverNewGroupBox.style.display = DisplayStyle.Flex;
                serverAddNewShowUiBtn.text = SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Success, "-"); // Show opposite, styled
                serverNicknameTxt.Focus();
                serverNicknameTxt.SelectAll();
            }
            else
            {
                // Hide
                serverNewGroupBox.style.display = DisplayStyle.None;
                serverAddNewShowUiBtn.text = "+"; // Show opposite
            }
        }
        
        /// Toggles the "new identity" group UI
        private void onIdentityAddNewShowUiBtnClick()
        {
            bool isHidden = identityNewGroupBox.style.display == DisplayStyle.None;
            if (isHidden)
            {
                // Show + UX: Focus the 1st field
                identityNewGroupBox.style.display = DisplayStyle.Flex;
                identityAddNewShowUiBtn.text = SpacetimeMeta.GetStyledStr(
                    SpacetimeMeta.StringStyle.Success, "-"); // Show opposite, styled
                identityNicknameTxt.Focus();
                identityNicknameTxt.SelectAll();
            }
            else
            {
                // Hide
                identityNewGroupBox.style.display = DisplayStyle.None;
                identityAddNewShowUiBtn.text = "+"; // Show opposite
            }
        }
        
        /// Show folder dialog -> Set path label
        private void OnPublishPathSetDirectoryBtnClick()
        {
            string pathBefore = publishModulePathTxt.value;
            // Show folder panel (modal FolderPicker dialog)
            _isFilePicking = true;
            
            string selectedPath = EditorUtility.OpenFolderPanel(
                "Select Server Module Dir", 
                Application.dataPath, 
                "");
            
            _isFilePicking = false;
            
            // Canceled or same path?
            bool pathChanged = selectedPath == pathBefore;
            if (string.IsNullOrEmpty(selectedPath) || pathChanged)
                return;
            
            // Path changed: set path val + reveal next UI group
            publishModulePathTxt.value = selectedPath;
            onDirPathSet();
        }
        
        /// Show [Install Package] btn if !optimized
        private void onPublishResultIsOptimizedBuildToggleChanged(ChangeEvent<bool> evt)
        {
            bool isOptimized = evt.newValue;
            installWasmOptBtn.style.display = isOptimized 
                ? DisplayStyle.None 
                : DisplayStyle.Flex;
        }
        
        private async void onIdentityAddBtnClickAsync()
        {
            string nickname = identityNicknameTxt.value;
            string email = identityEmailTxt.value;
            
            try
            {
                await addIdentityAsync(nickname, email);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
        }

        /// AKA AddServerBtnClick
        private async void onServerAddBtnClickAsync()
        {
            string nickname = serverNicknameTxt.value;
            string host = serverHostTxt.value;
            
            try
            {
                await addServerAsync(nickname, host);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
        }

        private async void onCancelPublishBtnClick()
        {
            Debug.Log("Warning: Cancelling Publish...");

            try
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            catch (ObjectDisposedException e)
            {
                // Already disposed - np
            }

            // Hide UI: Progress bar, cancel btn
            publishInstallProgressBar.style.display = DisplayStyle.None;
            publishCancelBtn.style.display = DisplayStyle.None;

            // Show UI: Canceled status, publish btn
            publishStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, "Canceled");
            publishStatusLabel.style.display = DisplayStyle.Flex;
            publishBtn.style.display = DisplayStyle.Flex;
            
            // Slight cooldown, then enable publish btn
            publishBtn.SetEnabled(false);

            try
            {
                await enableVisualElementInOneSec(publishBtn);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
        }

        /// Curried to an async Task, wrapped this way so
        /// we can unsubscribe and for better err handling 
        private async void onPublishBtnClickAsync()
        {
            setPublishStartUi();
            
            try
            {
                await publishAsync();
            }
            catch (TaskCanceledException e)
            {
                publishCancelBtn.SetEnabled(false);
            }
            finally
            {
                publishInstallProgressBar.style.display = DisplayStyle.None;
                _cts?.Dispose();
            }
        }
        #endregion // Direct UI Callbacks

        private void onAddIdentityFail(SpacetimeIdentity identity, AddIdentityResult addIdentityResult)
        {
            identityAddBtn.SetEnabled(true);
            identityStatusLabel.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                $"<b>Failed:</b> Couldn't add identity `{identity.Nickname}`\n" +
                addIdentityResult.StyledFriendlyErrorMessage);
                
            identityStatusLabel.style.display = DisplayStyle.Flex;
        }

        /// Success: Add to dropdown + set default + show. Hide the [+] add group.
        /// Don't worry about caching choices; we'll get the new choices via CLI each load
        private void onAddIdentitySuccess(SpacetimeIdentity identity)
        {
            Debug.Log($"Add new identity success: {identity.Nickname}");
            _ = onGetSetIdentitiesSuccessEnsureDefault(new List<SpacetimeIdentity> { identity });
        }

        /// Success: Show installed txt, keep button disabled, but don't actually check
        /// the optimization box since *this* publishAsync is not optimized: Next one will be
        private void onInstallWasmOptPackageViaNpmSuccess() =>
            installWasmOptBtn.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Success, "Installed");

        private void onInstallWasmOptPackageViaNpmFail(SpacetimeCliResult cliResult)
        {
            installWasmOptBtn.SetEnabled(true);
            installWasmOptBtn.text = SpacetimeMeta.GetStyledStr(
                SpacetimeMeta.StringStyle.Error, 
                $"<b>Failed:</b> Couldn't install wasm-opt\n{cliResult.CliError}");
        }
    }
}
