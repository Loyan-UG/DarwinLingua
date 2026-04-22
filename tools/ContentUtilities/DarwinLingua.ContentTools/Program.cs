using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DarwinLingua.ContentTools;

internal static class Program
{
    private static readonly HashSet<string> SalesLabels =
    [
        "crm",
        "sales",
        "customer-support",
        "customer-meeting",
        "presentation",
        "communication",
        "pricing",
        "consulting",
    ];

    private static readonly HashSet<string> FinanceLabels =
    [
        "finance",
        "accounting",
        "audit",
        "compliance",
        "legal",
        "banking",
        "reporting",
    ];

    private static readonly HashSet<string> SupplyLabels =
    [
        "warehouse",
        "inventory",
        "logistics",
        "delivery",
        "procurement",
    ];

    private static readonly HashSet<string> SoftwareLabels =
    [
        "software",
        "erp",
        "integration",
        "database",
        "debugging",
        "deployment",
        "security",
        "performance",
        "ui",
        "user-interface",
        "user-experience",
        "automation",
        "logging",
        "configuration",
        "permissions",
        "master-data",
        "data-entry",
        "data-processing",
        "data-migration",
        "migration",
        "availability",
        "system-operations",
        "network",
    ];

    private static readonly HashSet<string> TeamworkLabels =
    [
        "project-work",
        "workflow",
        "meeting",
        "teamwork",
        "management",
        "planning",
        "operations",
        "process",
        "process-optimization",
        "quality",
        "training",
        "hr",
        "recruiting",
        "technical-discussion",
        "approval",
        "service-level",
        "risk",
        "documentation",
        "work-and-jobs",
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static int Main(string[] args)
    {
        try
        {
            string contentPath = "content/generated";
            int minimumExamples = 2;
            bool rewriteSecondaryExamples = false;

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];

                if (string.Equals(argument, "--content-path", StringComparison.OrdinalIgnoreCase))
                {
                    index++;
                    contentPath = index < args.Length
                        ? args[index]
                        : throw new InvalidOperationException("The --content-path option requires a value.");
                    continue;
                }

                if (string.Equals(argument, "--minimum-examples", StringComparison.OrdinalIgnoreCase))
                {
                    index++;
                    minimumExamples = index < args.Length && int.TryParse(args[index], out int parsedMinimumExamples)
                        ? parsedMinimumExamples
                        : throw new InvalidOperationException("The --minimum-examples option requires an integer value.");
                    continue;
                }

                if (string.Equals(argument, "--rewrite-secondary-examples", StringComparison.OrdinalIgnoreCase))
                {
                    rewriteSecondaryExamples = true;
                    continue;
                }
            }

            if (minimumExamples < 2)
            {
                throw new InvalidOperationException("--minimum-examples must be at least 2.");
            }

            DirectoryInfo contentDirectory = new(contentPath);
            if (!contentDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Content path '{contentDirectory.FullName}' was not found.");
            }

            int updatedFiles = 0;
            int updatedEntries = 0;

            foreach (FileInfo file in contentDirectory.GetFiles("*.json").OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase))
            {
                int fileUpdates = ProcessFile(file.FullName, minimumExamples, rewriteSecondaryExamples);
                if (fileUpdates > 0)
                {
                    updatedFiles++;
                    updatedEntries += fileUpdates;
                }
            }

