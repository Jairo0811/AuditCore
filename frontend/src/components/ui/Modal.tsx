import { X } from "lucide-react";
import { useEffect, useId, useRef, type KeyboardEvent, type ReactNode } from "react";
import { createPortal } from "react-dom";

const FOCUSABLE_SELECTOR = [
  "a[href]",
  "button:not([disabled])",
  "textarea:not([disabled])",
  "input:not([disabled]):not([type='hidden'])",
  "select:not([disabled])",
  "[tabindex]:not([tabindex='-1'])",
].join(",");

// Pila de diálogos abiertos: el último es el único que retiene el foco.
const dialogStack: HTMLElement[] = [];

function focusableElements(container: HTMLElement) {
  return Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR)).filter(
    (element) => !element.closest("[inert]") && element.getClientRects().length > 0,
  );
}

function setBackgroundInert(inert: boolean) {
  const root = document.getElementById("root");
  if (!root) return;
  if (inert) root.setAttribute("inert", "");
  else root.removeAttribute("inert");
}

function handleDocumentFocus(event: FocusEvent) {
  const top = dialogStack.at(-1);
  if (!top || !(event.target instanceof Node) || top.contains(event.target)) return;
  (focusableElements(top)[0] ?? top).focus();
}

export interface ModalProps {
  title: string;
  onClose: () => void;
  children: ReactNode;
  eyebrow?: string;
  description?: string;
  role?: "dialog" | "alertdialog";
  size?: "default" | "small";
  closeLabel?: string;
}

/**
 * Diálogo modal accesible: role/aria-modal, título y descripción asociados, cierre con Escape,
 * foco inicial dentro del diálogo, focus trap, fondo inerte, bloqueo de scroll y restauración del foco.
 * El foco inicial va al elemento marcado con data-autofocus o al primer control del contenido.
 */
export function Modal({
  title,
  onClose,
  children,
  eyebrow,
  description,
  role = "dialog",
  size = "default",
  closeLabel = "Cerrar diálogo",
}: ModalProps) {
  const titleId = useId();
  const descriptionId = useId();
  const dialogRef = useRef<HTMLElement>(null);
  const bodyRef = useRef<HTMLDivElement>(null);
  const onCloseRef = useRef(onClose);

  useEffect(() => {
    onCloseRef.current = onClose;
  });

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return undefined;

    const trigger = document.activeElement instanceof HTMLElement ? document.activeElement : null;
    const previousOverflow = document.body.style.overflow;

    dialogStack.push(dialog);
    if (dialogStack.length === 1) {
      document.addEventListener("focusin", handleDocumentFocus);
      setBackgroundInert(true);
      document.body.style.overflow = "hidden";
    }

    const initialTarget =
      dialog.querySelector<HTMLElement>("[data-autofocus]") ??
      (bodyRef.current ? focusableElements(bodyRef.current)[0] : undefined) ??
      dialog;
    initialTarget.focus();

    return () => {
      const index = dialogStack.lastIndexOf(dialog);
      if (index >= 0) dialogStack.splice(index, 1);

      if (dialogStack.length === 0) {
        document.removeEventListener("focusin", handleDocumentFocus);
        setBackgroundInert(false);
        document.body.style.overflow = previousOverflow;
      }

      const fallback = dialogStack.at(-1) ?? document.getElementById("main-content");
      if (trigger?.isConnected && !trigger.closest("[inert]")) trigger.focus();
      else fallback?.focus();
    };
  }, []);

  function handleKeyDown(event: KeyboardEvent<HTMLElement>) {
    const dialog = dialogRef.current;
    if (!dialog) return;

    if (event.key === "Escape") {
      event.stopPropagation();
      onCloseRef.current();
      return;
    }

    if (event.key !== "Tab") return;
    event.stopPropagation();

    const elements = focusableElements(dialog);
    if (elements.length === 0) {
      event.preventDefault();
      dialog.focus();
      return;
    }

    const first = elements[0];
    const last = elements[elements.length - 1];
    const active = document.activeElement;

    if (event.shiftKey && (active === first || active === dialog)) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && active === last) {
      event.preventDefault();
      first.focus();
    }
  }

  return createPortal(
    <div className="modal-backdrop" role="presentation">
      <section
        ref={dialogRef}
        className={`modal-card${size === "small" ? " modal-card-small" : ""}`}
        role={role}
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={description ? descriptionId : undefined}
        tabIndex={-1}
        onKeyDown={handleKeyDown}
      >
        <header>
          <div>
            {eyebrow && <p className="eyebrow">{eyebrow}</p>}
            <h2 id={titleId}>{title}</h2>
          </div>
          <button type="button" className="icon-button" aria-label={closeLabel} onClick={() => onCloseRef.current()}>
            <X size={18} aria-hidden="true" />
          </button>
        </header>
        {description && <p id={descriptionId} className="modal-description">{description}</p>}
        <div ref={bodyRef} className="modal-body">{children}</div>
      </section>
    </div>,
    document.body,
  );
}
