import { useCallback, useState, type ReactNode } from "react";
import { Modal } from "./Modal";

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  tone?: "danger" | "default";
}

interface PendingConfirmation extends ConfirmOptions {
  resolve: (confirmed: boolean) => void;
}

/**
 * Reemplazo accesible de window.confirm. Devuelve una promesa que se resuelve con la decisión del usuario.
 * El foco inicial queda en la opción segura (volver) para evitar confirmaciones destructivas accidentales.
 */
export function useConfirmDialog(): {
  confirm: (options: ConfirmOptions) => Promise<boolean>;
  dialog: ReactNode;
} {
  const [pending, setPending] = useState<PendingConfirmation | null>(null);

  const confirm = useCallback(
    (options: ConfirmOptions) => new Promise<boolean>((resolve) => setPending({ ...options, resolve })),
    [],
  );

  function settle(confirmed: boolean) {
    pending?.resolve(confirmed);
    setPending(null);
  }

  const dialog = pending ? (
    <Modal
      role="alertdialog"
      size="small"
      eyebrow="CONFIRMACIÓN"
      title={pending.title}
      description={pending.message}
      onClose={() => settle(false)}
    >
      <div className="form-actions confirm-actions">
        <button type="button" className="secondary-button" data-autofocus onClick={() => settle(false)}>
          {pending.cancelLabel ?? "Volver"}
        </button>
        <button
          type="button"
          className={pending.tone === "danger" ? "danger-solid-button" : undefined}
          onClick={() => settle(true)}
        >
          {pending.confirmLabel ?? "Confirmar"}
        </button>
      </div>
    </Modal>
  ) : null;

  return { confirm, dialog };
}
