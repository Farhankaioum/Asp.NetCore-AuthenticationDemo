using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basic.CustomPolicyProvider
{
    public class SecurityLevelAttribute : AuthorizeAttribute
    {
        public SecurityLevelAttribute(int level)
        {
            Policy = $"{DynamicPolicies.SecurityLevel}.{level}"; 
        }
    }

    // {type}
    public static class DynamicPolicies
    {
        public static IEnumerable<string> Get()
        {
            yield return SecurityLevel;
            yield return Rank;
        }

        public const string SecurityLevel = "SecurityLevel";
        public const string Rank = "Rank";
    }

    public static class DynamicAuthorizationPolicyFactory
    {
        public static AuthorizationPolicy Create(string policyName)
        {
            var parts = policyName.Split('.');
            var type = parts.First();
            var value = parts.Last();

            switch (type)
            {
                case DynamicPolicies.Rank:
                    return new AuthorizationPolicyBuilder()
                        .AddRequirements(new RankLevelRequirement(Convert.ToInt32(value)))
                        .Build();
                case DynamicPolicies.SecurityLevel:
                    return new AuthorizationPolicyBuilder()
                        .AddRequirements(new SecurityLevelRequirement(Convert.ToInt32(value)))
                        .Build();

                default:
                    return null;
            }
        }
    }

    // custom attribute for Rank level
    public class RankLevelAttribute : AuthorizeAttribute
    {
        public RankLevelAttribute(int level)
        {
            Policy = $"{DynamicPolicies.Rank}.{level}";
        }
    }


    // Rank level authorization Handler implemented
    public class RankLevelRequirement : IAuthorizationRequirement
    {
        public int Rank { get; }

        public RankLevelRequirement(int rank)
        {
            Rank = rank;
        }
    }

    public class RankLevelHandler : AuthorizationHandler<RankLevelRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RankLevelRequirement requirement)
        {
            var claimValue = Convert.ToInt32(context.User.Claims
                .FirstOrDefault(x => x.Type == DynamicPolicies.Rank)
                ?.Value ?? "0");

            if (requirement.Rank <= claimValue)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }


    // Security level authorization Handler implemented
    public class SecurityLevelRequirement : IAuthorizationRequirement
    {
        public int Level { get; }
        public SecurityLevelRequirement(int level)
        {
            Level = level;
        }
    }

    public class SecurityLevelHandler : AuthorizationHandler<SecurityLevelRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SecurityLevelRequirement requirement)
        {
            var claimvalue = Convert.ToInt32(context.User.Claims
                .FirstOrDefault(x => x.Type == DynamicPolicies.SecurityLevel)
                ?.Value ?? "0");

            if (requirement.Level <= claimvalue)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        // {type:value}
        public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var value = DynamicPolicies.Get();
            foreach (var customPolicy in DynamicPolicies.Get())
            {
                if (policyName.StartsWith(customPolicy))
                {
                    var policy = DynamicAuthorizationPolicyFactory.Create(policyName); ;

                    return Task.FromResult(policy);
                }
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}
