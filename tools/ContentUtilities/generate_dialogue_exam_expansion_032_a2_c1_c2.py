import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_025_a2_c1_c2 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-032-a2-c1-c2.json"

A2_SITUATIONS = [
    ("city-fee-question", "Bei der Stadt nach einer Gebühr fragen", "everyday-life", "government-office", "ask-for-information", "service-counter", "formal"),
    ("doctor-medicine-time", "Die Einnahme eines Medikaments klären", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("travel-luggage-help", "Hilfe mit Gepäck am Bahnhof bekommen", "everyday-life", "transport-and-travel", "ask-for-help", "face-to-face", "neutral"),
    ("course-certificate", "Nach einer Kursbescheinigung fragen", "everyday-life", "integration-course", "ask-for-information", "classroom", "formal"),
    ("apartment-window", "Ein kaputtes Fenster melden", "housing", "housing", "explain-problem", "phone", "formal"),
    ("school-lunch", "Das Mittagessen in der Schule klären", "everyday-life", "school-kindergarten", "ask-for-information", "phone", "formal"),
    ("app-login-help", "Hilfe beim App-Login bekommen", "shopping", "digital-services", "ask-for-help", "phone", "formal"),
    ("restaurant-table-problem", "Ein Problem mit dem Tisch im Restaurant erklären", "shopping", "food-restaurant", "complain-politely", "face-to-face", "formal"),
    ("bank-card-limit", "Ein Kartenlimit bei der Bank erfragen", "shopping", "finance-insurance", "ask-for-information", "service-counter", "formal"),
    ("friend-walk-plan", "Einen Spaziergang mit Freunden planen", "everyday-life", "social-life", "discuss-plan", "phone", "informal"),
]

C1_SITUATIONS = [
    ("work-delegation-trust", "Delegation und Vertrauen im Team verhandeln", "work-and-jobs", "workplace", "negotiate-solution", "workplace", "formal"),
    ("digital-consent-design", "Einwilligung und Nutzerführung bei digitalen Diensten bewerten", "everyday-life", "digital-services", "compare-options", "group-work", "formal"),
    ("health-treatment-priority", "Therapieziele und Prioritäten im Gespräch abwägen", "appointments-and-health", "healthcare", "compare-options", "doctor-office", "formal"),
    ("school-parent-participation", "Elternbeteiligung an der Schule differenziert besprechen", "everyday-life", "school-kindergarten", "exam-discussion", "group-work", "formal"),
    ("housing-rent-increase", "Eine Mieterhöhung sachlich hinterfragen", "housing", "housing", "complain-politely", "face-to-face", "formal"),
    ("government-digital-proof", "Digitale Nachweise bei Behörden präzise erklären", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("work-meeting-fatigue", "Meetingkultur und Konzentration im Team verbessern", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("job-interview-change", "Motivation für einen Berufswechsel erklären", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("insurance-cost-benefit", "Kosten und Nutzen einer Versicherung abwägen", "shopping", "finance-insurance", "compare-options", "phone", "formal"),
    ("media-reputation-risk", "Reputationsrisiken in sozialen Medien diskutieren", "everyday-life", "digital-services", "give-opinion", "group-work", "formal"),
    ("education-hybrid-course", "Hybride Kursformate kritisch vergleichen", "work-and-jobs", "education", "compare-options", "classroom", "formal"),
    ("integration-language-cafe", "Ein Sprachcafé als Integrationsangebot planen", "everyday-life", "integration-course", "discuss-plan", "group-work", "formal"),
    ("complaint-delivery-delay", "Eine Lieferverzögerung mit Folgen verhandeln", "shopping", "complaint", "negotiate-solution", "phone", "formal"),
    ("project-stakeholder-expectations", "Erwartungen verschiedener Stakeholder ausgleichen", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("work-sick-leave-policy", "Krankschreibung und Teamverantwortung besprechen", "work-and-jobs", "healthcare", "agree-disagree", "workplace", "formal"),
]

C2_SITUATIONS = [
    ("ai-opacity-power", "Intransparenz und Macht bei KI-Systemen analysieren", "work-and-jobs", "digital-services", "exam-discussion", "group-work", "formal"),
    ("health-responsibility-narratives", "Verantwortungsnarrative in der Gesundheitspolitik hinterfragen", "appointments-and-health", "healthcare", "give-opinion", "group-work", "formal"),
    ("education-standardization", "Standardisierung und individuelle Bildung kritisch abwägen", "work-and-jobs", "education", "compare-options", "classroom", "formal"),
    ("housing-commons", "Gemeingüterlogik und Wohnraumversorgung diskutieren", "housing", "housing", "exam-discussion", "group-work", "formal"),
    ("administration-trust-data", "Datenbasierte Verwaltung und Vertrauen nuanciert bewerten", "everyday-life", "government-office", "exam-discussion", "group-work", "formal"),
    ("work-control-autonomy", "Kontrolle und Autonomie in Organisationen kontrovers diskutieren", "work-and-jobs", "workplace", "compare-options", "group-work", "formal"),
]


def situations_for_level(level: str):
    if level == "A2":
        return A2_SITUATIONS
    if level == "C1":
        return C1_SITUATIONS
    return C2_SITUATIONS


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = situations_for_level(level)[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 3200, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-032-{key}-{variant:02d}"
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
    sort_order = 710000
    level_variants = {"A2": 6, "C1": 8, "C2": 6}
    for level, variants in level_variants.items():
        situations = situations_for_level(level)
        for variant in range(1, variants + 1):
            for situation_index in range(len(situations)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-032-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 032 A2 C1 C2",
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
