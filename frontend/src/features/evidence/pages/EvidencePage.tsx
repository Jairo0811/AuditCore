import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Download, FileUp, Trash2 } from "lucide-react";
import { useId, useState, type FormEvent } from "react";
import { FeedbackMessage, type Feedback } from "../../../components/ui/FeedbackMessage";
import { useConfirmDialog } from "../../../components/ui/useConfirmDialog";
import { hasPermission, Permissions } from "../../auth/permissions";
import { useLookupOptions } from "../../../hooks/useLookupOptions";
import { getApiErrorMessage, readApiErrorMessage } from "../../../lib/apiErrors";
import { getFileNameFromDisposition, saveBlob } from "../../../lib/download";
import { formatDate } from "../../../lib/formatters";
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

// Refleja las reglas del backend (EvidenceFileRules y el límite de 20 MB del endpoint); el backend valida igualmente.
const MAX_FILE_BYTES = 20 * 1024 * 1024;
const ACCEPTED_FILES = ".pdf,.png,.jpg,.jpeg,.txt,.csv,.xlsx,.docx";

function formatSize(bytes: number) {
  return bytes >= 1024 * 1024 ? `${(bytes / (1024 * 1024)).toFixed(1)} MB` : `${(bytes / 1024).toFixed(1)} KB`;
}

