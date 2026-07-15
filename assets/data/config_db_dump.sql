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
INSERT INTO asset_gen_tasks VALUES(17,'31ed514ed3e24ebf9e53093025826fad','completed','A cute chibi medieval swordsman soldier, holding a sword, facing right, full body character, flat 2D, cute moe style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/1308ad1c7fecd083b8399d4539ba03da04d3cbe57f09c06086dda4ad550f436b.png','assets/art/cards/Swordsman.png','2026-07-12T13:56:56Z','2026-07-12T13:57:48Z',NULL);
INSERT INTO asset_gen_tasks VALUES(18,'57428a53f37d4f56b2ba541602c3f43b','completed','A medieval archer soldier, holding a bow, facing right, full body character, flat 2D, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/060bd4a948abc17c84c1ef81df14a7131807b6c1d0c96d7b679a3a07890920ef.png','assets/art/cards/Archer.png','2026-07-12T14:00:19Z','2026-07-12T14:01:06Z',NULL);
INSERT INTO asset_gen_tasks VALUES(19,'7a007ee63aa3447da65795c37863f125','completed','A medieval knight in armor, holding a lance and shield, facing right, full body, anime style, clean lineart, thick bold outlines, flat colors, no shading, game character','',512,512,1,15,20,7.0,-1,'6f7c4652458d4802969f8d089cf5b91f','',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/3545bd09ecf2c09b0ee811bf51576f02467ad6ac986095e1092a3c509b53ee3a.png','assets/art/cards/Knight.png','2026-07-12T14:04:04Z','2026-07-12T14:04:51Z',NULL);
INSERT INTO asset_gen_tasks VALUES(20,'0f784b9c2429437c91255baca7dde67e','completed','A medieval spearman soldier, holding a long spear, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/745eece791ee6fbb63dffe71318f2bda9c6f090f8a8948e6938a89964ac711ef.png','assets/art/cards/Spearman_20.png','2026-07-12T14:06:46Z','2026-07-12T14:07:43Z',NULL);
INSERT INTO asset_gen_tasks VALUES(21,'f9da5cf9391a468cbce6ab5a8cbd7466','completed','A medieval shield bearer soldier, holding a large shield, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/2e7e939b290b4b3ca7dc55358ed15fe5224cd1532c5da54bb10697e753e0a00a.png','assets/art/cards/ShieldBearer_21.png','2026-07-12T14:16:05Z','2026-07-12T14:20:44Z',NULL);
INSERT INTO asset_gen_tasks VALUES(24,'a4fe21972a114cb7808184910f19f392','completed','A medieval archer soldier, holding a bow, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/19f64bc221affcc738fab8fa5e8a151d9adc0e602a5fd0cdd714aeb40688b877.png','assets/art/cards/Archer_24.png','2026-07-12T14:19:52Z','2026-07-12T14:20:45Z',NULL);
INSERT INTO asset_gen_tasks VALUES(25,'175e1037233a496fb0cbcaea11cfd4ea','completed','A medieval knight in armor, holding a lance, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/07ca31c594e62cab805a4f2efa371bd44ecf2c66c40a3070d565120a62ea8f99.png','assets/art/cards/Knight_25.png','2026-07-12T14:20:58Z','2026-07-12T14:21:47Z',NULL);
INSERT INTO asset_gen_tasks VALUES(26,'faa44462c78744cfb617e6bc61ed9848','completed','A medieval scout soldier, light armor, fast runner with a dagger, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/5e21efda165b77f545b934fb52226214ad5d2737fc2dcc5e2f978026c83eb358.png','assets/art/cards/Scout_26.png','2026-07-12T14:21:58Z','2026-07-12T14:22:42Z',NULL);
INSERT INTO asset_gen_tasks VALUES(27,'47f701d52ba345fbb97c160bbc28bc6f','completed','A medieval swordsman soldier, holding a sword, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/94b567beb62f0c840efd9a19fb72647463b9ba490b30753898c21b3314ad7e7d.png','assets/art/cards/Swordsman_27.png','2026-07-12T14:22:52Z','2026-07-12T14:23:36Z',NULL);
INSERT INTO asset_gen_tasks VALUES(28,'08396d86782d409abfd5a5a344a65e80','completed','A medieval heavy shield soldier, holding a massive tower shield and mace, heavily armored, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/4f6aaa39c204da53805f16d2035da0066cbf6f6b2e8a0ab855e759f45a540222.png','assets/art/cards/HeavyShield_28.png','2026-07-12T14:23:50Z','2026-07-12T14:24:32Z',NULL);
INSERT INTO asset_gen_tasks VALUES(29,'32aacfe0a7d74af7b250c22ece212e8d','completed','A medieval crossbowman soldier, holding a crossbow, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/be9b145ce97c260ccfee6c5e72e62b48d66b0886412f0773278a304d7edac93c.png','assets/art/cards/Crossbowman_29.png','2026-07-12T14:24:43Z','2026-07-12T14:25:26Z',NULL);
INSERT INTO asset_gen_tasks VALUES(30,'44be777a5d6d4fc38493895d0549ad0d','completed','A medieval heavy cavalry knight on horseback, holding a lance, charging, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/61cc6051381b551f5237ded9243676637f2b8013da083de59c036f0048ed5c80.png','assets/art/cards/HeavyCavalry_30.png','2026-07-12T14:25:37Z','2026-07-12T14:26:27Z',NULL);
INSERT INTO asset_gen_tasks VALUES(31,'ed08be8227454b8ebd392a3d5c5e3dcd','completed','A medieval light cavalry rider on horseback, holding a saber, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/a6a50fc65fbd6e093504697a0af0bc37ad1d70aef7f50158473910931642b46c.png','assets/art/cards/LightCavalry_31.png','2026-07-12T14:26:28Z','2026-07-12T14:27:07Z',NULL);
INSERT INTO asset_gen_tasks VALUES(32,'','submitted','A medieval holy shield guardian soldier, holding a massive tower shield, wearing heavy silver plate armor with blue cape, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon',NULL,512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',1,NULL,NULL,'2026-07-14T14:52:39Z',NULL,NULL);
INSERT INTO asset_gen_tasks VALUES(33,'','submitted','A medieval holy shield guardian soldier, holding a massive tower shield, wearing heavy silver plate armor with blue cape, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon',NULL,512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',1,NULL,NULL,'2026-07-14T14:52:51Z',NULL,NULL);
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
, unit_cost INTEGER NOT NULL DEFAULT 2000, race TEXT NOT NULL DEFAULT '');
INSERT INTO unit_stats VALUES('Spearman',1,0,0,0,120,4,80.0,30.0,1.0,0,4289733792,250.0,2000,'Human');
INSERT INTO unit_stats VALUES('ShieldBearer',1,0,0,1,220,3,45.0,30.0,1.5,0,4286611584,200.0,2400,'Human');
INSERT INTO unit_stats VALUES('Archer',1,1,1,0,50,12,60.0,200.0,1.5,0,4286627968,300.0,2200,'Human');
INSERT INTO unit_stats VALUES('Knight',2,0,0,1,150,14,120.0,35.0,1.2,0,4291862624,280.0,2600,'Human');
INSERT INTO unit_stats VALUES('Scout',0,0,0,0,35,4,160.0,25.0,0.8,0,4286644095,400.0,1400,'Human');
INSERT INTO unit_stats VALUES('Swordsman',1,0,0,0,160,6,80.0,30.0,0.9,0,4285567184,260.0,2600,'Human');
INSERT INTO unit_stats VALUES('HeavyShield',2,0,0,1,330,4,40.0,35.0,1.5,0,4284515808,220.0,3000,'Human');
INSERT INTO unit_stats VALUES('Crossbowman',1,1,1,0,70,18,55.0,180.0,2.0,0,4283476000,280.0,2800,'Human');
INSERT INTO unit_stats VALUES('HeavyCavalry',2,0,0,1,200,22,110.0,35.0,1.3,0,4288712672,290.0,3400,'Human');
INSERT INTO unit_stats VALUES('LightCavalry',1,0,0,0,55,8,180.0,30.0,0.8,0,4286632191,450.0,1800,'Human');
INSERT INTO unit_stats VALUES('HolyShieldGuardian',2,0,0,1,700,8,45.0,35.0,1.5,0,4286616704,170.0,5000,'Human');
INSERT INTO unit_stats VALUES('DragonRiderCommander',2,1,1,0,300,40,120.0,180.0,0.8,0,4290526975,200.0,4600,'Human');
INSERT INTO unit_stats VALUES('ShadowLord',1,0,0,0,180,50,100.0,30.0,1.0,0,4284248320,220.0,4000,'Human');
INSERT INTO unit_stats VALUES('King',2,0,0,1,1200,20,60.0,35.0,1.2,0,4294956800,170.0,6000,'Human');
INSERT INTO unit_stats VALUES('RoyalGuard',1,0,0,0,220,8,80.0,30.0,0.9,0,4488440,270.0,3400,'Human');
INSERT INTO unit_stats VALUES('RoyalShieldGuard',2,0,0,1,480,5,40.0,35.0,1.5,0,12303291,230.0,3800,'Human');
INSERT INTO unit_stats VALUES('RoyalSharpshooter',1,1,1,0,90,28,55.0,200.0,2.0,0,5621589,300.0,3600,'Human');
INSERT INTO unit_stats VALUES('RoyalKnight',2,0,0,1,280,30,110.0,35.0,1.3,0,16754900,300.0,4200,'Human');
INSERT INTO unit_stats VALUES('RoyalScout',1,0,0,0,80,14,180.0,30.0,0.8,0,10040232,500.0,2400,'Human');
INSERT INTO unit_stats VALUES('ElfRanger',0,1,1,0,45,14,150.0,220.0,1.2,0,4294954080,300.0,0,'Elf');
INSERT INTO unit_stats VALUES('ElfSharpshooter',0,1,1,0,60,22,150.0,280.0,1.0,0,4294954320,350.0,0,'Elf');
INSERT INTO unit_stats VALUES('DwarfWarrior',1,0,0,1,100,16,90.0,90.0,1.5,0,4286611584,170.0,0,'Dwarf');
INSERT INTO unit_stats VALUES('DwarfBerserker',1,0,0,1,130,28,110.0,90.0,1.2,0,4286740992,170.0,0,'Dwarf');
INSERT INTO unit_stats VALUES('HalflingThief',0,0,0,0,35,10,200.0,70.0,0.7,0,4278241280,200.0,0,'Halfling');
INSERT INTO unit_stats VALUES('HalflingAssassin',0,0,0,0,50,20,220.0,70.0,0.6,0,4278237952,220.0,0,'Halfling');
INSERT INTO unit_stats VALUES('DragonbornMage',1,1,3,0,55,18,110.0,200.0,1.8,0,4278190336,250.0,0,'Dragonborn');
INSERT INTO unit_stats VALUES('DragonbornArchmage',1,1,3,0,75,30,110.0,240.0,1.5,0,4278190080,280.0,0,'Dragonborn');
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
, race_id TEXT NOT NULL DEFAULT 'human', production_rate REAL NOT NULL DEFAULT 0.0);
INSERT INTO building_defs VALUES('CastleHeart','城堡之心',500,NULL,0,0,0,0,NULL,0,0,1,'[[0,0],[1,0],[0,1],[1,1]]',180,180,'human',0.0);
INSERT INTO building_defs VALUES('RangerPost','游骑哨',150,5.0,0,0,1,1,'LightCavalry',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('Barracks','兵营',200,8.0,0,0,1,1,'Spearman',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('ShieldCamp','盾营',250,12.0,0,0,1,1,'ShieldBearer',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('ArcheryRange','靶场',150,10.0,0,0,1,1,'Archer',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('Stable','马厩',220,14.0,0,0,1,1,'Knight',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('ScoutCamp','斥候营',100,6.0,0,0,1,1,'Scout',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('Armory','军府',300,7.0,0,0,1,1,'Swordsman',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('Bulwark','壁垒',380,10.0,0,0,1,1,'HeavyShield',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('CrossbowTower','射楼',220,9.0,0,0,1,1,'Crossbowman',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('Ranch','牧场',330,12.0,0,0,1,1,'HeavyCavalry',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('RoyalArmory','皇家军营',300,7.0,0,0,1,1,'RoyalGuard',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('RoyalBulwark','皇家壁垒',380,10.0,0,0,1,1,'RoyalShieldGuard',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('RoyalArcheryRange','皇家射场',220,9.0,0,0,1,1,'RoyalSharpshooter',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('RoyalStable','皇家马场',330,12.0,0,0,1,1,'RoyalKnight',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('RoyalRanger','皇家游骑',150,5.0,0,0,1,1,'RoyalScout',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('HolyWall','圣壁',500,14.0,0,0,1,1,'HolyShieldGuardian',0,3,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('HeavenPunishmentTower','天罚塔',350,12.0,0,0,1,1,'DragonRiderCommander',0,3,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('ShadowSanctum','暗影圣所',250,8.0,0,0,1,1,'ShadowLord',0,3,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('RoyalCourt','王庭',800,30.0,0,0,1,1,'King',0,4,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'human',100.0);
INSERT INTO building_defs VALUES('RangerGuild','游侠公会',180,9.0,0,0,1,0,'ElfRanger',0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Elf',0.0);
INSERT INTO building_defs VALUES('WindSanctum','风行圣殿',260,7.0,0,0,1,0,'ElfSharpshooter',0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Elf',0.0);
INSERT INTO building_defs VALUES('WarriorGuild','战士公会',250,12.0,0,0,1,0,'DwarfWarrior',0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Dwarf',0.0);
INSERT INTO building_defs VALUES('MountainFort','山丘要塞',360,10.0,0,0,1,0,'DwarfBerserker',0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Dwarf',0.0);
INSERT INTO building_defs VALUES('ThiefGuild','盗贼行会',140,7.0,0,0,1,0,'HalflingThief',0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Halfling',0.0);
INSERT INTO building_defs VALUES('ShadowGuild','暗影公会',200,5.0,0,0,1,0,'HalflingAssassin',0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Halfling',0.0);
INSERT INTO building_defs VALUES('MageTower','术士高塔',160,14.0,0,0,1,0,'DragonbornMage',0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Dragonborn',0.0);
INSERT INTO building_defs VALUES('DragonfireTower','龙火塔',230,12.0,0,0,1,0,'DragonbornArchmage',0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Dragonborn',0.0);
INSERT INTO building_defs VALUES('Inn','酒馆',120,NULL,0,0,1,0,NULL,0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Human',0.0);
INSERT INTO building_defs VALUES('AdventurerGuild','冒险者公会',200,NULL,0,0,1,0,NULL,0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'Human',0.0);
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
INSERT INTO shop_catalog VALUES('Barracks','兵营',3,'Barracks',14);
INSERT INTO shop_catalog VALUES('ShieldCamp','盾营',3,'ShieldCamp',10);
INSERT INTO shop_catalog VALUES('ArcheryRange','靶场',4,'ArcheryRange',12);
INSERT INTO shop_catalog VALUES('Stable','马厩',5,'Stable',10);
INSERT INTO shop_catalog VALUES('ScoutCamp','斥候营',2,'ScoutCamp',8);
INSERT INTO shop_catalog VALUES('Armory','军府',6,'Armory',8);
INSERT INTO shop_catalog VALUES('Bulwark','壁垒',6,'Bulwark',6);
INSERT INTO shop_catalog VALUES('Ranch','牧场',8,'Ranch',6);
INSERT INTO shop_catalog VALUES('RoyalArmory','皇家军营',10,'RoyalArmory',4);
INSERT INTO shop_catalog VALUES('RoyalBulwark','皇家壁垒',10,'RoyalBulwark',4);
INSERT INTO shop_catalog VALUES('RoyalStable','皇家马场',12,'RoyalStable',4);
INSERT INTO shop_catalog VALUES('card_ranger_guild','游侠公会',3,'RangerGuild',12);
INSERT INTO shop_catalog VALUES('card_warrior_guild','战士公会',4,'WarriorGuild',10);
INSERT INTO shop_catalog VALUES('card_thief_guild','盗贼行会',2,'ThiefGuild',10);
INSERT INTO shop_catalog VALUES('card_mage_tower','术士高塔',5,'MageTower',6);
INSERT INTO shop_catalog VALUES('card_inn','酒馆',2,'Inn',14);
CREATE TABLE race_defs (
    id TEXT PRIMARY KEY,
    display_name TEXT NOT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0
);
INSERT INTO race_defs VALUES('human','人族',0);
INSERT INTO race_defs VALUES('wizard','巫师族',1);
INSERT INTO race_defs VALUES('dungeon','地下城',2);
INSERT INTO race_defs VALUES('Elf','精灵',3);
INSERT INTO race_defs VALUES('Dwarf','矮人',4);
INSERT INTO race_defs VALUES('Halfling','半身人',5);
INSERT INTO race_defs VALUES('Dragonborn','龙裔',6);
CREATE TABLE tech_tree_nodes (
    type_id TEXT PRIMARY KEY,
    race_id TEXT NOT NULL REFERENCES race_defs(id),
    tier INTEGER NOT NULL CHECK(tier BETWEEN 1 AND 5),
    col INTEGER NOT NULL DEFAULT 0,
    shop_available INTEGER NOT NULL DEFAULT 0,
    gold_cost INTEGER,
    shop_weight INTEGER,
    unlock_night INTEGER DEFAULT 0,
    display_name TEXT NOT NULL,
    unit_type_id TEXT,
    max_health INTEGER NOT NULL DEFAULT 200,
    spawn_interval REAL NOT NULL DEFAULT 10.0
);
INSERT INTO tech_tree_nodes VALUES('Barracks','human',1,0,1,3,14,0,'兵营','Spearman',200,8.0);
INSERT INTO tech_tree_nodes VALUES('ShieldCamp','human',1,1,1,3,10,0,'盾营','ShieldBearer',250,12.0);
INSERT INTO tech_tree_nodes VALUES('ArcheryRange','human',1,2,1,4,12,0,'靶场','Archer',150,10.0);
INSERT INTO tech_tree_nodes VALUES('Stable','human',1,3,1,5,10,0,'马厩','Knight',220,14.0);
INSERT INTO tech_tree_nodes VALUES('ScoutCamp','human',1,4,1,2,8,0,'斥候营','Scout',100,6.0);
INSERT INTO tech_tree_nodes VALUES('Armory','human',2,0,1,6,8,2,'军府','Swordsman',300,7.0);
INSERT INTO tech_tree_nodes VALUES('Bulwark','human',2,1,1,6,6,2,'壁垒','HeavyShield',380,10.0);
INSERT INTO tech_tree_nodes VALUES('CrossbowTower','human',2,2,0,NULL,NULL,0,'射楼','Crossbowman',220,9.0);
INSERT INTO tech_tree_nodes VALUES('Ranch','human',2,3,1,8,6,2,'牧场','HeavyCavalry',330,12.0);
INSERT INTO tech_tree_nodes VALUES('RoyalArmory','human',3,0,1,10,4,4,'皇家军营','RoyalGuard',300,7.0);
INSERT INTO tech_tree_nodes VALUES('RoyalBulwark','human',3,1,1,10,4,4,'皇家壁垒','RoyalShieldGuard',380,10.0);
INSERT INTO tech_tree_nodes VALUES('RoyalArcheryRange','human',3,2,0,NULL,NULL,0,'皇家射场','RoyalSharpshooter',220,9.0);
INSERT INTO tech_tree_nodes VALUES('RoyalStable','human',3,3,1,12,4,4,'皇家马场','RoyalKnight',330,12.0);
INSERT INTO tech_tree_nodes VALUES('RoyalRanger','human',3,4,0,NULL,NULL,0,'皇家游骑','RoyalScout',150,5.0);
INSERT INTO tech_tree_nodes VALUES('HolyWall','human',4,0,0,NULL,NULL,0,'圣壁','HolyShieldGuardian',500,14.0);
INSERT INTO tech_tree_nodes VALUES('HeavenPunishmentTower','human',4,2,0,NULL,NULL,0,'天罚塔','DragonRiderCommander',350,12.0);
INSERT INTO tech_tree_nodes VALUES('ShadowSanctum','human',4,4,0,NULL,NULL,0,'暗影圣所','ShadowLord',250,8.0);
INSERT INTO tech_tree_nodes VALUES('RoyalCourt','human',5,2,0,NULL,NULL,0,'王庭','King',800,30.0);
CREATE TABLE IF NOT EXISTS "combine_recipes" (
                main_type_id TEXT NOT NULL,
                material_type_id TEXT NOT NULL,
                material_count INTEGER NOT NULL,
                result_type_id TEXT NOT NULL,
                PRIMARY KEY (main_type_id, material_type_id, result_type_id)
            );
INSERT INTO combine_recipes VALUES('ShieldCamp','ShieldCamp',1,'Bulwark');
INSERT INTO combine_recipes VALUES('ArcheryRange','ArcheryRange',1,'CrossbowTower');
INSERT INTO combine_recipes VALUES('Stable','Stable',1,'Ranch');
INSERT INTO combine_recipes VALUES('Ranch','Ranch',2,'RoyalStable');
INSERT INTO combine_recipes VALUES('Armory','Armory',2,'RoyalBulwark');
INSERT INTO combine_recipes VALUES('Ranch','Ranch',2,'RoyalRanger');
INSERT INTO combine_recipes VALUES('RoyalBulwark','RoyalArmory',1,'HolyWall');
INSERT INTO combine_recipes VALUES('RoyalArcheryRange','RoyalStable',1,'HeavenPunishmentTower');
INSERT INTO combine_recipes VALUES('RoyalRanger','RoyalStable',1,'ShadowSanctum');
INSERT INTO combine_recipes VALUES('HolyWall','HeavenPunishmentTower',1,'RoyalCourt');
INSERT INTO combine_recipes VALUES('RangerGuild','RangerGuild',1,'WindSanctum');
INSERT INTO combine_recipes VALUES('WarriorGuild','WarriorGuild',1,'MountainFort');
INSERT INTO combine_recipes VALUES('ThiefGuild','ThiefGuild',1,'ShadowGuild');
INSERT INTO combine_recipes VALUES('MageTower','MageTower',1,'DragonfireTower');
INSERT INTO combine_recipes VALUES('Inn','Inn',1,'AdventurerGuild');
CREATE TABLE skill_defs (
    id TEXT PRIMARY KEY,
    display_name TEXT NOT NULL,
    skill_type TEXT NOT NULL CHECK(skill_type IN ('stat_modifier','aura','on_hit','special')),
    config_json TEXT NOT NULL
);
INSERT INTO skill_defs VALUES('berserker_rage','狂战士之怒','stat_modifier','{"trigger":"low_health","trigger_param":0.5,"modifiers":{"attack_speed_mult":1.5}}');
INSERT INTO skill_defs VALUES('backstab','背刺','stat_modifier','{"trigger":"target_isolated","modifiers":{"attack_damage_mult":2.0}}');
INSERT INTO skill_defs VALUES('slowing_strike','减速打击','on_hit','{"effects":[{"type":"slow","value":0.3,"duration":2.0}]}');
INSERT INTO skill_defs VALUES('motley_crew','鱼龙混杂','stat_modifier','{"trigger":"nearby_diverse","modifiers":{"dodge_chance":0.0}}');
CREATE TABLE unit_skills (
    unit_type_id TEXT NOT NULL,
    skill_id TEXT NOT NULL,
    PRIMARY KEY (unit_type_id, skill_id)
);
INSERT INTO unit_skills VALUES('DwarfBerserker','berserker_rage');
INSERT INTO unit_skills VALUES('HalflingAssassin','backstab');
INSERT INTO unit_skills VALUES('HalflingThief','slowing_strike');
INSERT INTO unit_skills VALUES('ElfRanger','motley_crew');
INSERT INTO unit_skills VALUES('ElfSharpshooter','motley_crew');
INSERT INTO unit_skills VALUES('DwarfWarrior','motley_crew');
INSERT INTO unit_skills VALUES('DwarfBerserker','motley_crew');
INSERT INTO unit_skills VALUES('HalflingThief','motley_crew');
INSERT INTO unit_skills VALUES('HalflingAssassin','motley_crew');
INSERT INTO unit_skills VALUES('DragonbornMage','motley_crew');
INSERT INTO unit_skills VALUES('DragonbornArchmage','motley_crew');
PRAGMA writable_schema=ON;
CREATE TABLE IF NOT EXISTS sqlite_sequence(name,seq);
DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('asset_gen_tasks',33);
PRAGMA writable_schema=OFF;
COMMIT;
