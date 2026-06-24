# NeuEdge Inv Supermarket ERP System Documentation

## 1. Purpose

NeuEdge Inv is a supermarket-focused inventory and retail ERP. The system should manage product master data, procurement, receiving, stock control, expiry-sensitive inventory, FIFO sales, retail counter sales, utility payments, bundle products, promotions, finance postings, and operational reporting.

This document defines the target system behavior for the supermarket version and the detailed requirements for the next capabilities:

- Item replenishment
- Expiry date handling
- FIFO stock issuing
- Pack items
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
- Pack item setup and pack-size selling
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
- Pack item management
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
- Base unit and pack conversion
- Inner pack, case, carton, and loose-unit selling rules
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
- Pack conversion
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

## 9. Pack Items

### 9.1 Objective

Pack items allow the supermarket to buy, stock, transfer, and sell the same product in different pack sizes while maintaining one reliable inventory position.

Examples:

- 1 bottle, 6-pack, and 24-bottle carton of soft drinks
- Single soap, 3-pack soap, and 12-pack case
- Loose biscuit packet and display box
- 1 kg rice bag and 5 kg rice bag, when sold as distinct SKUs
- Cigarette stick, pack, and carton, where regulation allows

Pack handling is different from bundle handling. A pack is the same item in a different quantity or packaging unit. A bundle is a combination of different component items sold together.

### 9.2 Pack Models

Supported pack models:

- Same item, multiple UoMs: one item has base unit and pack conversions.
- Separate sellable pack SKUs: each pack has its own barcode and price but converts to a base item.
- Break-pack workflow: a carton or case is opened and converted into loose units.
- Supplier purchase pack: purchase in cases/cartons while selling in units.

Recommended first implementation: same item with multiple UoMs and separate barcode per pack size.

### 9.3 Pack Master Data

Pack item fields:

- Item
- Pack code
- Pack barcode
- Pack name
- Base unit quantity
- Pack unit of measure
- Purchase allowed flag
- Sale allowed flag
- Transfer allowed flag
- Default purchase pack flag
- Default sales pack flag
- Price
- Tax code override, optional
- Active/inactive status

Example:

| Pack | Barcode | Base quantity |
| --- | --- | --- |
| Soft drink single bottle | 479000000001 | 1 bottle |
| Soft drink 6-pack | 479000000006 | 6 bottles |
| Soft drink carton | 479000000024 | 24 bottles |

### 9.4 Stock Keeping Rule

The system should keep stock in base units internally.

Example:

- Receive 10 cartons.
- 1 carton = 24 bottles.
- System stores 240 bottles as stock quantity.
- User can still view it as 10 cartons, 40 six-packs, or 240 bottles.

This avoids separate stock balances for the same product and prevents mismatch between carton stock and loose-unit stock.

### 9.5 Purchasing Packs

Purchase documents should support supplier pack selection:

- Purchase unit
- Pack quantity
- Base quantity conversion
- Unit cost per pack
- Unit cost per base unit
- Supplier barcode or supplier item code

Example:

- Purchase quantity: 5 cartons
- Carton conversion: 24 bottles
- Base quantity received: 120 bottles
- Carton cost: 7,200
- Base unit cost: 300 per bottle

### 9.6 Receiving Packs with Expiry

For expiry-tracked pack items:

- Expiry is captured once per received pack/batch line.
- Converted base quantity inherits the same batch and expiry.
- FIFO/FEFO stock layers store base quantity, batch, expiry, received date, and cost.

Example:

- Receive 3 cartons of yogurt drinks.
- 1 carton = 12 bottles.
- Expiry date = 2026-07-05.
- Stock layer quantity = 36 bottles with expiry 2026-07-05.

### 9.7 Selling Packs

POS should allow scanning any active pack barcode:

- Single barcode sells 1 base unit.
- Six-pack barcode sells 6 base units.
- Carton barcode sells 24 base units.

The sales document should show the scanned pack and quantity, while inventory deduction uses base quantity.

### 9.8 Pack Pricing

Each pack can have its own price:

- Single bottle: 350
- 6-pack: 1,950
- Carton: 7,200

The system should not calculate pack price only from base unit price unless configured to do so. Supermarkets often set independent prices by pack size.

### 9.9 Break-Pack Workflow

If the business wants controlled conversion from carton stock to loose-unit display stock, the system should support a break-pack document:

1. User selects source pack or stock layer.
2. User enters pack quantity to break.
3. System calculates resulting base-unit quantity.
4. User selects destination bin, such as sales floor.
5. Posting records stock movement and audit trail.

If stock is already held only in base units, break-pack may be optional and used mainly for operational tracking.

### 9.10 Pack Replenishment

