param(
    [Parameter(Mandatory = $true)]
    [string] $PackagePath,

    [string] $Slug = "",

    [int] $SectionDuplicateThreshold = 2,

    [int] $MistakeDuplicateThreshold = 1,

    [switch] $FailOnIssue
)

$ErrorActionPreference = "Stop"

$languages = @("en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq")
$forbiddenPhraseBase64 = @(
    "2KjYsdin24wg2YHYp9ix2LPbjOKAjNiy2KjYp9mG",
    "2YHYp9ix2LPbjOKAjNiy2KjYp9mG",
    "2YbZgti32Ycg2KjYsdix2LPbjCDYqNix2KfbjA==",
    "2KrYpyDZiNmC2KrbjCDZhdir2KfZhCDYotmE2YXYp9mG24w=",
    "2YbZgti32Kkg2YHYrdi1INmE2YLYs9mF",
    "2YTYpyDYqtiq2KfYqNi5INmC2KjZhA==",
    "0J/RgNC+0LLQtdGA0LrQsCDQtNC70Y8g0YDQsNC30LTQtdC70LA=",
    "0L3QtSDQv9GA0L7QtNC+0LvQttCw0LnRgtC1LCDQv9C+0LrQsA==",
    "2K7Yp9q124wg2b7YtNqp2YbbjNmGINio24Y=",
    "2Kjbldix2K/bldmI2KfZhSDZhduV2KjblQ=="
)
$forbiddenPhrases = @(
    "Persian speakers",
    "Arabic learners",
    "Turkish learners",
    "Kurdish learners",
    "Checkpoint for",
    "do not continue until",
    "German example works with the rule",
    "kontrol noktas",
    "Xala kontrolê",
    "berdewam neke",
    "Punkt kontroli",
    "nie idź dalej",
    "Punct de verificare",
    "nu continua până",
    "Pikë kontrolli",
    "mos vazhdo derisa"
)
$forbiddenPhrases += $forbiddenPhraseBase64 | ForEach-Object {
    [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($_))
}

$englishFallbackStarts = @(
    "An important point is that ",
    "It follows that ",
    "First, it can be stated that ",
    "This example also shows that ",
    "For this reason, it should be considered that ",
    "Although the situation is evaluated differently",
    "The text stays clear when ",
    "In a formal paragraph",
    "Finally, it becomes clear that ",
    "The statement becomes more convincing",
    "Use the corrected form",
    "This section focuses on",
    "The corrected sentence"
)

if (-not (Test-Path -LiteralPath $PackagePath)) {
    throw "Package not found: $PackagePath"
}

$json = Get-Content -LiteralPath $PackagePath -Raw | ConvertFrom-Json
$topics = @($json.grammarTopics)

if (-not [string]::IsNullOrWhiteSpace($Slug)) {
    $topics = @($topics | Where-Object { $_.slug -eq $Slug })
}

if ($topics.Count -eq 0) {
    throw "No grammar topics matched the requested filter."
}

$issues = New-Object System.Collections.Generic.List[string]

