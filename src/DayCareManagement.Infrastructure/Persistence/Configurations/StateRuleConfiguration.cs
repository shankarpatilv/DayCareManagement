using DayCareManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayCareManagement.Infrastructure.Persistence.Configurations;

public sealed class StateRuleConfiguration : IEntityTypeConfiguration<StateRule>
{
    public void Configure(EntityTypeBuilder<StateRule> builder)
    {
        builder.ToTable("StateRules");

        builder.HasKey(stateRule => new
        {
            stateRule.VaccineName,
            stateRule.DoseRequirement,
            stateRule.AgeMonths
        });

        builder.Property(stateRule => stateRule.VaccineName).IsRequired();
        builder.Property(stateRule => stateRule.DoseRequirement).IsRequired();
        builder.Property(stateRule => stateRule.AgeMonths).IsRequired();
    }
}