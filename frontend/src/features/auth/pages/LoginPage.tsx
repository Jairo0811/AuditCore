import { zodResolver } from "@hookform/resolvers/zod";
import axios from "axios";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { z } from "zod";
import { getLandingPath } from "../../../app/navigation";
import { getApiErrorMessage } from "../../../lib/apiErrors";
import { login } from "../auth";
import { useSession } from "../useSession";

const loginSchema = z.object({
  email: z.string().email("Ingresa un correo electrónico válido."),
  password: z
    .string()
    .min(8, "La contraseña debe tener al menos 8 caracteres."),
});

type LoginForm = z.infer<typeof loginSchema>;

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const session = useSession();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const requestedPath = (location.state as { from?: string } | null)?.from;

  // Una sesión válida ya existente no debe mostrar una pantalla vacía: se envía a la pantalla permitida.
  if (session) {
    return <Navigate to={requestedPath ?? getLandingPath()} replace />;
  }

  async function onSubmit(values: LoginForm) {
    setServerError(null);

    try {
      await login(values);
      navigate(requestedPath ?? getLandingPath(), { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 401) {
        setServerError("Correo o contraseña incorrectos.");
        return;
      }
      if (axios.isAxiosError(error) && error.response?.status === 429) {
        setServerError(getApiErrorMessage(error, "Demasiados intentos. Espera un momento e inténtalo nuevamente."));
        return;
      }

      setServerError(
        "No fue posible conectar con AuditCore. Verifica que la API esté disponible.",
      );
    }
  }

  return (
    <main className="login-page">
      <section
        className="login-brand"
        aria-label="AuditCore Enterprise IT Audit & Compliance Platform"
      >
        <div className="login-brand-visual">
          <img
            src="/assets/brand/auditcore-logo.webp"
            width={1536}
            height={1024}
            decoding="async"
            alt="AuditCore — Enterprise IT Audit & Compliance Platform. Audita, evalúa, protege."
          />
        </div>
      </section>

      <section className="login-panel">
        <form
          className="login-form"
          onSubmit={handleSubmit(onSubmit)}
          noValidate
        >
          {/* Identidad compacta visible cuando el panel visual se oculta (pantallas estrechas). */}
          <div className="login-mobile-brand" aria-hidden="true">
            <img src="/assets/brand/favicon.png" alt="" width={44} height={44} />
            <span>AUDIT<strong>CORE</strong></span>
          </div>

          <div>
            <p className="eyebrow">BIENVENIDO</p>
            <h1>Iniciar sesión</h1>
            <p>Accede al espacio de trabajo de tu organización.</p>
          </div>

          {serverError && (
            <div className="form-alert" role="alert">
              {serverError}
            </div>
          )}

          <label>
            Correo electrónico
            <input
              type="email"
              autoComplete="email"
              placeholder="usuario@empresa.com"
              aria-invalid={Boolean(errors.email)}
              {...register("email")}
            />
            {errors.email && (
              <small className="field-error">{errors.email.message}</small>
            )}
          </label>

          <label>
            Contraseña
            <input
              type="password"
              autoComplete="current-password"
              placeholder="••••••••"
              aria-invalid={Boolean(errors.password)}
              {...register("password")}
            />
            {errors.password && (
              <small className="field-error">{errors.password.message}</small>
            )}
          </label>

          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Validando..." : "Iniciar sesión"}
          </button>
        </form>
      </section>
    </main>
  );
}
