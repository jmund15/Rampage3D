using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public partial class UtilityContext : Resource
    {
        public IBlackboard BB { get; private set; } // could get all the info from this, but other properties help quicken the process
        public AIAffinitiesComponent Affinities { get; private set; }
        public float Health { get; private set; }
        public float HealthPercentage { get; private set; }

        public float ThreatLevel { get; private set; }
        public float SupportLevel { get; private set; }
        public float OpportunityLevel { get; private set; }
        public float MoneyOpportunityLevel { get; private set; }

        public Node2D CurrTarget { get; private set; }
        public float DistToTarget { get; private set; }

        public UtilityContext(IBlackboard bb, AIAffinitiesComponent affinities, 
            float health, float healthPercentage, 
            float threatLevel, float supportLevel, float opportunityLevel, float moneyOpportunityLevel,
            Node2D currTarget, float distToTarget)
        {
            BB = bb;
            Affinities = affinities;
            Health = health;
            HealthPercentage = healthPercentage;
            ThreatLevel = threatLevel;
            SupportLevel = supportLevel;
            OpportunityLevel = opportunityLevel;
            MoneyOpportunityLevel = moneyOpportunityLevel;
            CurrTarget = currTarget;
            DistToTarget = distToTarget;
        }
    }
}
