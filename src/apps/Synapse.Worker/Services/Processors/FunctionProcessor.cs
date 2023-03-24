/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Synapse.Worker.Services.Processors;


/// <summary>
/// Represents the base class for all <see cref="IWorkflowActivityProcessor"/>s used to process <see cref="FunctionDefinition"/>s
/// </summary>
public abstract class FunctionProcessor
    : ActionProcessor
{

    /// <summary>
    /// Initializes a new <see cref="WorkflowActivityProcessor"/>
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
    /// <param name="context">The current <see cref="IWorkflowRuntimeContext"/></param>
    /// <param name="activityProcessorFactory">The service used to create <see cref="IWorkflowActivityProcessor"/>s</param>
    /// <param name="options">The service used to access the current <see cref="ApplicationOptions"/></param>
    /// <param name="activity">The <see cref="V1WorkflowActivity"/> to process</param>
    /// <param name="action">The <see cref="ActionDefinition"/> to process</param>
    /// <param name="function">The <see cref="FunctionDefinition"/> to process</param>
    public FunctionProcessor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
        IOptions<ApplicationOptions> options, V1WorkflowActivity activity, ActionDefinition action, FunctionDefinition function)
        : base(loggerFactory, context, activityProcessorFactory, options, activity, action)
    {
        this.ServiceProvider = serviceProvider;
        this.Function = function;
    }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the <see cref="FunctionDefinition"/> to process
    /// </summary>
    protected FunctionDefinition Function { get; }

    /// <summary>
    /// Gets the <see cref="ServerlessWorkflow.Sdk.Models.FunctionReference"/> to process
    /// </summary>
    protected FunctionReference FunctionReference
    {
        get
        {
            return this.Action.Function!;
        }
    }

    /// <summary>
    /// Gets the object used to configure the authentication mechanism to use when invoking the function
    /// </summary>
    protected AuthenticationDefinition? Authentication { get; private set; }

    /// <summary>
    /// Gets the object that describes the authorization resolved using the specified <see cref="Authentication"/>
    /// </summary>
    protected AuthorizationInfo? Authorization { get; private set; }

    /// <inheritdoc/>
    protected override async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(this.Function.AuthRef))
        {
            if (this.Context.Workflow.Definition.TryGetAuthentication(this.Function.AuthRef, out var auth))
            {
                if (auth.Properties is SecretBasedAuthenticationProperties secretBased)
                {
                    auth.Properties = auth.Scheme switch
                    {
                        AuthenticationScheme.Basic => await this.Context.GetSecretAsync<BasicAuthenticationProperties>(secretBased.Secret),
                        AuthenticationScheme.Bearer => await this.Context.GetSecretAsync<BearerAuthenticationProperties>(secretBased.Secret),
                        AuthenticationScheme.OAuth2 => await this.Context.GetSecretAsync<OAuth2AuthenticationProperties>(secretBased.Secret),
                        _ => throw new NotSupportedException($"The specified {nameof(AuthenticationScheme)} '{auth.Scheme}' is not supported"),
                    };
                }
                this.Authentication = auth;
                if (this.Authentication != null)
                {
                    this.Authorization = await AuthorizationInfo.CreateAsync(this.ServiceProvider, this.Authentication, cancellationToken);
                }
            }
            else
            {
                throw new NullReferenceException($"Failed to find the authentication definition with name '{this.Function.AuthRef}'");
            }
        }
    }

    /// <inheritdoc/>
    protected override async Task OnNextAsync(IV1WorkflowActivityIntegrationEvent e, CancellationToken cancellationToken)
    {
        if (e is V1WorkflowActivityCompletedIntegrationEvent completedEvent)
        {
            var output = completedEvent.Output.ToObject();
            if (this.Action.ActionDataFilter != null)
            {
                output = await this.Context.FilterOutputAsync(this.Action, output!, this.Authorization, cancellationToken);
            }
            await base.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
        }
        else
        {
            await base.OnNextAsync(e, cancellationToken);
        }
    }

}