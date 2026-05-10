# Talk Topics Import Report

Date: 2026-05-10

## Shared WebApi Database

Commands run:

```powershell
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --target shared --yes D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510-fixes
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --target shared --yes D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\ContentUtilities\TestTalkTopicPackages.ps1 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\Server\Initialize-LocalServerContent.ps1 -ApiBaseUrl http://localhost:53945 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510 -AdminApiKey local-dev-admin-api-key-change-me
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\ContentUtilities\TestTalkTopicPackages.ps1 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\Server\Initialize-LocalServerContent.ps1 -ApiBaseUrl http://localhost:53945 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510 -AdminApiKey local-dev-admin-api-key-change-me
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\ContentUtilities\TestTalkTopicPackages.ps1 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\Server\Initialize-LocalServerContent.ps1 -ApiBaseUrl http://localhost:53945 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510 -AdminApiKey local-dev-admin-api-key-change-me
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\ContentUtilities\TestTalkTopicPackages.ps1 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\Server\Initialize-LocalServerContent.ps1 -ApiBaseUrl http://localhost:53945 -ContentPath D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510 -AdminApiKey local-dev-admin-api-key-change-me
powershell -NoProfile -ExecutionPolicy Bypass -File D:\_Projects\DarwinLingua\tools\ContentUtilities\TestTalkTopicVocabularyResolution.ps1 -ApiBaseUrl http://localhost:53945 -PrimaryMeaningLanguageCode en -SecondaryMeaningLanguageCode fa
```

Import result:

- Baseline fix files processed: 1
- Baseline fix files succeeded: 1
- Generated package files processed: 10
- Generated package files succeeded: 10
- Generated package files failed: 0
- TalkTopic items generated: 900
- Distinct generated topic groups: 300
- Editorial refresh package ids: `de-talk-topics-20260510-v6-001` through `de-talk-topics-20260510-v6-010`
- Distinct generated vocabulary lemmas: 161

Cleanup:

- Removed 192 TalkTopics from a failed earlier mojibake import attempt. The cleanup matched only TalkTopic titles containing mojibake/replacement characters.
- Removed 900 generated v2 TalkTopics before importing the v3 editorial refresh. The baseline sample `a1-gibt-es-ausserirdische` was preserved.
- Removed 900 generated v3 TalkTopics before importing the v4 article-quality refresh. The baseline sample `a1-gibt-es-ausserirdische` was preserved.
- Removed 900 generated v4 TalkTopics before importing the v5 German-orthography refresh. The baseline sample `a1-gibt-es-ausserirdische` was preserved.
- Removed 900 generated v5 TalkTopics before importing the v6 vocabulary-diversity refresh. The baseline sample `a1-gibt-es-ausserirdische` was preserved.

Final WebApi verification:

- Public TalkTopic count: 901
- Mojibake titles visible: 0
- Repeated title endings visible: 0
- Corrected baseline sample: present
- Corrected baseline A1 article length: 1062 characters
- Corrected baseline article translation: none
- Corrected baseline warm-up questions: 3
- Corrected baseline discussion questions: 8
- Corrected baseline vocabulary items: 12
- Sample refreshed generated topic: `a1-roboter-im-alltag`
- Sample refreshed generated category: `technology`
- Sample refreshed generated A1 article length: 1090 characters
- Sample refreshed generated article starts with German category text: `Roboter im Alltag ist ein Thema aus dem Bereich Technik.`
- Sample generated description uses canonical German spelling: `Ein Talk Topic für Diskussionen über Leben im Weltall in der Schule.`
- Sample generated question uses canonical German spelling: `Wie könnte eine ungewöhnliche Lösung aussehen?`
- Sample generated vocabulary starts with category-specific items: `der Roboter`, `das Gerät`, `die Technik`, `der Computer`, `die Software`, `der Datenschutz`
- TalkTopic vocabulary resolution through the WebApi: 17,307 of 22,212 items resolved (77.92%)
- Distinct unresolved TalkTopic vocabulary lemmas: 54
- Highest-impact unresolved lemmas: `die Frage`, `vorstellen`, `verändern`, `der Respekt`, `das Risiko`

Follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-001.json`
- Added 5 high-impact Word Catalog entries for unresolved TalkTopic important-word references
- TalkTopic vocabulary resolution after the bridge import: 20,564 of 22,212 items resolved (92.58%)
- Distinct unresolved TalkTopic vocabulary lemmas after the bridge import: 49

Second follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-002.json`
- Added 5 additional high-impact Word Catalog entries: `die Debatte`, `das Homeoffice`, `der Geschmack`, `der Roboter`, `die Software`
- TalkTopic vocabulary resolution after the second bridge import: 21,000 of 22,212 items resolved (94.54%)
- Distinct unresolved TalkTopic vocabulary lemmas after the second bridge import: 44

Third follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-003.json`
- Added 5 additional high-impact Word Catalog entries: `der Alltag`, `der Streit`, `die Reise`, `zuhören`, `der Körper`
- TalkTopic vocabulary resolution after the third bridge import: 21,273 of 22,212 items resolved (95.77%)
- Distinct unresolved TalkTopic vocabulary lemmas after the third bridge import: 39

Fourth follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-004.json`
- Added 5 additional high-impact Word Catalog entries: `der öffentliche Platz`, `die Bewegung`, `das Weltall`, `der Planet`, `der Stern`
- TalkTopic vocabulary resolution after the fourth bridge import: 21,456 of 22,212 items resolved (96.60%)
- Distinct unresolved TalkTopic vocabulary lemmas after the fourth bridge import: 34

Fifth follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-005.json`
- Added 5 additional high-impact Word Catalog entries: `die Erde`, `das Glück`, `das Haustier`, `das Konzert`, `das Museum`
- TalkTopic vocabulary resolution after the fifth bridge import: 21,607 of 22,212 items resolved (97.28%)
- Distinct unresolved TalkTopic vocabulary lemmas after the fifth bridge import: 29

Sixth follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-006.json`
- Added 5 additional high-impact Word Catalog entries: `das Teleskop`, `der öffentliche Raum`, `der Prompt`, `die Armut`, `die Bühne`
- TalkTopic vocabulary resolution after the sixth bridge import: 21,757 of 22,212 items resolved (97.95%)
- Distinct unresolved TalkTopic vocabulary lemmas after the sixth bridge import: 24

Seventh follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-007.json`
- Added 5 additional high-impact Word Catalog entries: `die Daten`, `die Figur`, `die Kunst`, `die Mahlzeit`, `die Musik`
- TalkTopic vocabulary resolution after the seventh bridge import: 21,907 of 22,212 items resolved (98.63%)
- Distinct unresolved TalkTopic vocabulary lemmas after the seventh bridge import: 19

Eighth follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-008.json`
- Added 5 additional high-impact Word Catalog entries: `die Stimme`, `die Unterstützung`, `das Fest`, `das Sprichwort`, `das Profil`
- TalkTopic vocabulary resolution after the eighth bridge import: 22,030 of 22,212 items resolved (99.18%)
- Distinct unresolved TalkTopic vocabulary lemmas after the eighth bridge import: 14

Ninth follow-up vocabulary bridge:

- Imported `content/generated/talk-topic-vocabulary-bridges-20260510/de-talk-topic-vocabulary-bridge-009.json`
- Added 5 additional high-impact Word Catalog entries: `das Spiel`, `der Fußball`, `der Schiedsrichter`, `die Fairness`, `die Freundschaft`
- TalkTopic vocabulary resolution after the ninth bridge import: 22,105 of 22,212 items resolved (99.52%)
- Distinct unresolved TalkTopic vocabulary lemmas after the ninth bridge import: 9