Replenishment should recommend purchase packs, not only base units.

Example:

- Required base quantity: 50 bottles.
- Preferred supplier pack: carton of 24.
- Recommended purchase quantity: 3 cartons.
- Resulting base quantity: 72 bottles.

Replenishment should show both:

- Recommended pack quantity
- Equivalent base quantity

### 9.11 Pack FIFO/FEFO Rules

FIFO/FEFO allocation should consume base-unit stock layers. When a pack is sold, the system allocates the converted base quantity.

Example:

- Customer buys 1 six-pack.
- Required base quantity = 6 bottles.
- FEFO allocates 6 bottles from earliest valid expiry layer.

If one pack sale spans multiple layers, the sales trace should record all consumed layers.

### 9.12 Pack Promotions

Promotions may apply to:

- Specific pack barcode
- All packs of an item
- Minimum base quantity
- Buy one carton, get single item free
- Case discount

Promotion rules must clearly define whether the offer applies to one pack size or every pack of the same item.

### 9.13 Pack Reports

- Pack Sales Report
- Pack Margin Report
- Pack Conversion Report
- Break-Pack Audit Report
- Supplier Pack Purchase Report
- Pack Barcode List

### 9.14 Pack Acceptance Criteria

- User can define multiple pack sizes for an item.
- Each pack can have a separate barcode and price.
- Purchases can be entered in supplier packs.
- Stock is stored and costed in base units.
- POS can sell by scanning pack barcode.
- FIFO/FEFO consumes the correct converted base quantity.
- Replenishment recommends supplier pack quantities.
- Reports show both pack quantity and equivalent base quantity.

## 10. Bundle Items

### 10.1 Objective

Bundle items allow the supermarket to sell multiple items as one sellable product, for example:

- Breakfast pack
- School snack pack
- Buy shampoo plus conditioner pack
- Festival grocery pack
- Rice plus dhal family pack

### 10.2 Bundle Types

Supported bundle models:

- Dynamic bundle: components are deducted at sale time.
- Pre-assembled bundle: components are consumed in advance and finished bundle stock is received.
- Price-only bundle: components are listed for promotion but stock is handled as normal individual sale lines.

Recommended first implementation: dynamic bundle.

### 10.3 Bundle Master Data

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

### 10.4 Bundle Availability

Available bundle quantity is calculated as:

```text
floor(min(component_available_qty / component_required_qty))
```

Example:

- Bundle requires 2 noodles and 1 soft drink.
- Noodles available: 20
- Soft drinks available: 7
- Bundle availability: 7

### 10.5 Bundle Sale Workflow

1. Cashier scans bundle barcode.
2. POS adds one bundle line.
3. System expands bundle internally into stock component requirements.
4. System checks component availability.
5. System applies bundle price.
6. On posting, stock ledger issues component items.
7. Sales document stores both bundle header and component issue trace.

### 10.6 Bundle Costing

Bundle cost should be calculated from component issue cost:

- FIFO/FEFO component cost layers are consumed.
- Total bundle cost is sum of component costs.
- Gross profit is bundle selling price minus component cost.

### 10.7 Bundle Returns

Return policy options:

- Return full bundle only
- Return individual components
- Manager approval required

Recommended default: full bundle return only for simple cashier workflow.

### 10.8 Bundle Reports

- Bundle Sales Report
- Bundle Profitability Report
- Bundle Component Usage Report
- Bundle Availability Report

### 10.9 Bundle Acceptance Criteria

- User can create a bundle with component quantities.
- POS can sell a bundle by barcode.
- Component stock is reduced, not only bundle header stock.
- Bundle availability is calculated from component stock.
- Bundle profitability uses actual issued component costs.

## 11. Promotion Creation

### 11.1 Objective

Promotion creation allows supermarket users to define discount and offer rules that automatically apply during sales within approved dates and conditions.

### 11.2 Promotion Types

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

### 11.3 Promotion Header

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

### 11.4 Promotion Lines

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

### 11.5 Promotion Statuses

- Draft
- Pending Approval
- Approved
- Active
- Paused
- Expired
- Cancelled

Only approved promotions can become active.

### 11.6 Promotion Validation

The system should validate:

- End date is after start date.
- Discount value is valid for the promotion type.
- Required item/category/brand fields are selected.
- Promotion does not conflict with non-stackable active promotions unless override is approved.
- User has permission to approve promotions.
- Promotion does not sell below minimum margin unless allowed.

### 11.7 Promotion Application at POS

1. Cashier scans items.
2. System identifies active promotions by date/time, store, item, category, brand, and customer.
3. System sorts applicable promotions by priority.
4. Non-stackable promotion rules are applied.
5. Discount is calculated and shown clearly.
6. Sales document stores promotion code, discount amount, and promotion snapshot.

