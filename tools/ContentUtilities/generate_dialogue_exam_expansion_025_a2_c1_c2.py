import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_024 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-025-a2-c1-c2.json"

DIALOGUE_SITUATIONS = [
    ("a2-city-appointment", "Einen Termin bei der Stadt machen", "everyday-life", "city-services", "make-appointment", "phone", "formal"),
    ("a2-pharmacy-question", "In der Apotheke nachfragen", "appointments-and-health", "healthcare", "ask-for-information", "service-counter", "formal"),
    ("a2-school-call", "Bei der Schule anrufen", "everyday-life", "school-kindergarten", "ask-for-information", "phone", "formal"),
    ("a2-bus-ticket", "Ein Busticket kaufen", "everyday-life", "transport-and-travel", "ask-for-information", "service-counter", "neutral"),
    ("a2-shop-return", "Etwas im Geschäft zurückgeben", "shopping", "shopping-and-services", "complain-politely", "service-counter", "formal"),
    ("a2-course-absence", "Im Kurs eine Abwesenheit erklären", "everyday-life", "integration-course", "explain-problem", "phone", "formal"),
    ("c1-work-policy", "Eine neue Arbeitsregel kritisch diskutieren", "work-and-jobs", "workplace", "exam-discussion", "workplace", "formal"),
    ("c1-data-privacy", "Datenschutz in einem Projekt abwägen", "work-and-jobs", "digital-services", "compare-options", "video-call", "formal"),
    ("c1-health-system", "Über Prioritäten im Gesundheitssystem sprechen", "appointments-and-health", "healthcare", "give-opinion", "group-work", "formal"),
    ("c1-housing-policy", "Wohnkosten und soziale Folgen diskutieren", "housing", "housing", "exam-discussion", "group-work", "formal"),
    ("c1-education-reform", "Eine Bildungsreform argumentativ bewerten", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("c1-migration-service", "Verwaltung und Integration differenziert besprechen", "everyday-life", "government-office", "exam-discussion", "government-office", "formal"),
    ("c1-conflict-mediation", "Eine komplexe Mediation im Team führen", "work-and-jobs", "conflict-resolution", "negotiate-solution", "workplace", "formal"),
    ("c1-budget-priority", "Budgetprioritäten begründet verhandeln", "work-and-jobs", "finance-insurance", "negotiate-solution", "video-call", "formal"),
    ("c1-academic-feedback", "Akademisches Feedback präzise besprechen", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("c1-public-complaint", "Eine öffentliche Beschwerde sachlich moderieren", "everyday-life", "complaint", "complain-politely", "group-work", "formal"),
    ("c2-ethics-automation", "Automatisierung und Verantwortung kontrovers debattieren", "work-and-jobs", "digital-services", "exam-discussion", "group-work", "formal"),
    ("c2-democratic-trust", "Vertrauen in Institutionen nuanciert diskutieren", "everyday-life", "government-office", "exam-discussion", "group-work", "formal"),
    ("c2-medical-prioritization", "Medizinische Priorisierung auf hohem Niveau abwägen", "appointments-and-health", "healthcare", "compare-options", "group-work", "formal"),
    ("c2-urban-future", "Die Zukunft urbanen Wohnens debattieren", "housing", "housing", "exam-discussion", "group-work", "formal"),
    ("c1-job-interview-leadership", "Führungserfahrung im Bewerbungsgespräch reflektieren", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("c1-workplace-disagreement", "Einen fachlichen Widerspruch diplomatisch formulieren", "work-and-jobs", "workplace", "agree-disagree", "workplace", "formal"),
    ("c1-service-escalation", "Eine Service-Eskalation professionell lösen", "shopping", "complaint", "negotiate-solution", "phone", "formal"),
    ("c1-research-planning", "Ein kleines Forschungsprojekt planen", "work-and-jobs", "education", "discuss-plan", "classroom", "formal"),
    ("a2-family-plan", "Einen Familienbesuch planen", "everyday-life", "family", "discuss-plan", "phone", "informal"),
    ("a2-restaurant-table", "Einen Tisch im Restaurant reservieren", "everyday-life", "food-restaurant", "make-appointment", "phone", "formal"),
    ("a2-bank-card", "Nach einer Bankkarte fragen", "shopping", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("a2-neighbor-help", "Nachbarn um Hilfe bitten", "housing", "social-life", "ask-for-help", "face-to-face", "neutral"),
    ("c2-language-power", "Sprache, Macht und soziale Teilhabe diskutieren", "everyday-life", "education", "exam-discussion", "group-work", "formal"),
    ("c2-work-culture", "Arbeitskultur und Leistungsideale kritisch beleuchten", "work-and-jobs", "workplace", "give-opinion", "group-work", "formal"),
]

TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
    "job-interview": ["greet", "introduce", "answer-question", "explain", "justify", "ask-question", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2400, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-025-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"{level} German dialogue for {title.lower()} with practical, exam-oriented speaking practice."
    dialogue["learnerGoal"] = "Practice level-appropriate German speaking with clear reasons, follow-up questions, and a final summary."
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
    if category == "exam-preparation" or task in {"give-opinion", "exam-discussion", "agree-disagree", "compare-options"}:
        skill_focus.append("discussion")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 500000
    level_variants = {"A2": 2, "C1": 4, "C2": 2}
    for level, variants in level_variants.items():
        for variant in range(1, variants + 1):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-025-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 025 A2 C1 C2",
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
