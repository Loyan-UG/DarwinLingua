param(
    [ValidateSet("Audit", "Write")]
    [string]$Mode = "Audit",

    [string]$SyllabusPath = "content/learning-portal/grammar/syllabus/grammar-syllabus-a1-c2-v1.json",

    [string]$PromptDirectory = "D:\_Projects\DarwinLingua.Content\Grammar\Prompts",

    [string]$ReportPath = ""
)

$ErrorActionPreference = "Stop"

$Languages = @("en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq")
$CefrOrder = @("A1", "A2", "B1", "B2", "C1", "C2")
$PackageByCefr = @{
    A1 = "content/learning-portal/grammar/packages/grammar-a1-core-v1.json"
    A2 = "content/learning-portal/grammar/packages/grammar-a2-core-v1.json"
    B1 = "content/learning-portal/grammar/packages/grammar-b1-core-v1.json"
    B2 = "content/learning-portal/grammar/packages/grammar-b2-core-v1.json"
    C1 = "content/learning-portal/grammar/packages/grammar-c1-core-v1.json"
    C2 = "content/learning-portal/grammar/packages/grammar-c2-core-v1.json"
}

# Only map prompts whose old slug/title clearly points at the same syllabus topic.
$KnownSlugAliases = @{
    "b1-connectors-for-cause-effect" = "b1-connectors-for-cause-and-effect"
    "b2-relative-clauses-with-was-wo" = "b2-relative-clauses-with-was-and-wo"
    "c1-presentation-grammar" = "c1-c1-presentation-grammar"
    "c1-discussion-grammar" = "c1-c1-discussion-grammar"
    "c2-c2-common-mistakes" = "c2-c2-common-pitfalls"
    "c2-final-german-grammar-mastery-map" = "c2-c2-grammar-review-map"
}

function Resolve-RepoPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return (Join-Path (Get-Location) $Path)
}

function Normalize-Key([string]$Value) {
    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    $normalized = $Value.ToLowerInvariant()
    $normalized = $normalized -replace "ä", "ae"
    $normalized = $normalized -replace "ö", "oe"
    $normalized = $normalized -replace "ü", "ue"
    $normalized = $normalized -replace "ß", "ss"
    $normalized = $normalized -replace "\+", " plus "
    $normalized = $normalized -replace "\.\.\.", " "
    $normalized = $normalized -replace "[^a-z0-9]+", "-"
    $tokens = $normalized.Split("-", [StringSplitOptions]::RemoveEmptyEntries)
    $stopWords = @("and", "the")
    return (($tokens | Where-Object { $stopWords -notcontains $_ }) -join "-")
}

function Get-LevenshteinDistance([string]$A, [string]$B) {
    if ($A -eq $B) { return 0 }
    if ([string]::IsNullOrEmpty($A)) { return $B.Length }
    if ([string]::IsNullOrEmpty($B)) { return $A.Length }

    $matrix = New-Object "int[,]" ($A.Length + 1), ($B.Length + 1)
    for ($i = 0; $i -le $A.Length; $i++) { $matrix[$i, 0] = $i }
    for ($j = 0; $j -le $B.Length; $j++) { $matrix[0, $j] = $j }

    for ($i = 1; $i -le $A.Length; $i++) {
        for ($j = 1; $j -le $B.Length; $j++) {
            $cost = if ($A[$i - 1] -eq $B[$j - 1]) { 0 } else { 1 }
            $delete = $matrix[($i - 1), $j] + 1
            $insert = $matrix[$i, ($j - 1)] + 1
            $substitute = $matrix[($i - 1), ($j - 1)] + $cost
            $matrix[$i, $j] = [Math]::Min([Math]::Min($delete, $insert), $substitute)
        }
    }

    return $matrix[$A.Length, $B.Length]
}

function Get-Similarity([string]$A, [string]$B) {
    $aKey = Normalize-Key $A
    $bKey = Normalize-Key $B
    if ($aKey.Length -eq 0 -and $bKey.Length -eq 0) { return 1.0 }
    if ($aKey.Length -eq 0 -or $bKey.Length -eq 0) { return 0.0 }
    $distance = Get-LevenshteinDistance $aKey $bKey
    $max = [Math]::Max($aKey.Length, $bKey.Length)
    return 1.0 - ($distance / $max)
}

