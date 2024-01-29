using System;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Core.Pipeline;
using UnityEngine;

namespace RegressionGames.Unity
{
    [CreateAssetMenu(fileName = "OpenAICredentials.asset", menuName = "Regression Games/OpenAI Credentials")]
    public class OpenAICredentials: ScriptableObject
    {
        public string apiKey;
        public string organizationId;

        public OpenAIClient CreateClient(OpenAIClientOptions options = null)
        {
            options ??= new();
            if (!string.IsNullOrEmpty(organizationId))
            {
                options.AddPolicy(new OrganizationIdPolicy(organizationId), HttpPipelinePosition.PerCall);
            }
            return new OpenAIClient(apiKey, options);
        }

        public class OrganizationIdPolicy : HttpPipelinePolicy
        {
            private readonly string m_OrganizationId;

            public OrganizationIdPolicy(string organizationId)
            {
                m_OrganizationId = organizationId;
            }

            public override ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
            {
                message.Request.Headers.Add("OpenAI-Organization", m_OrganizationId);
                return ProcessNextAsync(message, pipeline);
            }

            public override void Process(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
            {
                message.Request.Headers.Add("OpenAI-Organization", m_OrganizationId);
                ProcessNextAsync(message, pipeline);
            }
        }
    }
}
