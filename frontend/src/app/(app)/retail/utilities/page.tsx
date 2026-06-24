import { UtilityPaymentsWorkspace } from "./UtilityPaymentsWorkspace";

export default function UtilityPaymentsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Airtime & Bill Payments</h1>
        <p className="mt-1 text-sm text-[var(--muted-foreground)]">
          Counter workflow for prepaid reloads, utility bills, and other customer payment services.
        </p>
      </div>

      <UtilityPaymentsWorkspace />
    </div>
  );
}