            Console.WriteLine($"Updated files: {updatedFiles}");
            Console.WriteLine($"Updated entries: {updatedEntries}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static int ProcessFile(string path, int minimumExamples, bool rewriteSecondaryExamples)
    {
        JsonObject document = JsonNode.Parse(File.ReadAllText(path))
            ?.AsObject()
            ?? throw new InvalidOperationException($"The file '{path}' does not contain a valid JSON object.");

        JsonArray entries = document["entries"]?.AsArray()
            ?? throw new InvalidOperationException($"The file '{path}' does not contain an 'entries' array.");

        int updatedEntries = 0;

        foreach (JsonNode? entryNode in entries)
        {
            if (entryNode is not JsonObject entry)
            {
                continue;
            }

            JsonArray examples = entry["examples"] as JsonArray ?? [];
            entry["examples"] = examples;

            if (rewriteSecondaryExamples && examples.Count > 1)
            {
                while (examples.Count > 1)
                {
                    examples.RemoveAt(examples.Count - 1);
                }
            }

            HashSet<string> existingTexts = examples
                .OfType<JsonObject>()
                .Select(example => example["baseText"]?.GetValue<string>()?.Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => text!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool entryChanged = false;
            while (examples.Count < minimumExamples)
            {
                JsonObject example = CreateSecondExample(entry);
                string? baseText = example["baseText"]?.GetValue<string>()?.Trim();

                if (string.IsNullOrWhiteSpace(baseText) || !existingTexts.Add(baseText))
                {
                    break;
                }

                examples.Add(example);
                entryChanged = true;
            }

            if (entryChanged)
            {
                updatedEntries++;
            }
        }

        if (updatedEntries > 0)
        {
            File.WriteAllText(path, document.ToJsonString(JsonOptions) + Environment.NewLine);
        }

        return updatedEntries;
    }

    private static JsonObject CreateSecondExample(JsonObject entry)
    {
        string partOfSpeech = entry["partOfSpeech"]?.GetValue<string>() ?? string.Empty;
        string contextGroup = ResolveContextGroup(entry);

        string englishMeaning = ResolveMeaning(entry, "en");
        string persianMeaning = ResolveMeaning(entry, "fa");

        string seed;
        ExampleTemplate selectedTemplate;

        if (string.Equals(partOfSpeech, "Noun", StringComparison.OrdinalIgnoreCase))
        {
            string word = entry["word"]?.GetValue<string>() ?? string.Empty;
            string article = ResolvePrimaryArticle(entry);
            string articleWord = string.IsNullOrWhiteSpace(article) ? word : $"{article} {word}";
            string articleWordCapitalized = string.IsNullOrWhiteSpace(article) ? word : $"{char.ToUpperInvariant(article[0])}{article[1..]} {word}";

            ExampleTemplate[] templates = BuildNounTemplates(articleWord, articleWordCapitalized, englishMeaning, persianMeaning, contextGroup);
            seed = word;
            selectedTemplate = templates[GetStableIndex(seed, templates.Length)];
        }
        else if (string.Equals(partOfSpeech, "Verb", StringComparison.OrdinalIgnoreCase))
        {
            string infinitive = ResolvePrimaryInfinitive(entry);
            ExampleTemplate[] templates = BuildVerbTemplates(infinitive, englishMeaning, persianMeaning, contextGroup);
            seed = infinitive;
            selectedTemplate = templates[GetStableIndex(seed, templates.Length)];
        }
        else
        {
            string adjective = entry["word"]?.GetValue<string>() ?? string.Empty;
            ExampleTemplate[] templates = BuildAdjectiveTemplates(adjective, englishMeaning, persianMeaning, contextGroup);
            seed = adjective;
            selectedTemplate = templates[GetStableIndex(seed, templates.Length)];
        }

        return new JsonObject
        {
            ["baseText"] = selectedTemplate.German,
            ["translations"] = new JsonArray
            {
                new JsonObject
                {
                    ["language"] = "en",
                    ["text"] = selectedTemplate.English,
                },
                new JsonObject
                {
                    ["language"] = "fa",
                    ["text"] = selectedTemplate.Persian,
                },
            },
        };
    }

    private static string ResolvePrimaryArticle(JsonObject entry)
    {
        string? article = entry["article"]?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(article))
        {
            return article.Trim();
        }

        foreach (JsonNode? lexicalFormNode in entry["lexicalForms"]?.AsArray() ?? [])
        {
            if (lexicalFormNode is not JsonObject lexicalForm)
            {
                continue;
            }

            article = lexicalForm["article"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(article))
            {
                return article.Trim();
            }
        }

        return string.Empty;
    }

    private static string ResolvePrimaryInfinitive(JsonObject entry)
    {
        string? infinitive = entry["infinitive"]?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(infinitive))
        {
            return infinitive.Trim();
        }

        foreach (JsonNode? lexicalFormNode in entry["lexicalForms"]?.AsArray() ?? [])
        {
            if (lexicalFormNode is not JsonObject lexicalForm)
            {
                continue;
            }

            infinitive = lexicalForm["infinitive"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(infinitive))
            {
                return infinitive.Trim();
            }
        }

        return entry["word"]?.GetValue<string>() ?? string.Empty;
    }

