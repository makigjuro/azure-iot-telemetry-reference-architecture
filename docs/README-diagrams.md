# Architecture Diagrams Guide

This project includes multiple diagram formats for different use cases.

## Available Diagram Formats

### 1. Mermaid Diagrams (Markdown-native)
**Files**: `infrastructure-diagram.mmd`, `infrastructure-diagram.md`

✅ **Pros:**
- Renders directly on GitHub/Azure DevOps
- Version control friendly (text-based)
- No build step required
- Easy to maintain

❌ **Cons:**
- No official Azure icons (uses boxes and text)
- Limited styling options

**Use for**: Documentation, PRs, quick reviews

---

### 2. PlantUML C4 Diagrams (System Context)
**Files**: `architecture-c1-diagram.puml`

✅ **Pros:**
- Industry-standard C4 model
- Good for high-level architecture
- Renders via PlantUML proxy

❌ **Cons:**
- No Azure-specific icons
- Requires external rendering

**Use for**: Architecture decision records, presentations

---

### 3. Python Diagrams with Official Azure Icons
**Files**: `generate_diagram.py`

✅ **Pros:**
- **Uses official Azure service icons**
- Code-as-diagram (version controlled)
- High-quality PNG/SVG output
- Professional appearance

❌ **Cons:**
- Requires Python + Graphviz installed
- Build step needed
- PNG/SVG files need to be committed

**Use for**: Architecture reviews, external presentations, documentation

## How to Generate Diagram with Azure Icons

### Prerequisites
```bash
# Install Python dependencies
pip install diagrams

# Install Graphviz (required by diagrams library)
# macOS:
brew install graphviz

# Ubuntu/Debian:
sudo apt-get install graphviz

# Windows (use Chocolatey):
choco install graphviz
```

### Generate the Diagram
```bash
# Run the generator
python docs/generate_diagram.py

# Output: docs/architecture_diagram.png
```

### Update README to use generated diagram
```markdown
![Architecture Diagram](docs/architecture_diagram.png)
```

## Recommendation

**For this project, we recommend using:**

1. **Mermaid** (current) - For GitHub rendering and quick updates
2. **Python Diagrams** - For presentations and formal documentation with Azure icons

**Workflow:**
1. Maintain the Mermaid diagram in `infrastructure-diagram.mmd` for day-to-day use
2. When preparing for presentations or architecture reviews, run `python docs/generate_diagram.py` to generate the professional version with Azure icons
3. Commit both versions to the repo

## Example: Adding diagram to README

### Option A: Mermaid (Current)
```markdown
![Infrastructure Diagram](docs/infrastructure-diagram.mmd)
```

### Option B: Python Diagrams with Azure Icons
```markdown
![Architecture Diagram](docs/architecture_diagram.png)
```

---

## Azure Architecture Icons

Official Azure icons are available at:
- https://learn.microsoft.com/en-us/azure/architecture/icons/
- Download SVG format for custom diagrams

The Python `diagrams` library automatically downloads and uses these official icons.
