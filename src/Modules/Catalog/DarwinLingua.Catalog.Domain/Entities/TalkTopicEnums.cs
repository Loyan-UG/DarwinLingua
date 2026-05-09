namespace DarwinLingua.Catalog.Domain.Entities;

public enum TalkTopicContentType
{
    Article = 1,
    BookSummary = 2,
    MovieSummary = 3,
    Story = 4,
    FactSheet = 5,
    OpinionText = 6,
    Interview = 7,
    DebateText = 8,
}

public enum TalkTopicQuestionKind
{
    Warmup = 1,
    Discussion = 2,
}

public enum TalkTopicQuestionType
{
    Opinion = 1,
    PersonalExperience = 2,
    Prediction = 3,
    Comparison = 4,
    Imagination = 5,
    Debate = 6,
    Ethics = 7,
    Comprehension = 8,
}

public enum TalkTopicSpeakingGoal
{
    ExpressOpinion = 1,
    GiveReasons = 2,
    AgreeDisagree = 3,
    AskFollowUpQuestions = 4,
    CompareOptions = 5,
    MakePredictions = 6,
    DescribeExperiences = 7,
    ImaginePossibilities = 8,
    DebatePolitely = 9,
    SummarizePosition = 10,
}
