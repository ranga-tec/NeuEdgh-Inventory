using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupermarketInventoryPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bundles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    TaxCodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bundles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bundles_ItemCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ItemCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bundles_TaxCodes_TaxCodeId",
                        column: x => x.TaxCodeId,
                        principalTable: "TaxCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpiryWriteOffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VoidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VoidReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpiryWriteOffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpiryWriteOffs_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemPacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BaseQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PurchaseAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    SaleAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    TransferAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefaultPurchasePack = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefaultSalesPack = table.Column<bool>(type: "boolean", nullable: false),
                    TaxCodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPacks_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemPacks_TaxCodes_TaxCodeId",
                        column: x => x.TaxCodeId,
                        principalTable: "TaxCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsStackable = table.Column<bool>(type: "boolean", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promotions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReplenishmentRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PurchaseRequisitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplenishmentRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReplenishmentRuns_PurchaseRequisitions_PurchaseRequisitionId",
                        column: x => x.PurchaseRequisitionId,
                        principalTable: "PurchaseRequisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReplenishmentRuns_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockLayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseBinId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainingQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockLayers_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockLayers_WarehouseBins_WarehouseBinId",
                        column: x => x.WarehouseBinId,
                        principalTable: "WarehouseBins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockLayers_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BundleComponent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BundleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BundleComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BundleComponent_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BundleComponent_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PromotionLine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemPackId = table.Column<Guid>(type: "uuid", nullable: true),
                    BundleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: true),
                    BuyQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    GetItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    GetQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    SpecialPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MinimumBasketAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionLine_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionLine_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionLine_ItemCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ItemCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionLine_ItemPacks_ItemPackId",
                        column: x => x.ItemPackId,
                        principalTable: "ItemPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionLine_Items_GetItemId",
                        column: x => x.GetItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionLine_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionLine_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReplenishmentRunLine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplenishmentRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnHand = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    OpenPurchaseQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReorderPoint = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReorderQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RecommendedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PreferredSupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreferredPackId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecommendedPackQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplenishmentRunLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReplenishmentRunLine_ItemPacks_PreferredPackId",
                        column: x => x.PreferredPackId,
                        principalTable: "ItemPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReplenishmentRunLine_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReplenishmentRunLine_ReplenishmentRuns_ReplenishmentRunId",
                        column: x => x.ReplenishmentRunId,
                        principalTable: "ReplenishmentRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplenishmentRunLine_Suppliers_PreferredSupplierId",
                        column: x => x.PreferredSupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpiryWriteOffLine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiryWriteOffId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpiryWriteOffLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpiryWriteOffLine_ExpiryWriteOffs_ExpiryWriteOffId",
                        column: x => x.ExpiryWriteOffId,
                        principalTable: "ExpiryWriteOffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpiryWriteOffLine_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpiryWriteOffLine_StockLayers_StockLayerId",
                        column: x => x.StockLayerId,
                        principalTable: "StockLayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BundleComponent_BundleId_ItemId",
                table: "BundleComponent",
                columns: new[] { "BundleId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_BundleComponent_ItemId",
                table: "BundleComponent",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_Barcode",
                table: "Bundles",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_CategoryId",
                table: "Bundles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_CompanyId_IsActive",
                table: "Bundles",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_CompanyId_Sku",
                table: "Bundles",
                columns: new[] { "CompanyId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_TaxCodeId",
                table: "Bundles",
                column: "TaxCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiryWriteOffLine_ExpiryWriteOffId",
                table: "ExpiryWriteOffLine",
                column: "ExpiryWriteOffId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiryWriteOffLine_ItemId",
                table: "ExpiryWriteOffLine",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiryWriteOffLine_StockLayerId",
                table: "ExpiryWriteOffLine",
                column: "StockLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiryWriteOffs_Number",
                table: "ExpiryWriteOffs",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpiryWriteOffs_WarehouseId_Status",
                table: "ExpiryWriteOffs",
                columns: new[] { "WarehouseId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemPacks_Barcode",
                table: "ItemPacks",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPacks_ItemId_Code",
                table: "ItemPacks",
                columns: new[] { "ItemId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPacks_ItemId_IsActive",
                table: "ItemPacks",
                columns: new[] { "ItemId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemPacks_TaxCodeId",
                table: "ItemPacks",
                column: "TaxCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionLine_BrandId",
                table: "PromotionLine",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionLine_BundleId",
                table: "PromotionLine",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionLine_CategoryId",
                table: "PromotionLine",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionLine_GetItemId",
                table: "PromotionLine",
                column: "GetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionLine_ItemId",
                table: "PromotionLine",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionLine_ItemPackId",
                table: "PromotionLine",
                column: "ItemPackId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionLine_PromotionId",
                table: "PromotionLine",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_CompanyId_Code",
                table: "Promotions",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_CompanyId_Status_StartsAt_EndsAt",
                table: "Promotions",
                columns: new[] { "CompanyId", "Status", "StartsAt", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentRunLine_ItemId",
                table: "ReplenishmentRunLine",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentRunLine_PreferredPackId",
                table: "ReplenishmentRunLine",
                column: "PreferredPackId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentRunLine_PreferredSupplierId",
                table: "ReplenishmentRunLine",
                column: "PreferredSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentRunLine_ReplenishmentRunId",
                table: "ReplenishmentRunLine",
                column: "ReplenishmentRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentRuns_Number",
                table: "ReplenishmentRuns",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentRuns_PurchaseRequisitionId",
                table: "ReplenishmentRuns",
                column: "PurchaseRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplenishmentRuns_WarehouseId_Status_CalculatedAt",
                table: "ReplenishmentRuns",
                columns: new[] { "WarehouseId", "Status", "CalculatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockLayers_ItemId_BatchNumber",
                table: "StockLayers",
                columns: new[] { "ItemId", "BatchNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_StockLayers_WarehouseBinId",
                table: "StockLayers",
                column: "WarehouseBinId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLayers_WarehouseId_ItemId_Status_ExpiryDate",
                table: "StockLayers",
                columns: new[] { "WarehouseId", "ItemId", "Status", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockLayers_WarehouseId_WarehouseBinId_ItemId",
                table: "StockLayers",
                columns: new[] { "WarehouseId", "WarehouseBinId", "ItemId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BundleComponent");

            migrationBuilder.DropTable(
                name: "ExpiryWriteOffLine");

            migrationBuilder.DropTable(
                name: "PromotionLine");

            migrationBuilder.DropTable(
                name: "ReplenishmentRunLine");

            migrationBuilder.DropTable(
                name: "ExpiryWriteOffs");

            migrationBuilder.DropTable(
                name: "StockLayers");

            migrationBuilder.DropTable(
                name: "Bundles");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "ItemPacks");

            migrationBuilder.DropTable(
                name: "ReplenishmentRuns");
        }
    }
}
