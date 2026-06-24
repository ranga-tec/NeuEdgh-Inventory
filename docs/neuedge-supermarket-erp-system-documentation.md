# NeuEdge Inv Supermarket ERP System Documentation

## 1. Purpose

NeuEdge Inv is a supermarket-focused inventory and retail ERP. The system should manage product master data, procurement, receiving, stock control, expiry-sensitive inventory, FIFO sales, retail counter sales, utility payments, bundle products, promotions, finance postings, and operational reporting.

This document defines the target system behavior for the supermarket version and the detailed requirements for the next capabilities:

- Item replenishment
- Expiry date handling
- FIFO stock issuing
- Bundle items
- Promotion creation

The document is written as the working specification for product owners, developers, testers, and implementation teams.

## 2. Product Scope

### 2.1 In Scope

- Supermarket item catalog and barcode-driven selling
- Multi-warehouse and multi-bin inventory
- Batch and expiry tracking for relevant items
- FIFO and FEFO stock movement rules
- Reorder planning and replenishment suggestions
- Purchase requisition and purchase order generation from replenishment demand
- Goods receiving with expiry capture
- Counter sales and sales invoices
- Airtime and bill payment transaction recording
- Bundle item setup and selling
- Promotion setup, approval, activation, and expiry
- Stock ledger, valuation, expiry, replenishment, promotion, and sales reports
- Role-based permissions and audit trail

### 2.2 Out of Scope

- Service repair workflows
- AI assistant capabilities
- Workshop job cards, technician assignments, quality checks, and equipment handover flows
- Manufacturing or recipe production beyond simple retail bundle assembly/sale logic

Historical service tables may remain in the database while older migrations still depend on them, but they should not appear in the supermarket user interface or daily workflows.

## 3. Users and Roles

### 3.1 Roles

- Admin: full system configuration, user management, approvals, override permissions
- Store Manager: replenishment review, promotions, stock adjustments, reporting
- Purchasing Officer: supplier RFQ, purchase requisition, purchase order, supplier follow-up
- Receiving Clerk: goods receipt, barcode/expiry/batch capture, supplier return initiation
- Inventory Controller: stock transfers, adjustments, batch corrections, expiry controls
- Cashier: counter sales, barcode scan, payment capture, utility payments
- Finance Officer: payments, AP/AR, credit notes, debit notes, cash/card reconciliation
- Auditor: read-only access to transactions, stock ledger, and audit logs

### 3.2 Permission Areas

- Master data
- Procurement
- Receiving
- Inventory
- Sales/POS
- Retail utilities
- Replenishment
- Promotions
- Bundle management
- Finance
- Reports
- Administration

Permissions should be granular enough to separate create, edit, approve, post, void, export, and override actions.

## 4. Core Master Data

### 4.1 Item Master

Each supermarket product should support:

- SKU
- Barcode and optional alternate barcodes
- Item name
- Brand
- Category and subcategory
- Unit of measure
- Pack size
- Tax code
- Costing method
- Tracking type: none, batch, serial, expiry, or batch plus expiry
- Reorder policy
- Preferred supplier
- Shelf life rules
- Price rules
- Active/inactive status

### 4.2 Item Tracking Settings

Expiry and FIFO behavior depends on item tracking settings:

- Non-tracked item: quantity tracked only at warehouse/bin level
- Batch-tracked item: quantity tracked by batch number
- Expiry-tracked item: quantity tracked by expiry date
- Batch plus expiry item: quantity tracked by batch number and expiry date

Examples:

- Rice bag: batch tracking optional
- Milk, yogurt, medicine, baby food: expiry tracking required
- Electronics accessory: no expiry
- Cigarette carton or regulated goods: batch tracking may be required

### 4.3 Reorder Settings

Reorder settings can be defined per item and warehouse:

- Minimum stock level
- Maximum stock level
- Reorder point
- Reorder quantity
- Safety stock
- Lead time days
- Average daily sales source
- Preferred supplier
- Purchase unit of measure
- Pack rounding rule
- Seasonal factor
- Exclude from auto-replenishment flag

If item-warehouse settings are missing, the system may use item-level defaults.

## 5. Inventory Foundation

### 5.1 Stock Dimensions

Inventory should be tracked by:

- Company
- Warehouse
- Bin/location
- Item
- Batch number, when applicable
- Expiry date, when applicable
- Serial number, when applicable
- Cost layer