export function EvidencePage() {
  const queryClient = useQueryClient();
  const { confirm, dialog: confirmDialog } = useConfirmDialog();
  const canManage = hasPermission(Permissions.EvidenceManage);
  const formIds = { audit: useId(), finding: useId(), description: useId(), file: useId(), fileHelp: useId() };

  const [auditId, setAuditId] = useState("");
  const [findingId, setFindingId] = useState("");
  const [description, setDescription] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [fileInputKey, setFileInputKey] = useState(0);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<Feedback | null>(null);
  const [downloadingId, setDownloadingId] = useState<string | null>(null);

  const audits = useLookupOptions<AuditLookup>("audits", "/audits", (item) => `${item.code} — ${item.title}`, [Permissions.AuditsView]);
  const findings = useLookupOptions<FindingLookup>("findings", "/findings", (item) => `${item.code} — ${item.title}`, [Permissions.FindingsView]);
  const auditCodes = new Map(audits.records.map((audit) => [audit.id, audit.code]));

  const evidenceQuery = useQuery({
    queryKey: ["evidence"],
    queryFn: async () => (await apiClient.get<EvidenceDto[]>("/evidence")).data,
  });

  const uploadMutation = useMutation({
    mutationFn: async () => {
      if (!file || !auditId) throw new Error("Selecciona una auditoría y un archivo.");
      if (file.size === 0) throw new Error("El archivo está vacío.");
      if (file.size > MAX_FILE_BYTES) throw new Error("El archivo supera el tamaño máximo permitido (20 MB).");
      const body = new FormData();
      body.append("auditId", auditId);
      if (findingId) body.append("findingId", findingId);
      if (description.trim()) body.append("description", description.trim());
      body.append("file", file);
      // La autoría la asigna el backend a partir del usuario autenticado.
      await apiClient.post("/evidence", body, { headers: { "Content-Type": "multipart/form-data" }, timeout: 120_000 });
      return file.name;
    },
    onSuccess: async (fileName) => {
      setFile(null);
      setFileInputKey((key) => key + 1);
      setDescription("");
      setFindingId("");
      setFeedback({ tone: "success", message: `Evidencia «${fileName}» subida correctamente.` });
      await queryClient.invalidateQueries({ queryKey: ["evidence"] });
    },
    onError: (error) => setUploadError(getApiErrorMessage(error, "No fue posible subir la evidencia.")),
  });

  const deleteMutation = useMutation({
    mutationFn: async (item: EvidenceDto) => {
      await apiClient.delete(`/evidence/${item.id}`);
      return item.fileName;
    },
    onSuccess: async (fileName) => {
      setFeedback({ tone: "success", message: `Evidencia «${fileName}» eliminada.` });
      await queryClient.invalidateQueries({ queryKey: ["evidence"] });
    },
    onError: (error) => setFeedback({ tone: "error", message: getApiErrorMessage(error, "No fue posible eliminar la evidencia.") }),
  });

  async function downloadEvidence(item: EvidenceDto) {
    setFeedback(null);
    setDownloadingId(item.id);
    try {
      const response = await apiClient.get<Blob>(`/evidence/${item.id}/download`, { responseType: "blob", timeout: 120_000 });
      saveBlob(response.data, getFileNameFromDisposition(response.headers["content-disposition"], item.fileName));
    } catch (error) {
      setFeedback({ tone: "error", message: await readApiErrorMessage(error, "No fue posible descargar la evidencia.") });
      // Si la evidencia ya no existe (p. ej. eliminada en otra sesión), se refresca la lista.
      await queryClient.invalidateQueries({ queryKey: ["evidence"] });
    } finally {
      setDownloadingId(null);
    }
  }

  async function requestDelete(item: EvidenceDto) {
    setFeedback(null);
    const confirmed = await confirm({
      title: "Eliminar evidencia",
      message: `¿Eliminar la evidencia «${item.fileName}»? El archivo se eliminará y dejará de estar disponible para descarga.`,
      confirmLabel: "Eliminar",
      tone: "danger",
    });
    if (confirmed) deleteMutation.mutate(item);
  }

  function handleUpload(event: FormEvent) {
    event.preventDefault();
    setUploadError(null);
    setFeedback(null);
    uploadMutation.mutate();
  }

  const availableFindings = findings.records.filter((item) => !auditId || item.auditId === auditId);
  const items = evidenceQuery.data ?? [];

  return (
    <div className="module-page">
      <header className="module-header">
        <div>
          <p className="eyebrow">DOCUMENTACIÓN</p>
          <h1>Evidencias</h1>
          <p>Adjunta, consulta y descarga evidencias documentales vinculadas a auditorías y hallazgos.</p>
        </div>
      </header>

      <FeedbackMessage feedback={feedback} onDismiss={() => setFeedback(null)} />

      {canManage && (
        <section className="data-panel upload-panel" aria-labelledby="evidence-upload-title">
          <h2 id="evidence-upload-title">Subir evidencia</h2>
          <form className="inline-form" onSubmit={handleUpload} aria-busy={uploadMutation.isPending}>
            <div className="form-field">
              <label htmlFor={formIds.audit}>Auditoría <span className="required-mark" aria-hidden="true">*</span></label>
              <select
                id={formIds.audit}
                required
                value={auditId}
                onChange={(event) => {
                  setAuditId(event.target.value);
                  setFindingId("");
                }}
              >
                <option value="">{audits.isLoading ? "Cargando auditorías..." : "Selecciona una auditoría"}</option>
                {audits.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
              </select>
            </div>
            <div className="form-field">
              <label htmlFor={formIds.finding}>Hallazgo (opcional)</label>
              <select id={formIds.finding} value={findingId} onChange={(event) => setFindingId(event.target.value)} disabled={!auditId}>
                <option value="">Sin hallazgo relacionado</option>
                {availableFindings.map((item) => <option key={item.id} value={item.id}>{item.code} — {item.title}</option>)}
              </select>
            </div>
            <div className="form-field">
              <label htmlFor={formIds.description}>Descripción</label>
              <input id={formIds.description} maxLength={2000} value={description} onChange={(event) => setDescription(event.target.value)} />
            </div>
            <div className="form-field">
              <label htmlFor={formIds.file}>Archivo <span className="required-mark" aria-hidden="true">*</span></label>
              <input
                key={fileInputKey}
                id={formIds.file}
                type="file"
                required
                accept={ACCEPTED_FILES}
                aria-describedby={formIds.fileHelp}
                onChange={(event) => {
                  setUploadError(null);
                  setFile(event.target.files?.[0] ?? null);
                }}
              />
              <small id={formIds.fileHelp} className="field-hint">PDF, PNG, JPG, TXT, CSV, XLSX o DOCX. Máximo 20 MB.</small>
            </div>
            <button type="submit" disabled={!file || !auditId || uploadMutation.isPending}>
              <FileUp size={16} aria-hidden="true" /> {uploadMutation.isPending ? "Subiendo..." : "Subir"}
            </button>
            {uploadError && <div className="form-error full-width" role="alert">{uploadError}</div>}
          </form>
        </section>
      )}

      <section className="data-panel" aria-labelledby="evidence-list-title" aria-busy={evidenceQuery.isFetching}>
        <h2 id="evidence-list-title" className="sr-only">Evidencias registradas</h2>
        {evidenceQuery.isPending && <p className="panel-state" role="status">Cargando evidencias...</p>}
        {evidenceQuery.isError && (
          <div className="panel-state error-message" role="alert">
            <p>{getApiErrorMessage(evidenceQuery.error, "No fue posible cargar las evidencias.")}</p>
            <button type="button" className="secondary-button" onClick={() => evidenceQuery.refetch()}>Reintentar</button>
          </div>
        )}
        {evidenceQuery.isSuccess && (
          <>
            <div className="table-scroll">
              <table className="data-table">
                <caption className="sr-only">Evidencias registradas</caption>
                <thead>
                  <tr>
                    <th scope="col">Archivo</th><th scope="col">Auditoría</th><th scope="col">Tipo</th>
                    <th scope="col">Tamaño</th><th scope="col">Descripción</th><th scope="col">Fecha</th><th scope="col">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((item) => (
                    <tr key={item.id}>
                      <td>{item.fileName}</td>
                      <td>{auditCodes.get(item.auditId) ?? "—"}</td>
                      <td>{item.contentType}</td>
                      <td>{formatSize(item.sizeBytes)}</td>
                      <td>{item.description ?? "—"}</td>
                      <td>{formatDate(item.createdAtUtc)}</td>
                      <td>
                        <div className="row-actions">
                          <button
                            className="icon-button"
                            type="button"
                            aria-label={`Descargar ${item.fileName}`}
                            title="Descargar"
                            disabled={downloadingId === item.id}
                            aria-busy={downloadingId === item.id}
                            onClick={() => void downloadEvidence(item)}
                          >
                            <Download size={15} aria-hidden="true" />
                          </button>
                          {canManage && (
                            <button
                              className="icon-button danger-button"
                              type="button"
                              aria-label={`Eliminar ${item.fileName}`}
                              title="Eliminar"
                              disabled={deleteMutation.isPending}
                              onClick={() => void requestDelete(item)}
                            >
                              <Trash2 size={15} aria-hidden="true" />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {items.length === 0 && <p className="empty-table">No hay evidencias para mostrar.</p>}
          </>
        )}
      </section>

      {confirmDialog}
    </div>
  );
}
