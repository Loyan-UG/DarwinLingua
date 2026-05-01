using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents a topic link for a lexical entry.
/// </summary>
public sealed class WordTopic
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WordTopic"/> class for EF Core materialization.
    /// </summary>
    private WordTopic()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WordTopic"/> class.
    /// </summary>
    internal WordTopic(
        Guid id,
        Guid wordEntryId,
        Guid topicId,
        bool isPrimaryTopic,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Word topic identifier cannot be empty.");
        }

        if (wordEntryId == Guid.Empty)
        {
            throw new DomainRuleException("Word entry identifier cannot be empty for a topic link.");
        }

        if (topicId == Guid.Empty)
        {
            throw new DomainRuleException("Topic identifier cannot be empty for a topic link.");
        }

        if (createdAtUtc == default)
        {
            throw new DomainRuleException("createdAtUtc cannot be empty.");
        }

        Id = id;
        WordEntryId = wordEntryId;
        TopicId = topicId;
        IsPrimaryTopic = isPrimaryTopic;
        CreatedAtUtc = createdAtUtc.Kind == DateTimeKind.Utc ? createdAtUtc : createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }

    public Guid WordEntryId { get; private set; }

    public Guid TopicId { get; private set; }

    public bool IsPrimaryTopic { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Updates the primary-topic flag.
    /// </summary>
    internal void SetPrimaryTopic(bool isPrimaryTopic)
    {
        IsPrimaryTopic = isPrimaryTopic;
    }

    public void ReassignTopic(Guid topicId)
    {
        if (topicId == Guid.Empty)
        {
            throw new DomainRuleException("Topic identifier cannot be empty for a topic link.");
        }

        TopicId = topicId;
    }
}
