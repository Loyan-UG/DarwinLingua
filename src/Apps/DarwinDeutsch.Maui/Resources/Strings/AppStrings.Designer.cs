#nullable enable
using System.Globalization;
using System.Resources;

namespace DarwinDeutsch.Maui.Resources.Strings;

/// <summary>
/// Provides strongly typed access to localized UI resources.
/// </summary>
public static class AppStrings
{
    private static readonly ResourceManager ResourceManagerInstance =
        new("DarwinDeutsch.Maui.Resources.Strings.AppStrings", typeof(AppStrings).Assembly);

    /// <summary>
    /// Gets or sets the culture used by the resource manager.
    /// </summary>
    public static CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets the localized application title.
    /// </summary>
    public static string AppTitle => GetRequiredString(nameof(AppTitle));

    /// <summary>
    /// Gets the localized title for the home tab.
    /// </summary>
    public static string HomeTabTitle => GetRequiredString(nameof(HomeTabTitle));

    /// <summary>
    /// Gets the localized prompt asking which language the app should use.
    /// </summary>
    public static string WelcomeLanguageQuestion => GetRequiredString(nameof(WelcomeLanguageQuestion));

    /// <summary>
    /// Gets the localized headline for the welcome experience.
    /// </summary>
    public static string WelcomeHeadline => GetRequiredString(nameof(WelcomeHeadline));

    /// <summary>
    /// Gets the localized description for the welcome experience.
    /// </summary>
    public static string WelcomeDescription => GetRequiredString(nameof(WelcomeDescription));

    /// <summary>
    /// Gets the localized heading for currently available features.
    /// </summary>
    public static string WelcomeCurrentFeaturesTitle => GetRequiredString(nameof(WelcomeCurrentFeaturesTitle));

    /// <summary>
    /// Gets the localized body describing currently available features.
    /// </summary>
    public static string WelcomeCurrentFeaturesBody => GetRequiredString(nameof(WelcomeCurrentFeaturesBody));

    /// <summary>
    /// Gets the localized heading for supported learning languages.
    /// </summary>
    public static string WelcomeLearnWithLanguagesTitle => GetRequiredString(nameof(WelcomeLearnWithLanguagesTitle));

    /// <summary>
    /// Gets the localized format for supported learning languages.
    /// </summary>
    public static string WelcomeLearnWithLanguagesBodyFormat => GetRequiredString(nameof(WelcomeLearnWithLanguagesBodyFormat));

    /// <summary>
    /// Gets the localized heading for interface-language availability.
    /// </summary>
    public static string WelcomeInterfaceLanguagesTitle => GetRequiredString(nameof(WelcomeInterfaceLanguagesTitle));

    /// <summary>
    /// Gets the localized format describing current and planned interface languages.
    /// </summary>
    public static string WelcomeInterfaceLanguagesBodyFormat => GetRequiredString(nameof(WelcomeInterfaceLanguagesBodyFormat));

    /// <summary>
    /// Gets the localized fallback shown when no extra UI languages are planned.
    /// </summary>
    public static string WelcomeNoPlannedUiLanguages => GetRequiredString(nameof(WelcomeNoPlannedUiLanguages));

    /// <summary>
    /// Gets the localized heading for upcoming features.
    /// </summary>
    public static string WelcomeFutureFeaturesTitle => GetRequiredString(nameof(WelcomeFutureFeaturesTitle));

    /// <summary>
    /// Gets the localized body describing upcoming features.
    /// </summary>
    public static string WelcomeFutureFeaturesBody => GetRequiredString(nameof(WelcomeFutureFeaturesBody));

    /// <summary>
    /// Gets the localized caption for the welcome-start button.
    /// </summary>
    public static string WelcomeStartButton => GetRequiredString(nameof(WelcomeStartButton));

    /// <summary>
    /// Gets the localized title for the practice tab.
    /// </summary>
    public static string PracticeTabTitle => GetRequiredString(nameof(PracticeTabTitle));

    /// <summary>
    /// Gets the localized title for the settings tab.
    /// </summary>
    public static string SettingsTabTitle => GetRequiredString(nameof(SettingsTabTitle));

    /// <summary>
    /// Gets the localized title for the favorites tab.
    /// </summary>
    public static string FavoritesTabTitle => GetRequiredString(nameof(FavoritesTabTitle));

    /// <summary>
    /// Gets the localized headline for the home page.
    /// </summary>
    public static string HomeHeadline => GetRequiredString(nameof(HomeHeadline));

    /// <summary>
    /// Gets the localized introduction shown on the home page.
    /// </summary>
    public static string HomeIntro => GetRequiredString(nameof(HomeIntro));

    /// <summary>
    /// Gets the localized status badge shown on the home page hero.
    /// </summary>
    public static string HomePageStatusBadge => GetRequiredString(nameof(HomePageStatusBadge));

    /// <summary>
    /// Gets the localized placeholder text for the home logo box.
    /// </summary>
    public static string HomeLogoPlaceholder => GetRequiredString(nameof(HomeLogoPlaceholder));

    /// <summary>
    /// Gets the localized subtitle shown below the app name on the home header.
    /// </summary>
    public static string HomeHeaderSubtitle => GetRequiredString(nameof(HomeHeaderSubtitle));

    /// <summary>
    /// Gets the localized profile section heading on the home page.
    /// </summary>
    public static string HomePageProfileSectionLabel => GetRequiredString(nameof(HomePageProfileSectionLabel));

    /// <summary>
    /// Gets the localized action section heading on the home page.
    /// </summary>
    public static string HomePageExploreSectionLabel => GetRequiredString(nameof(HomePageExploreSectionLabel));