function Get-MatchValue([string]$Text, [string]$Pattern) {
    $match = [regex]::Match($Text, $Pattern, [Text.RegularExpressions.RegexOptions]::Multiline)
    if ($match.Success) {
        return $match.Groups[1].Value.Trim()
    }

    return $null
}

function Get-PromptNumber([string]$FileName) {
    $match = [regex]::Match($FileName, "\((\d+)\)")
    if (-not $match.Success) {
        return 0
    }

    return [int]$match.Groups[1].Value
}

function Get-PromptMetadata([IO.FileInfo]$File) {
    $text = Get-Content -Raw -Encoding UTF8 -LiteralPath $File.FullName
    $slug = Get-MatchValue $text "(?m)^slug:\s*([^\r\n]+)"
    $expectedSlug = $null
    $expectedMatch = [regex]::Match($text, "(?ms)^Expected slug(?: is likely| may be)?:\s*(?:\r?\n)?([^\r\n]+)")
    if ($expectedMatch.Success) {
        $expectedSlug = $expectedMatch.Groups[1].Value.Trim()
    }

    $title = Get-MatchValue $text "(?m)^title:\s*([^\r\n]+)"
    if (-not $title) {
        $topicTitleMatch = [regex]::Match($text, "(?ms)^Topic title:\s*(?:\r?\n)?([^\r\n]+)")
        if ($topicTitleMatch.Success) {
            $title = $topicTitleMatch.Groups[1].Value.Trim()
        }
    }

    $cefr = Get-MatchValue $text "(?m)^CEFR:\s*([^\r\n]+)"
    $category = Get-MatchValue $text "(?m)^grammarCategory:\s*([^\r\n]+)"

    [pscustomobject]@{
        Number = Get-PromptNumber $File.Name
        Name = $File.Name
        FullName = $File.FullName
        Text = $text
        Slug = $slug
        ExpectedSlug = $expectedSlug
        Title = $title
        Cefr = $cefr
        Category = $category
    }
}

function Get-PurposeBlock([string]$Text, [object]$Topic) {
    $match = [regex]::Match($Text, "(?ms)^Purpose:\s*(.*?)(?=^\s*Target languages:)", [Text.RegularExpressions.RegexOptions]::Multiline)
    if ($match.Success) {
        $purpose = $match.Groups[1].Value.Trim()
        $purpose = $purpose -replace "native-language background", "selected explanation language"
        return "Purpose:`r`n$purpose"
    }

    return @"
Purpose:
Teach the official Grammar Guide topic "$($Topic.titleEn)" at $($Topic.cefrLevel) level. The lesson must explain the German grammar clearly, keep German examples central, and prepare learners to use or recognize this topic in realistic communication.
"@.Trim()
}

function Get-PreservedInstructionTail([string]$Text) {
    $markers = @(
        "(?m)^Required sections:",
        "(?m)^Required content structure:",
        "(?m)^For each section:",
        "(?m)^Examples:"
    )

    $bestIndex = -1
    foreach ($marker in $markers) {
        $match = [regex]::Match($Text, $marker)
        if ($match.Success -and ($bestIndex -lt 0 -or $match.Index -lt $bestIndex)) {
            $bestIndex = $match.Index
        }
    }

    if ($bestIndex -lt 0) {
        return $null
    }

    $tail = $Text.Substring($bestIndex).Trim()
    if ($tail -match 'Create \d+-\d+ sections that fit the syllabus topic') {
        return $null
    }

    return $tail
}