    private static string ResolveContextGroup(JsonObject entry)
    {
        HashSet<string> labels = entry["contextLabels"]?.AsArray()
            .OfType<JsonValue>()
            .Select(node => node.GetValue<string>())
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? [];

        if (labels.Overlaps(SalesLabels))
        {
            return "sales";
        }

        if (labels.Overlaps(FinanceLabels))
        {
            return "finance";
        }

        if (labels.Overlaps(SupplyLabels))
        {
            return "supply";
        }

        if (labels.Overlaps(SoftwareLabels))
        {
            return "software";
        }

        if (labels.Overlaps(TeamworkLabels))
        {
            return "teamwork";
        }

        return "general";
    }

    private static string ResolveMeaning(JsonObject entry, string language)
    {
        foreach (JsonNode? meaningNode in entry["meanings"]?.AsArray() ?? [])
        {
            if (meaningNode is not JsonObject meaning)
            {
                continue;
            }

            string? currentLanguage = meaning["language"]?.GetValue<string>();
            if (!string.Equals(currentLanguage, language, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string? text = meaning["text"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return SimplifyMeaning(text.Trim());
            }
        }

        return entry["word"]?.GetValue<string>() ?? string.Empty;
    }

    private static string SimplifyMeaning(string value)
    {
        string[] separators = [";", ",", "؛", "،", "/"];

        foreach (string separator in separators)
        {
            string[] parts = value.Split(separator, 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                value = parts[0];
            }
        }

        return value.Trim();
    }

    private static int GetStableIndex(string seed, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        int sum = 0;
        foreach (char character in seed)
        {
            sum += character;
        }

        return Math.Abs(sum) % count;
    }

    private static ExampleTemplate[] BuildNounTemplates(string articleWord, string articleWordCapitalized, string englishMeaning, string persianMeaning, string group) =>
        group switch
        {
            "sales" =>
            [
                new($"{articleWordCapitalized} ist für die Abstimmung mit dem Kunden im aktuellen Projekt besonders wichtig.", $"The {englishMeaning} is especially important for coordinating with the customer in the current project.", $"{persianMeaning} برای هماهنگی با مشتری در پروژه فعلی اهمیت ویژه‌ای دارد."),
                new($"Im heutigen Kundentermin war {articleWord} eines der wichtigsten Themen für das Vertriebsteam.", $"In today's customer meeting, the {englishMeaning} was one of the most important topics for the sales team.", $"در جلسه امروز با مشتری، {persianMeaning} یکی از مهم‌ترین موضوعات برای تیم فروش بود."),
                new($"{articleWordCapitalized} bleibt auch im nächsten Sprint für CRM und Angebotserstellung relevant.", $"The {englishMeaning} remains relevant for CRM work and quotation preparation in the next sprint as well.", $"{persianMeaning} در اسپرینت بعدی هم برای کارهای CRM و آماده‌سازی پیشنهاد قیمت مهم باقی می‌ماند."),
            ],
            "finance" =>
            [
                new($"{articleWordCapitalized} ist für den Monatsabschluss im ERP besonders wichtig.", $"The {englishMeaning} is especially important for the month-end closing in the ERP system.", $"{persianMeaning} برای بستن حساب‌های پایان ماه در ERP اهمیت ویژه‌ای دارد."),
                new($"Im Finanzprozess wurde {articleWord} heute nochmals mit dem Controlling abgestimmt.", $"In the finance process, the {englishMeaning} was coordinated once again with controlling today.", $"در فرایند مالی، امروز {persianMeaning} یک بار دیگر با واحد کنترل مالی هماهنگ شد."),
                new($"{articleWordCapitalized} hilft dem Team, Berichte und Buchungen nachvollziehbar zu halten.", $"The {englishMeaning} helps the team keep reports and postings traceable.", $"{persianMeaning} به تیم کمک می‌کند گزارش‌ها و ثبت‌های مالی را قابل پیگیری نگه دارد."),
            ],
            "supply" =>
            [
                new($"{articleWordCapitalized} ist für den täglichen Ablauf im Lager besonders wichtig.", $"The {englishMeaning} is especially important for the daily workflow in the warehouse.", $"{persianMeaning} برای روند روزانه در انبار اهمیت ویژه‌ای دارد."),
                new($"Im Beschaffungsprozess wurde {articleWord} heute direkt im System aktualisiert.", $"In the procurement process, the {englishMeaning} was updated directly in the system today.", $"در فرایند تأمین، امروز {persianMeaning} مستقیماً در سیستم به‌روز شد."),
                new($"{articleWordCapitalized} spielt bei Lieferung, Bestand und Einkauf eine wichtige Rolle.", $"The {englishMeaning} plays an important role in delivery, inventory, and purchasing.", $"{persianMeaning} در تحویل، موجودی و خرید نقش مهمی دارد."),
            ],
            "software" =>
            [
                new($"{articleWordCapitalized} ist für die Einführung des neuen ERP-Moduls besonders wichtig.", $"The {englishMeaning} is especially important for rolling out the new ERP module.", $"{persianMeaning} برای راه‌اندازی ماژول جدید ERP اهمیت ویژه‌ای دارد."),
                new($"Im technischen Review wurde {articleWord} als kritischer Punkt für die Implementierung genannt.", $"In the technical review, the {englishMeaning} was mentioned as a critical point for the implementation.", $"در بازبینی فنی، {persianMeaning} به عنوان یک نکته حساس برای پیاده‌سازی مطرح شد."),
                new($"{articleWordCapitalized} hilft dem Team, Daten und Prozesse im System sauber abzubilden.", $"The {englishMeaning} helps the team model data and processes cleanly in the system.", $"{persianMeaning} به تیم کمک می‌کند داده‌ها و فرایندها را در سیستم به‌صورت منظم پیاده کند."),
            ],
            "teamwork" =>
            [
                new($"{articleWordCapitalized} ist für die Zusammenarbeit im aktuellen Projekt besonders wichtig.", $"The {englishMeaning} is especially important for collaboration in the current project.", $"{persianMeaning} برای همکاری در پروژه فعلی اهمیت ویژه‌ای دارد."),
                new($"Im Teammeeting wurde {articleWord} als nächster Schwerpunkt festgelegt.", $"In the team meeting, the {englishMeaning} was defined as the next main focus.", $"در جلسه تیم، {persianMeaning} به عنوان تمرکز بعدی مشخص شد."),
                new($"{articleWordCapitalized} erleichtert dem Team die Planung der nächsten Schritte.", $"The {englishMeaning} makes it easier for the team to plan the next steps.", $"{persianMeaning} برنامه‌ریزی مراحل بعدی را برای تیم آسان‌تر می‌کند."),
            ],
            _ =>
            [
                new($"{articleWordCapitalized} ist im Berufsalltag unseres Teams weiterhin sehr wichtig.", $"The {englishMeaning} remains very important in our team's daily work.", $"{persianMeaning} همچنان در کار روزمره تیم ما بسیار مهم است."),
                new($"Im heutigen Arbeitsablauf kam {articleWord} mehrfach zur Sprache.", $"The {englishMeaning} came up several times in today's workflow.", $"در روند کاری امروز، چند بار درباره {persianMeaning} صحبت شد."),
                new($"{articleWordCapitalized} unterstützt das Team dabei, strukturierter zu arbeiten.", $"The {englishMeaning} helps the team work in a more structured way.", $"{persianMeaning} به تیم کمک می‌کند ساختارمندتر کار کند."),
            ],
        };

    private static ExampleTemplate[] BuildVerbTemplates(string verb, string englishMeaning, string persianMeaning, string group) =>
        group switch
        {
            "sales" =>
            [
                new($"Vor dem Kundentermin müssen wir die offenen Punkte im CRM noch {verb}.", $"Before the customer meeting, we still need to {englishMeaning} the open items in the CRM.", $"قبل از جلسه با مشتری، هنوز باید موارد باز را در CRM {persianMeaning}."),
                new($"Das Vertriebsteam will die neuen Anforderungen zuerst intern {verb}, bevor ein Angebot rausgeht.", $"The sales team wants to {englishMeaning} the new requirements internally first before sending a quotation.", $"تیم فروش می‌خواهد قبل از ارسال پیشنهاد قیمت، نیازهای جدید را ابتدا داخلی {persianMeaning}."),
                new($"Im Kundenprojekt müssen wir Informationen klar {verb}, damit alle denselben Stand haben.", $"In the customer project, we need to {englishMeaning} information clearly so that everyone has the same status.", $"در پروژه مشتری باید اطلاعات را شفاف {persianMeaning} تا همه درک مشترکی از وضعیت داشته باشند."),
            ],
            "finance" =>
            [
                new($"Vor dem Monatsabschluss müssen wir die offenen Buchungen sauber {verb}.", $"Before month-end closing, we need to {englishMeaning} the open postings properly.", $"قبل از بستن حساب‌های ماه، باید ثبت‌های باز را به‌درستی {persianMeaning}."),
                new($"Das Controlling verlangt, dass wir jede Abweichung im Bericht nachvollziehbar {verb}.", $"Controlling requires us to {englishMeaning} every deviation in the report in a traceable way.", $"واحد کنترل مالی می‌خواهد هر اختلاف را در گزارش به‌صورت قابل پیگیری {persianMeaning}."),
                new($"Im Finanzprozess sollen wir die Daten erst {verb}, bevor die Freigabe erfolgt.", $"In the finance process, we should {englishMeaning} the data before approval happens.", $"در فرایند مالی باید ابتدا داده‌ها را {persianMeaning} و بعد تأیید انجام شود."),
            ],
            "supply" =>
            [
                new($"Im Lager müssen wir den aktuellen Bestand täglich {verb}.", $"In the warehouse, we need to {englishMeaning} the current stock every day.", $"در انبار باید موجودی فعلی را هر روز {persianMeaning}."),
                new($"Vor der Lieferung soll das Team alle Positionen im System noch einmal {verb}.", $"Before delivery, the team should {englishMeaning} all positions in the system once again.", $"قبل از ارسال، تیم باید همه اقلام را در سیستم یک بار دیگر {persianMeaning}."),
                new($"Im Einkauf müssen wir neue Daten schnell {verb}, damit der Prozess nicht stehen bleibt.", $"In purchasing, we need to {englishMeaning} new data quickly so the process does not stop.", $"در خرید باید داده‌های جدید را سریع {persianMeaning} تا فرایند متوقف نشود."),
            ],
            "software" =>
            [
                new($"Vor dem Deployment müssen wir die Änderungen im neuen Modul noch einmal {verb}.", $"Before deployment, we need to {englishMeaning} the changes in the new module once more.", $"قبل از استقرار، باید تغییرها را در ماژول جدید یک بار دیگر {persianMeaning}."),
                new($"Bei der ERP-Einführung wollen wir die Prozesse so früh wie möglich {verb}.", $"During the ERP rollout, we want to {englishMeaning} the processes as early as possible.", $"در راه‌اندازی ERP می‌خواهیم فرایندها را تا حد ممکن زودتر {persianMeaning}."),
                new($"Im technischen Team müssen wir Fehler schnell {verb}, damit der Kunde weiterarbeiten kann.", $"In the technical team, we need to {englishMeaning} issues quickly so the customer can continue working.", $"در تیم فنی باید خطاها را سریع {persianMeaning} تا مشتری بتواند به کارش ادامه دهد."),
            ],
            "teamwork" =>
            [
                new($"Im Projekt müssen wir die nächsten Schritte gemeinsam {verb}.", $"In the project, we need to {englishMeaning} the next steps together.", $"در پروژه باید مراحل بعدی را با هم {persianMeaning}."),
                new($"Vor dem nächsten Meeting will das Team die offenen Punkte noch einmal {verb}.", $"Before the next meeting, the team wants to {englishMeaning} the open items once more.", $"قبل از جلسه بعدی، تیم می‌خواهد موارد باز را یک بار دیگر {persianMeaning}."),
                new($"Damit der Ablauf stabil bleibt, sollten wir die Verantwortung klar {verb}.", $"To keep the workflow stable, we should {englishMeaning} responsibilities clearly.", $"برای اینکه روند کار پایدار بماند، باید مسئولیت‌ها را شفاف {persianMeaning}."),
            ],
            _ =>
            [
                new($"Im Arbeitsalltag müssen wir solche Aufgaben oft schnell {verb}.", $"In daily work, we often need to {englishMeaning} such tasks quickly.", $"در کار روزمره، اغلب لازم است این نوع کارها را سریع {persianMeaning}."),
                new($"Unser Team sollte diesen Schritt sauber {verb}, bevor wir weitermachen.", $"Our team should {englishMeaning} this step properly before we continue.", $"تیم ما باید این مرحله را درست {persianMeaning} و بعد ادامه بدهیم."),
                new($"Im aktuellen Prozess hilft es, Informationen direkt im System zu {verb}.", $"In the current process, it helps to {englishMeaning} information directly in the system.", $"در فرایند فعلی، مفید است اطلاعات را مستقیماً در سیستم {persianMeaning}."),
            ],
        };

    private static ExampleTemplate[] BuildAdjectiveTemplates(string adjective, string englishMeaning, string persianMeaning, string group) =>
        group switch
        {
            "sales" =>
            [
                new($"Die Antwort an den Kunden muss {adjective} sein, damit es später keine Missverständnisse gibt.", $"The response to the customer must be {englishMeaning} so that there are no misunderstandings later.", $"پاسخ به مشتری باید {persianMeaning} باشد تا بعداً سوءتفاهمی پیش نیاید."),
                new($"Im Vertrieb ist eine {adjective} Kommunikation oft wichtiger als eine schnelle Zusage.", $"In sales, {englishMeaning} communication is often more important than a quick commitment.", $"در فروش، ارتباط {persianMeaning} اغلب از یک قول سریع مهم‌تر است."),
                new($"Für den Kundentermin brauchen wir eine Lösung, die fachlich und sprachlich {adjective} wirkt.", $"For the customer meeting, we need a solution that appears {englishMeaning} both professionally and linguistically.", $"برای جلسه با مشتری، به راه‌حلی نیاز داریم که هم از نظر فنی و هم زبانی {persianMeaning} به نظر برسد."),
            ],
            "finance" =>
            [
                new($"Im Bericht müssen alle Zahlen {adjective} und gut begründet sein.", $"In the report, all figures must be {englishMeaning} and well justified.", $"در گزارش، همه اعداد باید {persianMeaning} و خوب مستند باشند."),
                new($"Für das Audit brauchen wir einen Prozess, der dauerhaft {adjective} bleibt.", $"For the audit, we need a process that remains {englishMeaning} over time.", $"برای ممیزی به فرایندی نیاز داریم که در طول زمان {persianMeaning} بماند."),
                new($"Im Finanzbereich ist eine {adjective} Dokumentation keine Option, sondern Pflicht.", $"In finance, {englishMeaning} documentation is not optional, it is mandatory.", $"در بخش مالی، مستندسازی {persianMeaning} یک انتخاب نیست، بلکه الزام است."),
            ],
            "supply" =>
            [
                new($"Der Ablauf im Lager muss auch unter Zeitdruck {adjective} bleiben.", $"The workflow in the warehouse must remain {englishMeaning} even under time pressure.", $"روند کار در انبار باید حتی زیر فشار زمانی هم {persianMeaning} بماند."),
                new($"Für Einkauf und Lieferung ist eine {adjective} Datenbasis besonders wichtig.", $"For purchasing and delivery, a {englishMeaning} data basis is especially important.", $"برای خرید و تحویل، یک پایه داده {persianMeaning} اهمیت ویژه‌ای دارد."),
                new($"Im Bestandsprozess brauchen wir Schritte, die einfach, aber trotzdem {adjective} sind.", $"In the inventory process, we need steps that are simple, but still {englishMeaning}.", $"در فرایند موجودی، به مراحلی نیاز داریم که ساده اما در عین حال {persianMeaning} باشند."),
            ],
            "software" =>
            [
                new($"Die neue ERP-Lösung muss {adjective} sein, bevor wir sie produktiv einsetzen.", $"The new ERP solution must be {englishMeaning} before we use it in production.", $"راه‌حل جدید ERP باید {persianMeaning} باشد قبل از اینکه آن را در محیط واقعی استفاده کنیم."),
                new($"Für das Entwicklungsteam ist eine {adjective} Architektur langfristig entscheidend.", $"For the development team, a {englishMeaning} architecture is decisive in the long run.", $"برای تیم توسعه، یک معماری {persianMeaning} در بلندمدت تعیین‌کننده است."),
                new($"Im technischen Review zeigte sich, ob die Implementierung wirklich {adjective} genug ist.", $"The technical review showed whether the implementation is really {englishMeaning} enough.", $"در بازبینی فنی مشخص شد که آیا پیاده‌سازی واقعاً به اندازه کافی {persianMeaning} هست یا نه."),
            ],
            "teamwork" =>
            [
                new($"Für die Zusammenarbeit im Team müssen Rollen und Erwartungen {adjective} sein.", $"For teamwork, roles and expectations must be {englishMeaning}.", $"برای همکاری در تیم، نقش‌ها و انتظارها باید {persianMeaning} باشند."),
                new($"Im Projektalltag ist eine {adjective} Abstimmung oft wichtiger als eine perfekte Planung.", $"In day-to-day project work, {englishMeaning} coordination is often more important than perfect planning.", $"در کار روزمره پروژه، هماهنگی {persianMeaning} اغلب از برنامه‌ریزی کاملاً بی‌نقص مهم‌تر است."),
                new($"Damit das Team effizient bleibt, müssen Entscheidungen möglichst {adjective} sein.", $"To keep the team efficient, decisions should be as {englishMeaning} as possible.", $"برای اینکه تیم کارآمد بماند، تصمیم‌ها باید تا حد ممکن {persianMeaning} باشند."),
            ],
            _ =>
            [
                new($"Im Berufsalltag ist eine {adjective} Arbeitsweise für das ganze Team hilfreich.", $"In daily work, a {englishMeaning} way of working is helpful for the whole team.", $"در کار روزمره، یک شیوه کاری {persianMeaning} برای کل تیم مفید است."),
                new($"Der aktuelle Prozess muss {adjective} genug sein, damit neue Kollegen schnell einsteigen können.", $"The current process must be {englishMeaning} enough so that new colleagues can get started quickly.", $"فرایند فعلی باید به اندازه کافی {persianMeaning} باشد تا همکاران جدید سریع وارد کار شوند."),
                new($"In unserem Umfeld gilt eine {adjective} Lösung meist als die bessere Wahl.", $"In our environment, a {englishMeaning} solution is usually considered the better choice.", $"در محیط ما، معمولاً یک راه‌حل {persianMeaning} انتخاب بهتری محسوب می‌شود."),
            ],
        };

    private sealed record ExampleTemplate(string German, string English, string Persian);
}