    /// <summary>
    /// Gets the localized label for the current UI language section.
    /// </summary>
    public static string HomeCurrentUiLanguageLabel => GetRequiredString(nameof(HomeCurrentUiLanguageLabel));

    /// <summary>
    /// Gets the localized label for the supported languages section.
    /// </summary>
    public static string HomeSupportedLanguagesLabel => GetRequiredString(nameof(HomeSupportedLanguagesLabel));

    /// <summary>
    /// Gets the localized label for the meaning-language preferences section.
    /// </summary>
    public static string HomeMeaningLanguagesLabel => GetRequiredString(nameof(HomeMeaningLanguagesLabel));

    /// <summary>
    /// Gets the localized placeholder shown when no languages are available.
    /// </summary>
    public static string HomeNoLanguages => GetRequiredString(nameof(HomeNoLanguages));

    /// <summary>
    /// Gets the localized label for the seeded topics section.
    /// </summary>
    public static string HomeTopicsLabel => GetRequiredString(nameof(HomeTopicsLabel));

    /// <summary>
    /// Gets the localized label for the CEFR browse section.
    /// </summary>
    public static string HomeCefrBrowseLabel => GetRequiredString(nameof(HomeCefrBrowseLabel));

    /// <summary>
    /// Gets the localized label for the practice quick action section.
    /// </summary>
    public static string HomePracticeLabel => GetRequiredString(nameof(HomePracticeLabel));

    /// <summary>
    /// Gets the localized caption for the practice navigation button.
    /// </summary>
    public static string HomePracticeButton => GetRequiredString(nameof(HomePracticeButton));

    /// <summary>
    /// Gets the localized label for the search section.
    /// </summary>
    public static string HomeSearchLabel => GetRequiredString(nameof(HomeSearchLabel));

    /// <summary>
    /// Gets the localized caption for the search navigation button.
    /// </summary>
    public static string HomeSearchButton => GetRequiredString(nameof(HomeSearchButton));

    /// <summary>
    /// Gets the localized label for the browse-topics quick action section.
    /// </summary>
    public static string HomeBrowseTopicsLabel => GetRequiredString(nameof(HomeBrowseTopicsLabel));

    /// <summary>
    /// Gets the localized caption for the browse-topics quick action button.
    /// </summary>
    public static string HomeBrowseTopicsButton => GetRequiredString(nameof(HomeBrowseTopicsButton));

    /// <summary>
    /// Gets the localized label for the favorites quick action section.
    /// </summary>
    public static string HomeFavoritesLabel => GetRequiredString(nameof(HomeFavoritesLabel));

    /// <summary>
    /// Gets the localized caption for the favorites quick action button.
    /// </summary>
    public static string HomeFavoritesButton => GetRequiredString(nameof(HomeFavoritesButton));

    /// <summary>
    /// Gets the localized placeholder shown when no topics are available.
    /// </summary>
    public static string HomeNoTopics => GetRequiredString(nameof(HomeNoTopics));

    /// <summary>
    /// Gets the localized title for the browse tab.
    /// </summary>
    public static string BrowseTabTitle => GetRequiredString(nameof(BrowseTabTitle));

    /// <summary>
    /// Gets the localized headline for the topics page.
    /// </summary>
    public static string TopicsPageHeadline => GetRequiredString(nameof(TopicsPageHeadline));

    /// <summary>
    /// Gets the localized description for the topics page.
    /// </summary>
    public static string TopicsPageDescription => GetRequiredString(nameof(TopicsPageDescription));

    /// <summary>
    /// Gets the localized status badge shown on the topics page hero.
    /// </summary>
    public static string TopicsPageStatusBadge => GetRequiredString(nameof(TopicsPageStatusBadge));

    /// <summary>
    /// Gets the localized shortcuts section heading on the topics page.
    /// </summary>
    public static string TopicsPageShortcutsLabel => GetRequiredString(nameof(TopicsPageShortcutsLabel));

    /// <summary>
    /// Gets the localized topic-list section heading on the topics page.
    /// </summary>
    public static string TopicsPageTopicListLabel => GetRequiredString(nameof(TopicsPageTopicListLabel));

    /// <summary>
    /// Gets the localized empty state for the topics page.
    /// </summary>
    public static string TopicsPageEmpty => GetRequiredString(nameof(TopicsPageEmpty));

    /// <summary>
    /// Gets the localized title for the topic-words page.
    /// </summary>
    public static string TopicWordsPageTitle => GetRequiredString(nameof(TopicWordsPageTitle));

    /// <summary>
    /// Gets the localized headline format for the topic-words page.
    /// </summary>
    public static string TopicWordsPageHeadlineFormat => GetRequiredString(nameof(TopicWordsPageHeadlineFormat));

    /// <summary>
    /// Gets the localized description for the topic-words page.
    /// </summary>
    public static string TopicWordsPageDescription => GetRequiredString(nameof(TopicWordsPageDescription));

    /// <summary>
    /// Gets the localized empty state for the topic-words page.
    /// </summary>
    public static string TopicWordsPageEmpty => GetRequiredString(nameof(TopicWordsPageEmpty));

    /// <summary>
    /// Gets the localized placeholder shown when no preferred-language meaning is available.
    /// </summary>
    public static string TopicWordsPageMeaningUnavailable => GetRequiredString(nameof(TopicWordsPageMeaningUnavailable));

    /// <summary>
    /// Gets the localized default title for the word-detail page.
    /// </summary>
    public static string WordDetailTitle => GetRequiredString(nameof(WordDetailTitle));

    /// <summary>
    /// Gets the localized caption for adding the current word to favorites.
    /// </summary>
    public static string WordDetailAddFavoriteButton => GetRequiredString(nameof(WordDetailAddFavoriteButton));