### 11.8 Promotion Conflict Rules

If multiple promotions apply:

- Highest priority promotion is applied first.
- If stackable is false, no further promotions apply to the same line.
- If stackable is true, additional promotions may apply until max discount is reached.
- Manager override may be required for below-cost selling.

### 11.9 Promotion Reports

- Active Promotions Report
- Promotion Sales Report
- Promotion Margin Report
- Promotion Usage Report
- Expired Promotions Report
- Supplier-Funded Promotion Claim Report

### 11.10 Promotion Acceptance Criteria

- Store Manager can create a draft promotion.
- Admin or authorized manager can approve promotions.
- Active promotions apply automatically at POS.
- Expired promotions stop applying automatically.
- Sales documents retain promotion traceability.
- Reports show sales, discount, and margin impact per promotion.

## 12. Retail Utility Payments

The current system includes retail utility payment capture for:

- Airtime reloads
- Bill payments

Required operational behavior:

- Capture provider, account/mobile number, amount, fee, and reference number.
- Track transaction status: draft, completed, failed, reversed.
- Post successful payments to cash/card settlement and income/fee accounts where applicable.
- Allow reversal only with permission and reason.
- Provide daily cashier utility transaction report.

## 13. Finance and Accounting Integration

### 13.1 Posting Principles

Every posted operational transaction should create financial or inventory impact:

- Purchase receipt increases inventory value.
- Supplier invoice records AP liability.
- Sale records revenue, tax, COGS, and inventory reduction.
- Pack sale records revenue at pack price and inventory reduction in base units.
- Expiry write-off records inventory reduction and write-off expense.
- Promotion discount records sales discount.
- Bundle sale records revenue and component COGS.
- Utility payment records cash movement and fee/income according to configuration.

### 13.2 Required Accounts

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

## 14. Audit and Controls

All sensitive actions must be audited:

- Promotion approval
- Promotion cancellation
- Manual FIFO/FEFO override
- Expired stock sale override, if ever allowed
- Expiry write-off approval
- Replenishment recommendation adjustment
- Purchase document creation from replenishment
- Pack conversion and pack barcode changes
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

## 15. API Design

### 15.1 Replenishment APIs

- `GET /api/replenishment/recommendations`
- `POST /api/replenishment/runs`
- `GET /api/replenishment/runs/{id}`
- `POST /api/replenishment/runs/{id}/create-purchase-requisition`
- `POST /api/replenishment/runs/{id}/create-purchase-order`
- `POST /api/replenishment/runs/{id}/create-transfer-request`

### 15.2 Expiry APIs

- `GET /api/inventory/expiry/near-expiry`
- `GET /api/inventory/expiry/expired`
- `POST /api/inventory/expiry/write-offs`
- `POST /api/inventory/expiry/write-offs/{id}/approve`
- `POST /api/inventory/expiry/write-offs/{id}/post`

### 15.3 Pack APIs

- `GET /api/retail/packs`
- `POST /api/retail/packs`
- `GET /api/retail/packs/{id}`
- `PUT /api/retail/packs/{id}`
- `POST /api/retail/packs/{id}/activate`
- `POST /api/retail/packs/{id}/deactivate`
- `GET /api/retail/packs/barcode/{barcode}`
- `POST /api/inventory/pack-conversions`

### 15.4 Bundle APIs

- `GET /api/retail/bundles`
- `POST /api/retail/bundles`
- `GET /api/retail/bundles/{id}`
- `PUT /api/retail/bundles/{id}`
- `POST /api/retail/bundles/{id}/activate`
- `POST /api/retail/bundles/{id}/deactivate`
- `GET /api/retail/bundles/{id}/availability`

### 15.5 Promotion APIs

- `GET /api/retail/promotions`
- `POST /api/retail/promotions`
- `GET /api/retail/promotions/{id}`
- `PUT /api/retail/promotions/{id}`
- `POST /api/retail/promotions/{id}/submit`
- `POST /api/retail/promotions/{id}/approve`
- `POST /api/retail/promotions/{id}/pause`
- `POST /api/retail/promotions/{id}/cancel`
- `POST /api/retail/promotions/evaluate`

## 16. Suggested Database Entities

### 16.1 Replenishment

- `ReplenishmentRun`
- `ReplenishmentRunLine`
- `ItemReorderPolicy`
- `SupplierLeadTime`
- `SalesVelocitySnapshot`

### 16.2 Expiry and FIFO

- `StockLayer`
- `StockReservation`
- `ExpiryWriteOff`
- `ExpiryWriteOffLine`
- `BatchStatus`

