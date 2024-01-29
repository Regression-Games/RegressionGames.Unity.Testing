using System;
using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.AI.OpenAI;
using Newtonsoft.Json;
using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    [Serializable]
    public class PromptConfiguration
    {
        [TextArea(5, 20)]
        public string systemPrompt = GptAssistantBot.DefaultSystemPrompt;
    }

    class AssistantFunction
    {
        public ChatCompletionsFunctionToolDefinition definition;
        public Func<GptAssistantBot, ChatCompletionsFunctionToolCall, Dictionary<string, string>, FunctionOutcome> handler;

        public AssistantFunction(
            ChatCompletionsFunctionToolDefinition definition, Func<GptAssistantBot, ChatCompletionsFunctionToolCall, Dictionary<string, string>, FunctionOutcome> handler)
        {
            this.definition = definition;
            this.handler = handler;
        }
    }

    enum FunctionOutcome
    {
        Continue,
        GoalReached,
        TestFailed,
        InvalidResponseFromModel,
    }

    [AddComponentMenu("Regression Games/Bots/GPT Assistant")]
    public class GptAssistantBot: Bot
    {
        internal static readonly string DefaultSystemPrompt = @"You are an automated QA testing engine for a video game.
Each prompt will contain information about your goal, the entities in the scene, their state, and the available actions on those entities.
Your response should contain a call to one of the provided tools, specifying the action to take.
You must not call an undefined function, or reference an entity or action that does not exist in the frame snapshot.
Your objective is to take an action to progress towards achieving the goal in the prompt.
It may be necessary to wait for the game to progress after taking an action.
Be patient, but raise an exception if the game has not progressed after 5 or more wait calls.";

        private static readonly List<ChatCompletionsToolDefinition> Tools;
        private static readonly Dictionary<string, Func<GptAssistantBot, ChatCompletionsFunctionToolCall, Dictionary<string, string>, FunctionOutcome>> Handlers;

        static GptAssistantBot()
        {
            var fns = CreateTools().ToList();
            Tools = fns.Select(f => (ChatCompletionsToolDefinition)f.definition).ToList();
            Handlers = fns.ToDictionary(f => f.definition.Name, f => f.handler);
        }

        private readonly Logger<GptAssistantBot> m_Log;

        public int activationIntervalInSeconds = 1;
        public OpenAICredentials openAiCredentials;
        public string openAiModel = "gpt-4-1106-preview";

        [TextArea(3, 10)]
        public string goal;

        public PromptConfiguration promptConfiguration = new();

        private float m_NextActivation;
        private bool m_Thinking;
        private OpenAIClient m_OpenAIClient;
        private readonly ChatCompletionsOptions m_Conversation = new();

        public GptAssistantBot()
        {
            m_Log = Logger.For(this);
        }

        protected override void Awake()
        {
            base.Awake();

            if(openAiCredentials == null)
            {
                m_Log.Error("OpenAI credentials are not set. Please set this in the inspector.");
                StopBot();
                return;
            }

            m_OpenAIClient = openAiCredentials.CreateClient();
            m_Conversation.DeploymentName = openAiModel;
            foreach (var tool in Tools)
            {
                m_Conversation.Tools.Add(tool);
            }
            m_Conversation.ToolChoice = ChatCompletionsToolChoice.Auto;
            ResetConversation();

            m_NextActivation = Time.time + activationIntervalInSeconds;
        }

        private async void Update()
        {
            // Don't run if it's not yet time to run.
            if(m_Thinking || Time.time < m_NextActivation)
            {
                return;
            }

            m_Thinking = true;

            try
            {
                m_Log.Verbose($"Activating at {Time.time}");

                var prompt = GeneratePrompt();

                // TODO: Clean up the conversation history? We just keep appending here, so we need a large context model like gpt-4-1106-preview!
                m_Conversation.Messages.Add(new ChatRequestUserMessage(prompt));

                // This will wait for the response and resume us on the next frame after the response arrives.
                m_Log.Info("Requesting response from OpenAI.");
                var response = await m_OpenAIClient.GetChatCompletionsAsync(m_Conversation);

                m_Log.Info($"Response received. Prompt tokens: {response.Value.Usage.PromptTokens}, Completion tokens: {response.Value.Usage.CompletionTokens}");

                var choice = response.Value.Choices.FirstOrDefault();
                if (choice == null)
                {
                    m_Log.Error("No choices in response.");
                    return;
                }

                var outcome = HandleCall(choice);
                if (outcome != FunctionOutcome.Continue)
                {
                    // If any function returns a non-Continue outcome, stop the bot.
                    StopBot();
                }
            }
            finally
            {
                m_Thinking = false;
                m_NextActivation = Time.time + activationIntervalInSeconds;
                m_Log.Info($"Activation complete. Next activation at {m_NextActivation}");
            }
        }

        private void ResetConversation()
        {
            m_Conversation.Messages.Clear();
            m_Conversation.Messages.Add(new ChatRequestSystemMessage(promptConfiguration.systemPrompt));
        }

        private string GeneratePrompt()
        {
            // Serialize the frame snapshot to JSON.
            var snapshot = AutomationController.automationRecorder.CreateFrameSnapshot();
            var snapshotJson = RegressionGamesJsonFormat.Serialize(snapshot, Formatting.None);

            // Generate the prompt
            var prompt = $@"Goal: {goal}\nFrame Snapshot:\n```\n{snapshotJson}\n```";
            return prompt;
        }

        private FunctionOutcome HandleCall(ChatChoice choice)
        {
            m_Conversation.Messages.Add(new ChatRequestAssistantMessage(choice.Message));

            if (choice.Message.ToolCalls.Count == 0)
            {
                m_Log.Error("No tool calls in response.");
                return FunctionOutcome.InvalidResponseFromModel;
            }

            foreach (var call in choice.Message.ToolCalls)
            {
                if (call is not ChatCompletionsFunctionToolCall functionCall)
                {
                    m_Log.Error("Non-function tool call in response.");
                    return FunctionOutcome.InvalidResponseFromModel;
                }

                if (Handlers.TryGetValue(functionCall.Name, out var handler))
                {
                    // TODO: Support non-string arguments.
                    var arguments = JsonConvert.DeserializeObject<Dictionary<string, string>>(functionCall.Arguments);
                    if (!arguments.TryGetValue("reasoning", out var reasoning))
                    {
                        m_Log.Error("Missing reasoning parameter.");
                        return FunctionOutcome.InvalidResponseFromModel;
                    }
                    m_Log.Info($"[{functionCall.Name}] Model is invoking because '{reasoning}");
                    var outcome = handler(this, functionCall, arguments);
                    m_Log.Info($"[{functionCall.Name}] Outcome: {outcome}");
                    if(outcome != FunctionOutcome.Continue)
                    {
                        // If any function returns a non-Continue outcome, stop now.
                        return outcome;
                    }

                    // Make sure we put a tool response in the conversation history for the next invocation.
                    m_Conversation.Messages.Add(new ChatRequestToolMessage("Success", functionCall.Id));
                }
                else
                {
                    m_Log.Error($"Unknown function: {functionCall.Name}.");
                    return FunctionOutcome.InvalidResponseFromModel;
                }
            }

            // All calls were handled and returned Continue.
            return FunctionOutcome.Continue;
        }

        private FunctionOutcome HandleInvokeAction(ChatCompletionsFunctionToolCall call, Dictionary<string, string> arguments)
        {
            if (!arguments.TryGetValue("entity_id", out var entityIdStr) || !int.TryParse(entityIdStr, out var entityId))
            {
                m_Log.Error("Missing entity_id parameter.");
                return FunctionOutcome.InvalidResponseFromModel;
            }

            if (!arguments.TryGetValue("action_id", out var actionId))
            {
                m_Log.Error("Missing action_id parameter.");
                return FunctionOutcome.InvalidResponseFromModel;
            }

            var entity = AutomationController.Entities.FirstOrDefault(e => e.Id == entityId);
            if (entity == null)
            {
                m_Log.Error($"Unknown entity: {entityId}");
                return FunctionOutcome.InvalidResponseFromModel;
            }

            var action = entity.Actions.TryGetValue(actionId, out var a) ? a : null;
            if(action == null)
            {
                m_Log.Error($"Unknown action: {actionId}");
                return FunctionOutcome.InvalidResponseFromModel;
            }

            m_Log.Info($"Activating action {entityId}/{actionId}");
            action.Activate();
            return FunctionOutcome.Continue;
        }

        private FunctionOutcome HandleWait(ChatCompletionsFunctionToolCall call, Dictionary<string, string> arguments)
        {
            m_Log.Info("Waiting for next tick.");
            return FunctionOutcome.Continue;
        }

        private FunctionOutcome HandleGoalReached(ChatCompletionsFunctionToolCall call, Dictionary<string, string> arguments)
        {
            m_Log.Info("Goal Reached!");
            return FunctionOutcome.GoalReached;
        }

        private FunctionOutcome HandleException(ChatCompletionsFunctionToolCall call, Dictionary<string, string> arguments)
        {
            if (!arguments.TryGetValue("message", out var message))
            {
                m_Log.Error("Missing message parameter.");
                return FunctionOutcome.InvalidResponseFromModel;
            }

            m_Log.Error($"Test Failure: {message}");
            return FunctionOutcome.TestFailed;
        }

        private static IEnumerable<AssistantFunction> CreateTools()
        {
            yield return new(
                new ()
                {
                    Name = "wait",
                    Description = "Waits for the next tick of the game.",
                    Parameters = BinaryData.FromObjectAsJson(new
                    {
                        type = "object",
                        properties = new
                        {
                            reasoning = new
                            {
                                type = "string",
                                description = "A description of why you believe waiting is the correct action to take.",
                            },
                        }
                    }),
                },
                (self, call, arguments) => self.HandleWait(call, arguments));
            yield return new(
                new()
                {
                    Name = "invoke_action",
                    Description = "Invokes an action on an entity.",
                    Parameters = BinaryData.FromObjectAsJson(new
                    {
                        type = "object",
                        properties = new
                        {
                            entity_id = new
                            {
                                type = "string",
                                description = "The ID of the entity, as provided in the previous prompt",
                            },
                            action_id = new
                            {
                                type = "string",
                                description = "The ID of the action, as provided in the previous prompt",
                            },
                            reasoning = new
                            {
                                type = "string",
                                description =
                                    "A description of why you are taking the specified action and how it makes progress towards the goal.",
                            },
                        },
                        required = new[] { "entity_id", "action_id", "reasoning" }
                    }),
                },
                (self, call, arguments) => self.HandleInvokeAction(call, arguments));
            yield return new(
                new()
                {
                    Name = "goal_reached",
                    Description = "Indicate that the goal has been reached and terminate the test.",
                    Parameters = BinaryData.FromObjectAsJson(new
                    {
                        type = "object",
                        properties = new
                        {
                            reasoning = new
                            {
                                type = "string",
                                description = "A description of why you believe the goal has been reached.",
                            },
                        }
                    }),
                },
                (self, call, arguments) => self.HandleGoalReached(call, arguments));
            yield return new(
                new()
                {
                    Name = "throw_exception",
                    Description = "Throw an exception to terminate the test.",
                    Parameters = BinaryData.FromObjectAsJson(new
                    {
                        type = "object",
                        properties = new
                        {
                            message = new
                            {
                                type = "string",
                                description =
                                    "A message to be presented to a human QA engineer to understand why the test failed to reach the goal.",
                            },
                            reasoning = new
                            {
                                type = "string",
                                description = "A description of why you are throwing the exception.",
                            },
                        }
                    }),
                },
                (self, call, arguments) => self.HandleException(call, arguments));
        }
    }
}
