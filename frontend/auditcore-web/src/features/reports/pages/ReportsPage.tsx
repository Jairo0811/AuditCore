import { Download } from "lucide-react";
import { apiClient } from "../../../services/apiClient";

const formats = [
  { label: "CSV", value: 1, extension: "csv" },
  { label: "Excel", value: 2, extension: "xlsx" },
  { label: "PDF", value: 3, extension: "pdf" },
];

export function ReportsPage() {
  async function download(format: number, extension: string) {
    const response = await apiClient.get(`/reports/audits/export?format=${format}`, { responseType: "blob" });
    const url = URL.createObjectURL(response.data);
    const link = document.createElement("a");
    link.href = url;
    link.download = `auditcore-auditorias.${extension}`;
    link.click();
    URL.revokeObjectURL(url);
  }

  return (
    <main className="module-page">
      <header className="module-header">
        <div>
          <p className="eyebrow">ANÁLISIS Y SALIDA</p>
          <h1>Reportes</h1>
          <p>Exporta el resumen de auditorías para análisis, archivo y presentación ejecutiva.</p>
        </div>
      </header>

      <section className="report-grid">
        {formats.map((format) => (
          <article key={format.value} className="report-card">
            <div>
              <span>Resumen de auditorías</span>
              <strong>{format.label}</strong>
              <p>Genera el reporte consolidado con la información disponible para tu organización.</p>
            </div>
            <button type="button" onClick={() => download(format.value, format.extension)}>
              <Download size={17} /> Descargar {format.label}
            </button>
          </article>
        ))}
      </section>
    </main>
  );
}
