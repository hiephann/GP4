import os
import re

pages_dir = r"d:\FU Learning\Summer2026\SWD392\EduNexus\GP4\G5_GP1\src\EduNexus.Web\Components\Pages"

replacements = {
    r'\bclass="card"\b': 'class="lms-card"',
    r'\bclass="card-body"\b': 'class="p-4"',
    r'\bclass="card-header"\b': 'class="lms-section-header pb-0 border-0 pt-3"',
    r'\bbtn-primary\b': 'lms-btn-primary',
    r'\bbtn-secondary\b': 'lms-btn-secondary',
    r'\bbtn-danger\b': 'lms-btn-danger',
    r'\bbtn-success\b': 'lms-btn-success',
    r'\bclass="form-control"': 'class="form-control lms-input"',
    r'\bclass="form-select"': 'class="form-select lms-input"',
    r'\bclass="table\b': 'class="table lms-table',
    r'\bclass="page-header"\b': 'class="page-header mb-4"',
}

def refactor_file(filepath):
    # skip Assignment folder as it's already done perfectly
    if "Assignment" in filepath:
        return
        
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
        
    original = content
    for pattern, replacement in replacements.items():
        content = re.sub(pattern, replacement, content)
        
    if content != original:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Refactored: {os.path.basename(filepath)}")

for root, _, files in os.walk(pages_dir):
    for file in files:
        if file.endswith(".razor"):
            refactor_file(os.path.join(root, file))

print("Refactoring complete.")
