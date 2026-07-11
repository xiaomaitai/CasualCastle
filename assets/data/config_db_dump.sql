PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE asset_gen_tasks (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  generate_uuid TEXT NOT NULL,
  status TEXT NOT NULL DEFAULT 'submitted',
  prompt TEXT,
  negative_prompt TEXT,
  width INTEGER,
  height INTEGER,
  img_count INTEGER,
  sampler INTEGER,
  steps INTEGER,
  cfg_scale REAL,
  seed INTEGER,
  template_uuid TEXT,
  checkpoint_id TEXT,
  liblib_status INTEGER,
  image_url TEXT,
  local_path TEXT,
  submitted_at TEXT,
  completed_at TEXT,
  error_msg TEXT
);
INSERT INTO asset_gen_tasks VALUES(1,'d2476572697c424a94e1dc792f177a7a','failed','a cute orange tabby cat sitting on a stone castle wall, simple bold line art illustration, cartoon style, white background','',512,512,1,15,20,7.0,-1,'6f7c4652458d4802969f8d089cf5b91f','6bce42cc0df444dd866f7c9f7855d917',6,NULL,NULL,'2026-07-05T14:39:55Z','2026-07-05T14:40:50Z','[-1]执行异常');
INSERT INTO asset_gen_tasks VALUES(2,'10adf8dd5eb545e693d475c228ae9c91','failed','a cute orange tabby cat sitting on a stone castle wall, simple clean line art, cute anime style, white background','',512,512,1,15,20,7.0,-1,'6f7c4652458d4802969f8d089cf5b91f','491c449be47b41299bf0e9bae3e5cdba',6,NULL,NULL,'2026-07-05T14:41:09Z','2026-07-05T14:42:20Z','[-1]执行异常');
INSERT INTO asset_gen_tasks VALUES(3,'95d8d7a3cae04fb98a795b4529f64224','failed','a cute orange tabby cat icon, flat game icon style, simple design, white background, clean edges','',512,512,1,15,20,7.0,-1,'bf085132c7134622895b783b520b39ff','39bfe441fd8641e78dbb57069b215fb3',6,NULL,NULL,'2026-07-05T14:42:27Z','2026-07-05T14:43:27Z','[-1]执行异常');
INSERT INTO asset_gen_tasks VALUES(4,'c28a75fee7544924ba8ef5877386fa25','failed','a cute orange tabby cat, simple clean line art, white background','',512,512,1,15,20,7.0,-1,'e10adc3949ba59abbe56e057f20f883e','491c449be47b41299bf0e9bae3e5cdba',6,NULL,NULL,'2026-07-05T14:43:35Z','2026-07-05T14:44:35Z','[-1]执行异常');
INSERT INTO asset_gen_tasks VALUES(5,'a77ae76e3a7a46759dd8d14fcd290232','completed','a cute orange cat, simple cartoon style','',512,512,1,15,20,7.0,-1,'6f7c4652458d4802969f8d089cf5b91f','',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/8e9a5137906397c5034c95ef4292dd6ffbe6f19757855c4c589d2a67416fdb81.png','/c/Users/zhouy/Desktop/test_cat_castle.png','2026-07-05T14:44:42Z','2026-07-05T14:45:50Z',NULL);
INSERT INTO asset_gen_tasks VALUES(8,'384bacc14c7c42d99f4e6c37bdae2857','completed','seamless grass tile texture','shadows realistic 3D',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/93f608fae4fd51b6ff57d23f39f40499b4c8931a86ee3ee5c70829ab0b6d2ede.png','assets/art/battlefield/grass_tile.png','2026-07-06T10:37:11Z','2026-07-06T10:39:24Z',NULL);
INSERT INTO asset_gen_tasks VALUES(9,'eba96125fb2c42319e863d3df01a013c','completed','cloud sprite cartoon','realistic 3D',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/8435722412b4278bb48733034b8c44c5b61a1fa1e330ca39f1c92bb3eaf93b2b.png','assets/art/battlefield/cloud_sprite.png','2026-07-06T10:39:24Z','2026-07-06T10:39:24Z',NULL);
INSERT INTO asset_gen_tasks VALUES(10,'360de4e9667940d2b02267c700fef2b3','completed','mountain background green hills sky','trees buildings realistic',1024,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/1fc81d5689910fdb99f6040b0058f06619e37ec57ddf2669641d46ee7ec1df5a.png','assets/art/battlefield/mountain_bg.png','2026-07-06T10:39:51Z','2026-07-06T10:40:43Z',NULL);
INSERT INTO asset_gen_tasks VALUES(11,'e9e134ce299f4eb2bff1b363aa1d1598','completed','grass tuft decorative sprite','realistic 3D shadows',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/e83c33fb1daa3bad9b56b26bb3b1f821677be06f7f2b96504091357e69e05bd3.png','assets/art/battlefield/grass_clump.png','2026-07-06T10:40:58Z','2026-07-06T10:43:12Z',NULL);
INSERT INTO asset_gen_tasks VALUES(12,'6dc2044a29e14adea8128217e9b3c22d','completed','flat 2D, cute moe style, thick bold outlines, no shading, no gradient, simple flat colors, a circular patch of loess dirt ground, sandy yellow-brown earth texture, some small scattered stones and pebbles, isolated on white background, game tile asset, top-down view','3D, realistic, shadow, gradient, perspective, depth, photorealistic, grass, plants, complex background',512,512,4,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/a40a10adfee748ad5c749fc22096842f3c1a57bc6373cc0031183339cf4ecede.png','assets/art/tiles/cell_dirt_01.png','2026-07-06T11:32:41Z','2026-07-06T11:33:48Z',NULL);
INSERT INTO asset_gen_tasks VALUES(13,'b5d49c1196904a888eefb6b0b4337c56','completed','remove_bg:cell_dirt_01.png',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'4df2efa0f18d46dc9758803e478eb51c',NULL,5,NULL,'assets/art/tiles/cell_dirt_01.png','2026-07-06T11:36:49Z','2026-07-06T11:38:57Z',NULL);
INSERT INTO asset_gen_tasks VALUES(14,'0a653feb8e744a92a3920aec3f7cabdd','completed','remove_bg:cell_dirt_03.png',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'4df2efa0f18d46dc9758803e478eb51c',NULL,5,NULL,'assets/art/tiles/cell_dirt_03.png','2026-07-06T11:37:18Z','2026-07-06T11:38:57Z',NULL);
INSERT INTO asset_gen_tasks VALUES(15,'e317b9a18d094cbbb5ef0c74dc69bbd5','completed','remove_bg:cell_dirt_04.png',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'4df2efa0f18d46dc9758803e478eb51c',NULL,5,NULL,'assets/art/tiles/cell_dirt_04.png','2026-07-06T11:37:29Z','2026-07-06T11:38:57Z',NULL);
INSERT INTO asset_gen_tasks VALUES(16,'7b0ac8092eb34c16a6368481a5ca1658','completed','remove_bg:cell_dirt_02.png',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'4df2efa0f18d46dc9758803e478eb51c',NULL,5,NULL,'assets/art/tiles/cell_dirt_02.png','2026-07-06T11:37:34Z','2026-07-06T11:38:57Z',NULL);
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
INSERT INTO unit_stats VALUES('Spearman',1,0,0,0,40,10,80.0,30.0,1.0,0,4289733792,250.0);
INSERT INTO unit_stats VALUES('ShieldBearer',1,0,0,1,70,5,45.0,30.0,1.8,0,4286611584,200.0);
INSERT INTO unit_stats VALUES('Archer',1,1,1,0,25,12,60.0,200.0,1.5,0,4286627968,300.0);
INSERT INTO unit_stats VALUES('Knight',2,0,0,1,60,18,120.0,35.0,1.2,0,4291862624,280.0);
INSERT INTO unit_stats VALUES('Scout',0,0,0,0,15,3,160.0,25.0,1.0,0,4286644095,400.0);
INSERT INTO unit_stats VALUES('Swordsman',1,0,0,0,55,16,80.0,30.0,0.9,0,4285567184,260.0);
INSERT INTO unit_stats VALUES('HeavyShield',2,0,0,1,110,8,40.0,35.0,1.5,0,4284515808,220.0);
INSERT INTO unit_stats VALUES('Crossbowman',1,1,1,0,30,22,55.0,180.0,2.0,0,4283476000,280.0);
INSERT INTO unit_stats VALUES('HeavyCavalry',2,0,0,1,85,28,110.0,35.0,1.3,0,4288712672,290.0);
INSERT INTO unit_stats VALUES('LightCavalry',1,0,0,0,25,8,180.0,30.0,0.8,0,4286632191,450.0);
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
INSERT INTO building_defs VALUES('CastleHeart','城堡之心',500,NULL,0,0,0,0,NULL,0,0,1,'[[0,0],[1,0],[0,1],[1,1]]',180,180);
INSERT INTO building_defs VALUES('Barracks','兵营',200,8.0,0,0,1,1,'Spearman',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('ShieldCamp','盾营',250,12.0,0,0,1,1,'ShieldBearer',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('ArcheryRange','靶场',150,10.0,0,0,1,1,'Archer',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('Stable','马厩',220,14.0,0,0,1,1,'Knight',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('ScoutCamp','斥候营',100,6.0,0,0,1,1,'Scout',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('Armory','军府',300,7.0,0,0,1,1,'Swordsman',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('Bulwark','壁垒',380,10.0,0,0,1,1,'HeavyShield',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('CrossbowTower','射楼',220,9.0,0,0,1,1,'Crossbowman',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('Ranch','牧场',330,12.0,0,0,1,1,'HeavyCavalry',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
INSERT INTO building_defs VALUES('RangerPost','游骑哨',150,5.0,0,0,1,1,'LightCavalry',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188);
CREATE TABLE damage_matrix (
    damage_type INTEGER NOT NULL,
    armor_type  INTEGER NOT NULL,
    multiplier  REAL    NOT NULL,
    PRIMARY KEY (damage_type, armor_type)
);
INSERT INTO damage_matrix VALUES(0,0,1.0);
INSERT INTO damage_matrix VALUES(0,1,0.8);
INSERT INTO damage_matrix VALUES(0,2,0.6);
INSERT INTO damage_matrix VALUES(0,3,1.0);
INSERT INTO damage_matrix VALUES(1,0,1.2);
INSERT INTO damage_matrix VALUES(1,1,0.9);
INSERT INTO damage_matrix VALUES(1,2,0.5);
INSERT INTO damage_matrix VALUES(1,3,0.8);
INSERT INTO damage_matrix VALUES(2,0,0.8);
INSERT INTO damage_matrix VALUES(2,1,1.0);
INSERT INTO damage_matrix VALUES(2,2,1.5);
INSERT INTO damage_matrix VALUES(2,3,0.9);
INSERT INTO damage_matrix VALUES(3,0,1.0);
INSERT INTO damage_matrix VALUES(3,1,1.2);
INSERT INTO damage_matrix VALUES(3,2,1.0);
INSERT INTO damage_matrix VALUES(3,3,1.0);
CREATE TABLE shop_catalog (
    id            TEXT PRIMARY KEY,
    name          TEXT    NOT NULL,
    cost          INTEGER NOT NULL,
    building_type TEXT    NOT NULL,
    weight        INTEGER NOT NULL DEFAULT 1
);
INSERT INTO shop_catalog VALUES('barracks','兵营',3,'Barracks',28);
INSERT INTO shop_catalog VALUES('shield_camp','盾营',3,'ShieldCamp',20);
INSERT INTO shop_catalog VALUES('archery_range','靶场',4,'ArcheryRange',24);
INSERT INTO shop_catalog VALUES('stable','马厩',5,'Stable',20);
INSERT INTO shop_catalog VALUES('scout_camp','斥候营',2,'ScoutCamp',16);
CREATE TABLE combine_recipes (
    main_type_id     TEXT NOT NULL,
    material_type_id TEXT NOT NULL,
    material_count   INTEGER NOT NULL,
    result_type_id   TEXT NOT NULL,
    PRIMARY KEY (main_type_id, material_type_id)
);
INSERT INTO combine_recipes VALUES('Barracks','Barracks',1,'Armory');
INSERT INTO combine_recipes VALUES('ShieldCamp','ShieldCamp',1,'Bulwark');
INSERT INTO combine_recipes VALUES('ArcheryRange','ArcheryRange',1,'CrossbowTower');
INSERT INTO combine_recipes VALUES('Stable','Stable',1,'Ranch');
INSERT INTO combine_recipes VALUES('ScoutCamp','ScoutCamp',1,'RangerPost');
PRAGMA writable_schema=ON;
CREATE TABLE IF NOT EXISTS sqlite_sequence(name,seq);
DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('asset_gen_tasks',16);
PRAGMA writable_schema=OFF;
COMMIT;