### 5.2 Stock Ledger

Every stock movement must write a signed stock ledger entry.

Common movement types:

- Opening balance
- Purchase receipt
- Direct purchase
- Supplier return
- Sales issue
- Customer return
- Stock adjustment
- Stock transfer out
- Stock transfer in
- Bundle component issue
- Bundle finished item receipt, if pre-assembled
- Promotion/free item issue
- Expiry write-off

Ledger entries must be immutable after posting. Corrections should be made through reversal or adjustment documents.

## 6. Item Replenishment

### 6.1 Objective

The replenishment module helps supermarket users identify items that need restocking, calculate recommended purchase quantities, and convert approved recommendations into purchase requisitions or purchase orders.

### 6.2 Replenishment Inputs

The replenishment engine should consider:

- Current on-hand quantity
- Available quantity, excluding reserved stock
- Open purchase orders
- Open purchase requisitions
- Pending stock transfers
- Average daily sales
- Lead time
- Safety stock
- Minimum and maximum stock levels
- Supplier pack size
- Promotion forecast demand
- Seasonal demand factor
- Expiry risk and near-expiry stock exclusions

### 6.3 Calculation Rules

Recommended reorder quantity:

```text
projected_available = on_hand + open_po_qty + inbound_transfer_qty - reserved_qty
required_stock = demand_during_lead_time + safety_stock
shortage = required_stock - projected_available
recommended_qty = shortage rounded to supplier pack size
```

If maximum stock is defined:

```text
recommended_qty = max_stock - projected_available
```

The system should not recommend negative quantities. If calculated quantity is less than or equal to zero, no replenishment is required.

### 6.4 Replenishment Statuses

- Not Required
- Below Minimum
- Reorder Required
- Critical Stock
- Overstock
- Pending Purchase
- Pending Transfer
- Excluded

### 6.5 Replenishment Workflow

1. User opens Replenishment Workbench.
2. User filters by warehouse, category, supplier, critical status, or stockout risk.
3. System calculates recommended quantities.
4. User reviews item-level recommendations.
5. User adjusts quantity or supplier where permitted.
6. User selects lines for action.
7. System creates purchase requisition, purchase order, or stock transfer request.
8. The generated document follows normal approval and posting rules.

### 6.6 Replenishment Workbench Screen

Fields:

- Item
- SKU
- Barcode
- Category
- Warehouse
- On hand
- Available
- Reserved
- Open PO
- Average daily sales
- Lead time
- Safety stock
- Reorder point
- Minimum stock
- Maximum stock
- Recommended quantity
- Preferred supplier
- Last purchase cost
- Last received date
- Expiry risk indicator
- Action

Actions:

- Recalculate
- Create purchase requisition
- Create purchase order
- Create stock transfer request
- Exclude selected items
- Export report

### 6.7 Replenishment Reports

- Reorder Required Report
- Critical Stock Report
- Overstock Report
- Supplier Replenishment Plan
- Stockout Risk Report
- Replenishment Recommendation Audit

### 6.8 Acceptance Criteria

- The system recommends reorder quantities using configured stock rules.
- Open purchase orders reduce the recommended quantity.
- Items marked as excluded are not recommended.
- Supplier pack rounding is applied.
- Store Manager can review and adjust recommendations.
- Purchasing Officer can convert approved lines into purchase documents.
- All generated documents retain traceability back to the replenishment run.

## 7. Expiry Date Handling

### 7.1 Objective

Expiry handling ensures supermarket stock is received, stored, sold, reported, and written off according to expiry dates. It prevents expired goods from being sold and helps staff act before items expire.

### 7.2 Expiry Capture

Expiry date should be captured during:

- Opening stock entry
- Goods receipt
- Direct purchase
- Customer return, if returning expiry-tracked goods
- Stock adjustment
- Stock transfer receipt, if expiry data was missing or corrected

For expiry-tracked items, expiry date is mandatory before posting the document.

### 7.3 Shelf Life Rules

Item master may define:

- Minimum shelf life on receipt, in days
- Near-expiry warning threshold, in days
- Block sales after expiry
- Block receiving if expiry is too close
- Allow manager override
- Auto-write-off policy

Example:

- Fresh milk minimum shelf life on receipt: 5 days
- Near-expiry warning: 2 days
- Sales blocked after expiry: yes
- Receiving close-to-expiry stock requires manager approval

### 7.4 Expiry Statuses

