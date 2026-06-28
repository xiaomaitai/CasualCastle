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
    unit_color       INTEGER NOT NULL
);

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
    footprint_json    TEXT    NOT NULL
);

CREATE TABLE damage_matrix (
    damage_type INTEGER NOT NULL,
    armor_type  INTEGER NOT NULL,
    multiplier  REAL    NOT NULL,
    PRIMARY KEY (damage_type, armor_type)
);

CREATE TABLE shop_catalog (
    id            TEXT PRIMARY KEY,
    name          TEXT    NOT NULL,
    cost          INTEGER NOT NULL,
    building_type TEXT    NOT NULL
);

CREATE TABLE fusion_recipes (
    main_type_id     TEXT NOT NULL,
    material_type_id TEXT NOT NULL,
    material_count   INTEGER NOT NULL,
    result_type_id   TEXT NOT NULL,
    PRIMARY KEY (main_type_id, material_type_id)
);

INSERT INTO unit_stats VALUES ('Swordsman',      1,0,0,0,30,10,170,60,1,   0,0xFF4488FF);
INSERT INTO unit_stats VALUES ('Archer',         1,1,1,0,20,8, 150,100,1.2,0,0xFF44CC44);
INSERT INTO unit_stats VALUES ('Cavalry',        2,0,0,1,50,12,220,60,1,   0,0xFFFFAA22);
INSERT INTO unit_stats VALUES ('Werewolf',       1,0,0,3,35,12,200,60,1,   1,0xFF8844AA);
INSERT INTO unit_stats VALUES ('HeavySwordsman', 1,0,0,1,45,14,160,60,0.9, 0,0xFF6688FF);
INSERT INTO unit_stats VALUES ('WerewolfLord',   1,0,3,3,50,16,200,60,0.9, 1,0xFFCC44CC);

INSERT INTO building_defs VALUES ('CastleHeart', '城堡之心', 500, NULL, 0,0,0,0, NULL,         0,0,1, '[[0,0],[1,0],[0,1],[1,1]]');
INSERT INTO building_defs VALUES ('Barracks',    '兵营',     100, 5,    0,0,0,0, 'Swordsman', 0,0,0, '[[0,0]]');
INSERT INTO building_defs VALUES ('ArcheryRange','靶场',     120, 6,    0,0,1,0, 'Archer',    0,0,0, '[[0,0],[1,0]]');
INSERT INTO building_defs VALUES ('Stable',      '马厩',     150, 5,    0,1,1,2, 'Cavalry',   0,0,0, '[[0,0],[0,1],[0,2],[1,2]]');
INSERT INTO building_defs VALUES ('WolfDen',     '狼穴',     90,  6,    0,0,0,0, 'Werewolf',  1,0,0, '[[0,0]]');
INSERT INTO building_defs VALUES ('BarracksT2',  '强化兵营', 130, 4,    0,0,0,0, 'HeavySwordsman', 0,1,0, '[[0,0]]');
INSERT INTO building_defs VALUES ('WolfDenT2',   '强化狼穴', 120, 5,    0,0,0,0, 'WerewolfLord',   1,1,0, '[[0,0]]');

INSERT INTO damage_matrix VALUES (0,0,1.0);
INSERT INTO damage_matrix VALUES (0,1,0.75);
INSERT INTO damage_matrix VALUES (0,2,0.5);
INSERT INTO damage_matrix VALUES (0,3,1.0);
INSERT INTO damage_matrix VALUES (1,0,0.75);
INSERT INTO damage_matrix VALUES (1,1,1.5);
INSERT INTO damage_matrix VALUES (1,2,1.0);
INSERT INTO damage_matrix VALUES (1,3,0.75);
INSERT INTO damage_matrix VALUES (2,0,0.5);
INSERT INTO damage_matrix VALUES (2,1,1.0);
INSERT INTO damage_matrix VALUES (2,2,1.5);
INSERT INTO damage_matrix VALUES (2,3,1.0);
INSERT INTO damage_matrix VALUES (3,0,1.0);
INSERT INTO damage_matrix VALUES (3,1,1.0);
INSERT INTO damage_matrix VALUES (3,2,1.25);
INSERT INTO damage_matrix VALUES (3,3,1.5);

INSERT INTO shop_catalog VALUES ('barracks',      '兵营', 10, 'Barracks');
INSERT INTO shop_catalog VALUES ('archery_range', '靶场', 14, 'ArcheryRange');
INSERT INTO shop_catalog VALUES ('stable',        '马厩', 18, 'Stable');
INSERT INTO shop_catalog VALUES ('wolf_den',      '狼穴', 16, 'WolfDen');

INSERT INTO fusion_recipes VALUES ('Barracks', 'Barracks', 1, 'BarracksT2');
INSERT INTO fusion_recipes VALUES ('WolfDen',  'WolfDen',  1, 'WolfDenT2');