    /// <summary>
    /// Gets the localized caption for speaking the current word aloud.
    /// </summary>
    public static string WordDetailSpeakWordButton => GetRequiredString(nameof(WordDetailSpeakWordButton));

    /// <summary>
    /// Gets the localized caption for speaking a German example sentence aloud.
    /// </summary>
    public static string WordDetailSpeakExampleButton => GetRequiredString(nameof(WordDetailSpeakExampleButton));

    /// <summary>
    /// Gets the localized audio failure message shown when TTS is unavailable on the device.
    /// </summary>
    public static string WordDetailAudioNotSupported => GetRequiredString(nameof(WordDetailAudioNotSupported));

    /// <summary>
    /// Gets the localized audio failure message shown when no compatible German voice is available.
    /// </summary>
    public static string WordDetailAudioLocaleUnavailable => GetRequiredString(nameof(WordDetailAudioLocaleUnavailable));

    /// <summary>
    /// Gets the localized audio failure message shown when playback fails unexpectedly.
    /// </summary>
    public static string WordDetailAudioFailed => GetRequiredString(nameof(WordDetailAudioFailed));

    /// <summary>
    /// Gets the localized caption for removing the current word from favorites.
    /// </summary>
    public static string WordDetailRemoveFavoriteButton => GetRequiredString(nameof(WordDetailRemoveFavoriteButton));

    /// <summary>
    /// Gets the localized label for the lightweight learning-state section.
    /// </summary>
    public static string WordDetailLearningStateLabel => GetRequiredString(nameof(WordDetailLearningStateLabel));

    /// <summary>
    /// Gets the localized placeholder shown when no lightweight learning state is available.
    /// </summary>
    public static string WordDetailLearningStateUnknown => GetRequiredString(nameof(WordDetailLearningStateUnknown));

    /// <summary>
    /// Gets the localized caption for marking the current word as known.
    /// </summary>
    public static string WordDetailMarkKnownButton => GetRequiredString(nameof(WordDetailMarkKnownButton));

    /// <summary>
    /// Gets the localized caption for clearing the known marker from the current word.
    /// </summary>
    public static string WordDetailClearKnownButton => GetRequiredString(nameof(WordDetailClearKnownButton));

    /// <summary>
    /// Gets the localized caption for marking the current word as difficult.
    /// </summary>
    public static string WordDetailMarkDifficultButton => GetRequiredString(nameof(WordDetailMarkDifficultButton));

    /// <summary>
    /// Gets the localized caption for clearing the difficult marker from the current word.
    /// </summary>
    public static string WordDetailClearDifficultButton => GetRequiredString(nameof(WordDetailClearDifficultButton));

    /// <summary>
    /// Gets the localized status text shown when a word is marked as known.
    /// </summary>
    public static string WordDetailStateKnown => GetRequiredString(nameof(WordDetailStateKnown));

    /// <summary>
    /// Gets the localized status text shown when a word is marked as difficult.
    /// </summary>
    public static string WordDetailStateDifficult => GetRequiredString(nameof(WordDetailStateDifficult));

    /// <summary>
    /// Gets the localized format string for the tracked word-detail view count.
    /// </summary>
    public static string WordDetailViewCountFormat => GetRequiredString(nameof(WordDetailViewCountFormat));

    /// <summary>
    /// Gets the localized label for the topic section on the word-detail page.
    /// </summary>
    public static string WordDetailTopicsLabel => GetRequiredString(nameof(WordDetailTopicsLabel));

    /// <summary>
    /// Gets the localized label for the usage-label section on the word-detail page.
    /// </summary>
    public static string WordDetailUsageLabelsLabel => GetRequiredString(nameof(WordDetailUsageLabelsLabel));

    /// <summary>
    /// Gets the localized label for the context-label section on the word-detail page.
    /// </summary>
    public static string WordDetailContextLabelsLabel => GetRequiredString(nameof(WordDetailContextLabelsLabel));

    /// <summary>
    /// Gets the localized label for the grammar-notes section on the word-detail page.
    /// </summary>
    public static string WordDetailGrammarNotesLabel => GetRequiredString(nameof(WordDetailGrammarNotesLabel));

    /// <summary>
    /// Gets the localized label for the collocations section on the word-detail page.
    /// </summary>
    public static string WordDetailCollocationsLabel => GetRequiredString(nameof(WordDetailCollocationsLabel));

    /// <summary>
    /// Gets the localized label for the word-family section on the word-detail page.
    /// </summary>
    public static string WordDetailWordFamiliesLabel => GetRequiredString(nameof(WordDetailWordFamiliesLabel));

    /// <summary>
    /// Gets the localized label for the lexical-relations section on the word-detail page.
    /// </summary>
    public static string WordDetailLexicalRelationsLabel => GetRequiredString(nameof(WordDetailLexicalRelationsLabel));

    /// <summary>
    /// Gets the localized label for the synonym group on the word-detail page.
    /// </summary>
    public static string WordDetailSynonymsLabel => GetRequiredString(nameof(WordDetailSynonymsLabel));

    /// <summary>
    /// Gets the localized label for the antonym group on the word-detail page.
    /// </summary>
    public static string WordDetailAntonymsLabel => GetRequiredString(nameof(WordDetailAntonymsLabel));

    /// <summary>
    /// Gets the localized empty-topic message for the word-detail page.
    /// </summary>
    public static string WordDetailNoTopics => GetRequiredString(nameof(WordDetailNoTopics));

    /// <summary>
    /// Gets the localized format string for lexical metadata lines.
    /// </summary>
    public static string LexiconMetadataFormat => GetRequiredString(nameof(LexiconMetadataFormat));

