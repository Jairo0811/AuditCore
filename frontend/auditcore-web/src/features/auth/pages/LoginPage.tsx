import { zodResolver } from "@hookform/resolvers/zod";
import axios from "axios";
import { ShieldCheck } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useLocation, useNavigate } from "react-router-dom";
import { z } from "zod";
import { isAuthenticated, login } from "../auth";

const loginSchema = z.object({
  email: z.string().email("Ingresa un correo electrónico válido."),
  password: z.string().min(8, "La contraseña debe tener al menos 8 caracteres."),
});

type LoginForm = z.infer<typeof loginSchema>;

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
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

  if (isAuthenticated()) {
    return null;
  }

  async function onSubmit(values: LoginForm) {
    setServerError(null);

    try {
      await login(values);
      const from = (location.state as { from?: string } | null)?.from ?? "/dashboard";
      navigate(from, { replace: true });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 401) {
        setServerError("Correo o contraseña incorrectos.");
        return;
      }

      setServerError("No fue posible conectar con AuditCore. Verifica que la API esté disponible.");
    }
  }

  return (
    <main className="login-page">
      <section className="login-brand">
        <div className="brand-symbol"><ShieldCheck size={46} strokeWidth={1.8} /></div>
        <p className="eyebrow">IT AUDIT & COMPLIANCE PLATFORM</p>
        <h1>AUDIT<span>CORE</span></h1>
        <p className="brand-description">
          Gestiona auditorías, riesgos, hallazgos, evidencias y cumplimiento desde una sola plataforma empresarial.
        </p>
      </section>

      <section className="login-panel">
        <form className="login-form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <div>
            <p className="eyebrow">BIENVENIDO</p>
            <h2>Iniciar sesión</h2>
            <p>Accede al espacio de trabajo de tu organización.</p>
          </div>

          {serverError && <div className="form-alert" role="alert">{serverError}</div>}

          <label>
            Correo electrónico
            <input
              type="email"
              autoComplete="email"
              placeholder="usuario@empresa.com"
              aria-invalid={Boolean(errors.email)}
              {...register("email")}
            />
            {errors.email && <small className="field-error">{errors.email.message}</small>}
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
            {errors.password && <small className="field-error">{errors.password.message}</small>}
          </label>

          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Validando..." : "Iniciar sesión"}
          </button>
        </form>
      </section>
    </main>
  );
}
