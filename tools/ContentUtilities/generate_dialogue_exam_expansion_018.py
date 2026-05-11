import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_017 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-018-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("finance-installment-change", "Eine Ratenzahlung neu vereinbaren", "shopping", "finance-insurance", "negotiate-solution", "phone", "formal"),
    ("finance-card-blocked", "Eine gesperrte Bankkarte klären", "shopping", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("finance-tax-advice", "Einen einfachen Steuertermin vorbereiten", "work-and-jobs", "finance-insurance", "make-appointment", "phone", "formal"),
    ("finance-insurance-cancel", "Eine Versicherung kündigen und Alternativen vergleichen", "shopping", "finance-insurance", "compare-options", "phone", "formal"),
    ("school-kindergarten-sick-child", "Ein krankes Kind in der Kita abmelden", "appointments-and-health", "school-kindergarten", "explain-problem", "phone", "formal"),
    ("school-kindergarten-meal", "Essensregeln in der Kita besprechen", "everyday-life", "school-kindergarten", "ask-for-information", "face-to-face", "formal"),
    ("school-kindergarten-conflict", "Einen Konflikt zwischen Kindern klären", "everyday-life", "school-kindergarten", "negotiate-solution", "face-to-face", "formal"),
    ("school-kindergarten-parent-evening", "Einen Elternabend vorbereiten", "everyday-life", "school-kindergarten", "discuss-plan", "school-kindergarten", "formal"),
    ("education-certificate-recognition", "Eine Anerkennung von Zeugnissen besprechen", "work-and-jobs", "education", "ask-for-information", "service-counter", "formal"),
    ("education-course-complaint", "Ein Problem mit einem Kurs sachlich ansprechen", "everyday-life", "education", "complain-politely", "face-to-face", "formal"),
    ("education-presentation-plan", "Eine Präsentation im Kurs planen", "everyday-life", "education", "discuss-plan", "classroom", "neutral"),
    ("education-exam-feedback", "Feedback nach einer Prüfung verstehen", "everyday-life", "education", "ask-for-information", "face-to-face", "formal"),
    ("city-dog-registration", "Einen Hund bei der Stadt anmelden", "everyday-life", "city-services", "ask-for-information", "government-office", "formal"),
    ("city-noise-permit", "Eine Genehmigung für eine Veranstaltung klären", "everyday-life", "city-services", "ask-for-information", "phone", "formal"),
    ("city-public-pool", "Regeln im städtischen Schwimmbad erfragen", "everyday-life", "city-services", "ask-for-information", "service-counter", "neutral"),
    ("city-roadwork-complaint", "Eine Baustelle in der Straße melden", "housing", "city-services", "complain-politely", "phone", "formal"),
    ("job-interview-salary", "Gehalt und Arbeitsbedingungen besprechen", "work-and-jobs", "job-interview", "negotiate-solution", "face-to-face", "formal"),
    ("job-interview-team-fit", "Über Zusammenarbeit im Team sprechen", "work-and-jobs", "job-interview", "give-opinion", "face-to-face", "formal"),
    ("job-interview-experience", "Berufserfahrung konkret beschreiben", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("job-interview-questions", "Eigene Fragen am Ende des Gesprächs stellen", "work-and-jobs", "job-interview", "ask-for-information", "face-to-face", "formal"),
    ("digital-two-factor", "Zwei-Faktor-Anmeldung einrichten", "work-and-jobs", "digital-services", "ask-for-help", "phone", "formal"),
    ("digital-cloud-file", "Eine Datei in der Cloud teilen", "work-and-jobs", "digital-services", "explain-problem", "video-call", "neutral"),
    ("digital-online-form", "Ein Onlineformular gemeinsam ausfüllen", "everyday-life", "digital-services", "ask-for-help", "video-call", "formal"),
    ("digital-subscription", "Ein digitales Abo kündigen", "shopping", "digital-services", "complain-politely", "phone", "formal"),
    ("conflict-flatshare-costs", "Kosten in einer Wohngemeinschaft aufteilen", "housing", "conflict-resolution", "negotiate-solution", "face-to-face", "mixed"),
    ("conflict-work-feedback", "Kritik im Team höflich ansprechen", "work-and-jobs", "conflict-resolution", "give-opinion", "workplace", "formal"),
    ("conflict-school-parent", "Ein Missverständnis mit der Schule klären", "everyday-life", "conflict-resolution", "handle-misunderstanding", "school-kindergarten", "formal"),
    ("conflict-service-delay", "Eine verspätete Dienstleistung klären", "shopping", "conflict-resolution", "negotiate-solution", "phone", "formal"),
    ("exam-finance-roleplay", "Eine Bank- oder Versicherungssituation in der Prüfung üben", "shopping", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-digital-services", "Über digitale Dienste in der Prüfung diskutieren", "work-and-jobs", "exam-preparation", "exam-discussion", "exam-room", "formal"),
]


TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "handle-misunderstanding": ["greet", "clarify", "correct", "explain", "apologize", "confirm", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1700, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-018-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice asking targeted questions, explaining reasons, reacting politely, and confirming the agreed solution."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, formal questions, conflict resolution, and practical service communication."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"workplace", "job-interview"} or mode in {"workplace", "video-call"}:
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
    sort_order = 360000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-018-b1-b2",
        "packageName": "Dialogue Exam Expansion 018 B1 B2",
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