    /// <summary>
    /// Gets the localized display label for noun parts of speech.
    /// </summary>
    public static string PartOfSpeechNoun => GetRequiredString(nameof(PartOfSpeechNoun));

    /// <summary>
    /// Gets the localized display label for verb parts of speech.
    /// </summary>
    public static string PartOfSpeechVerb => GetRequiredString(nameof(PartOfSpeechVerb));

    /// <summary>
    /// Gets the localized display label for adjective parts of speech.
    /// </summary>
    public static string PartOfSpeechAdjective => GetRequiredString(nameof(PartOfSpeechAdjective));

    /// <summary>
    /// Gets the localized display label for adverb parts of speech.
    /// </summary>
    public static string PartOfSpeechAdverb => GetRequiredString(nameof(PartOfSpeechAdverb));

    /// <summary>
    /// Gets the localized display label for pronoun parts of speech.
    /// </summary>
    public static string PartOfSpeechPronoun => GetRequiredString(nameof(PartOfSpeechPronoun));

    /// <summary>
    /// Gets the localized display label for preposition parts of speech.
    /// </summary>
    public static string PartOfSpeechPreposition => GetRequiredString(nameof(PartOfSpeechPreposition));

    /// <summary>
    /// Gets the localized display label for conjunction parts of speech.
    /// </summary>
    public static string PartOfSpeechConjunction => GetRequiredString(nameof(PartOfSpeechConjunction));

    /// <summary>
    /// Gets the localized display label for interjection parts of speech.
    /// </summary>
    public static string PartOfSpeechInterjection => GetRequiredString(nameof(PartOfSpeechInterjection));

    /// <summary>
    /// Gets the localized display label for numeral parts of speech.
    /// </summary>
    public static string PartOfSpeechNumeral => GetRequiredString(nameof(PartOfSpeechNumeral));

    /// <summary>
    /// Gets the localized display label for phrase parts of speech.
    /// </summary>
    public static string PartOfSpeechPhrase => GetRequiredString(nameof(PartOfSpeechPhrase));

    /// <summary>
    /// Gets the localized display label for expression parts of speech.
    /// </summary>
    public static string PartOfSpeechExpression => GetRequiredString(nameof(PartOfSpeechExpression));

    /// <summary>
    /// Gets the localized display label for uncategorized parts of speech.
    /// </summary>
    public static string PartOfSpeechOther => GetRequiredString(nameof(PartOfSpeechOther));

    /// <summary>
    /// Gets the localized display label for formal lexical usage.
    /// </summary>
    public static string WordLabelFormal => GetRequiredString(nameof(WordLabelFormal));

    /// <summary>
    /// Gets the localized display label for informal lexical usage.
    /// </summary>
    public static string WordLabelInformal => GetRequiredString(nameof(WordLabelInformal));

    /// <summary>
    /// Gets the localized display label for spoken lexical usage.
    /// </summary>
    public static string WordLabelSpoken => GetRequiredString(nameof(WordLabelSpoken));

    /// <summary>
    /// Gets the localized display label for written lexical usage.
    /// </summary>
    public static string WordLabelWritten => GetRequiredString(nameof(WordLabelWritten));

    /// <summary>
    /// Gets the localized display label for daily-life context.
    /// </summary>
    public static string WordLabelDailyLife => GetRequiredString(nameof(WordLabelDailyLife));

    /// <summary>
    /// Gets the localized display label for shopping context.
    /// </summary>
    public static string WordLabelShopping => GetRequiredString(nameof(WordLabelShopping));

    /// <summary>
    /// Gets the localized display label for work context.
    /// </summary>
    public static string WordLabelWork => GetRequiredString(nameof(WordLabelWork));

    /// <summary>
    /// Gets the localized display label for paperwork context.
    /// </summary>
    public static string WordLabelPaperwork => GetRequiredString(nameof(WordLabelPaperwork));

    /// <summary>
    /// Gets the localized display label for doctor/medical context.
    /// </summary>
    public static string WordLabelDoctor => GetRequiredString(nameof(WordLabelDoctor));

    /// <summary>
    /// Gets the localized not-found state for the word-detail page.
    /// </summary>
    public static string WordDetailNotFound => GetRequiredString(nameof(WordDetailNotFound));

    /// <summary>
    /// Gets the localized title for the CEFR browse page.
    /// </summary>
    public static string CefrWordsPageTitle => GetRequiredString(nameof(CefrWordsPageTitle));

    /// <summary>
    /// Gets the localized headline format for the CEFR browse page.
    /// </summary>
    public static string CefrWordsPageHeadlineFormat => GetRequiredString(nameof(CefrWordsPageHeadlineFormat));

    /// <summary>
    /// Gets the localized description for the CEFR browse page.
    /// </summary>
    public static string CefrWordsPageDescription => GetRequiredString(nameof(CefrWordsPageDescription));

    /// <summary>
    /// Gets the localized empty state for the CEFR browse page.
    /// </summary>
    public static string CefrWordsPageEmpty => GetRequiredString(nameof(CefrWordsPageEmpty));

    /// <summary>
    /// Gets the localized title for the search page.
    /// </summary>
    public static string SearchWordsPageTitle => GetRequiredString(nameof(SearchWordsPageTitle));

    /// <summary>
    /// Gets the localized headline for the search page.
    /// </summary>
    public static string SearchWordsPageHeadline => GetRequiredString(nameof(SearchWordsPageHeadline));

    /// <summary>
    /// Gets the localized description for the search page.
    /// </summary>
    public static string SearchWordsPageDescription => GetRequiredString(nameof(SearchWordsPageDescription));

