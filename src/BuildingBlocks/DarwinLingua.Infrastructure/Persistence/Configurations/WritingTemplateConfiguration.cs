using DarwinLingua.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarwinLingua.Infrastructure.Persistence.Configurations;

internal sealed class WritingTemplateConfiguration : IEntityTypeConfiguration<WritingTemplate>
{
    public void Configure(EntityTypeBuilder<WritingTemplate> builder)
    {
        builder.ToTable("WritingTemplates");
        builder.HasKey(template => template.Id);
        builder.Property(template => template.Id).ValueGeneratedNever();
        builder.Property(template => template.Slug).HasMaxLength(128).IsRequired();
        builder.Property(template => template.Title).HasMaxLength(256).IsRequired();
        builder.Property(template => template.ShortDescription).HasMaxLength(1000).IsRequired();
        builder.Property(template => template.CefrLevel).HasConversion<string>().HasMaxLength(8).IsRequired();
        builder.Property(template => template.Category).HasMaxLength(96).IsRequired();
        builder.Property(template => template.Situation).HasMaxLength(512).IsRequired();
        builder.Property(template => template.Register).HasMaxLength(64).IsRequired();
        builder.Property(template => template.TemplateText).HasColumnType("TEXT").IsRequired();
        builder.Property(template => template.Explanation).HasMaxLength(4000).IsRequired();
        builder.Property(template => template.VariablesJson).HasColumnType("TEXT").IsRequired();
        builder.Property(template => template.SampleFilledVersion).HasColumnType("TEXT").IsRequired();
        builder.Property(template => template.LinkedGrammarTopicSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(template => template.LinkedWordSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(template => template.LinkedExpressionSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(template => template.LinkedExerciseSlugsJson).HasColumnType("TEXT").IsRequired();
        builder.Property(template => template.PublicationStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(template => template.SortOrder).IsRequired();
        builder.Property(template => template.CreatedAtUtc).IsRequired();
        builder.Property(template => template.UpdatedAtUtc).IsRequired();
        builder.HasIndex(template => template.Slug).IsUnique();
        builder.HasIndex(template => new { template.CefrLevel, template.Category, template.Register });
    }
}
