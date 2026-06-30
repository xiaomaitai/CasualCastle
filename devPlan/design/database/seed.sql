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
('Swordsman',     1, 0, 0, 0, 30, 10, 170.0, 60.0,  1.0, 0, 4282681599, 170.0),
('Archer',        1, 1, 1, 0, 20,  8, 150.0, 100.0, 1.2, 0, 4282698820, 170.0),
('Cavalry',       2, 0, 0, 1, 50, 12, 220.0, 60.0,  1.0, 0, 4294945314, 170.0),
('Werewolf',      1, 0, 0, 3, 35, 12, 200.0, 60.0,  1.0, 1, 4287120554, 170.0),
('HeavySwordsman',1, 0, 0, 1, 45, 14, 160.0, 60.0,  0.9, 0, 4284909823, 170.0),
('WerewolfLord',  1, 0, 3, 3, 50, 16, 200.0, 60.0,  0.9, 1, 4291577036, 170.0);

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
    fusion_tier       INTEGER DEFAULT 0,
    is_core           INTEGER DEFAULT 0,
    footprint_json    TEXT    NOT NULL,
    collision_width   INTEGER NOT NULL DEFAULT 80,
    collision_height  INTEGER NOT NULL DEFAULT 80
);

INSERT INTO building_defs VALUES
('CastleHeart',  '城堡之心',   500, NULL, 0, 0, 0, 0, NULL,           0, 0, 1, '[[0,0],[1,0],[0,1],[1,1]]',             180, 180),
('Barracks',     '兵营',       100, 5.0,  0, 0, 0, 0, 'Swordsman',    0, 0, 0, '[[0,0]]',                                   80,  80),
('ArcheryRange', '靶场',       120, 6.0,  0, 0, 1, 0, 'Archer',       0, 0, 0, '[[0,0],[1,0]]',                             180, 80),
('Stable',       '马厩',       150, 5.0,  0, 1, 1, 2, 'Cavalry',      0, 0, 0, '[[0,0],[0,1],[0,2],[1,2]]',               180, 280),
('WolfDen',      '狼穴',        90, 6.0,  0, 0, 0, 0, 'Werewolf',     1, 0, 0, '[[0,0]]',                                   80,  80),
('BarracksT2',   '强化兵营',   130, 4.0,  0, 0, 0, 0, 'HeavySwordsman',0, 1, 0, '[[0,0]]',                                  80,  80),
('WolfDenT2',    '强化狼穴',   120, 5.0,  0, 0, 0, 0, 'WerewolfLord', 1, 1, 0, '[[0,0]]',                                   80,  80);

CREATE TABLE damage_matrix (
    damage_type INTEGER NOT NULL,
    armor_type  INTEGER NOT NULL,
    multiplier  REAL    NOT NULL,
    PRIMARY KEY (damage_type, armor_type)
);

INSERT INTO damage_matrix VALUES
(0,0,1.0), (0,1,0.75), (0,2,0.5), (0,3,1.0),
(1,0,0.75), (1,1,1.5), (1,2,1.0), (1,3,0.75),
(2,0,0.5), (2,1,1.0), (2,2,1.5), (2,3,1.0),
(3,0,1.0), (3,1,1.0), (3,2,1.25), (3,3,1.5);

CREATE TABLE shop_catalog (
    id            TEXT PRIMARY KEY,
    name          TEXT    NOT NULL,
    cost          INTEGER NOT NULL,
    building_type TEXT    NOT NULL
);

INSERT INTO shop_catalog VALUES
('barracks',      '兵营', 10, 'Barracks'),
('archery_range', '靶场', 14, 'ArcheryRange'),
('stable',        '马厩', 18, 'Stable'),
('wolf_den',      '狼穴', 16, 'WolfDen');

CREATE TABLE fusion_recipes (
    main_type_id     TEXT NOT NULL,
    material_type_id TEXT NOT NULL,
    material_count   INTEGER NOT NULL,
    result_type_id   TEXT NOT NULL,
    PRIMARY KEY (main_type_id, material_type_id)
);

INSERT INTO fusion_recipes VALUES
('Barracks', 'Barracks', 1, 'BarracksT2'),
('WolfDen',  'WolfDen',  1, 'WolfDenT2');
