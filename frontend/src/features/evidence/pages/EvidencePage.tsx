import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Download, FileUp, Trash2 } from "lucide-react";
import { useState } from "react";
import { useLookupOptions } from "../../../hooks/useLookupOptions";
import { apiClient } from "../../../services/apiClient";

interface EvidenceDto {
  id: string;
  auditId: string;
  findingId?: string | null;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  sha256: string;
  description?: string | null;
  createdAtUtc: string;
}

interface AuditLookup {
  id: string;
  code: string;
  title: string;
}

interface FindingLookup {
  id: string;
  auditId: string;
  code: string;
  title: string;
}

export function EvidencePage() {
  const queryClient = useQueryClient();
  const [auditId, setAuditId] = useState("");
  const [findingId, setFindingId] = useState("");
  const [description, setDescription] = useState("");
  const [file, setFile] = useState<File | null>(null);

  const audits = useLookupOptions<AuditLookup>(
    "audits",
    "/audits",
    (item) => `${item.code} — ${item.title}`,
  );

  const findings = useLookupOptions<FindingLookup>(
    "findings",
    "/findings",
    (item) => `${item.code} — ${item.title}`,
  );

  const evidenceQuery = useQuery({
    queryKey: ["evidence"],
    queryFn: async () => (await apiClient.get<EvidenceDto[]>("/evidence")).data,
  });

  const uploadMutation = useMutation({
    mutationFn: async () => {
      if (!file || !auditId) throw new Error("Selecciona una auditoría y un archivo.");
      const body = new FormData();
      body.append("auditId", auditId);
      if (findingId) body.append("findingId", findingId);
      if (description) body.append("description", description);
      body.append("file", file);
      await apiClient.post("/evidence", body, { headers: { "Content-Type": "multipart/form-data" } });
    },
    onSuccess: async () => {
      setFile(null);
      setDescription("");
      setFindingId("");
      await queryClient.invalidateQueries({ queryKey: ["evidence"] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => apiClient.delete(`/evidence/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["evidence"] }),
  });

  async function downloadEvidence(item: EvidenceDto) {
    const response = await apiClient.get(`/evidence/${item.id}/download`, { responseType: "blob" });
    const url = URL.createObjectURL(response.data);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = item.fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }

  const availableFindings = findings.records.filter((item) => !auditId || item.auditId === auditId);

  return (
    <main className="module-page">
      <header className="module-header">
        <div>
          <p className="eyebrow">DOCUMENTACIÓN</p>
          <h1>Evidencias</h1>
          <p>Adjunta, consulta y descarga evidencias documentales vinculadas a auditorías y hallazgos.</p>
        </div>
      </header>

      <section className="data-panel upload-panel">
        <h2>Subir evidencia</h2>
        <div className="inline-form">
          <label>
            Auditoría
            <select
              value={auditId}
              onChange={(event) => {
                setAuditId(event.target.value);
                setFindingId("");
              }}
            >
              <option value="">{audits.isLoading ? "Cargando auditorías..." : "Selecciona una auditoría"}</option>
              {audits.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
            </select>
          </label>
          <label>
            Hallazgo (opcional)
            <select value={findingId} onChange={(event) => setFindingId(event.target.value)} disabled={!auditId}>
              <option value="">Sin hallazgo relacionado</option>
              {availableFindings.map((item) => <option key={item.id} value={item.id}>{item.code} — {item.title}</option>)}
            </select>
          </label>
          <label>Descripción<input value={description} onChange={(event) => setDescription(event.target.value)} /></label>
          <label>Archivo<input type="file" onChange={(event) => setFile(event.target.files?.[0] ?? null)} /></label>
          <button type="button" disabled={!file || !auditId || uploadMutation.isPending} onClick={() => uploadMutation.mutate()}>
            <FileUp size={16} /> {uploadMutation.isPending ? "Subiendo..." : "Subir"}
          </button>
        </div>
      </section>

      <section className="data-panel">
        <div className="table-scroll">
          <table className="data-table">
            <thead><tr><th>Archivo</th><th>Tipo</th><th>Tamaño</th><th>Descripción</th><th>Fecha</th><th>Acciones</th></tr></thead>
            <tbody>
              {(evidenceQuery.data ?? []).map((item) => (
                <tr key={item.id}>
                  <td>{item.fileName}</td>
                  <td>{item.contentType}</td>
                  <td>{(item.sizeBytes / 1024).toFixed(1)} KB</td>
                  <td>{item.description ?? "—"}</td>
                  <td>{new Date(item.createdAtUtc).toLocaleDateString()}</td>
                  <td><div className="row-actions">
                    <button className="icon-button" type="button" title="Descargar" onClick={() => downloadEvidence(item)}><Download size={15} /></button>
                    <button className="icon-button danger-button" type="button" title="Eliminar" disabled={deleteMutation.isPending} onClick={() => window.confirm("¿Eliminar esta evidencia?") && deleteMutation.mutate(item.id)}><Trash2 size={15} /></button>
                  </div></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {!evidenceQuery.isLoading && (evidenceQuery.data?.length ?? 0) === 0 && <div className="empty-table">No hay evidencias para mostrar.</div>}
      </section>
    </main>
  );
}
