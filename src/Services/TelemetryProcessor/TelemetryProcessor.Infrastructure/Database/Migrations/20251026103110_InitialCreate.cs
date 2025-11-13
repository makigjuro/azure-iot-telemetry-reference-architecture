using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemetryProcessor.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "telemetry");

            migrationBuilder.CreateTable(
                name: "alerts",
                schema: "telemetry",
                columns: table => new
                {
                    alert_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "text", nullable: false),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    acknowledged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    acknowledged_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    metadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.alert_id);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "telemetry",
                columns: table => new
                {
                    device_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    properties = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.device_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_device_id",
                schema: "telemetry",
                table: "alerts",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_device_timestamp",
                schema: "telemetry",
                table: "alerts",
                columns: new[] { "device_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_alerts_is_acknowledged",
                schema: "telemetry",
                table: "alerts",
                column: "is_acknowledged");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_severity",
                schema: "telemetry",
                table: "alerts",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_timestamp",
                schema: "telemetry",
                table: "alerts",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_devices_last_seen_at",
                schema: "telemetry",
                table: "devices",
                column: "last_seen_at");

            migrationBuilder.CreateIndex(
                name: "ix_devices_name",
                schema: "telemetry",
                table: "devices",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_devices_status",
                schema: "telemetry",
                table: "devices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_devices_type",
                schema: "telemetry",
                table: "devices",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts",
                schema: "telemetry");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "telemetry");
        }
    }
}
