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
```

Import result:

- Baseline fix files processed: 1
- Baseline fix files succeeded: 1
- Generated package files processed: 10
- Generated package files succeeded: 10
- Generated package files failed: 0
- TalkTopic items generated: 900
- Distinct generated topic groups: 300
- Editorial refresh package ids: `de-talk-topics-20260510-v5-001` through `de-talk-topics-20260510-v5-010`

Cleanup:

- Removed 192 TalkTopics from a failed earlier mojibake import attempt. The cleanup matched only TalkTopic titles containing mojibake/replacement characters.
- Removed 900 generated v2 TalkTopics before importing the v3 editorial refresh. The baseline sample `a1-gibt-es-ausserirdische` was preserved.
- Removed 900 generated v3 TalkTopics before importing the v4 article-quality refresh. The baseline sample `a1-gibt-es-ausserirdische` was preserved.
- Removed 900 generated v4 TalkTopics before importing the v5 German-orthography refresh. The baseline sample `a1-gibt-es-ausserirdische` was preserved.

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