function Get-CountProfile([string]$Cefr) {
    switch ($Cefr) {
        "A1" { return [pscustomobject]@{ Sections = "8-10"; Examples = 45; Rules = "16-22"; Mistakes = 20; Words = "50-80" } }
        "A2" { return [pscustomobject]@{ Sections = "10-14"; Examples = 90; Rules = "20-28"; Mistakes = 35; Words = "70-110" } }
        "B1" { return [pscustomobject]@{ Sections = "12-16"; Examples = 130; Rules = "24-34"; Mistakes = 50; Words = "90-140" } }
        "B2" { return [pscustomobject]@{ Sections = "14-18"; Examples = 150; Rules = "26-36"; Mistakes = 55; Words = "100-150" } }
        "C1" { return [pscustomobject]@{ Sections = "14-18"; Examples = 150; Rules = "26-36"; Mistakes = 55; Words = "100-150" } }
        default { return [pscustomobject]@{ Sections = "14-20"; Examples = 140; Rules = "24-34"; Mistakes = 50; Words = "90-140" } }
    }
}

function Get-CategoryFocus([string]$Category) {
    switch ($Category) {
        "articles" { return "Focus on article choice, gender signals, case changes, article+noun chunks, and contrast with no-article contexts." }
        "nouns" { return "Focus on noun phrase structure, nominal style, article/case signals, compounds, register, and formal written patterns." }
        "gender" { return "Focus on der/die/das patterns, article+noun learning, endings that give clues, and avoiding translation-based gender guesses." }
        "plural" { return "Focus on plural patterns, article changes, common endings, dative plural where relevant, and noun chunk memorization." }
        "pronouns" { return "Focus on pronoun role, case, subject/object position, formal Sie/Ihnen, and avoiding unnecessary repetition." }
        "verbs" { return "Focus on verb form, helper verbs, infinitive/participle contrast, sentence bracket, separable verbs where relevant, and realistic verb chunks." }
        "modal-verbs" { return "Focus on modal meaning, conjugation, infinitive-at-end patterns, politeness/register, and real vs hypothetical use where relevant." }
        "tenses" { return "Focus on tense meaning, auxiliary choice, participle placement, time expressions, narrative sequence, and spoken/written usage." }
        "separable-verbs" { return "Focus on prefix position, infinitive/Perfekt forms, sentence bracket, subordinate clauses, and common everyday chunks." }
        "reflexive-verbs" { return "Focus on reflexive pronoun choice, accusative/dative distinction, word order, and fixed reflexive verb chunks." }
        "cases" { return "Focus on noun/pronoun role, article changes, case decision questions, prepositions, and same-noun different-case comparisons." }
        "nominative" { return "Focus on subject role, predicate patterns, article/pronoun forms, and contrast with accusative/dative." }
        "accusative" { return "Focus on direct-object role, masculine changes, pronouns, prepositions, and contrast with nominative/dative." }
        "dative" { return "Focus on recipient/beneficiary/location roles, dative verbs, dative prepositions, pronouns, and plural -n where relevant." }
        "genitive" { return "Focus on formal written recognition, possession/relationship meaning, article changes, noun -s/-es, and genitive prepositions." }
        "adjective-declension" { return "Focus on article+adjective+noun chunks, case/gender/number signals, weak/mixed/strong patterns, and high-frequency phrase practice." }
        "prepositions" { return "Focus on fixed preposition chunks, case after prepositions, person/thing question forms, and warnings against word-for-word translation." }
        "word-order" { return "Focus on German sentence slots, finite verb position, verb-final clauses, sentence bracket, punctuation, and readability in longer sentences." }
        "subordinate-clauses" { return "Focus on connector choice, verb-final order, clause-first inversion, comma use, and meaning contrasts between related connectors." }
        "connectors" { return "Focus on connector meaning, connector type, word-order effect, register, cause/contrast/result direction, and paired examples." }
        "negation" { return "Focus on nicht/kein placement, scope of negation, article changes, and avoiding direct translation of negative structures." }
        "questions" { return "Focus on direct and indirect question patterns, W-words, ob, formal polite frames, verb position, and answer structure." }
        "imperative" { return "Focus on du/ihr/Sie forms, tone, bitte placement, separable verbs, and polite alternatives." }
        "passive" { return "Focus on werden + Partizip II, actor focus, participle position, passive vs man, and formal/official usage." }
        "konjunktiv" { return "Focus on wäre/hätte/würde/könnte forms, politeness, hypothetical meaning, wenn clauses, infinitive placement, and register." }
        "reported-speech" { return "Focus on reporting verbs, dass/ob/W-question clauses, Konjunktiv where relevant, verb-final order, and formal written reporting." }
        "punctuation" { return "Focus on comma rules, clause boundaries, readable sentence rhythm, and punctuation as a grammar signal." }
        default { return "Focus on the core form, meaning, sentence pattern, register, common confusions, and realistic German usage for this syllabus topic." }
    }
}

