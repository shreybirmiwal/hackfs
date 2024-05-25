using System;
using Monaverse.Api;
using Monaverse.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Monaverse.Examples
{
    public class MonaWalletConnectTest : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TMP_Text _connectButtonLabel;
        [SerializeField] private TMP_Text _resultLabel;

        [Header("Buttons")]
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private Button _authorizeButton;
        [SerializeField] private Button _signOutButton;
        [SerializeField] private Button _postAuthorizeButton;

        [Header("States")]
        [Space][SerializeField] private GameObject _dappButtons;
        [SerializeField] private GameObject _connectedState;
        [SerializeField] private GameObject _authorizedState;


        public GameObject errorText;
        public TMP_Dropdown mapDropdown;
        public Button submitButton;
        bool isAUTHORIZED = false;


        private enum WalletState
        {
            Disconnected,
            Connecting,
            Connected,
            Authorized
        }

        /// <summary>
        /// Initializes the UI to the disconnected state on start.
        /// </summary>
        private void Start()
        {
            SetUIState(WalletState.Disconnected);

            MonaverseManager.Instance.SDK.Connected += OnConnected;
            MonaverseManager.Instance.SDK.Disconnected += OnDisconnected;
            MonaverseManager.Instance.SDK.Authorized += OnAuthorized;
            MonaverseManager.Instance.SDK.AuthorizationFailed += OnAuthorizationFailed;
            MonaverseManager.Instance.SDK.ConnectionErrored += OnConnectionErrored;
            MonaverseManager.Instance.SDK.SignMessageErrored += OnSignMessageErrored;

            mapDropdown.onValueChanged.AddListener(delegate { UpdateMapIssues(); });

            UpdateMapIssues();
        }

        async void UpdateMapIssues()
        {

            Debug.Log("Map Dropdown Value: " + mapDropdown.value + " | " + isAUTHORIZED);

            //if default allg
            if (mapDropdown.value == 0)
            {
                errorText.SetActive(false);
                submitButton.interactable = true;
            }



            //not authorized and not default
            if (mapDropdown.value != 0 && !isAUTHORIZED)
            {
                errorText.SetActive(true);
                submitButton.interactable = false;
            }


            if (mapDropdown.value != 0 && isAUTHORIZED)
            {

                //_resultLabel.text = "Getting wallet collectibles...";

                bool ownsNFT = false;

                var getCollectiblesResult = await MonaApi.ApiClient.Collectibles.GetWalletCollectibles();
                Debug.Log("[MonaWalletConnectTest] Collectibles: " + getCollectiblesResult);

                if (getCollectiblesResult.IsSuccess && getCollectiblesResult.Data != null)
                {
                    var collectibles = getCollectiblesResult.Data.Data; // This is the list of CollectibleDto
                    foreach (var collectible in collectibles)
                    {
                        if (collectible.Title == mapDropdown.options[mapDropdown.value].text)
                        {
                            ownsNFT = true;
                        }
                        Debug.Log("Collectible Title: " + collectible.Title);
                    }
                }
                else
                {
                    Debug.LogError("Failed to fetch collectibles or no data available.");
                }



                //if it owns, no problem
                //if it doesn't own, show error
                if (ownsNFT)
                {
                    errorText.SetActive(false);
                    submitButton.interactable = true;
                }
                else
                {
                    errorText.SetActive(true);
                    submitButton.interactable = false;
                }

            }
        }



        #region SDK Event Handlers

        private void OnSignMessageErrored(object sender, Exception exception)
        {
            Debug.LogError("[MonaWalletConnectTest] OnSignMessageErrored: " + exception.Message);
            _authorizeButton.interactable = true;
        }

        private void OnConnectionErrored(object sender, Exception exception)
        {
            Debug.LogError("[MonaWalletConnectTest] OnConnectionErrored: " + exception.Message);
            _connectButton.interactable = true;
        }

        private void OnAuthorizationFailed(object sender, MonaWalletSDK.AuthorizationResult authorizationResult)
        {
            Debug.LogError("[MonaWalletConnectTest] OnAuthorizationFailed: " + authorizationResult);
            _authorizeButton.interactable = true;
        }

        private void OnAuthorized(object sender, EventArgs e)
        {
            isAUTHORIZED = true;
            Debug.Log("[MonaWalletConnectTest.OnAuthorized]");
            SetUIState(WalletState.Authorized);

            OnGetCollectibles();
        }

        private void OnConnected(object sender, string address)
        {
            Debug.Log("[MonaWalletConnectTest.OnConnected] address: " + address);
            SetUIState(WalletState.Connected);
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            isAUTHORIZED = false;
            UpdateMapIssues();
            Debug.Log("[MonaWalletConnectTest.OnDisconnected]");
            SetUIState(WalletState.Disconnected);
        }

        #endregion

        #region UI Click Events

        /// <summary>
        /// Handles the connect button click event to initiate wallet connection.
        /// </summary>
        public async void OnConnectButton()
        {
            Debug.Log("[MonaWalletConnectTest] OnConnectButton");
            _connectButton.interactable = false;

            try
            {
                SetUIState(WalletState.Connecting);
                await MonaverseManager.Instance.SDK.ConnectWallet();
            }
            catch (Exception exception)
            {
                Debug.Log("[MonaWalletConnectTest] failed: " + exception);
            }
        }

        /// <summary>
        /// Handles the disconnect button click event to disconnect the wallet.
        /// </summary>
        public async void OnDisconnectButton()
        {
            isAUTHORIZED = false;
            UpdateMapIssues();
            Debug.Log("[MonaWalletConnectTest] OnDisconnectButton");

            try
            {
                _disconnectButton.interactable = false;
                await MonaverseManager.Instance.SDK.Disconnect();
            }
            catch (Exception e)
            {
                _disconnectButton.interactable = true;
                Debug.LogError("[MonaWalletConnectTest] Disconnect Exception: " + e.Message);
            }
        }

        /// <summary>
        /// Handles the authorize button click event to authorize the connected wallet.
        /// </summary>
        public async void OnAuthorizeWallet()
        {
            try
            {
                _authorizeButton.interactable = false;
                Debug.Log("[MonaWalletConnectTest] OnAuthorizeWallet");

                _resultLabel.text = "Authorizing Wallet...";

                var authorizationResult = await MonaverseManager.Instance.SDK.AuthorizeWallet();

                var resultText = authorizationResult switch
                {
                    MonaWalletSDK.AuthorizationResult.WalletNotConnected => "Wallet Not Connected",
                    MonaWalletSDK.AuthorizationResult.Authorized => "Wallet Authorized",
                    MonaWalletSDK.AuthorizationResult.FailedAuthorizing => "Failed authorizing wallet",
                    MonaWalletSDK.AuthorizationResult.UserNotRegistered => "User not registered",
                    MonaWalletSDK.AuthorizationResult.FailedValidatingWallet => "Failed validating wallet",
                    MonaWalletSDK.AuthorizationResult.FailedSigningMessage => "Failed signing message",
                    MonaWalletSDK.AuthorizationResult.Error => "Unexpected error authorizing wallet",
                    _ => throw new ArgumentOutOfRangeException()
                };

                _authorizeButton.interactable = true;

                Debug.Log("[MonaWalletConnectTest] Authorization Result: " + resultText);

                if (authorizationResult != MonaWalletSDK.AuthorizationResult.Authorized)
                    _resultLabel.text = resultText;
            }
            catch (Exception exception)
            {
                Debug.LogError("[MonaWalletConnectTest] AuthorizeWallet Exception: " + exception.Message);
            }
        }

        /// <summary>
        /// Handles the sign-out button click event to sign out from the Monaverse.
        /// </summary>
        public void OnSignOut()
        {
            isAUTHORIZED = false;
            UpdateMapIssues();
            try
            {
                Debug.Log("[MonaWalletConnectTest] OnSignOut");

                _resultLabel.text = "Signing out from the Monaverse...";

                _signOutButton.interactable = false;

                MonaverseManager.Instance.SDK.ApiClient.ClearSession();

                _signOutButton.interactable = true;

                SetUIState(WalletState.Connected);
            }
            catch (Exception exception)
            {
                _authorizeButton.interactable = true;
                Debug.LogError("[MonaWalletConnectTest] SignOut Exception: " + exception.Message);
            }
        }

        /// <summary>
        /// Handles the GetCollectibles button click event to get authorized wallet collectibles.
        /// The wallet must be authorized first.
        /// </summary>
        public async void OnGetCollectibles()
        {
            if (!MonaverseManager.Instance.SDK.IsWalletAuthorized())
            {
                _resultLabel.text = "Wallet must be authorized first!";
                return;
            }

            _resultLabel.text = "Getting wallet collectibles...";

            var getCollectiblesResult = await MonaApi.ApiClient.Collectibles.GetWalletCollectibles();
            Debug.Log("[MonaWalletConnectTest] Collectibles: " + getCollectiblesResult);
            if (!getCollectiblesResult.IsSuccess)
            {
                _resultLabel.text = "Error: GetCollectibles failed: " + getCollectiblesResult.Message;
                return;
            }

            _//resultLabel.text = "Success: wallet collectible count: " + getCollectiblesResult.Data.TotalCount;
        }

        #endregion

        /// <summary>
        /// Sets the UI state based on the current wallet state.
        /// </summary>
        /// <param name="state"></param>
        private void SetUIState(WalletState state)
        {
            _dappButtons.SetActive(false);
            _connectedState.SetActive(false);
            _authorizedState.SetActive(false);

            switch (state)
            {
                case WalletState.Disconnected:
                    _connectButtonLabel.text = "Connect";
                    _connectButton.interactable = true;
                    _dappButtons.SetActive(false);
                    _connectedState.SetActive(false);
                    _disconnectButton.gameObject.SetActive(false);
                    _resultLabel.text = "Disconnected";
                    break;
                case WalletState.Connecting:
                    _connectButtonLabel.text = "Connecting...";
                    _connectButton.interactable = false;
                    break;
                case WalletState.Connected:
                    _connectButton.interactable = false;
                    _connectButtonLabel.text = "Connected";
                    _resultLabel.text = "Wallet Connected!";
                    _dappButtons.SetActive(true);
                    _connectedState.SetActive(true);
                    _disconnectButton.interactable = true;
                    _disconnectButton.gameObject.SetActive(true);
                    break;
                case WalletState.Authorized:
                    _dappButtons.SetActive(true);
                    _authorizedState.SetActive(true);
                    _resultLabel.text = "Wallet Authorized!";
                    break;
            }
        }
    }
}