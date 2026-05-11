import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_025_a2_c1_c2 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-026-a2-c1-c2.json"

A2_SITUATIONS = [
    ("city-form-help", "Hilfe mit einem Formular bekommen", "everyday-life", "city-services", "ask-for-help", "government-office", "formal"),
    ("doctor-simple-symptom", "Ein einfaches Symptom erklären", "appointments-and-health", "healthcare", "explain-problem", "doctor-office", "formal"),
    ("school-late-call", "Eine Verspätung bei der Schule melden", "everyday-life", "school-kindergarten", "explain-problem", "phone", "formal"),
    ("train-platform", "Nach dem richtigen Gleis fragen", "everyday-life", "transport-and-travel", "ask-for-information", "service-counter", "neutral"),
    ("shop-warranty", "Nach Garantie im Geschäft fragen", "shopping", "shopping-and-services", "ask-for-information", "service-counter", "formal"),
    ("course-homework", "Hausaufgaben im Sprachkurs klären", "everyday-life", "integration-course", "ask-for-information", "classroom", "neutral"),
    ("family-weekend", "Ein Wochenende mit der Familie planen", "everyday-life", "family", "discuss-plan", "phone", "informal"),
    ("restaurant-bill", "Im Restaurant die Rechnung klären", "shopping", "food-restaurant", "ask-for-information", "face-to-face", "formal"),
    ("bank-pin", "Eine neue PIN bei der Bank anfragen", "shopping", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("neighbor-package", "Ein Paket beim Nachbarn abholen", "housing", "social-life", "ask-for-help", "face-to-face", "neutral"),
]

C1_SITUATIONS = [
    ("work-leadership-feedback", "Führungsfeedback differenziert besprechen", "work-and-jobs", "workplace", "give-opinion", "workplace", "formal"),
    ("digital-ai-risk", "KI-Risiken im Arbeitsprozess abwägen", "work-and-jobs", "digital-services", "compare-options", "video-call", "formal"),
    ("health-prevention-policy", "Prävention und Verantwortung im Gesundheitswesen diskutieren", "appointments-and-health", "healthcare", "exam-discussion", "group-work", "formal"),
    ("housing-gentrification", "Gentrifizierung und soziale Verantwortung besprechen", "housing", "housing", "exam-discussion", "group-work", "formal"),
    ("education-assessment", "Prüfungsformen kritisch vergleichen", "work-and-jobs", "education", "compare-options", "classroom", "formal"),
    ("government-digitalization", "Digitalisierung der Verwaltung kritisch bewerten", "everyday-life", "government-office", "give-opinion", "group-work", "formal"),
    ("conflict-stakeholders", "Mehrere Interessen in einem Konflikt vermitteln", "work-and-jobs", "conflict-resolution", "negotiate-solution", "workplace", "formal"),
    ("finance-investment-priority", "Investitionen und Risiken argumentativ abwägen", "work-and-jobs", "finance-insurance", "compare-options", "video-call", "formal"),
    ("academic-method", "Eine Forschungsmethode begründen", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("public-complaint", "Eine öffentliche Beschwerde deeskalieren", "everyday-life", "complaint", "negotiate-solution", "group-work", "formal"),
    ("job-leadership-style", "Den eigenen Führungsstil im Interview erklären", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("work-disagreement", "Fachlichen Widerspruch konstruktiv äußern", "work-and-jobs", "workplace", "agree-disagree", "workplace", "formal"),
    ("service-contract-escalation", "Eine Vertragseskalation professionell verhandeln", "shopping", "complaint", "negotiate-solution", "phone", "formal"),
    ("research-team-plan", "Rollen in einem Forschungsprojekt klären", "work-and-jobs", "education", "discuss-plan", "classroom", "formal"),
    ("media-integration", "Mediennutzung und Integration diskutieren", "everyday-life", "integration-course", "exam-discussion", "group-work", "formal"),
]

C2_SITUATIONS = [
    ("ethics-algorithmic-control", "Algorithmische Kontrolle und Autonomie debattieren", "work-and-jobs", "digital-services", "exam-discussion", "group-work", "formal"),
    ("democracy-administration", "Demokratische Legitimation in Verwaltungsprozessen diskutieren", "everyday-life", "government-office", "exam-discussion", "group-work", "formal"),
    ("medicine-resource-ethics", "Ressourcenethik in der Medizin nuanciert abwägen", "appointments-and-health", "healthcare", "compare-options", "group-work", "formal"),
    ("urban-density", "Urbane Verdichtung und Lebensqualität debattieren", "housing", "housing", "exam-discussion", "group-work", "formal"),
    ("language-inequality", "Sprachliche Ungleichheit und Teilhabe analysieren", "everyday-life", "education", "exam-discussion", "group-work", "formal"),
    ("work-meritocracy", "Leistungsideale in der Arbeitswelt kritisch beleuchten", "work-and-jobs", "workplace", "give-opinion", "group-work", "formal"),
]


def situations_for_level(level: str):
    if level == "A2":
        return A2_SITUATIONS
    if level == "C1":
        return C1_SITUATIONS
    return C2_SITUATIONS


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = situations_for_level(level)[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2600, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-026-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"{level} German dialogue for {title.lower()} with practical and exam-oriented speaking practice."
    dialogue["learnerGoal"] = "Practice level-appropriate speaking with reasons, clarification, comparison, and a final summary."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for: {title}."
    dialogue["examRelevance"] = "Useful for oral exam roleplay, structured discussion, and practical German conversation."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace", "job-interview"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if task == "negotiate-solution":
        skill_focus.append("negotiation")
    if task in {"give-opinion", "exam-discussion", "agree-disagree", "compare-options"}:
        skill_focus.append("discussion")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 530000
    level_variants = {"A2": 6, "C1": 8, "C2": 6}
    for level, variants in level_variants.items():
        situations = situations_for_level(level)
        for variant in range(1, variants + 1):
            for situation_index in range(len(situations)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-026-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 026 A2 C1 C2",
        "packageVersion": "1.0",
        "generatedAtUtc": "2026-05-10T00:00:00Z",
        "entries": [],
        "dialogues": dialogues,
    }

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    OUT_FILE.write_text(json.dumps(package, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Wrote {len(dialogues)} dialogues to {OUT_FILE}")


if __name__ == "__main__":
    main()
