CREATE TABLE unit_stats (
    type_id          TEXT PRIMARY KEY,
    size             INTEGER NOT NULL,
    attack_type      INTEGER NOT NULL,
    damage_type      INTEGER NOT NULL,
    armor_type       INTEGER NOT NULL,
    health           INTEGER NOT NULL,
    damage           INTEGER NOT NULL,
    speed            REAL    NOT NULL,
    attack_range     REAL    NOT NULL,
    attack_cooldown  REAL    NOT NULL,
    has_night_combat INTEGER NOT NULL DEFAULT 0,
    unit_color       INTEGER NOT NULL,
    vision_range     REAL    NOT NULL DEFAULT 170.0
);

INSERT INTO unit_stats VALUES
('Spearman',     1, 0, 0, 0, 40, 10, 80.0,  30.0,  1.0, 0, 4289733792, 250.0),
('ShieldBearer', 1, 0, 0, 1, 70,  5, 45.0,  30.0,  1.8, 0, 4286611584, 200.0),
('Archer',       1, 1, 1, 0, 25, 12, 60.0,  200.0, 1.5, 0, 4286627968, 300.0),
('Knight',       2, 0, 0, 1, 60, 18, 120.0, 35.0,  1.2, 0, 4291862624, 280.0),
('Scout',        0, 0, 0, 0, 15,  3, 160.0, 25.0,  1.0, 0, 4286644095, 400.0),
('Swordsman',    1, 0, 0, 0, 55, 16, 80.0,  30.0,  0.9, 0, 4285567184, 260.0),
('HeavyShield',  2, 0, 0, 1, 110, 8, 40.0,  35.0,  1.5, 0, 4284515808, 220.0),
('Crossbowman',  1, 1, 1, 0, 30, 22, 55.0,  180.0, 2.0, 0, 4283476000, 280.0),
('HeavyCavalry', 2, 0, 0, 1, 85, 28, 110.0, 35.0,  1.3, 0, 4288712672, 290.0),
('LightCavalry', 1, 0, 0, 0, 25,  8, 180.0, 30.0,  0.8, 0, 4286632191, 450.0);

CREATE TABLE building_defs (
    type_id           TEXT PRIMARY KEY,
    display_name      TEXT    NOT NULL,
    max_health        INTEGER NOT NULL,
    spawn_interval    REAL,
    main_cell_x       INTEGER DEFAULT 0,
    main_cell_y       INTEGER DEFAULT 0,
    spawn_cell_x      INTEGER DEFAULT 0,
    spawn_cell_y      INTEGER DEFAULT 0,
    unit_type_id      TEXT,
    has_night_combat  INTEGER DEFAULT 0,
    combine_tier       INTEGER DEFAULT 0,
    is_core           INTEGER DEFAULT 0,
    footprint_json    TEXT    NOT NULL,
    collision_width   INTEGER NOT NULL DEFAULT 80,
    collision_height  INTEGER NOT NULL DEFAULT 80
);

INSERT INTO building_defs VALUES
('CastleHeart',   '城堡之心', 500, NULL, 0, 0, 0, 0, NULL,          0, 0, 1, '[[0,0],[1,0],[0,1],[1,1]]', 180, 180),
('Barracks',      '兵营',     200, 8.0,  0, 0, 1, 1, 'Spearman',    0, 0, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('ShieldCamp',    '盾营',     250, 12.0, 0, 0, 1, 1, 'ShieldBearer',0, 0, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('ArcheryRange',  '靶场',     150, 10.0, 0, 0, 1, 1, 'Archer',      0, 0, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('Stable',        '马厩',     220, 14.0, 0, 0, 1, 1, 'Knight',      0, 0, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('ScoutCamp',     '斥候营',   100, 6.0,  0, 0, 1, 1, 'Scout',       0, 0, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('Armory',        '军府',     300, 7.0,  0, 0, 1, 1, 'Swordsman',   0, 1, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('Bulwark',       '壁垒',     380, 10.0, 0, 0, 1, 1, 'HeavyShield', 0, 1, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('CrossbowTower', '射楼',     220, 9.0,  0, 0, 1, 1, 'Crossbowman', 0, 1, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('Ranch',         '牧场',     330, 12.0, 0, 0, 1, 1, 'HeavyCavalry',0, 1, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188),
('RangerPost',    '游骑哨',   150, 5.0,  0, 0, 1, 1, 'LightCavalry',0, 1, 0, '[[0,0],[1,0],[0,1],[1,1]]', 188, 188);

CREATE TABLE damage_matrix (
    damage_type INTEGER NOT NULL,
    armor_type  INTEGER NOT NULL,
    multiplier  REAL    NOT NULL,
    PRIMARY KEY (damage_type, armor_type)
);

INSERT INTO damage_matrix VALUES
(0,0,1.0), (0,1,0.8), (0,2,0.6), (0,3,1.0),
(1,0,1.2), (1,1,0.9), (1,2,0.5), (1,3,0.8),
(2,0,0.8), (2,1,1.0), (2,2,1.5), (2,3,0.9),
(3,0,1.0), (3,1,1.2), (3,2,1.0), (3,3,1.0);

CREATE TABLE shop_catalog (
    id            TEXT PRIMARY KEY,
    name          TEXT    NOT NULL,
    cost          INTEGER NOT NULL,
    building_type TEXT    NOT NULL,
    weight        INTEGER NOT NULL DEFAULT 1
);

INSERT INTO shop_catalog VALUES
('barracks',      '兵营',   3, 'Barracks',     28),
('shield_camp',   '盾营',   3, 'ShieldCamp',   20),
('archery_range', '靶场',   4, 'ArcheryRange', 24),
('stable',        '马厩',   5, 'Stable',       20),
('scout_camp',    '斥候营', 2, 'ScoutCamp',    16);

CREATE TABLE combine_recipes (
    main_type_id     TEXT NOT NULL,
    material_type_id TEXT NOT NULL,
    material_count   INTEGER NOT NULL,
    result_type_id   TEXT NOT NULL,
    PRIMARY KEY (main_type_id, material_type_id)
);

INSERT INTO combine_recipes VALUES
('Barracks',     'Barracks',     1, 'Armory'),
('ShieldCamp',   'ShieldCamp',   1, 'Bulwark'),
('ArcheryRange', 'ArcheryRange', 1, 'CrossbowTower'),
('Stable',       'Stable',       1, 'Ranch'),
('ScoutCamp',    'ScoutCamp',    1, 'RangerPost');
