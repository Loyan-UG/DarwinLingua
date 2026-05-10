# Talk Topics Import Report

Date: 2026-05-10

## Shared WebApi Database

Commands run:

```powershell
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --target shared --yes D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510-fixes
dotnet run --project D:\_Projects\DarwinLingua\src\Apps\DarwinLingua.ImportTool\DarwinLingua.ImportTool.csproj -- --target shared --yes D:\_Projects\DarwinLingua\content\generated\talk-topics-20260510
```

Import result:

- Baseline fix files processed: 1
- Baseline fix files succeeded: 1
- Generated package files processed: 10
- Generated package files succeeded: 10
- Generated package files failed: 0
- TalkTopic items generated: 900
- Distinct generated topic groups: 300

Cleanup:

- Removed 192 TalkTopics from a failed earlier mojibake import attempt. The cleanup matched only TalkTopic titles containing mojibake/replacement characters.

Final WebApi verification:

- Public TalkTopic count: 901
- Mojibake titles visible: 0
- Corrected baseline sample: present
- Corrected baseline A1 article length: 1086 characters
- Corrected baseline article translation: none
- Corrected baseline warm-up questions: 3
- Corrected baseline discussion questions: 8
- Corrected baseline vocabulary items: 12
