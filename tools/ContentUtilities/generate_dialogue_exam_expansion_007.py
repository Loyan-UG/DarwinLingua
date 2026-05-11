import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-007-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("city-noise", "Lärm bei der Stadt melden", "everyday-life", "city-services", "complain-politely", "phone", "formal"),
    ("street-light", "Eine kaputte Straßenlaterne melden", "everyday-life", "city-services", "explain-problem", "phone", "formal"),
    ("park-permit", "Eine Genehmigung für ein Fest im Park erfragen", "everyday-life", "city-services", "ask-for-information", "government-office", "formal"),
    ("course-certificate", "Ein Zertifikat vom Kursbüro bekommen", "everyday-life", "education", "ask-for-information", "service-counter", "formal"),
    ("training-schedule", "Einen Trainingsplan für die Weiterbildung abstimmen", "work-and-jobs", "education", "discuss-plan", "workplace", "formal"),
    ("internship-task", "Aufgaben im Praktikum klären", "work-and-jobs", "education", "ask-for-information", "workplace", "neutral"),
    ("exam-course-feedback", "Feedback zum Prüfungskurs geben", "everyday-life", "education", "give-opinion", "classroom", "neutral"),
    ("work-contract-question", "Eine Frage zum Arbeitsvertrag stellen", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("work-break-rule", "Pausenregeln am Arbeitsplatz klären", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("work-transfer", "Über einen Wechsel in eine andere Abteilung sprechen", "work-and-jobs", "work", "compare-options", "workplace", "formal"),
    ("work-warning", "Eine Abmahnung sachlich besprechen", "work-and-jobs", "work", "explain-problem", "workplace", "formal"),
    ("complaint-delivery", "Eine falsche Lieferung reklamieren", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-service", "Schlechten Service ruhig ansprechen", "shopping", "complaint", "complain-politely", "service-counter", "formal"),
    ("complaint-contract", "Eine Vertragsänderung beanstanden", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-repair-delay", "Eine verspätete Reparatur beanstanden", "housing", "complaint", "complain-politely", "phone", "formal"),
    ("conflict-roommate", "Einen Konflikt in der Wohngemeinschaft lösen", "housing", "conflict-resolution", "negotiate-solution", "face-to-face", "neutral"),
    ("conflict-colleague", "Ein Problem mit einem Kollegen klären", "work-and-jobs", "conflict-resolution", "handle-misunderstanding", "workplace", "neutral"),
    ("conflict-course", "Eine Meinungsverschiedenheit im Kurs lösen", "everyday-life", "conflict-resolution", "agree-disagree", "classroom", "neutral"),
    ("conflict-family", "Unterschiedliche Wünsche in der Familie besprechen", "everyday-life", "conflict-resolution", "negotiate-solution", "face-to-face", "mixed"),
    ("phone-landlord", "Den Vermieter telefonisch um Hilfe bitten", "housing", "housing", "ask-for-help", "phone", "formal"),
    ("phone-office", "Beim Amt telefonisch nach dem Stand fragen", "everyday-life", "government-office", "ask-for-information", "phone", "formal"),
    ("phone-school", "In der Schule telefonisch etwas nachfragen", "everyday-life", "school-kindergarten", "ask-for-information", "phone", "formal"),
    ("phone-customer", "Einen Kunden telefonisch informieren", "work-and-jobs", "workplace", "explain-problem", "phone", "formal"),
    ("exam-phone-call", "Eine Telefonaufgabe in der Prüfung üben", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-polite-complaint", "Eine höfliche Beschwerde in der Prüfung formulieren", "everyday-life", "exam-preparation", "complain-politely", "exam-room", "formal"),
    ("exam-suggestion", "In der Prüfung einen Vorschlag machen", "everyday-life", "exam-preparation", "make-suggestion", "exam-room", "formal"),
    ("exam-refusal", "In der Prüfung höflich ablehnen", "everyday-life", "exam-preparation", "refuse-politely", "exam-room", "formal"),
    ("digital-login", "Ein Login-Problem beim Support erklären", "everyday-life", "digital-services", "explain-problem", "phone", "formal"),
    ("digital-data-export", "Einen Datenexport anfordern", "work-and-jobs", "digital-services", "ask-for-information", "workplace", "formal"),
    ("digital-video-class", "Technische Probleme im Online-Unterricht klären", "everyday-life", "digital-services", "ask-for-help", "video-call", "neutral"),
]

TASK_TO_FUNCTIONS = {
    "complain-politely": ["greet", "explain", "complain", "request", "negotiate", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "give-opinion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "negotiate-solution": ["greet", "explain", "suggest", "compare", "negotiate", "summarize", "close-conversation"],
    "handle-misunderstanding": ["greet", "clarify", "correct", "apologize", "confirm", "summarize", "close-conversation"],
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
    "refuse-politely": ["greet", "explain", "apologize", "request", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 600, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-007-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"German B1/B2 dialogue for {title.lower()} with practical and exam-oriented speaking tasks."
    dialogue["learnerGoal"] = "Practice asking clearly, explaining reasons, reacting politely, and confirming the result."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a realistic task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 roleplay, polite complaint tasks, workplace speaking, and formal phone calls."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"workplace", "work"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if category == "complaint" or task == "complain-politely":
        skill_focus.append("complaint-handling")
    if category == "conflict-resolution" or task == "negotiate-solution":
        skill_focus.append("negotiation")
    if task in {"give-opinion", "agree-disagree"} or category == "exam-preparation":
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 140000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-007-b1-b2",
        "packageName": "Dialogue Exam Expansion 007 B1 B2",
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
