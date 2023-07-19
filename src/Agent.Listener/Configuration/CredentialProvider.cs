// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Services.Agent.Util;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;

namespace Microsoft.VisualStudio.Services.Agent.Listener.Configuration
{
    public interface ICredentialProvider
    {
        Boolean RequireInteractive { get; }
        CredentialData CredentialData { get; set; }
        VssCredentials GetVssCredentials(IHostContext context);
        void EnsureCredential(IHostContext context, CommandSettings command, string serverUrl);
    }

    public abstract class CredentialProvider : ICredentialProvider
    {
        public CredentialProvider(string scheme)
        {
            CredentialData = new CredentialData();
            CredentialData.Scheme = scheme;
        }

        public virtual Boolean RequireInteractive => false;
        public CredentialData CredentialData { get; set; }

        public abstract VssCredentials GetVssCredentials(IHostContext context);
        public abstract void EnsureCredential(IHostContext context, CommandSettings command, string serverUrl);
    }

    public sealed class PersonalAccessToken : CredentialProvider
    {
        public PersonalAccessToken() : base(Constants.Configuration.PAT) { }

        public override VssCredentials GetVssCredentials(IHostContext context)
        {
            ArgUtil.NotNull(context, nameof(context));
            Tracing trace = context.GetTrace(nameof(PersonalAccessToken));
            trace.Info(nameof(GetVssCredentials));
            ArgUtil.NotNull(CredentialData, nameof(CredentialData));
            string token;
            if (!CredentialData.Data.TryGetValue(Constants.Agent.CommandLine.Args.Token, out token))
            {
                token = null;
            }

            ArgUtil.NotNullOrEmpty(token, nameof(token));

            trace.Info("token retrieved: {0} chars", token.Length);

            // PAT uses a basic credential
            VssBasicCredential basicCred = new VssBasicCredential("VstsAgent", token);
            VssCredentials creds = new VssCredentials(null, basicCred, CredentialPromptType.DoNotPrompt);
            trace.Info("cred created");

            return creds;
        }

        public override void EnsureCredential(IHostContext context, CommandSettings command, string serverUrl)
        {
            ArgUtil.NotNull(context, nameof(context));
            Tracing trace = context.GetTrace(nameof(PersonalAccessToken));
            trace.Info(nameof(EnsureCredential));
            ArgUtil.NotNull(command, nameof(command));
            CredentialData.Data[Constants.Agent.CommandLine.Args.Token] = command.GetToken();
        }
    }

    public sealed class ServiceIdentityCredential : CredentialProvider
    {
        public ServiceIdentityCredential() : base(Constants.Configuration.ServiceIdentity) { }

        public override VssCredentials GetVssCredentials(IHostContext context)
        {
            ArgUtil.NotNull(context, nameof(context));
            Tracing trace = context.GetTrace(nameof(ServiceIdentityCredential));
            trace.Info(nameof(GetVssCredentials));
            ArgUtil.NotNull(CredentialData, nameof(CredentialData));
            string token;
            if (!CredentialData.Data.TryGetValue(Constants.Agent.CommandLine.Args.Token, out token))
            {
                token = null;
            }

            string username;
            if (!CredentialData.Data.TryGetValue(Constants.Agent.CommandLine.Args.UserName, out username))
            {
                username = null;
            }

            ArgUtil.NotNullOrEmpty(token, nameof(token));
            ArgUtil.NotNullOrEmpty(username, nameof(username));

            trace.Info("token retrieved: {0} chars", token.Length);

            // ServiceIdentity uses a service identity credential
            VssServiceIdentityToken identityToken = new VssServiceIdentityToken(token);
            VssServiceIdentityCredential serviceIdentityCred = new VssServiceIdentityCredential(username, "", identityToken);
            VssCredentials creds = new VssCredentials(null, serviceIdentityCred, CredentialPromptType.DoNotPrompt);
            trace.Info("cred created");

            return creds;
        }

        public override void EnsureCredential(IHostContext context, CommandSettings command, string serverUrl)
        {
            ArgUtil.NotNull(context, nameof(context));
            Tracing trace = context.GetTrace(nameof(ServiceIdentityCredential));
            trace.Info(nameof(EnsureCredential));
            ArgUtil.NotNull(command, nameof(command));
            CredentialData.Data[Constants.Agent.CommandLine.Args.Token] = command.GetToken();
            CredentialData.Data[Constants.Agent.CommandLine.Args.UserName] = command.GetUserName();
        }
    }

    public sealed class AlternateCredential : CredentialProvider
    {
        public AlternateCredential() : base(Constants.Configuration.Alternate) { }