function Get-GeneratedInstructionTail([object]$Topic) {
    $profile = Get-CountProfile $Topic.cefrLevel
    $categoryFocus = Get-CategoryFocus $Topic.grammarCategory
    $level = $Topic.cefrLevel
    $title = $Topic.titleEn
    $category = $Topic.grammarCategory

@"
Required sections:
Create $($profile.Sections) sections that fit the syllabus topic "$title".

Category-specific focus:
$categoryFocus

The sections must include:
1. sectionKey: what-this-topic-is
Explain the core grammar idea and when learners use or recognize it.

2. sectionKey: why-it-matters
Explain practical value for communication, reading, writing, exams, or formal German at $level.

3. sectionKey: core-patterns
Show the main German forms, sentence patterns, or connector/case/verb structures.

4. sectionKey: form-or-structure-table
Include a table with the most important forms, patterns, or sentence slots.

5. sectionKey: word-order-or-case-focus
Explain the word-order, case, agreement, tense, or register issue that causes the most mistakes.

6. sectionKey: meaning-and-use
Explain how meaning changes in real examples and where literal translation can mislead.

7. sectionKey: common-contexts
Use daily life, work, school/course, appointments, emails, exams, and formal/informal contexts as appropriate.

8. sectionKey: comparison-table
Include a comparison table with a related structure or a common confusion.

9. sectionKey: common-patterns
Include reusable sentence patterns or phrase chunks.

10. sectionKey: practice-advice
Teach a concrete review routine for checking meaning, form, word order, and case.

For each section:
- localizedBlocks for all 10 languages
- section translations for all 10 languages, including localized heading and text; section headings must not remain English-only
- substantial explanation
- use paragraph blocks, and table/callout/rule-list blocks where useful
- no raw HTML

Examples:
Provide at least $($profile.Examples) German examples.
Each example must have translations for all 10 languages.
Examples must cover the core forms and realistic $level contexts for "$title".

Rule summaries:
Provide $($profile.Rules) localized rule summaries for all 10 languages.

Common mistakes:
Provide at least $($profile.Mistakes) common mistakes with localized explanations.
Every mistake explanation must be specific to that mistake and must not repeat a generic learner-background sentence.

linkedWords:
Provide $($profile.Words) useful German word references without meanings.

prerequisiteSlugs:
Use exact syllabus slugs that are truly needed and already present in the syllabus. If unsure, keep this list short.

relatedTopicSlugs:
Use exact syllabus slugs for closely related GrammarTopic records only.

Quality:
Official $level Grammar Guide content for grammarCategory `$category`.
Do not make it short.
Use tables, practical examples, and learner-safe explanations.
Do not invent sources, citations, legal claims, medical claims, official exam-provider claims, or copyrighted task text.

Validation:
- JSON valid
- package imports
- upsert by exact syllabus slug
- no duplicate topic
- all 10 languages present
- examples translated
- common mistakes localized
- linkedWords contain no meanings
- tables render safely
- Web detail page renders

Tests:
Add/update affected tests for topic existence, required tables/sections, translated examples, localized common mistakes, linkedWords without meanings, and WebApi/Web rendering where applicable.

Final response:
Return files changed, exact topic slug used, examples count, common mistakes count, rule summaries count, tables count, tests run, and Web URL path:
/grammar/$($Topic.slug)
"@.Trim()
}

