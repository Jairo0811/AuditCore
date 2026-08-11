import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, ClipboardCheck, FileSearch, Gauge, ListTodo, ShieldAlert } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { getDashboard } from "../dashboardApi";

export function DashboardPage() {
  const navigate = useNavigate();
  const { data, isPending, isError, refetch } = useQuery({
    queryKey: ["dashboard"],
    queryFn: getDashboard,
  });

  return (
    <main className="dashboard-page">
      <header className="dashboard-header">
        <div>
          <p className="eyebrow">VISIÓN EJECUTIVA</p>
          <h1>Dashboard</h1>
          <p>Supervisa auditorías, riesgos, hallazgos y cumplimiento con datos reales.</p>
        </div>

        <button type="button" onClick={() => navigate("/audits")}>
          Ver auditorías
        </button>
      </header>

      {isError && (
        <section className="error-state" role="alert">
          <AlertTriangle size={22} />
          <div>
            <strong>No fue posible cargar el dashboard.</strong>
            <p>Comprueba la conexión con la API e inténtalo nuevamente.</p>
          </div>
          <button type="button" onClick={() => refetch()}>Reintentar</button>
        </section>
      )}

      <section className="stats-grid" aria-busy={isPending}>
        <StatCard icon={ClipboardCheck} label="Auditorías abiertas" value={formatValue(data?.openAudits, isPending)} detail={`${data?.totalAudits ?? 0} auditorías registradas`} />
        <StatCard icon={Gauge} label="Cumplimiento promedio" value={isPending ? "—" : `${Number(data?.averageComplianceScore ?? 0).toFixed(1)}%`} detail="Promedio de controles evaluados" />
        <StatCard icon={ShieldAlert} label="Riesgos críticos" value={formatValue(data?.criticalRisks, isPending)} detail={`${data?.totalRisks ?? 0} riesgos registrados`} />
        <StatCard icon={FileSearch} label="Hallazgos abiertos" value={formatValue(data?.openFindings, isPending)} detail={`${data?.totalFindings ?? 0} hallazgos registrados`} />
        <StatCard icon={ListTodo} label="Planes vencidos" value={formatValue(data?.overdueActionPlans, isPending)} detail="Requieren seguimiento inmediato" />
        <StatCard icon={ClipboardCheck} label="Auditorías cerradas" value={formatValue(data?.closedAudits, isPending)} detail="Procesos concluidos" />
      </section>
    </main>
  );
}

interface StatCardProps {
  icon: typeof ClipboardCheck;
  label: string;
  value: string;
  detail: string;
}

function StatCard({ icon: Icon, label, value, detail }: StatCardProps) {
  return (
    <article className="stat-card">
      <div className="stat-icon"><Icon size={20} /></div>
      <span>{label}</span>
      <strong>{value}</strong>
      <small>{detail}</small>
    </article>
  );
}

function formatValue(value: number | undefined, loading: boolean) {
  return loading ? "—" : String(value ?? 0);
}
