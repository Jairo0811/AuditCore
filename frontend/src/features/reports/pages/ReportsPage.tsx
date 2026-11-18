import { Download } from "lucide-react";
import { useState } from "react";
import { FeedbackMessage, type Feedback } from "../../../components/ui/FeedbackMessage";
import { readApiErrorMessage } from "../../../lib/apiErrors";
import { getFileNameFromDisposition, saveBlob } from "../../../lib/download";
import { apiClient } from "../../../services/apiClient";

// Valores de ReportFormat en la API.
const formats = [
  { label: "CSV", value: 1, extension: "csv", description: "Texto separado por comas para análisis y hojas de cálculo." },
  { label: "Excel", value: 2, extension: "xlsx", description: "Libro de Excel con el resumen consolidado de auditorías." },
  { label: "PDF", value: 3, extension: "pdf", description: "Documento listo para archivo y presentación ejecutiva." },
] as const;

export function ReportsPage() {
  const [downloading, setDownloading] = useState<number | null>(null);
  const [feedback, setFeedback] = useState<Feedback | null>(null);

  async function download(format: (typeof formats)[number]) {
    setFeedback(null);
    setDownloading(format.value);
    try {
      const response = await apiClient.get<Blob>("/reports/audits/export", {
        params: { format: format.value },
        responseType: "blob",
        timeout: 120_000,
      });
      const fileName = getFileNameFromDisposition(response.headers["content-disposition"], `auditcore-auditorias.${format.extension}`);
      saveBlob(response.data, fileName);
      setFeedback({ tone: "success", message: `Reporte ${format.label} generado: ${fileName}.` });
    } catch (error) {
      setFeedback({ tone: "error", message: await readApiErrorMessage(error, `No fue posible generar el reporte ${format.label}.`) });
    } finally {
      setDownloading(null);
    }
  }

  return (
    <div className="module-page">
      <header className="module-header">
        <div>
          <p className="eyebrow">ANÁLISIS Y SALIDA</p>
          <h1>Reportes</h1>
          <p>Exporta el resumen de auditorías para análisis, archivo y presentación ejecutiva.</p>
        </div>
      </header>

      <FeedbackMessage feedback={feedback} onDismiss={() => setFeedback(null)} />

      <section className="report-grid" aria-label="Formatos de exportación">
        {formats.map((format) => {
          const busy = downloading === format.value;
          return (
            <article key={format.value} className="report-card" aria-busy={busy}>
              <div>
                <span>Resumen de auditorías</span>
                <h2 className="report-card-title">{format.label}</h2>
                <p>{format.description}</p>
              </div>
              <button type="button" disabled={downloading !== null} onClick={() => void download(format)}>
                <Download size={17} aria-hidden="true" /> {busy ? `Generando ${format.label}...` : `Descargar ${format.label}`}
              </button>
            </article>
          );
        })}
      </section>
    </div>
  );
}