function Get-CanonicalPrompt([object]$Topic, [string]$ExistingText) {
    $targetPackage = $PackageByCefr[$Topic.cefrLevel]
    $purpose = if ($ExistingText) { Get-PurposeBlock $ExistingText $Topic } else { Get-PurposeBlock "" $Topic }
    $tail = if ($ExistingText) { Get-PreservedInstructionTail $ExistingText } else { $null }
    if (-not $tail) {
        $tail = Get-GeneratedInstructionTail $Topic
    }

    $languages = ($Languages | ForEach-Object { "- $_" }) -join "`r`n"

@"
You are working in the Loyan-UG/DarwinLingua repository.

Goal:
Generate or update one official $($Topic.cefrLevel) Grammar Guide lesson with complete localized content.

Do not generate multiple lessons.
Do not delete existing official grammar content.
Do not modify mobile/MAUI.
Do not generate exercises yet.
Do not copy exam-provider, textbook, article, essay-bank, lecture, or copyrighted material.
Do not invent sources, citations, statistics, studies, legal claims, medical claims, financial claims, career claims, HR claims, immigration claims, or official exam claims.

Target package:
$targetPackage

Generate or update exactly one GrammarTopic.

Important slug rule:
Use the exact slug from:
content/learning-portal/grammar/syllabus/grammar-syllabus-a1-c2-v1.json

slug: $($Topic.slug)
contentRevision: 1
title: $($Topic.titleEn)
CEFR: $($Topic.cefrLevel)
grammarCategory: $($Topic.grammarCategory)

$purpose

Target languages:
$languages

Localization and anti-boilerplate rules:
- Write each localized explanation in the selected content language.
- Do not address the reader as a native-language group. Avoid phrases like "Persian speakers", "Arabic learners", "Turkish learners", "for Persian speakers", and equivalent wording in any target language.
- If a comparison helps, describe the language system directly, for example: "In Persian ..." or "In Turkish ..."; use this sparingly and only when it clarifies the German grammar.
- Do not repeat the same localization sentence across sections, rules, or common mistakes.
- Every common mistake explanation must explain that exact wrong/correct pair.
- Non-English localized content must not fall back to English, except for German grammar terms, German examples, or unavoidable technical labels.
- Keep German examples central; translations explain meaning, not new grammar rules.

Output contract requirements:
- Use only supported rich block types from docs/77-Grammar-Content-Package-Contract.md: paragraph, table, callout, rule-list, example-list, mistake-pair, image-slot.
- No raw HTML.
- sections must include translations for all 10 languages with localized heading and text, not only localizedBlocks.
- ruleSummaries must be localized. Prefer ruleSummaries items with localizedText for all 10 languages.
- examples must include germanText and translations for all 10 languages.
- commonMistakes must include localized explanations for all 10 languages.
- linkedWords are references only and must not include meanings or translations.
- prerequisiteSlugs and relatedTopicSlugs must use exact syllabus slugs only.

$tail
"@.Trim() + "`r`n"
}

function Get-TopicPromptMatches([array]$Prompts, [array]$Topics) {
    $topicBySlug = @{}
    $topicByTitle = @{}
    foreach ($topic in $Topics) {
        $topicBySlug[$topic.slug] = $topic
        if ($topic.titleEn) {
            $topicByTitle[(Normalize-Key $topic.titleEn)] = $topic
        }
        if ($topic.title) {
            $topicByTitle[(Normalize-Key $topic.title)] = $topic
        }
    }

    $candidates = @()
    foreach ($prompt in $Prompts) {
        $promptSlugs = @($prompt.Slug, $prompt.ExpectedSlug) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        foreach ($slug in $promptSlugs) {
            $targetSlug = if ($KnownSlugAliases.ContainsKey($slug)) { $KnownSlugAliases[$slug] } else { $slug }
            if ($topicBySlug.ContainsKey($targetSlug)) {
                $candidates += [pscustomobject]@{ Prompt = $prompt; Topic = $topicBySlug[$targetSlug]; Score = 100; MatchType = "slug" }
            }
        }

        if ($prompt.Title) {
            $titleKey = Normalize-Key $prompt.Title
            if ($topicByTitle.ContainsKey($titleKey)) {
                $candidates += [pscustomobject]@{ Prompt = $prompt; Topic = $topicByTitle[$titleKey]; Score = 92; MatchType = "title" }
            }
        }

        foreach ($topic in $Topics | Where-Object { $_.cefrLevel -eq $prompt.Cefr }) {
            $best = [Math]::Max((Get-Similarity $prompt.ExpectedSlug $topic.slug), (Get-Similarity $prompt.Title $topic.titleEn))
            if ($best -ge 0.88) {
                $candidates += [pscustomobject]@{ Prompt = $prompt; Topic = $topic; Score = [int]($best * 80); MatchType = "fuzzy" }
            }
        }
    }

    $topicMatches = @{}
    $usedPromptNames = @{}
    foreach ($candidate in ($candidates | Sort-Object @{Expression = "Score"; Descending = $true}, @{Expression = { $_.Prompt.Number }; Ascending = $true})) {
        if (-not $topicMatches.ContainsKey($candidate.Topic.slug) -and -not $usedPromptNames.ContainsKey($candidate.Prompt.Name)) {
            $topicMatches[$candidate.Topic.slug] = $candidate
            $usedPromptNames[$candidate.Prompt.Name] = $true
        }
    }

    return $topicMatches
}

