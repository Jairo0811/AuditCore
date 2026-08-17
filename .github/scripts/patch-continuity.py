from pathlib import Path

path = Path("README.md")
text = path.read_text(encoding="utf-8")

anchor = "| Pedro Arturo de León Parra | 2015-3018 |\n| Yeidy Khris Utate | 2015-3143 |\n\n---"
section = '''| Pedro Arturo de León Parra | 2015-3018 |
| Yeidy Khris Utate | 2015-3143 |

## 🧭 Continuidad académica

**AuditCore** representa el primer punto documentado de una continuidad académica por **compañero recurrente** con [**IngSoft Studio**](https://github.com/Jairo0811/IngSoft-Studio) dentro de la trayectoria de Francis Jairo Matías Rosario en el Instituto Tecnológico de Las Américas (ITLA). La relación entre ambos proyectos es **formativa y cronológica**: no existe una dependencia técnica entre las aplicaciones, sino la coincidencia de un mismo integrante en dos grupos académicos de materias teóricas cursadas en períodos consecutivos.

La primera coincidencia ocurrió en **2017-C2** durante **Auditoría Informática (SOF-009)**, asignatura que posteriormente sirvió como base conceptual para AuditCore. En el período siguiente, **2017-C3**, **Pedro Arturo de León Parra (2015-3018)** volvió a coincidir con Francis Jairo Matías Rosario en **Introducción a la Ingeniería en Software (SOF-015)**, cuyos contenidos inspiraron posteriormente IngSoft Studio.

| Orden | Código | Asignatura | Proyecto | Período | Compañero recurrente |
|---:|---|---|---|---|---|
| 1 | SOF-009 | Auditoría Informática | **AuditCore** | 2017-C2 | **Pedro Arturo de León Parra — 2015-3018** |
| 2 | SOF-015 | Introducción a la Ingeniería en Software | [**IngSoft Studio**](https://github.com/Jairo0811/IngSoft-Studio) | 2017-C3 | **Pedro Arturo de León Parra — 2015-3018** |

Vistos en conjunto, ambos proyectos documentan una continuidad real entre compañeros a lo largo de dos períodos académicos consecutivos y muestran una progresión conceptual desde **auditoría, controles y cumplimiento** hacia **ingeniería de software, calidad y ciclo de vida del desarrollo**. Cada repositorio conserva su identidad académica original y su implementación profesional posterior.

---'''

if anchor not in text:
    raise SystemExit("Academic team anchor not found")

text = text.replace(anchor, section, 1)
path.write_text(text, encoding="utf-8")
