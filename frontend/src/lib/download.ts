/** Extrae el nombre de archivo de un encabezado Content-Disposition (RFC 6266 / RFC 5987). */
export function getFileNameFromDisposition(header: unknown, fallback: string) {
  if (typeof header !== "string" || !header) return fallback;

  const encoded = /filename\*\s*=\s*([^']*)''([^;]+)/i.exec(header);
  if (encoded?.[2]) {
    try {
      return sanitizeFileName(decodeURIComponent(encoded[2].trim().replace(/^"|"$/g, ""))) || fallback;
    } catch {
      // Valor mal codificado: se intenta con filename= más abajo.
    }
  }

  const plain = /filename\s*=\s*("([^"]*)"|[^;]+)/i.exec(header);
  const value = plain?.[2] ?? plain?.[1];
  return value ? sanitizeFileName(value.trim()) || fallback : fallback;
}

function sanitizeFileName(value: string) {
  return value.replace(/[\\/:*?"<>|]+/g, "_").trim();
}

/** Descarga un Blob en el navegador y libera la URL temporal después de iniciar la descarga. */
export function saveBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName;
  anchor.rel = "noopener";
  anchor.style.display = "none";
  document.body.appendChild(anchor);

  try {
    anchor.click();
  } finally {
    anchor.remove();
    window.setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
}