function New-Audit([array]$Prompts, [array]$Topics, [hashtable]$Matches) {
    $matchedSlugs = @($Matches.Keys)
    $unmatchedPrompts = @($Prompts | Where-Object {
        $promptName = $_.Name
        -not (@($Matches.Values | ForEach-Object { $_.Prompt.Name }) -contains $promptName)
    })
    $missingTopics = @($Topics | Where-Object { $matchedSlugs -notcontains $_.slug })
    $duplicates = @(
        $Prompts |
            Group-Object {
                $slug = if ($_.Slug) { $_.Slug } else { $_.ExpectedSlug }
                if ($KnownSlugAliases.ContainsKey($slug)) { $KnownSlugAliases[$slug] } else { $slug }
            } |
            Where-Object { $_.Name -and $_.Count -gt 1 } |
            ForEach-Object {
                [pscustomobject]@{ Slug = $_.Name; Count = $_.Count; Files = @($_.Group | ForEach-Object Name) }
            }
    )
    $categoryMismatches = @(
        foreach ($match in $Matches.Values) {
            if ($match.Prompt.Category -and $match.Prompt.Category -ne $match.Topic.grammarCategory) {
                [pscustomobject]@{
                    Prompt = $match.Prompt.Name
                    Slug = $match.Topic.slug
                    PromptCategory = $match.Prompt.Category
                    SyllabusCategory = $match.Topic.grammarCategory
                }
            }
        }
    )

    [pscustomobject]@{
        PromptCount = $Prompts.Count
        SyllabusTopicCount = $Topics.Count
        Matched = $Matches.Count
        Unmatched = $unmatchedPrompts.Count
        MissingSyllabus = $missingTopics.Count
        DuplicateSlugs = $duplicates.Count
        CategoryMismatches = $categoryMismatches.Count
        MissingExactSlugRule = @($Prompts | Where-Object { $_.Text -notmatch "Use the exact slug from:" }).Count
        MissingNoExercises = @($Prompts | Where-Object { $_.Text -notmatch "Do not generate exercises" }).Count
        MissingNoMaui = @($Prompts | Where-Object { $_.Text -notmatch "Do not modify mobile/MAUI" }).Count
        LanguageListProblems = @(
            $Prompts | Where-Object {
                $text = $_.Text
                @($Languages | Where-Object { $text -notmatch "(?m)^- $_\s*$" }).Count -gt 0
            }
        ).Count
        UnmatchedPrompts = @($unmatchedPrompts | Select-Object Number, Name, Slug, ExpectedSlug, Title, Cefr, Category)
        MissingTopics = @($missingTopics | Select-Object globalNumber, cefrLevel, sequenceInLevel, slug, titleEn, grammarCategory)
        CategoryMismatchDetails = $categoryMismatches
        DuplicateDetails = $duplicates
    }
}

$resolvedSyllabusPath = Resolve-RepoPath $SyllabusPath
if (-not (Test-Path -LiteralPath $resolvedSyllabusPath)) {
    throw "Syllabus file was not found: $resolvedSyllabusPath"
}
if (-not (Test-Path -LiteralPath $PromptDirectory)) {
    throw "Prompt directory was not found: $PromptDirectory"
}

