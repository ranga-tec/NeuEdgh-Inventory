# NeuEdgh Inventory

General inventory and retail ERP system adapted for supermarket operations.

## Stack

- Backend: ASP.NET Core (.NET 8) + PostgreSQL
- Frontend: Next.js App Router + TypeScript + Tailwind
- Deployment assets: Docker and Docker Compose under `deploy/`

## Current Scope

- Master data: items, brands, categories, UoMs, warehouses, customers, suppliers, taxes, currencies, payment types, and reorder settings
- Procurement: purchase requisitions, RFQs, purchase orders, goods receipts, direct purchases, supplier invoices, and supplier returns
- Sales and retail: sales invoices, counter stock issues, customer returns, counter-sales shortcuts
- Retail utilities: airtime reloads and bill-payment transaction capture
- Inventory: availability, on-hand inquiry, reorder alerts, stock adjustments, and stock transfers
- Finance: chart of accounts, AR/AP, payments, petty cash funds, credit notes, and debit notes
- Reporting: stock ledger, AR/AP aging, tax summary, sales analysis, purchase analysis, supplier performance, and costing

## Documentation

- [Supermarket ERP system documentation](docs/neuedge-supermarket-erp-system-documentation.md)

## Notes

- Service-repair frontend routes and API controllers have been removed from the copied ISS surface.
- AI assistant capabilities were not copied into this product.
- Historical service-domain types remain in backend projects where still required by existing migrations and shared sales/procurement records.