    /// <summary>
    /// Gets the localized status badge shown on the search page hero.
    /// </summary>
    public static string SearchWordsPageStatusBadge => GetRequiredString(nameof(SearchWordsPageStatusBadge));

    /// <summary>
    /// Gets the localized heading for the search input section.
    /// </summary>
    public static string SearchWordsPageSearchSectionLabel => GetRequiredString(nameof(SearchWordsPageSearchSectionLabel));

    /// <summary>
    /// Gets the localized helper text shown below the search bar.
    /// </summary>
    public static string SearchWordsPageSearchHint => GetRequiredString(nameof(SearchWordsPageSearchHint));

    /// <summary>
    /// Gets the localized heading for the search results section.
    /// </summary>
    public static string SearchWordsPageResultsLabel => GetRequiredString(nameof(SearchWordsPageResultsLabel));

    /// <summary>
    /// Gets the localized placeholder for the search bar.
    /// </summary>
    public static string SearchWordsPagePlaceholder => GetRequiredString(nameof(SearchWordsPagePlaceholder));

    /// <summary>
    /// Gets the localized empty state for the search page.
    /// </summary>
    public static string SearchWordsPageEmpty => GetRequiredString(nameof(SearchWordsPageEmpty));

    /// <summary>
    /// Gets the localized title for the favorites page.
    /// </summary>
    public static string FavoritesPageTitle => GetRequiredString(nameof(FavoritesPageTitle));

    /// <summary>
    /// Gets the localized headline for the favorites page.
    /// </summary>
    public static string FavoritesPageHeadline => GetRequiredString(nameof(FavoritesPageHeadline));

    /// <summary>
    /// Gets the localized description for the favorites page.
    /// </summary>
    public static string FavoritesPageDescription => GetRequiredString(nameof(FavoritesPageDescription));

    /// <summary>
    /// Gets the localized empty state for the favorites page.
    /// </summary>
    public static string FavoritesPageEmpty => GetRequiredString(nameof(FavoritesPageEmpty));

    /// <summary>
    /// Gets the localized headline for the practice page.
    /// </summary>
    public static string PracticePageHeadline => GetRequiredString(nameof(PracticePageHeadline));

    /// <summary>
    /// Gets the localized description for the practice page.
    /// </summary>
    public static string PracticePageDescription => GetRequiredString(nameof(PracticePageDescription));

    /// <summary>
    /// Gets the localized status badge shown on the practice page hero.
    /// </summary>
    public static string PracticePageStatusBadge => GetRequiredString(nameof(PracticePageStatusBadge));

    /// <summary>
    /// Gets the localized heading for the practice progress section.
    /// </summary>
    public static string PracticePageProgressSectionLabel => GetRequiredString(nameof(PracticePageProgressSectionLabel));

    /// <summary>
    /// Gets the localized heading for the practice actions section.
    /// </summary>
    public static string PracticePageActionsSectionLabel => GetRequiredString(nameof(PracticePageActionsSectionLabel));

    /// <summary>
    /// Gets the localized label for the due-now metric.
    /// </summary>
    public static string PracticePageDueNowLabel => GetRequiredString(nameof(PracticePageDueNowLabel));

    /// <summary>
    /// Gets the localized value format for the due-now metric.
    /// </summary>
    public static string PracticePageDueNowValueFormat => GetRequiredString(nameof(PracticePageDueNowValueFormat));

    /// <summary>
    /// Gets the localized label for the success-rate metric.
    /// </summary>
    public static string PracticePageSuccessRateLabel => GetRequiredString(nameof(PracticePageSuccessRateLabel));

    /// <summary>
    /// Gets the localized value format for the success-rate metric.
    /// </summary>
    public static string PracticePageSuccessRateValueFormat => GetRequiredString(nameof(PracticePageSuccessRateValueFormat));

    /// <summary>
    /// Gets the localized label for the mastered metric.
    /// </summary>
    public static string PracticePageMasteredLabel => GetRequiredString(nameof(PracticePageMasteredLabel));

    /// <summary>
    /// Gets the localized value format for the mastered metric.
    /// </summary>
    public static string PracticePageMasteredValueFormat => GetRequiredString(nameof(PracticePageMasteredValueFormat));

    /// <summary>
    /// Gets the localized label for the struggling metric.
    /// </summary>
    public static string PracticePageStrugglingLabel => GetRequiredString(nameof(PracticePageStrugglingLabel));

    /// <summary>
    /// Gets the localized value format for the struggling metric.
    /// </summary>
    public static string PracticePageStrugglingValueFormat => GetRequiredString(nameof(PracticePageStrugglingValueFormat));

    /// <summary>
    /// Gets the localized label for the flashcard-start action block.
    /// </summary>
    public static string PracticePageStartFlashcardsLabel => GetRequiredString(nameof(PracticePageStartFlashcardsLabel));

    /// <summary>
    /// Gets the localized caption for the flashcard-start action button.
    /// </summary>
    public static string PracticePageStartFlashcardsButton => GetRequiredString(nameof(PracticePageStartFlashcardsButton));

    /// <summary>
    /// Gets the localized label for the quiz-start action block.
    /// </summary>
    public static string PracticePageStartQuizLabel => GetRequiredString(nameof(PracticePageStartQuizLabel));

    /// <summary>
    /// Gets the localized caption for the quiz-start action button.
    /// </summary>
    public static string PracticePageStartQuizButton => GetRequiredString(nameof(PracticePageStartQuizButton));

    /// <summary>
    /// Gets the localized label for the refresh action block.
    /// </summary>
    public static string PracticePageRefreshLabel => GetRequiredString(nameof(PracticePageRefreshLabel));

