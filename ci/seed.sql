-- Seed for the schema as of migration AddGuildTaxModels.
-- Foreign keys ON so a bad seed fails loudly.
PRAGMA foreign_keys = ON;

INSERT INTO "AOGuildInfo" ("Id", "AlbionOnlineGuildName", "DiscordGuildId", "SilverTaxRate", "ItemValueTaxRate", "ResourceTaxRate")
VALUES (1, 'Test Guild A', 111111111111111111, 5.00, 10.00, 2.50),
       (2, 'Test Guild B', 222222222222222222, 0.00, 0.00, 0.00);

INSERT INTO "Players" ("Id", "AOGuildInfoId", "CreatedAt", "DiscordName", "DiscordPlayerId", "Name")
VALUES (1, 1,    '2026-10-01 00:00:00', 'alice#1', 1001, 'Alice'),
       (2, 1,    '2026-10-01 00:00:00', 'bob#2',   1002, 'Bob'),
       (3, NULL, '2026-10-01 00:00:00', 'cara#3',  1003, 'Cara');

INSERT INTO "LootSplit" ("Id", "SessionName", "Status", "TaxRate", "CreatedAt", "CompletedAt")
VALUES (1, 'Seed split', 'Completed', 0.05, '2026-10-02 00:00:00', '2026-10-02 01:00:00');

INSERT INTO "LootEntity" ("Id", "LootsplitId", "TaxableEntity", "ValueSilver", "AddedAt")
VALUES (1, 1, 'Item',      500000, '2026-10-02 00:10:00'),
       (2, 1, 'SilverBag', 250000, '2026-10-02 00:20:00');

INSERT INTO "Participants" ("Id", "LootsplitId", "PlayerId")
VALUES (1, 1, 1), (2, 1, 2), (3, 1, 3);

INSERT INTO "Calculations" ("Id", "CreatedAtUtc", "ItemTotal", "LootsplitId", "NetTotalAfterTax", "PerPersonNetShare", "RecipientCount", "Remainder", "SilverBagTotal", "TaxRatePercent", "TotalTaxAmount")
VALUES ('11111111-1111-1111-1111-111111111111', '2026-10-02 01:00:00', 750000, 1, 712500, 237500, 3, 0, 0, 5.00, 37500);

-- Payouts and tax entries: these are what a bad rebuild would cascade-delete
INSERT INTO "ParticipantPayout" ("Id", "CalculationEntityId", "AOGuildInfoId", "PlayerId", "GrossShare", "NetShare")
VALUES (1, '11111111-1111-1111-1111-111111111111', 1,    1, 250000, 237500),
       (2, '11111111-1111-1111-1111-111111111111', 1,    2, 250000, 237500),
       (3, '11111111-1111-1111-1111-111111111111', NULL, 3, 250000, 237500);

INSERT INTO "GuildTaxEntry" ("Id", "ParticipantPayoutId", "AOGuildInfoId", "TaxType", "TaxableAmountSilver", "RateApplied", "TaxAmountSilver", "CreatedAt", "PaidAt")
VALUES (1, 1, 1, 0, 250000, '0.05', 12500, '2026-10-02 01:00:00', NULL),
       (2, 2, 1, 0, 250000, '0.05', 12500, '2026-10-02 01:00:00', '2026-10-03 00:00:00'),
       (3, 1, 1, 1, 100000, '0.10', 10000, '2026-10-02 01:00:00', NULL);