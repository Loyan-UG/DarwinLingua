# Content Strategy

## Content Philosophy

The content should grow gradually over time. The system must support continuous content expansion instead of requiring a complete dataset from the beginning.

The product should allow structured content production from:

- manual authoring
- AI-assisted generation
- semi-automated editorial review
- bulk import files

---

## Content Sources

### Manual Content

Human-created and reviewed content with high quality.

### AI-Assisted Content

AI-generated content prepared in a strict structured format and then imported into the system after manual review.

### Hybrid Content

AI generates the first draft and a human corrects or improves the result before import.

---

## Content Quality Principles

- meaning must be accurate
- example sentences must be natural
- examples should match the intended CEFR level
- translations should not be machine-noisy
- topic tags must be relevant
- duplicate entries should be rejected or flagged
- high-value metadata should be optional but supported
- content generation must follow the active module contract and the recurring lessons in `84-Content-Generation-Lessons-Learned.md`

---

## Content Production Workflow

### Step 1

Generate content in a predefined structured file format.

### Step 2

Review the content manually.

### Step 3

Provide the file to the import utility.

### Step 4

Validate the structure and required fields.

### Step 5

Detect duplicates.

### Step 6

Insert valid records into the database.

### Step 7

Create an import report.

---

## Duplicate Handling Rules

The first versions of the import tool should be intentionally simple.

Expected behavior:

- if a duplicate word is detected, do not import that word
- log a warning
- continue processing the remaining records

Later versions may support smarter duplicate strategies, but the first version should remain conservative and predictable.