foreach ($topic in $topics) {
    $serializedTopic = $topic | ConvertTo-Json -Depth 100 -Compress

    foreach ($phrase in $forbiddenPhrases) {
        if ($serializedTopic.Contains($phrase)) {
            $issues.Add("$($topic.slug): forbidden localization boilerplate '$phrase'")
        }
    }

    foreach ($language in $languages) {
        $sectionTexts = New-Object System.Collections.Generic.List[string]

        foreach ($section in @($topic.sections)) {
            $translation = @($section.translations | Where-Object { $_.language -eq $language } | Select-Object -First 1)
            if ($translation.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($translation[0].text)) {
                $sectionTexts.Add($translation[0].text.Trim())
            }

            $blocks = $section.localizedBlocks.$language
            foreach ($block in @($blocks)) {
                if ($block.type -eq "paragraph" -and -not [string]::IsNullOrWhiteSpace($block.text)) {
                    $sectionTexts.Add($block.text.Trim())
                }

                if ($block.type -eq "example-list") {
                    if (-not ($block.PSObject.Properties.Name -contains "items") -or @($block.items).Count -eq 0) {
                        $issues.Add("$($topic.slug): $language section '$($section.sectionKey)' example-list block must contain non-empty items")
                    }

                    if ($block.PSObject.Properties.Name -contains "examples") {
                        $issues.Add("$($topic.slug): $language section '$($section.sectionKey)' example-list block uses unsupported examples property; use items")
                    }
                }

                if ($block.type -eq "table") {
                    if (-not ($block.PSObject.Properties.Name -contains "caption") -or [string]::IsNullOrWhiteSpace($block.caption)) {
                        $issues.Add("$($topic.slug): $language section '$($section.sectionKey)' table block must contain non-empty caption")
                    }

                    if (-not ($block.PSObject.Properties.Name -contains "columns") -or @($block.columns).Count -eq 0) {
                        $issues.Add("$($topic.slug): $language section '$($section.sectionKey)' table block must contain non-empty columns")
                    }

                    if (-not ($block.PSObject.Properties.Name -contains "rows") -or @($block.rows).Count -eq 0) {
                        $issues.Add("$($topic.slug): $language section '$($section.sectionKey)' table block must contain non-empty rows")
                    }
                }

                if ($block.type -eq "callout") {
                    if ([string]::IsNullOrWhiteSpace($block.text)) {
                        $issues.Add("$($topic.slug): $language section '$($section.sectionKey)' callout block must contain text")
                    }
                }
            }
        }

        $sectionDuplicates = $sectionTexts |
            Where-Object { $_.Length -ge 40 } |
            Group-Object |
            Where-Object { $_.Count -gt $SectionDuplicateThreshold }

        foreach ($duplicate in $sectionDuplicates) {
            $issues.Add("$($topic.slug): $language repeated section/paragraph text $($duplicate.Count)x: $($duplicate.Name.Substring(0, [Math]::Min(90, $duplicate.Name.Length)))")
        }

        $mistakeTexts = New-Object System.Collections.Generic.List[string]
        foreach ($mistake in @($topic.commonMistakes)) {
            $text = $null
            if ($mistake.explanationLocalized -and $mistake.explanationLocalized.PSObject.Properties.Name -contains $language) {
                $text = $mistake.explanationLocalized.$language
            }

            if ([string]::IsNullOrWhiteSpace($text)) {
                $translation = @($mistake.translations | Where-Object { $_.language -eq $language } | Select-Object -First 1)
                if ($translation.Count -gt 0) {
                    $text = $translation[0].explanation
                }
            }

            if (-not [string]::IsNullOrWhiteSpace($text)) {
                $mistakeTexts.Add($text.Trim())
            }
        }

        $mistakeDuplicates = $mistakeTexts |
            Where-Object { $_.Length -ge 40 } |
            Group-Object |
            Where-Object { $_.Count -gt $MistakeDuplicateThreshold }

        foreach ($duplicate in $mistakeDuplicates) {
            $issues.Add("$($topic.slug): $language repeated common-mistake explanation $($duplicate.Count)x: $($duplicate.Name.Substring(0, [Math]::Min(90, $duplicate.Name.Length)))")
        }

        if ($language -ne "en") {
            foreach ($example in @($topic.examples)) {
                $translation = @($example.translations | Where-Object { $_.language -eq $language } | Select-Object -First 1)
                if ($translation.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($translation[0].text)) {
                    foreach ($prefix in $englishFallbackStarts) {
                        if ($translation[0].text.StartsWith($prefix, [StringComparison]::Ordinal)) {
                            $issues.Add("$($topic.slug): $language example appears to use English fallback text: $($translation[0].text.Substring(0, [Math]::Min(90, $translation[0].text.Length)))")
                        }
                    }
                }
            }
        }
    }
}

$summary = [pscustomobject]@{
    Package = $PackagePath
    TopicCount = $topics.Count
    IssueCount = $issues.Count
    Issues = $issues
}

$summary | ConvertTo-Json -Depth 4

if ($FailOnIssue -and $issues.Count -gt 0) {
    exit 1
}