- Valid
- Near Expiry
- Expired
- Blocked
- Written Off

### 7.5 Receiving Rules

When receiving expiry-tracked items:

- Expiry date must be entered.
- Batch number should be entered if supplier provides it.
- System validates expiry date against minimum shelf life.
- System warns if expiry is within near-expiry threshold.
- Manager override is required for short-dated goods if the item policy allows it.
- If the item policy blocks short-dated goods, posting is not allowed.

### 7.6 Sales Rules

During POS or invoice posting:

- Expired stock must not be issued.
- Near-expiry stock may be sold unless blocked by item policy.
- FIFO or FEFO selection must select valid stock layers only.
- If insufficient valid stock exists, sale must be blocked or require backorder handling.

### 7.7 Expiry Write-Off

Expired stock should be removed using a controlled write-off document:

- User selects expired batches.
- System calculates quantity and value.
- Store Manager approves.
- Posting reduces stock and records expense.
- Stock ledger records expiry write-off movement.

### 7.8 Expiry Dashboard

Dashboard widgets:

- Expiring today
- Expiring in 3 days
- Expiring in 7 days
- Expiring in 30 days
- Expired stock value
- Top categories by expiry risk
- Supplier short-dated receipt count

### 7.9 Expiry Reports

- Near-Expiry Stock Report
- Expired Stock Report
- Expiry Write-Off Report
- Supplier Short-Dated Goods Report
- Batch Expiry Traceability Report

### 7.10 Acceptance Criteria

- Expiry date is mandatory for expiry-tracked items at receiving.
- Expired stock cannot be sold.
- Near-expiry stock appears in alerts and reports.
- Expiry write-offs create stock ledger entries.
- Users can trace an item from receipt batch to sale/write-off.

## 8. FIFO and FEFO Stock Issuing

### 8.1 Definitions

FIFO means first in, first out. The system issues the oldest received stock first.

FEFO means first expiry, first out. The system issues the stock with the earliest expiry date first.

### 8.2 Rule Selection

Item master should define issue method:

- FIFO
- FEFO
- Manual batch selection

Recommended defaults:

- Expiry-tracked supermarket goods: FEFO
- Non-expiry goods: FIFO
- High-control goods: manual batch selection with manager permission

### 8.3 Cost Layers

Each receipt creates or updates a stock layer:

- Item
- Warehouse
- Bin
- Batch
- Expiry date
- Received date
- Quantity received
- Quantity remaining
- Unit cost
- Source document

FIFO uses received date order. FEFO uses expiry date first, then received date.

### 8.4 Issue Algorithm

When selling or issuing stock:

1. Load available stock layers for item and warehouse.
2. Exclude expired or blocked layers.
3. Sort by issue method.
4. Allocate required quantity across layers.
5. Write stock ledger entries per consumed layer.
6. Reduce layer remaining quantities.
7. If required quantity cannot be fully allocated, block posting.

### 8.5 Manual Override

Manual batch/layer selection should require permission and audit trail:

- Selected by user
- Reason
- Approved by
- Timestamp
- Original recommended layer
- Actual issued layer

### 8.6 FIFO/FEFO Acceptance Criteria

- FIFO issues older receipt layers first.
- FEFO issues earliest valid expiry first.
- Expired layers are skipped and blocked.
- Stock ledger records the actual consumed layer.
- Manual override requires permission and reason.

## 9. Bundle Items

### 9.1 Objective

Bundle items allow the supermarket to sell multiple items as one sellable product, for example:

- Breakfast pack
- School snack pack
- Buy shampoo plus conditioner pack
- Festival grocery pack
- Rice plus dhal family pack

### 9.2 Bundle Types

Supported bundle models:

- Dynamic bundle: components are deducted at sale time.
- Pre-assembled bundle: components are consumed in advance and finished bundle stock is received.
- Price-only bundle: components are listed for promotion but stock is handled as normal individual sale lines.

Recommended first implementation: dynamic bundle.

### 9.3 Bundle Master Data

Bundle header:

- Bundle SKU
- Bundle barcode
- Bundle name
- Description
- Category
- Active date range
- Selling price
- Tax code
- Bundle type
- Active/inactive status

Bundle components:

- Component item
- Quantity
- Unit of measure
- Optional/mandatory flag
- Substitute group, optional
- Component cost contribution

### 9.4 Bundle Availability

Available bundle quantity is calculated as:

```text
floor(min(component_available_qty / component_required_qty))
```

Example:

- Bundle requires 2 noodles and 1 soft drink.
- Noodles available: 20
- Soft drinks available: 7
- Bundle availability: 7

### 9.5 Bundle Sale Workflow

1. Cashier scans bundle barcode.
2. POS adds one bundle line.
3. System expands bundle internally into stock component requirements.
4. System checks component availability.
5. System applies bundle price.
6. On posting, stock ledger issues component items.
7. Sales document stores both bundle header and component issue trace.

### 9.6 Bundle Costing

Bundle cost should be calculated from component issue cost:

- FIFO/FEFO component cost layers are consumed.
- Total bundle cost is sum of component costs.
- Gross profit is bundle selling price minus component cost.

### 9.7 Bundle Returns

Return policy options:

- Return full bundle only
- Return individual components
- Manager approval required

Recommended default: full bundle return only for simple cashier workflow.

### 9.8 Bundle Reports

- Bundle Sales Report
- Bundle Profitability Report
- Bundle Component Usage Report
- Bundle Availability Report

### 9.9 Bundle Acceptance Criteria

- User can create a bundle with component quantities.
- POS can sell a bundle by barcode.
- Component stock is reduced, not only bundle header stock.
- Bundle availability is calculated from component stock.
- Bundle profitability uses actual issued component costs.

## 10. Promotion Creation

### 10.1 Objective

Promotion creation allows supermarket users to define discount and offer rules that automatically apply during sales within approved dates and conditions.

### 10.2 Promotion Types

Initial promotion types:

- Percentage discount
- Fixed amount discount
- Special price
- Buy X get Y free
- Bundle price
- Category discount
- Brand discount
- Quantity break discount
- Member/customer group discount

Future promotion types:

- Coupon code
- Loyalty points multiplier
- Supplier-funded promotion
- Time-of-day promotion
- Branch-specific promotion

### 10.3 Promotion Header

Fields:

- Promotion code
- Promotion name
- Description
- Promotion type
- Start date and time
- End date and time
- Store/warehouse scope
- Customer scope
- Priority
- Stackable flag
- Maximum discount amount
- Budget amount, optional
- Status
- Approval information

### 10.4 Promotion Lines

Promotion line fields depend on promotion type:

- Item
- Category
- Brand
- Buy quantity
- Get item
- Get quantity
- Discount percent
- Discount amount
- Promotional price
- Minimum basket value
- Maximum uses per transaction
- Maximum uses per customer

### 10.5 Promotion Statuses

- Draft
- Pending Approval
- Approved
- Active
- Paused
- Expired
- Cancelled

Only approved promotions can become active.

### 10.6 Promotion Validation

The system should validate:

- End date is after start date.
- Discount value is valid for the promotion type.
- Required item/category/brand fields are selected.
- Promotion does not conflict with non-stackable active promotions unless override is approved.
- User has permission to approve promotions.
- Promotion does not sell below minimum margin unless allowed.

### 10.7 Promotion Application at POS

1. Cashier scans items.
2. System identifies active promotions by date/time, store, item, category, brand, and customer.
3. System sorts applicable promotions by priority.
4. Non-stackable promotion rules are applied.
5. Discount is calculated and shown clearly.
6. Sales document stores promotion code, discount amount, and promotion snapshot.

### 10.8 Promotion Conflict Rules

If multiple promotions apply:

- Highest priority promotion is applied first.
- If stackable is false, no further promotions apply to the same line.
- If stackable is true, additional promotions may apply until max discount is reached.
- Manager override may be required for below-cost selling.

### 10.9 Promotion Reports

- Active Promotions Report
- Promotion Sales Report
- Promotion Margin Report
- Promotion Usage Report
- Expired Promotions Report
- Supplier-Funded Promotion Claim Report

### 10.10 Promotion Acceptance Criteria

- Store Manager can create a draft promotion.
- Admin or authorized manager can approve promotions.
- Active promotions apply automatically at POS.
- Expired promotions stop applying automatically.
- Sales documents retain promotion traceability.
- Reports show sales, discount, and margin impact per promotion.

## 11. Retail Utility Payments

The current system includes retail utility payment capture for:

- Airtime reloads
- Bill payments

Required operational behavior:

- Capture provider, account/mobile number, amount, fee, and reference number.
- Track transaction status: draft, completed, failed, reversed.
- Post successful payments to cash/card settlement and income/fee accounts where applicable.
- Allow reversal only with permission and reason.
- Provide daily cashier utility transaction report.