The existing stock ledger should continue as the source of movement history. `StockLayer` should represent remaining issueable quantities for FIFO/FEFO allocation.

### 16.3 Packs

- `ItemPack`
- `ItemPackBarcode`
- `PackConversion`
- `PackConversionLine`

### 16.4 Bundles

- `Bundle`
- `BundleComponent`
- `BundleSaleExpansion`

### 16.5 Promotions

- `Promotion`
- `PromotionLine`
- `PromotionApproval`
- `PromotionApplication`
- `PromotionUsageLimit`

## 17. Frontend Navigation

Recommended menu structure:

- Retail
  - POS
  - Airtime and Bill Pay
  - Pack Items
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

## 18. Implementation Phases

### Phase 1: Documentation and Data Foundation

- Finalize this specification.
- Add item tracking settings for expiry/FIFO behavior.
- Add item pack and barcode setup.
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

### Phase 4: Pack Items

- Add pack master data.
- Add pack barcode lookup.
- Add purchase and receiving support for supplier packs.
- Add POS sale support for pack barcode scanning.
- Add replenishment pack rounding.
- Add pack sales and conversion reports.

### Phase 5: Bundles

- Add bundle master data.
- Add bundle availability calculation.
- Add POS bundle sale expansion.
- Add bundle profitability report.

### Phase 6: Promotions

- Add promotion master data and approval flow.
- Add promotion evaluation engine.
- Apply promotions at POS.
- Add promotion reporting and margin controls.

### Phase 7: Hardening

- Add integration tests for posting flows.
- Add audit reports.
- Add performance indexes for POS promotion lookup and FIFO allocation.
- Add role-based UAT checklists.
- Add backup and restore procedure validation.

## 19. Testing Strategy

### 19.1 Unit Tests

- Reorder quantity calculation
- FIFO layer selection
- FEFO layer selection
- Expiry validation
- Pack barcode conversion
- Bundle availability
- Promotion eligibility and stacking

### 19.2 Integration Tests

- Goods receipt creates stock layers.
- POS sale consumes correct FIFO/FEFO layers.
- Pack barcode sale consumes converted base quantity.
- Expired stock cannot be sold.
- Replenishment run creates purchase requisition.
- Bundle sale consumes components.
- Promotion applies and stores trace.

### 19.3 Manual UAT Scenarios

- Receive milk with expiry date and sell using FEFO.
- Try selling expired yogurt and confirm sale is blocked.
- Create single, six-pack, and carton barcodes for soft drinks and sell each pack size.
- Create replenishment recommendation for low-stock rice.
- Convert recommendation into purchase order.
- Create breakfast bundle and sell it at POS.
- Create buy 2 get 1 free promotion and verify POS discount.
- Expire promotion and confirm it no longer applies.

## 20. Reporting Requirements

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
- Pack Sales
- Pack Conversion
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

## 21. Key Design Decisions

- Use a separate supermarket product copy instead of one large service-plus-supermarket ERP toggle.
- Keep service repair screens and APIs out of the supermarket user surface.
- Use the new `neuedge_inv` PostgreSQL database for NeuEdge Inv.
- Use FIFO for general non-expiry stock and FEFO for expiry-sensitive stock.
- Store pack inventory in base units while allowing independent pack barcodes and prices.
- Implement dynamic bundles first because they avoid separate assembly stock complexity.
- Require approval for promotions and expiry write-offs.
- Keep stock ledger immutable and use reversal/adjustment for corrections.

## 22. Open Decisions

These decisions should be confirmed before implementation:

- Should POS allow negative stock for trusted users, or always block it?
- Should near-expiry stock be discounted automatically or only reported?
- Should break-pack be a mandatory operational document or only an optional audit document?
- Should every pack have independent pricing, or should some packs inherit price from base unit conversion?
- Should bundle returns allow component-level returns?
- Should promotions be branch-specific from day one?
- Should supplier-funded promotions create receivable claims automatically?
- Should utility payment provider integration be external API based or manual transaction capture only in the first version?

## 23. Success Criteria

The supermarket ERP is ready for operational rollout when:

- Users can manage product master data with expiry and reorder settings.
- Goods receipt captures batch/expiry data for required items.
- Users can buy, receive, scan, sell, and report pack items correctly.
- POS consumes stock using FIFO/FEFO and blocks expired goods.
- Replenishment recommendations are reliable and convertible into purchase documents.
- Bundles can be sold and component stock is deducted correctly.
- Promotions apply automatically and expire automatically.
- Store managers can monitor near-expiry, stockout risk, and promotion margin impact.
- Audit trail exists for approvals, overrides, and sensitive inventory changes.