    /// <summary>
    /// Gets the localized caption for the refresh action button.
    /// </summary>
    public static string PracticePageRefreshButton => GetRequiredString(nameof(PracticePageRefreshButton));

    /// <summary>
    /// Gets the localized heading for the review-session preview section.
    /// </summary>
    public static string PracticePageReviewSessionHeading => GetRequiredString(nameof(PracticePageReviewSessionHeading));

    /// <summary>
    /// Gets the localized empty state for the review-session preview section.
    /// </summary>
    public static string PracticePageReviewSessionEmpty => GetRequiredString(nameof(PracticePageReviewSessionEmpty));

    /// <summary>
    /// Gets the localized summary format for the review-session preview section.
    /// </summary>
    public static string PracticePageReviewSessionSummaryFormat => GetRequiredString(nameof(PracticePageReviewSessionSummaryFormat));

    /// <summary>
    /// Gets the localized heading for the recent-activity section.
    /// </summary>
    public static string PracticePageRecentActivityHeading => GetRequiredString(nameof(PracticePageRecentActivityHeading));

    /// <summary>
    /// Gets the localized empty state for the recent-activity section.
    /// </summary>
    public static string PracticePageRecentActivityEmpty => GetRequiredString(nameof(PracticePageRecentActivityEmpty));

    /// <summary>
    /// Gets the localized summary format for the recent-activity section.
    /// </summary>
    public static string PracticePageRecentActivitySummaryFormat => GetRequiredString(nameof(PracticePageRecentActivitySummaryFormat));

    /// <summary>
    /// Gets the localized due badge for review items.
    /// </summary>
    public static string PracticePageDueBadge => GetRequiredString(nameof(PracticePageDueBadge));

    /// <summary>
    /// Gets the localized queued badge for review items.
    /// </summary>
    public static string PracticePageQueuedBadge => GetRequiredString(nameof(PracticePageQueuedBadge));

    /// <summary>
    /// Gets the localized difficult badge for review items.
    /// </summary>
    public static string PracticePageDifficultBadge => GetRequiredString(nameof(PracticePageDifficultBadge));

    /// <summary>
    /// Gets the localized known badge for review items.
    /// </summary>
    public static string PracticePageKnownBadge => GetRequiredString(nameof(PracticePageKnownBadge));

    /// <summary>
    /// Gets the localized learning badge for review items.
    /// </summary>
    public static string PracticePageLearningBadge => GetRequiredString(nameof(PracticePageLearningBadge));

    /// <summary>
    /// Gets the localized flashcard session label for recent activity.
    /// </summary>
    public static string PracticePageSessionFlashcard => GetRequiredString(nameof(PracticePageSessionFlashcard));

    /// <summary>
    /// Gets the localized quiz session label for recent activity.
    /// </summary>
    public static string PracticePageSessionQuiz => GetRequiredString(nameof(PracticePageSessionQuiz));

    /// <summary>
    /// Gets the localized incorrect-outcome label for recent activity.
    /// </summary>
    public static string PracticePageOutcomeIncorrect => GetRequiredString(nameof(PracticePageOutcomeIncorrect));

    /// <summary>
    /// Gets the localized hard-outcome label for recent activity.
    /// </summary>
    public static string PracticePageOutcomeHard => GetRequiredString(nameof(PracticePageOutcomeHard));

    /// <summary>
    /// Gets the localized correct-outcome label for recent activity.
    /// </summary>
    public static string PracticePageOutcomeCorrect => GetRequiredString(nameof(PracticePageOutcomeCorrect));

    /// <summary>
    /// Gets the localized easy-outcome label for recent activity.
    /// </summary>
    public static string PracticePageOutcomeEasy => GetRequiredString(nameof(PracticePageOutcomeEasy));

    /// <summary>
    /// Gets the localized title for the flashcard session page.
    /// </summary>
    public static string PracticeFlashcardPageTitle => GetRequiredString(nameof(PracticeFlashcardPageTitle));

    /// <summary>
    /// Gets the localized headline for the flashcard session page.
    /// </summary>
    public static string PracticeFlashcardPageHeadline => GetRequiredString(nameof(PracticeFlashcardPageHeadline));

    /// <summary>
    /// Gets the localized description for the flashcard session page.
    /// </summary>
    public static string PracticeFlashcardPageDescription => GetRequiredString(nameof(PracticeFlashcardPageDescription));

    /// <summary>
    /// Gets the localized title for the quiz session page.
    /// </summary>
    public static string PracticeQuizPageTitle => GetRequiredString(nameof(PracticeQuizPageTitle));

    /// <summary>
    /// Gets the localized headline for the quiz session page.
    /// </summary>
    public static string PracticeQuizPageHeadline => GetRequiredString(nameof(PracticeQuizPageHeadline));

    /// <summary>
    /// Gets the localized description for the quiz session page.
    /// </summary>
    public static string PracticeQuizPageDescription => GetRequiredString(nameof(PracticeQuizPageDescription));

    /// <summary>
    /// Gets the localized caption for revealing the current answer.
    /// </summary>
    public static string PracticeSessionRevealButton => GetRequiredString(nameof(PracticeSessionRevealButton));

    /// <summary>
    /// Gets the localized caption for advancing to the next practice item.
    /// </summary>
    public static string PracticeSessionNextButton => GetRequiredString(nameof(PracticeSessionNextButton));

    /// <summary>
    /// Gets the localized caption for finishing the current practice session.
    /// </summary>
    public static string PracticeSessionFinishButton => GetRequiredString(nameof(PracticeSessionFinishButton));

    /// <summary>
    /// Gets the localized caption for returning to the practice overview.
    /// </summary>
    public static string PracticeSessionReturnButton => GetRequiredString(nameof(PracticeSessionReturnButton));