## 12. Finance and Accounting Integration

### 12.1 Posting Principles

Every posted operational transaction should create financial or inventory impact:

- Purchase receipt increases inventory value.
- Supplier invoice records AP liability.
- Sale records revenue, tax, COGS, and inventory reduction.
- Expiry write-off records inventory reduction and write-off expense.
- Promotion discount records sales discount.
- Bundle sale records revenue and component COGS.
- Utility payment records cash movement and fee/income according to configuration.

### 12.2 Required Accounts

Configuration should include:

- Inventory control account
- Cost of goods sold account
- Sales revenue account
- Sales discount account
- Tax payable account
- Supplier payable account
- Cash account
- Card settlement account
- Expiry write-off expense account
- Promotion funding receivable account, optional
- Utility commission income account

## 13. Audit and Controls

All sensitive actions must be audited:

- Promotion approval
- Promotion cancellation
- Manual FIFO/FEFO override
- Expired stock sale override, if ever allowed
- Expiry write-off approval
- Replenishment recommendation adjustment
- Purchase document creation from replenishment
- Bundle setup changes
- Price changes
- Stock adjustment
- Utility payment reversal

Audit records should include:

- User
- Timestamp
- Action
- Entity type
- Entity ID
- Previous value
- New value
- Reason
- Approval reference, when applicable

## 14. API Design

### 14.1 Replenishment APIs

- `GET /api/replenishment/recommendations`
- `POST /api/replenishment/runs`
- `GET /api/replenishment/runs/{id}`
- `POST /api/replenishment/runs/{id}/create-purchase-requisition`
- `POST /api/replenishment/runs/{id}/create-purchase-order`
- `POST /api/replenishment/runs/{id}/create-transfer-request`

### 14.2 Expiry APIs

- `GET /api/inventory/expiry/near-expiry`
- `GET /api/inventory/expiry/expired`
- `POST /api/inventory/expiry/write-offs`
- `POST /api/inventory/expiry/write-offs/{id}/approve`
- `POST /api/inventory/expiry/write-offs/{id}/post`

### 14.3 Bundle APIs

- `GET /api/retail/bundles`
- `POST /api/retail/bundles`
- `GET /api/retail/bundles/{id}`
- `PUT /api/retail/bundles/{id}`
- `POST /api/retail/bundles/{id}/activate`
- `POST /api/retail/bundles/{id}/deactivate`
- `GET /api/retail/bundles/{id}/availability`

### 14.4 Promotion APIs

- `GET /api/retail/promotions`
- `POST /api/retail/promotions`
- `GET /api/retail/promotions/{id}`
- `PUT /api/retail/promotions/{id}`
- `POST /api/retail/promotions/{id}/submit`
- `POST /api/retail/promotions/{id}/approve`
- `POST /api/retail/promotions/{id}/pause`
- `POST /api/retail/promotions/{id}/cancel`
- `POST /api/retail/promotions/evaluate`

## 15. Suggested Database Entities

### 15.1 Replenishment

- `ReplenishmentRun`
- `ReplenishmentRunLine`
- `ItemReorderPolicy`
- `SupplierLeadTime`
- `SalesVelocitySnapshot`

### 15.2 Expiry and FIFO

- `StockLayer`
- `StockReservation`
- `ExpiryWriteOff`
- `ExpiryWriteOffLine`
- `BatchStatus`

The existing stock ledger should continue as the source of movement history. `StockLayer` should represent remaining issueable quantities for FIFO/FEFO allocation.

### 15.3 Bundles

- `Bundle`
- `BundleComponent`
- `BundleSaleExpansion`

### 15.4 Promotions

- `Promotion`
- `PromotionLine`
- `PromotionApproval`
- `PromotionApplication`
- `PromotionUsageLimit`

## 16. Frontend Navigation

Recommended menu structure:

- Retail
  - POS
  - Airtime and Bill Pay
  - Promotions
  - Bundles
- Inventory
  - On Hand
  - Stock Ledger
  - Replenishment
  - Expiry Dashboard
  - Expiry Write-Offs
  - Adjustments
  - Transfers
- Procurement
  - Purchase Requisitions
  - Purchase Orders
  - Goods Receipts
  - Supplier Returns