        public override VssCredentials GetVssCredentials(IHostContext context)
        {
            ArgUtil.NotNull(context, nameof(context));
            Tracing trace = context.GetTrace(nameof(AlternateCredential));
            trace.Info(nameof(GetVssCredentials));

            string username;
            if (!CredentialData.Data.TryGetValue(Constants.Agent.CommandLine.Args.UserName, out username))
            {
                username = null;
            }

            string password;
            if (!CredentialData.Data.TryGetValue(Constants.Agent.CommandLine.Args.Password, out password))
            {
                password = null;
            }

            ArgUtil.NotNull(username, nameof(username));
            ArgUtil.NotNull(password, nameof(password));

            trace.Info("username retrieved: {0} chars", username.Length);
            trace.Info("password retrieved: {0} chars", password.Length);

            VssBasicCredential loginCred = new VssBasicCredential(username, password);
            VssCredentials creds = new VssCredentials(null, loginCred, CredentialPromptType.DoNotPrompt);
            trace.Info("cred created");

            return creds;
        }

        public override void EnsureCredential(IHostContext context, CommandSettings command, string serverUrl)
        {
            ArgUtil.NotNull(context, nameof(context));
            Tracing trace = context.GetTrace(nameof(AlternateCredential));
            trace.Info(nameof(EnsureCredential));
            ArgUtil.NotNull(command, nameof(command));
            CredentialData.Data[Constants.Agent.CommandLine.Args.UserName] = command.GetUserName();
            CredentialData.Data[Constants.Agent.CommandLine.Args.Password] = command.GetPassword();
        }
    }


    public sealed class DeviceCodeCredential : CredentialProvider
    {
        private VssCredentials _cacheCreds = null;

        //Default Application (Client) Id
        private readonly string _clientId = "97877f11-0fc6-4aee-b1ff-febb0519dd00";

        //Default ADO resource ID for user impersonation scope
        private readonly string _userImpersonationScope = "499b84ac-1321-427f-aa17-267ca6975798/user_impersonation";
        public DeviceCodeCredential() : base(Constants.Configuration.DeviceCode) { }

        public override VssCredentials GetVssCredentials(IHostContext context)
        {
            if (_cacheCreds != null)
                return _cacheCreds;

            ArgUtil.NotNull(context, nameof(context));
            Tracing trace = context.GetTrace(nameof(DeviceCodeCredential));
            trace.Info(nameof(GetVssCredentials));

            var app = PublicClientApplicationBuilder.Create(_clientId).Build();

            var authResult = AcquireATokenFromCacheOrDeviceCodeFlowAsync(context, app, new string[] { _userImpersonationScope }).GetAwaiter().GetResult(); ;

            var aadCred = new VssAadCredential(new VssAadToken(authResult.TokenType, authResult.AccessToken));
            VssCredentials creds = new VssCredentials(null, aadCred, CredentialPromptType.DoNotPrompt);
            trace.Info("cred created");
            _cacheCreds = creds;
            return creds;
        }
        public override void EnsureCredential(IHostContext context, CommandSettings command, string serverUrl)
        {
            ArgUtil.NotNull(context, nameof(context));
            Tracing trace = context.GetTrace(nameof(DeviceCodeCredential));
            trace.Info(nameof(EnsureCredential));
            ArgUtil.NotNull(command, nameof(command));
        }

        public async Task<AuthenticationResult> AcquireATokenFromCacheOrDeviceCodeFlowAsync(IHostContext context, IPublicClientApplication app, IEnumerable<String> scopes)
        {
            AuthenticationResult result = null;
            var accounts = await app.GetAccountsAsync().ConfigureAwait(false);

            if (accounts.Any())
            {

                // Attempt to get a token from the cache (or refresh it silently if needed)
                result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync().ConfigureAwait(false);

            }

            // Cache empty or no token for account in the cache, attempt by device code flow
            if (result == null)
            {
                result = await GetTokenUsingDeviceCodeFlowAsync(context, app, scopes).ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Gets an access token so that the application accesses the web api in the name of the user
        /// who signs-in on a separate device
        /// </summary>
        /// <returns>An authentication result, or null if the user canceled sign-in, or did not sign-in on a separate device
        /// after a timeout (15 mins)</returns>
        private async Task<AuthenticationResult> GetTokenUsingDeviceCodeFlowAsync(IHostContext context, IPublicClientApplication app, IEnumerable<string> scopes)
        {
            AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenWithDeviceCode(scopes,
                    deviceCodeCallback =>
                    {
                        // This will print the message on the console which tells the user where to go sign-in using 
                        // a separate browser and the code to enter once they sign in.
                        var term = context.GetService<ITerminal>();
                        term.WriteLine($"Please finish AAD device code flow in browser ({deviceCodeCallback.VerificationUrl}), user code: {deviceCodeCallback.UserCode}"); return Task.FromResult(0);
                    }).ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalServiceException ex)
            {
                string errorCode = ex.ErrorCode;
                // AADSTS50059: No tenant-identifying information found in either the request or implied by any provided credentials.
                // AADSTS90133: Device Code flow is not supported under /common or /consumers endpoint.
                // AADSTS90002: Tenant <tenantId or domain you used in the authority> not found. This may happen if there are 
                // no active subscriptions for the tenant. Check with your subscription administrator.
                throw;
            }
            catch (OperationCanceledException)
            {
                result = null;
            }
            catch (MsalClientException ex)
            {
                string errorCode = ex.ErrorCode;
                result = null;
            }
            return result;
        }
    }

}