    /// <summary>
    /// Gets the localized heading for per-answer feedback.
    /// </summary>
    public static string PracticeSessionFeedbackHeadline => GetRequiredString(nameof(PracticeSessionFeedbackHeadline));

    /// <summary>
    /// Gets the localized heading for the session summary.
    /// </summary>
    public static string PracticeSessionSummaryHeadline => GetRequiredString(nameof(PracticeSessionSummaryHeadline));

    /// <summary>
    /// Gets the localized state label shown when the session is complete.
    /// </summary>
    public static string PracticeSessionSummaryState => GetRequiredString(nameof(PracticeSessionSummaryState));

    /// <summary>
    /// Gets the localized empty state for practice sessions with no available items.
    /// </summary>
    public static string PracticeSessionEmpty => GetRequiredString(nameof(PracticeSessionEmpty));

    /// <summary>
    /// Gets the localized progress format for practice sessions.
    /// </summary>
    public static string PracticeSessionProgressFormat => GetRequiredString(nameof(PracticeSessionProgressFormat));

    /// <summary>
    /// Gets the localized metadata format for one session item.
    /// </summary>
    public static string PracticeSessionMetadataFormat => GetRequiredString(nameof(PracticeSessionMetadataFormat));

    /// <summary>
    /// Gets the localized due-at format shown after answers.
    /// </summary>
    public static string PracticeSessionDueAtFormat => GetRequiredString(nameof(PracticeSessionDueAtFormat));

    /// <summary>
    /// Gets the localized feedback-body format shown after answers.
    /// </summary>
    public static string PracticeSessionFeedbackBodyFormat => GetRequiredString(nameof(PracticeSessionFeedbackBodyFormat));

    /// <summary>
    /// Gets the localized summary-body format shown at the end of a session.
    /// </summary>
    public static string PracticeSessionSummaryBodyFormat => GetRequiredString(nameof(PracticeSessionSummaryBodyFormat));

    /// <summary>
    /// Gets the localized hint shown before the answer is revealed.
    /// </summary>
    public static string PracticeSessionAnswerHiddenHint => GetRequiredString(nameof(PracticeSessionAnswerHiddenHint));

    /// <summary>
    /// Gets the localized hint shown after the answer is revealed.
    /// </summary>
    public static string PracticeSessionAnswerRevealedHint => GetRequiredString(nameof(PracticeSessionAnswerRevealedHint));

    /// <summary>
    /// Gets the localized label for the flashcard prompt.
    /// </summary>
    public static string PracticeFlashcardPromptLabel => GetRequiredString(nameof(PracticeFlashcardPromptLabel));

    /// <summary>
    /// Gets the localized hint for the flashcard flow.
    /// </summary>
    public static string PracticeFlashcardPromptHint => GetRequiredString(nameof(PracticeFlashcardPromptHint));

    /// <summary>
    /// Gets the localized label for the flashcard revealed answer.
    /// </summary>
    public static string PracticeFlashcardAnswerLabel => GetRequiredString(nameof(PracticeFlashcardAnswerLabel));

    /// <summary>
    /// Gets the localized completion state for the flashcard flow.
    /// </summary>
    public static string PracticeFlashcardSessionComplete => GetRequiredString(nameof(PracticeFlashcardSessionComplete));

    /// <summary>
    /// Gets the localized label for the quiz prompt.
    /// </summary>
    public static string PracticeQuizPromptLabel => GetRequiredString(nameof(PracticeQuizPromptLabel));

    /// <summary>
    /// Gets the localized hint for the quiz flow.
    /// </summary>
    public static string PracticeQuizPromptHint => GetRequiredString(nameof(PracticeQuizPromptHint));

    /// <summary>
    /// Gets the localized label for the quiz revealed answer.
    /// </summary>
    public static string PracticeQuizAnswerLabel => GetRequiredString(nameof(PracticeQuizAnswerLabel));

    /// <summary>
    /// Gets the localized completion state for the quiz flow.
    /// </summary>
    public static string PracticeQuizSessionComplete => GetRequiredString(nameof(PracticeQuizSessionComplete));

    /// <summary>
    /// Gets the localized headline for the settings page.
    /// </summary>
    public static string SettingsHeadline => GetRequiredString(nameof(SettingsHeadline));

    /// <summary>
    /// Gets the localized description shown on the settings page.
    /// </summary>
    public static string SettingsDescription => GetRequiredString(nameof(SettingsDescription));

    /// <summary>
    /// Gets the localized heading for the app-information section on the settings page.
    /// </summary>
    public static string SettingsAppInfoSectionLabel => GetRequiredString(nameof(SettingsAppInfoSectionLabel));

    /// <summary>
    /// Gets the localized picker title for the UI language selector.
    /// </summary>
    public static string SettingsUiLanguageLabel => GetRequiredString(nameof(SettingsUiLanguageLabel));

    /// <summary>
    /// Gets the localized picker title for the primary meaning language selector.
    /// </summary>
    public static string SettingsPrimaryMeaningLanguageLabel => GetRequiredString(nameof(SettingsPrimaryMeaningLanguageLabel));

    /// <summary>
    /// Gets the localized picker title for the secondary meaning language selector.
    /// </summary>
    public static string SettingsSecondaryMeaningLanguageLabel => GetRequiredString(nameof(SettingsSecondaryMeaningLanguageLabel));

    /// <summary>
    /// Gets the localized display text for the empty secondary meaning-language option.
    /// </summary>
    public static string SettingsSecondaryMeaningLanguageNone => GetRequiredString(nameof(SettingsSecondaryMeaningLanguageNone));

