import Link from "next/link";
import { Card, SecondaryLink } from "@/components/ui";

const actions = [
  {
    href: "/sales/invoices",
    title: "Sales invoices",
    description: "Create and post customer invoices for normal supermarket sales.",
  },
  {
    href: "/sales/direct-dispatches",
    title: "Counter stock issues",
    description: "Issue stock directly from the selling warehouse when a separate dispatch record is needed.",
  },
  {
    href: "/retail/utilities",
    title: "Airtime & bill pay",
    description: "Record non-stock customer services such as mobile reloads and utility bills.",
  },
  {
    href: "/sales/customer-returns",
    title: "Customer returns",
    description: "Receive returned goods and create the related credit flow.",
  },
];

export default function CounterSalesPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Counter Sales</h1>
        <p className="mt-1 text-sm text-[var(--muted-foreground)]">
          Retail shortcuts for supermarket checkout, customer returns, and utility-service sales.
        </p>
      </div>

      <Card className="border-[var(--card-border)] bg-[var(--card-bg)]">
        <div className="text-lg font-semibold">Recommended cashier flow</div>
        <p className="mt-2 max-w-3xl text-sm leading-6 text-[var(--muted-foreground)]">
          Use sales invoices for ordinary goods, utility payments for airtime and bill collections, and customer
          returns for refunds or exchanges. A dedicated barcode-first POS screen can be layered on top of these
          transaction APIs next.
        </p>
        <div className="mt-4 flex flex-wrap gap-2">
          <SecondaryLink href="/sales/invoices">Open sales invoices</SecondaryLink>
          <SecondaryLink href="/retail/utilities">Open utilities</SecondaryLink>
        </div>
      </Card>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {actions.map((action) => (
          <Link key={action.href} href={action.href}>
            <Card className="h-full transition hover:bg-[var(--surface-soft)]">
              <div className="text-base font-semibold">{action.title}</div>
              <p className="mt-2 text-sm leading-6 text-[var(--muted-foreground)]">{action.description}</p>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}
