import { CheckCircle2, AlertTriangle, X } from "lucide-react";

export interface Feedback {
  tone: "success" | "error";
  message: string;
}

interface FeedbackMessageProps {
  feedback: Feedback | null;
  onDismiss: () => void;
}

/**
 * Mensaje de resultado en línea. La región viva existe siempre para que los lectores de pantalla
 * anuncien el contenido cuando aparece; los errores usan role="alert".
 */
export function FeedbackMessage({ feedback, onDismiss }: FeedbackMessageProps) {
  return (
    <div className="feedback-region" aria-live="polite" aria-atomic="true">
      {feedback && (
        <div className={`feedback feedback-${feedback.tone}`} role={feedback.tone === "error" ? "alert" : "status"}>
          {feedback.tone === "error"
            ? <AlertTriangle size={18} aria-hidden="true" />
            : <CheckCircle2 size={18} aria-hidden="true" />}
          <p>{feedback.message}</p>
          <button type="button" className="feedback-dismiss" aria-label="Descartar mensaje" onClick={onDismiss}>
            <X size={16} aria-hidden="true" />
          </button>
        </div>
      )}
    </div>
  );
}