    /// <summary>
    /// Gets the localized heading for the packaged content-updates section.
    /// </summary>
    public static string SettingsContentUpdatesSectionLabel => GetRequiredString(nameof(SettingsContentUpdatesSectionLabel));

    /// <summary>
    /// Gets the localized label for the packaged seed-content status line.
    /// </summary>
    public static string SettingsContentUpdatesStatusLabel => GetRequiredString(nameof(SettingsContentUpdatesStatusLabel));

    /// <summary>
    /// Gets the localized label for the packaged seed-content detail line.
    /// </summary>
    public static string SettingsContentUpdatesDetailsLabel => GetRequiredString(nameof(SettingsContentUpdatesDetailsLabel));

    /// <summary>
    /// Gets the localized caption for applying a packaged content update.
    /// </summary>
    public static string SettingsContentUpdatesApplyButton => GetRequiredString(nameof(SettingsContentUpdatesApplyButton));

    /// <summary>
    /// Gets the localized caption shown when packaged content is already current.
    /// </summary>
    public static string SettingsContentUpdatesAppliedButton => GetRequiredString(nameof(SettingsContentUpdatesAppliedButton));

    /// <summary>
    /// Gets the localized caption shown while a packaged content update is running.
    /// </summary>
    public static string SettingsContentUpdatesApplyingButton => GetRequiredString(nameof(SettingsContentUpdatesApplyingButton));

    /// <summary>
    /// Gets the localized status shown when a packaged content update is available.
    /// </summary>
    public static string SettingsContentUpdatesAvailableStatus => GetRequiredString(nameof(SettingsContentUpdatesAvailableStatus));

    /// <summary>
    /// Gets the localized detail format shown when a packaged content update is available.
    /// </summary>
    public static string SettingsContentUpdatesAvailableDetailsFormat => GetRequiredString(nameof(SettingsContentUpdatesAvailableDetailsFormat));

    /// <summary>
    /// Gets the localized status shown when packaged content is already current.
    /// </summary>
    public static string SettingsContentUpdatesCurrentStatus => GetRequiredString(nameof(SettingsContentUpdatesCurrentStatus));

    /// <summary>
    /// Gets the localized detail shown when packaged content is already current.
    /// </summary>
    public static string SettingsContentUpdatesCurrentDetails => GetRequiredString(nameof(SettingsContentUpdatesCurrentDetails));

    /// <summary>
    /// Gets the localized status shown when no packaged seed database is available.
    /// </summary>
    public static string SettingsContentUpdatesUnavailableStatus => GetRequiredString(nameof(SettingsContentUpdatesUnavailableStatus));

    /// <summary>
    /// Gets the localized detail shown when no packaged seed database is available.
    /// </summary>
    public static string SettingsContentUpdatesUnavailableDetails => GetRequiredString(nameof(SettingsContentUpdatesUnavailableDetails));

    /// <summary>
    /// Gets the localized dialog title shown after a successful packaged content update.
    /// </summary>
    public static string SettingsContentUpdatesCompletedTitle => GetRequiredString(nameof(SettingsContentUpdatesCompletedTitle));

    /// <summary>
    /// Gets the localized dialog body format shown after a successful packaged content update.
    /// </summary>
    public static string SettingsContentUpdatesCompletedMessageFormat => GetRequiredString(nameof(SettingsContentUpdatesCompletedMessageFormat));

    /// <summary>
    /// Gets the localized dialog title shown when no packaged content update is needed.
    /// </summary>
    public static string SettingsContentUpdatesUpToDateTitle => GetRequiredString(nameof(SettingsContentUpdatesUpToDateTitle));

    /// <summary>
    /// Gets the localized dialog body shown when no packaged content update is needed.
    /// </summary>
    public static string SettingsContentUpdatesUpToDateMessage => GetRequiredString(nameof(SettingsContentUpdatesUpToDateMessage));

    /// <summary>
    /// Gets the localized dialog title shown when a packaged content update fails.
    /// </summary>
    public static string SettingsContentUpdatesFailedTitle => GetRequiredString(nameof(SettingsContentUpdatesFailedTitle));

    /// <summary>
    /// Gets the localized dialog body format shown when a packaged content update fails.
    /// </summary>
    public static string SettingsContentUpdatesFailedMessageFormat => GetRequiredString(nameof(SettingsContentUpdatesFailedMessageFormat));

    /// <summary>
    /// Gets the localized dismiss-button caption for packaged content update dialogs.
    /// </summary>
    public static string SettingsContentUpdatesDismissButton => GetRequiredString(nameof(SettingsContentUpdatesDismissButton));

    /// <summary>
    /// Gets the localized display label for English.
    /// </summary>
    public static string LanguageOptionEnglish => GetRequiredString(nameof(LanguageOptionEnglish));

    /// <summary>
    /// Gets the localized display label for German.
    /// </summary>
    public static string LanguageOptionGerman => GetRequiredString(nameof(LanguageOptionGerman));

    /// <summary>
    /// Gets the localized loading-state message used by data-driven pages.
    /// </summary>
    public static string CommonStateLoading => GetRequiredString(nameof(CommonStateLoading));

    /// <summary>
    /// Gets the localized generic error-state message used by data-driven pages.
    /// </summary>
    public static string CommonStateError => GetRequiredString(nameof(CommonStateError));

    /// <summary>
    /// Gets a required localized string by key.
    /// </summary>
    /// <param name="name">The resource key name.</param>
    /// <returns>The localized string value.</returns>
    private static string GetRequiredString(string name)
    {
        string? value = ResourceManagerInstance.GetString(name, Culture);

        return value ?? throw new MissingManifestResourceException($"Missing resource value for '{name}'.");
    }
}
#nullable restore
