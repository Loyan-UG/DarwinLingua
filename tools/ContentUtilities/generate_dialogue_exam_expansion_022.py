import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_021 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-022-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("complaint-cleaning-service", "Eine Reinigungsleistung reklamieren", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-wrong-contract", "Einen falsch gebuchten Vertrag klären", "shopping", "complaint", "explain-problem", "phone", "formal"),
    ("complaint-course-refund", "Eine Kursgebühr zurückfordern", "everyday-life", "complaint", "negotiate-solution", "phone", "formal"),
    ("complaint-delayed-repair", "Eine verzögerte Reparatur nachverfolgen", "shopping", "complaint", "ask-for-information", "phone", "formal"),
    ("conflict-childcare-plan", "Unterschiedliche Betreuungszeiten klären", "everyday-life", "conflict-resolution", "negotiate-solution", "face-to-face", "formal"),
    ("conflict-work-breaks", "Pausenzeiten im Team fair abstimmen", "work-and-jobs", "conflict-resolution", "negotiate-solution", "workplace", "neutral"),
    ("conflict-neighbor-storage", "Abstellflächen im Hausflur besprechen", "housing", "conflict-resolution", "complain-politely", "face-to-face", "mixed"),
    ("conflict-group-project", "Aufgaben in einem Gruppenprojekt neu verteilen", "everyday-life", "conflict-resolution", "discuss-plan", "group-work", "neutral"),
    ("school-kindergarten-late-pickup", "Eine verspätete Abholung in der Kita erklären", "everyday-life", "school-kindergarten", "explain-problem", "phone", "formal"),
    ("school-kindergarten-learning-talk", "Ein Entwicklungsgespräch vorbereiten", "everyday-life", "school-kindergarten", "make-appointment", "phone", "formal"),
    ("school-kindergarten-activity-choice", "Aktivitäten für ein Kind vergleichen", "everyday-life", "school-kindergarten", "compare-options", "face-to-face", "formal"),
    ("school-kindergarten-rule-question", "Eine Schul- oder Kitaregel nachfragen", "everyday-life", "school-kindergarten", "ask-for-information", "face-to-face", "formal"),
    ("finance-loan-question", "Eine Kreditrate bei der Bank besprechen", "shopping", "finance-insurance", "ask-for-information", "phone", "formal"),
    ("finance-payment-error", "Einen Zahlungsfehler erklären", "shopping", "finance-insurance", "explain-problem", "phone", "formal"),
    ("finance-insurance-deductible", "Eine Selbstbeteiligung bei der Versicherung klären", "shopping", "finance-insurance", "ask-for-information", "phone", "formal"),
    ("finance-budget-meeting", "Ein kleines Budget im Team abstimmen", "work-and-jobs", "finance-insurance", "discuss-plan", "workplace", "formal"),
    ("job-interview-remote-work", "Homeoffice im Vorstellungsgespräch besprechen", "work-and-jobs", "job-interview", "negotiate-solution", "face-to-face", "formal"),
    ("job-interview-start-date", "Einen möglichen Arbeitsbeginn klären", "work-and-jobs", "job-interview", "ask-for-information", "face-to-face", "formal"),
    ("job-interview-conflict-example", "Ein Konfliktbeispiel im Bewerbungsgespräch erklären", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("job-interview-career-goal", "Ein berufliches Ziel im Gespräch darstellen", "work-and-jobs", "job-interview", "give-opinion", "face-to-face", "formal"),
    ("shopping-repair-choice", "Reparatur oder Neukauf vergleichen", "shopping", "shopping-and-services", "compare-options", "service-counter", "formal"),
    ("shopping-payment-method", "Eine passende Zahlungsart auswählen", "shopping", "shopping-and-services", "compare-options", "service-counter", "formal"),
    ("shopping-member-discount", "Nach einem Kundenrabatt fragen", "shopping", "shopping-and-services", "ask-for-information", "service-counter", "formal"),
    ("shopping-appointment-service", "Einen Servicetermin im Geschäft vereinbaren", "shopping", "shopping-and-services", "make-appointment", "phone", "formal"),
    ("digital-app-permission", "App-Berechtigungen erklären lassen", "work-and-jobs", "digital-services", "ask-for-information", "phone", "formal"),
    ("digital-account-delete", "Ein Onlinekonto löschen lassen", "shopping", "digital-services", "ask-for-help", "phone", "formal"),
    ("digital-video-problem", "Ein Tonproblem im Videoanruf lösen", "work-and-jobs", "digital-services", "explain-problem", "video-call", "neutral"),
    ("digital-data-export", "Daten aus einem Onlineportal exportieren", "work-and-jobs", "digital-services", "ask-for-help", "video-call", "formal"),
    ("exam-complaint-service", "Eine Servicebeschwerde in der Prüfung üben", "shopping", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-conflict-solution", "Eine Konfliktlösung in der Prüfung diskutieren", "everyday-life", "exam-preparation", "exam-discussion", "exam-room", "formal"),
]


TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "job-interview": ["greet", "introduce", "answer-question", "explain", "justify", "ask-question", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2100, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-022-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice explaining the background, asking precise questions, negotiating politely, and confirming the result."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, complaints, workplace communication, and practical problem solving."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace", "job-interview"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if task == "negotiate-solution":
        skill_focus.append("negotiation")
    if category == "exam-preparation" or task in {"give-opinion", "exam-discussion"}:
        skill_focus.append("discussion")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 440000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-022-b1-b2",
        "packageName": "Dialogue Exam Expansion 022 B1 B2",
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