$syllabus = Get-Content -Raw -Encoding UTF8 -LiteralPath $resolvedSyllabusPath | ConvertFrom-Json
$topics = @(
    $syllabus.topics |
        Sort-Object @{ Expression = { [array]::IndexOf($CefrOrder, $_.cefrLevel) } }, sequenceInLevel |
        ForEach-Object -Begin { $global = 0 } -Process {
            $global++
            $_ | Add-Member -NotePropertyName globalNumber -NotePropertyValue $global -Force
            $_
        }
)

$promptFiles = @(
    Get-ChildItem -LiteralPath $PromptDirectory -Filter "*.txt" |
        Where-Object { $_.Name -match "^Prompts \(\d+\)\.txt$" } |
        Sort-Object { Get-PromptNumber $_.Name }
)
$prompts = @($promptFiles | ForEach-Object { Get-PromptMetadata $_ })
$topicPromptMatches = Get-TopicPromptMatches $prompts $topics
$audit = New-Audit $prompts $topics $topicPromptMatches

if ($Mode -eq "Audit") {
    $json = $audit | ConvertTo-Json -Depth 8
    if ($ReportPath) {
        $resolvedReportPath = Resolve-RepoPath $ReportPath
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $resolvedReportPath) | Out-Null
        Set-Content -LiteralPath $resolvedReportPath -Value $json -Encoding UTF8
    }
    $json
    return
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupRoot = Join-Path (Split-Path -Parent $PromptDirectory) "PromptBackups"
$backupDirectory = Join-Path $backupRoot "Prompts-$timestamp"
New-Item -ItemType Directory -Force -Path $backupDirectory | Out-Null

foreach ($promptFile in $promptFiles) {
    Copy-Item -LiteralPath $promptFile.FullName -Destination $backupDirectory -Force
}

$backupPromptCount = @(
    Get-ChildItem -LiteralPath $backupDirectory -Filter "*.txt" |
        Where-Object { $_.Name -match "^Prompts \(\d+\)\.txt$" }
).Count
if ($backupPromptCount -ne $promptFiles.Count) {
    throw "Prompt backup failed. Expected $($promptFiles.Count) prompt files, but copied $backupPromptCount to $backupDirectory."
}

$audit | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $backupDirectory "pre-sync-audit.json") -Encoding UTF8

Get-ChildItem -LiteralPath $PromptDirectory -Filter "*.txt" |
    Where-Object { $_.Name -match "^Prompts \(\d+\)\.txt$" } |
    Remove-Item -Force

foreach ($topic in $topics) {
    $match = if ($topicPromptMatches.ContainsKey($topic.slug)) { $topicPromptMatches[$topic.slug] } else { $null }
    $existingText = if ($match) { $match.Prompt.Text } else { $null }
    $promptText = Get-CanonicalPrompt $topic $existingText
    $targetPath = Join-Path $PromptDirectory ("Prompts ({0}).txt" -f $topic.globalNumber)
    Set-Content -LiteralPath $targetPath -Value $promptText -Encoding UTF8
}

$finalPromptFiles = @(
    Get-ChildItem -LiteralPath $PromptDirectory -Filter "*.txt" |
        Where-Object { $_.Name -match "^Prompts \(\d+\)\.txt$" } |
        Sort-Object { Get-PromptNumber $_.Name }
)
$finalPrompts = @($finalPromptFiles | ForEach-Object { Get-PromptMetadata $_ })
$finalMatches = Get-TopicPromptMatches $finalPrompts $topics
$finalAudit = New-Audit $finalPrompts $topics $finalMatches
$finalAudit | Add-Member -NotePropertyName BackupDirectory -NotePropertyValue $backupDirectory -Force
$finalAudit | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $backupDirectory "post-sync-audit.json") -Encoding UTF8

if ($ReportPath) {
    $resolvedReportPath = Resolve-RepoPath $ReportPath
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $resolvedReportPath) | Out-Null
    $finalAudit | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $resolvedReportPath -Encoding UTF8
}

$finalAudit | ConvertTo-Json -Depth 8