- Reports
  - Stock Reports
  - Expiry Reports
  - Replenishment Reports
  - Promotion Reports
  - Sales Reports
  - Finance Reports

## 17. Implementation Phases

### Phase 1: Documentation and Data Foundation

- Finalize this specification.
- Add item tracking settings for expiry/FIFO behavior.
- Add stock layer model.
- Add reorder policy per item and warehouse.
- Add permissions for replenishment, expiry, bundles, and promotions.

### Phase 2: Expiry and FIFO Core

- Capture expiry date on receiving.
- Build FIFO/FEFO allocation service.
- Block expired stock sales.
- Add near-expiry and expired stock reports.
- Add expiry write-off document.

### Phase 3: Replenishment

- Build replenishment calculation service.
- Add replenishment workbench.
- Convert selected recommendations into purchase requisitions or purchase orders.
- Add replenishment reports.

### Phase 4: Bundles

- Add bundle master data.
- Add bundle availability calculation.
- Add POS bundle sale expansion.
- Add bundle profitability report.

### Phase 5: Promotions

- Add promotion master data and approval flow.
- Add promotion evaluation engine.
- Apply promotions at POS.
- Add promotion reporting and margin controls.

### Phase 6: Hardening

- Add integration tests for posting flows.
- Add audit reports.
- Add performance indexes for POS promotion lookup and FIFO allocation.
- Add role-based UAT checklists.
- Add backup and restore procedure validation.

## 18. Testing Strategy

### 18.1 Unit Tests

- Reorder quantity calculation
- FIFO layer selection
- FEFO layer selection
- Expiry validation
- Bundle availability
- Promotion eligibility and stacking

### 18.2 Integration Tests

- Goods receipt creates stock layers.
- POS sale consumes correct FIFO/FEFO layers.
- Expired stock cannot be sold.
- Replenishment run creates purchase requisition.
- Bundle sale consumes components.
- Promotion applies and stores trace.

### 18.3 Manual UAT Scenarios

- Receive milk with expiry date and sell using FEFO.
- Try selling expired yogurt and confirm sale is blocked.
- Create replenishment recommendation for low-stock rice.
- Convert recommendation into purchase order.
- Create breakfast bundle and sell it at POS.
- Create buy 2 get 1 free promotion and verify POS discount.
- Expire promotion and confirm it no longer applies.

## 19. Reporting Requirements

Operational reports:

- Daily Sales Summary
- Cashier Shift Summary
- Stock On Hand
- Stock Ledger
- Reorder Required
- Critical Stock
- Near Expiry
- Expired Stock
- Expiry Write-Off
- Bundle Sales
- Bundle Profitability
- Active Promotions
- Promotion Margin
- Utility Payment Transactions

Management reports:

- Category Sales
- Supplier Performance
- Inventory Valuation
- Slow Moving Stock
- Fast Moving Stock
- Gross Margin by Item
- Promotion Return on Investment
- Stock Loss and Write-Off Summary

## 20. Key Design Decisions

- Use a separate supermarket product copy instead of one large service-plus-supermarket ERP toggle.
- Keep service repair screens and APIs out of the supermarket user surface.
- Use the new `neuedge_inv` PostgreSQL database for NeuEdge Inv.
- Use FIFO for general non-expiry stock and FEFO for expiry-sensitive stock.
- Implement dynamic bundles first because they avoid separate assembly stock complexity.
- Require approval for promotions and expiry write-offs.
- Keep stock ledger immutable and use reversal/adjustment for corrections.

## 21. Open Decisions

These decisions should be confirmed before implementation:

- Should POS allow negative stock for trusted users, or always block it?
- Should near-expiry stock be discounted automatically or only reported?
- Should bundle returns allow component-level returns?
- Should promotions be branch-specific from day one?
- Should supplier-funded promotions create receivable claims automatically?
- Should utility payment provider integration be external API based or manual transaction capture only in the first version?

## 22. Success Criteria

The supermarket ERP is ready for operational rollout when:

- Users can manage product master data with expiry and reorder settings.
- Goods receipt captures batch/expiry data for required items.
- POS consumes stock using FIFO/FEFO and blocks expired goods.
- Replenishment recommendations are reliable and convertible into purchase documents.
- Bundles can be sold and component stock is deducted correctly.
- Promotions apply automatically and expire automatically.
- Store managers can monitor near-expiry, stockout risk, and promotion margin impact.
- Audit trail exists for approvals, overrides, and sensitive inventory changes.
