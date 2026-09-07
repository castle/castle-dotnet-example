using System;
using System.Threading.Tasks;
using Castle;
using Castle.Messages.Requests;
using Castle.Messages.Responses;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CastleDemo.Demos
{
    /// <summary>
    /// Decides which Castle endpoint/event each demo action maps to, builds the
    /// exact payload sent to Castle and executes it. Every method returns a JSON
    /// envelope the browser renders: the payload that was sent and Castle's
    /// verdict, so the SDK behaviour is fully transparent.
    /// </summary>
    public class CastleFlows
    {
        private readonly CastleClient _client;
        private readonly DemoConfig _cfg;

        private static readonly JsonSerializer SnakeSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                NullValueHandling = NullValueHandling.Ignore
            });

        public CastleFlows(CastleClient client, DemoConfig cfg)
        {
            _client = client;
            _cfg = cfg;
        }

        // --- sign up -------------------------------------------------------

        // A registration is evaluated before the account exists, so it is
        // anonymous activity sent to Filter with the submitted form params. A
        // brand-new email is an attempt; an email that already belongs to a
        // user is a failed registration resolved via matching_user_id.
        public async Task<JObject> EvaluateSignup(string email, string requestToken, HttpRequest http)
        {
            var existing = email == _cfg.ValidUsername;
            var status = existing ? "$failed" : "$attempted";

            var payload = BuildPayload("$registration", status, requestToken, http);
            payload["params"] = new JObject { ["email"] = email };
            if (existing)
            {
                payload["matching_user_id"] = _cfg.ValidUserId;
            }

            return await RunSingle("filter", "$registration", status, payload);
        }

        // --- login ---------------------------------------------------------

        // A login reuses one request token across two calls: first Filter the
        // attempt while the visitor is still anonymous, then — on success —
        // assess the authenticated user with Risk. A failed attempt stays on
        // Filter.
        public async Task<JObject> EvaluateLogin(string email, string password, string requestToken, HttpRequest http)
        {
            var steps = new JArray();

            // Step 1 — always filter the attempt up front (anonymous -> params).
            var attempt = BuildPayload("$login", "$attempted", requestToken, http);
            attempt["params"] = new JObject { ["email"] = email };
            steps.Add(await RunStep("filter", "$login", "$attempted", attempt));

            // Step 2 — the outcome, on the same request token.
            if (email == _cfg.ValidUsername && password == _cfg.ValidPassword)
            {
                var success = BuildPayload("$login", "$succeeded", requestToken, http);
                success["user"] = AuthenticatedUser(email);
                steps.Add(await RunStep("risk", "$login", "$succeeded", success));
            }
            else
            {
                var failed = BuildPayload("$login", "$failed", requestToken, http);
                failed["params"] = new JObject { ["email"] = email };
                if (email == _cfg.ValidUsername)
                {
                    failed["matching_user_id"] = _cfg.ValidUserId;
                }
                steps.Add(await RunStep("filter", "$login", "$failed", failed));
            }

            return new JObject { ["steps"] = steps };
        }

        // --- account -------------------------------------------------------

        public async Task<JObject> EvaluateProfileUpdate(string name, string email, string requestToken, HttpRequest http)
        {
            email = string.IsNullOrEmpty(email) ? _cfg.ValidUsername : email;

            var payload = BuildPayload("$profile_update", "$succeeded", requestToken, http);
            var user = AuthenticatedUser(email);
            user["name"] = name;
            payload["user"] = user;

            return await RunSingle("risk", "$profile_update", "$succeeded", payload);
        }

        public async Task<JObject> EvaluateLogout(string requestToken, HttpRequest http)
        {
            var payload = BuildPayload("$logout", "$succeeded", requestToken, http);
            payload["user"] = new JObject
            {
                ["id"] = _cfg.ValidUserId,
                ["email"] = _cfg.ValidUsername
            };

            return await RunSingle("log", "$logout", "$succeeded", payload);
        }

        // --- password reset ------------------------------------------------

        // A new password that differs from the current one is a successful
        // reset; reusing the current password is a failed one. Recorded with
        // the non-blocking Log endpoint.
        public async Task<JObject> EvaluatePasswordReset(string password, string requestToken, HttpRequest http)
        {
            var status = password == _cfg.ValidPassword ? "$failed" : "$succeeded";

            var payload = BuildPayload("$password_reset", status, requestToken, http);
            payload["user"] = AuthenticatedUser(_cfg.ValidUsername);

            return await RunSingle("log", "$password_reset", status, payload);
        }

        // --- lists ---------------------------------------------------------

        public async Task<JObject> CreateListDemo(string name, string color, string primaryField)
        {
            var request = new CreateListRequest
            {
                Name = string.IsNullOrEmpty(name) ? "demo-blocklist" : name,
                Color = string.IsNullOrEmpty(color) ? "$red" : color,
                PrimaryField = string.IsNullOrEmpty(primaryField) ? "user.email" : primaryField
            };

            var payload = JObject.FromObject(request, SnakeSerializer);
            JToken result;
            try
            {
                var created = await _client.CreateList(request);
                var allLists = await _client.GetAllLists();
                result = new JObject
                {
                    ["created"] = created == null ? null : JObject.FromObject(created, SnakeSerializer),
                    ["all_lists"] = allLists == null ? new JArray() : JArray.FromObject(allLists, SnakeSerializer)
                };
            }
            catch (Exception e)
            {
                result = new JObject { ["error"] = e.Message };
            }

            return new JObject
            {
                ["api_endpoint"] = "lists",
                ["payload_to_castle"] = payload,
                ["result"] = result
            };
        }

        // --- privacy -------------------------------------------------------

        public async Task<JObject> PrivacyUserData(string action, string identifier, string identifierType)
        {
            var request = new PrivacyRequest
            {
                Identifier = string.IsNullOrEmpty(identifier) ? _cfg.ValidUsername : identifier,
                IdentifierType = string.IsNullOrEmpty(identifierType) ? "$email" : identifierType
            };

            var payload = JObject.FromObject(request, SnakeSerializer);
            var isDelete = action == "delete";
            string apiEndpoint;
            JToken result;
            try
            {
                if (isDelete)
                {
                    apiEndpoint = "privacy (delete)";
                    await _client.DeleteUserData(request);
                }
                else
                {
                    apiEndpoint = "privacy (request)";
                    await _client.RequestUserData(request);
                }
                result = new JObject { ["submitted"] = true };
            }
            catch (Exception e)
            {
                apiEndpoint = "privacy";
                result = new JObject { ["error"] = e.Message };
            }

            return new JObject
            {
                ["api_endpoint"] = apiEndpoint,
                ["payload_to_castle"] = payload,
                ["result"] = result
            };
        }

        // --- helpers -------------------------------------------------------

        private JObject AuthenticatedUser(string email)
        {
            return new JObject
            {
                ["id"] = _cfg.ValidUserId,
                ["email"] = email,
                ["registered_at"] = _cfg.RegisteredAt
            };
        }

        // Build the base payload JObject for an event using the SDK, so it
        // carries the same shape (context, sent_at, ...) the SDK would send.
        private JObject BuildPayload(string type, string status, string requestToken, HttpRequest http)
        {
            var request = new ActionRequest
            {
                Type = type,
                Status = status,
                RequestToken = requestToken,
                Context = BuildContext(http),
                User = null,
                UserTraits = null
            };

            // The endpoint doesn't change the serialized payload, so Filter's
            // builder is fine for shaping every event's JSON.
            return _client.BuildFilterRequest(request);
        }

        private static RequestContext BuildContext(HttpRequest http)
        {
            var context = Context.FromHttpRequest(http);

            // Castle rejects loopback addresses; substitute a sample public IP
            // when running on localhost so the demo calls go through.
            if (string.IsNullOrEmpty(context.Ip)
                || context.Ip == "::1"
                || context.Ip.StartsWith("127.")
                || context.Ip == "localhost")
            {
                context.Ip = "1.2.3.4";
            }

            return context;
        }

        private async Task<JObject> RunSingle(string endpoint, string type, string status, JObject payload)
        {
            var result = await Execute(endpoint, payload);

            var envelope = new JObject
            {
                ["api_endpoint"] = endpoint,
                ["payload_to_castle"] = payload,
                ["castle_type"] = type,
                ["castle_status"] = status
            };

            // The log endpoint is non-blocking and returns no verdict.
            if (endpoint != "log")
            {
                envelope["result"] = result;
            }

            return envelope;
        }

        private async Task<JObject> RunStep(string endpoint, string type, string status, JObject payload)
        {
            var result = await Execute(endpoint, payload);

            return new JObject
            {
                ["api_endpoint"] = endpoint,
                ["payload_to_castle"] = payload,
                ["castle_type"] = type,
                ["castle_status"] = status,
                ["result"] = result
            };
        }

        // Execute a built payload against the chosen Castle endpoint. Returns
        // the verdict for risk/filter, null for the non-blocking log endpoint,
        // or an {error} object on failure.
        private async Task<JToken> Execute(string endpoint, JObject payload)
        {
            try
            {
                if (endpoint == "risk")
                {
                    return VerdictToJson(await _client.SendRiskRequest(payload));
                }
                if (endpoint == "filter")
                {
                    return VerdictToJson(await _client.SendFilterRequest(payload));
                }

                await _client.SendLogRequest(payload);
                return null;
            }
            catch (Exception e)
            {
                return new JObject { ["error"] = e.Message };
            }
        }

        private static JObject VerdictToJson(RiskResponse res)
        {
            if (res == null)
            {
                return null;
            }

            var verdict = new JObject { ["risk"] = res.Risk };

            if (res.Policy != null)
            {
                verdict["policy"] = new JObject
                {
                    ["name"] = res.Policy.Name,
                    ["action"] = res.Policy.Action.ToString().ToLowerInvariant(),
                    ["id"] = res.Policy.Id,
                    ["revision_id"] = res.Policy.RevisionId
                };
            }

            if (res.Signals != null)
            {
                verdict["signals"] = res.Signals;
            }
            if (res.Scores != null)
            {
                verdict["scores"] = JObject.FromObject(res.Scores, SnakeSerializer);
            }
            if (res.Device != null)
            {
                verdict["device"] = JObject.FromObject(res.Device, SnakeSerializer);
            }

            verdict["failover"] = res.Failover;
            if (res.Failover)
            {
                verdict["failover_reason"] = res.FailoverReason;
            }

            return verdict;
        }
    }
}
