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
INSERT INTO asset_gen_tasks VALUES(34,'296f2637752a4c898bec2c9afd95f600','failed','A green-skinned orc warrior girl, beautiful feminine face, tall and muscular strong build, long black hair in high ponytail, gripping a massive battle axe with both hands, wearing a simple ragged tube top and short leather skirt, toned athletic fit physique, confident fierce smirk looking to the right, charging into battle, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','3D, realistic, shading, gradient, nsfw, nude, disfigured, ugly, deformed, extra limbs, blurry',768,1024,1,15,20,7.0,-1,'6f7c4652458d4802969f8d089cf5b91f','',0,NULL,NULL,'2026-07-17T18:25:37Z','2026-07-17T18:26:43Z','Task not found on status check, resubmitted as 02bf5508dca44e0bb7dcb52ea32b8e2f');
INSERT INTO asset_gen_tasks VALUES(35,'02bf5508dca44e0bb7dcb52ea32b8e2f','completed','A green-skinned orc warrior girl, beautiful feminine face, tall and muscular strong build, long black hair in high ponytail, gripping a massive battle axe with both hands, wearing a simple ragged tube top and short leather skirt, toned athletic fit physique, confident fierce smirk looking to the right, charging into battle, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','3D, realistic, shading, gradient, nsfw, nude, disfigured, ugly, deformed, extra limbs, blurry',768,1024,1,15,20,7.0,-1,'6f7c4652458d4802969f8d089cf5b91f','',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/d9c5883de745abc11363082e9c36ac1699459f4b92d980abf41a6ea017811c2a.png','assets/art/units/orc_warrior_girl_35.png','2026-07-17T18:26:43Z','2026-07-17T18:27:41Z',NULL);
INSERT INTO asset_gen_tasks VALUES(36,'3d2ce066fce94696813f9d7fe0abbdbe','completed','A hooded eye with a red diagonal slash across it, dark blue stealth theme, game skill icon, centered composition, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/f4b0941045433413af72450ecdd4df8c90baa4788de4871a4f883d2e632a4e70.png','assets/art/skills/stealth_36.png','2026-08-04T08:06:56Z','2026-08-04T08:07:40Z',NULL);
INSERT INTO asset_gen_tasks VALUES(37,'da078088d62a4e0fa622e2b481d54cbd','completed','A medieval royal guard soldier, holding a tall polearm, wearing ornate silver and gold plate armor with a red cape, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/3c97e671e75199400b8b7a9b782bdacd85b5b96d87d5c855080c2e610d55d671.png','assets/art/cards\RoyalGuard_37.png','2026-08-04T08:08:07Z','2026-08-04T08:08:43Z',NULL);
INSERT INTO asset_gen_tasks VALUES(38,'9763716f661c49be8f0265ef2e39375e','completed','A medieval royal shield guardian, holding a large golden tower shield, wearing heavy silver plate armor with blue cape, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/c85f3beb2942ba28fd827109ad514f90561af017a968855bc0adda2d086e3892.png','assets/art/cards\RoyalShieldGuard_38.png','2026-08-04T08:08:08Z','2026-08-04T08:09:14Z',NULL);
INSERT INTO asset_gen_tasks VALUES(39,'f20094757a6f4e8d8577d980212a82c9','completed','A medieval royal sharpshooter archer, drawing a longbow with a nocked arrow, wearing light silver armor with green hood and cape, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/178d277f0ec5b65ef7479f423c88736dfd0d895a630c5e28bd91210707551a1a.png','assets/art/cards\RoyalSharpshooter_39.png','2026-08-04T08:08:09Z','2026-08-04T08:09:15Z',NULL);
INSERT INTO asset_gen_tasks VALUES(40,'b581ca7dad1644c6b3a590025070c8b2','completed','A medieval royal knight on horseback, holding a lance and a shield, wearing full silver plate armor with a flowing blue cape, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/68bc8d30f577b9190e3ba9164bc11fc29b50965fcf8f56e14ff6f59268c59b3b.png','assets/art/cards\RoyalKnight_40.png','2026-08-04T08:08:10Z','2026-08-04T08:09:15Z',NULL);
INSERT INTO asset_gen_tasks VALUES(41,'f01f186921db4e7cbe881c7478811e1a','completed','A medieval royal scout, light leather armor with a green cloak, holding a short bow, moving forward stealthily, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/a7a005c7023e3245c5514f28f89d3f0b098d82c9649d1ee863c67132e0955bfa.png','assets/art/cards\RoyalScout_41.png','2026-08-04T08:08:10Z','2026-08-04T08:09:16Z',NULL);
INSERT INTO asset_gen_tasks VALUES(42,'272146a431464ba280514e5c9110a04f','completed','An elven ranger with long pointed ears, green hooded cloak and leather armor, holding a wooden longbow, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/2aec7dc882840d4f5801a4a0f1bc5f8ceac00dba06bded9b2f12e2a74a6caf94.png','assets/art/cards\ElfRanger_42.png','2026-08-04T08:09:39Z','2026-08-04T08:11:23Z',NULL);
INSERT INTO asset_gen_tasks VALUES(43,'e28a6ed45bef4b67b7572d728dac7f3f','completed','An elven sharpshooter with long pointed ears, elegant silver and green clothing, aiming a longbow with a glowing arrow, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/ba04215eddaed3651fe7fcd3bbaa06c398af8ba66056f687d24c8dab418bff89.png','assets/art/cards\ElfSharpshooter_43.png','2026-08-04T08:09:48Z','2026-08-04T08:11:24Z',NULL);
INSERT INTO asset_gen_tasks VALUES(44,'7402142529704ad1ac923b50ad2861d2','completed','A dwarf warrior, short stocky build with a braided red beard, heavy iron armor and a round shield, holding a war hammer, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/e640783340d006cc58dd55a8ec2af37847507f7ae5d9a45d7608703fc51a98ba.png','assets/art/cards\DwarfWarrior_44.png','2026-08-04T08:09:57Z','2026-08-04T08:11:24Z',NULL);
INSERT INTO asset_gen_tasks VALUES(45,'c41c83fb489f41bfbc76b3bdc6ff290d','completed','A dwarf berserker, short stocky build with a wild orange beard, wielding two battle axes with bare muscular arms and war paint, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/e4b543afb88a6f1065956ea0b7535a40ba0244bf6cc25e366bb789b05717abcf.png','assets/art/cards\DwarfBerserker_45.png','2026-08-04T08:10:05Z','2026-08-04T08:11:25Z',NULL);
INSERT INTO asset_gen_tasks VALUES(46,'b52d165655d84709bd6299f621cb1d64','completed','A halfling thief, small nimble build, dark hooded cloak and leather vest, holding a dagger in a sneaking pose, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/d996872ddb125e714a3d5bbf747657b84caa2192c6a0fd2f2226e3fdf87c9ef1.png','assets/art/cards\HalflingThief_46.png','2026-08-04T08:10:14Z','2026-08-04T08:11:25Z',NULL);
INSERT INTO asset_gen_tasks VALUES(47,'95c710234fc24ccf84969297cba6c2a0','completed','A halfling assassin, small nimble build, black hooded outfit with daggers on the back, crouching in the shadows, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/b0dd7fda1db9f2f9d2dfad376a05b2fa51b44bf1bf4c4076e12464c5f0647e5f.png','assets/art/cards\HalflingAssassin_47.png','2026-08-04T08:10:23Z','2026-08-04T08:11:26Z',NULL);
INSERT INTO asset_gen_tasks VALUES(48,'8e08bc39555f4e60a53e4abf050e16b9','completed','A dragonborn mage with a reptilian dragon-like face and horns, purple robe, casting a glowing magic orb, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/70fa173201f66531c35d545d95fbb32724d75caf820be9ac608648f5b6d70d30.png','assets/art/cards\DragonbornMage_48.png','2026-08-04T08:10:32Z','2026-08-04T08:11:27Z',NULL);
INSERT INTO asset_gen_tasks VALUES(49,'9d84c26d516f4407bffc20d87584bbd3','completed','A dragonborn archmage with a reptilian dragon-like face and large horns, ornate purple and gold robe, casting a powerful swirling magic vortex, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/4eacef2481cb6dbec6ff82e219d707839355d63689fe58414cc7379b1907099f.png','assets/art/cards\DragonbornArchmage_49.png','2026-08-04T08:10:40Z','2026-08-04T08:11:27Z',NULL);
INSERT INTO asset_gen_tasks VALUES(50,'ea5b36b9055d40fb9a0a294f44f62082','completed','A shining steel dagger stabbing down into a red target mark, game skill icon, centered composition, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/3fcafac4cf8db1270e38f3b6b4b36e4dd4a4532161846765d1a58e235d0048ce.png','assets/art/skills\backstab_50.png','2026-08-04T08:10:49Z','2026-08-04T08:11:58Z',NULL);
INSERT INTO asset_gen_tasks VALUES(51,'b147a1399b6b4aec8f24e3399744cd37','completed','An ice blue snowflake with a sword behind it, frost effect, game skill icon, centered composition, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/f07396c05e36e613dc82518a84405403500ddac73f9c787fa4c96f528995e25a.png','assets/art/skills\slowing_strike_51.png','2026-08-04T08:10:57Z','2026-08-04T08:11:58Z',NULL);
INSERT INTO asset_gen_tasks VALUES(52,'4c39c36898744620be34ed6828a61c4f','completed','Four small adventurer head portraits together, an elf, a dwarf, a halfling and a dragonborn, colorful group emblem, game skill icon, centered composition, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/c1d10a5af3d21bb70f78346e6f0a9ee3c09033f35fb326bf0165895230283602.png','assets/art/skills\motley_crew_52.png','2026-08-04T08:11:06Z','2026-08-04T08:11:59Z',NULL);
INSERT INTO asset_gen_tasks VALUES(53,'e4627ebc17f2437c9b708dc3feab3d9f','completed','A blazing red and orange flame shaped like an angry face, rage energy burst, game skill icon, centered composition, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/4d96e6617d3d929a65b6021eb9c9c3b87ff7fdc9a35e6a127de9d61149481b73.png','assets/art/skills\berserker_rage_53.png','2026-08-04T08:11:15Z','2026-08-04T08:11:59Z',NULL);
INSERT INTO asset_gen_tasks VALUES(54,'d529b001471c4b3e8c9e5aaa1fdb70d7','completed','A holy shield guardian paladin, holding a radiant golden tower shield, glowing white and blue armor with holy light aura, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/9978730b3748295363a1d6ebac37e5c5466695788f4a50a47a0383a49b35c43e.png','assets/art/cards\HolyShieldGuardian_54.png','2026-08-04T08:20:34Z','2026-08-04T08:21:09Z',NULL);
INSERT INTO asset_gen_tasks VALUES(55,'8fb2ea9e6ab14a8595be95ab0da615ea','completed','A dragon rider commander riding a dragon with spread wings, holding a lance, silver armor with a green cape, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/ef9ca729694b0c25fb0c2e745983c684ddea57a0602e95e630b26ca3b8456eeb.png','assets/art/cards\DragonRiderCommander_55.png','2026-08-04T08:20:43Z','2026-08-04T08:21:09Z',NULL);
INSERT INTO asset_gen_tasks VALUES(56,'d07e868c5a26406ab7d1f218c8a3d50c','completed','A shadow lord warlord assassin, dark hooded black armor with glowing purple eyes, holding twin shadow daggers, surrounded by dark mist, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/a2db045ed37565956fd14cad90fc48308c7fcea69a507d69a8825b366b813d44.png','assets/art/cards\ShadowLord_56.png','2026-08-04T08:20:52Z','2026-08-04T08:21:40Z',NULL);
INSERT INTO asset_gen_tasks VALUES(57,'64ffabdcdcce4ebe9aad455bc9016eb4','completed','A medieval king wearing a golden crown and ornate royal red and gold robe with ermine trim, holding a scepter and a sword, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/9af4d2f459ba27d53bbdf571a4a7e040278e7209c4d0d77da9642572afb32058.png','assets/art/cards\King_57.png','2026-08-04T08:21:00Z','2026-08-04T08:21:40Z',NULL);
INSERT INTO asset_gen_tasks VALUES(61,'d56f806f70d3432194cce34a116c6fca','completed','A hooded eye with a red diagonal slash across it, dark blue stealth theme, game skill icon, centered composition, complete icon design with decorative emblem background, thick gold frame, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','white background, plain solid color background, transparency',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/a740ef5f9681cf799a761243953e4f042fc72cb6c50704e6ccd3508124ddec17.png','assets/art/skills\stealth_61.png','2026-08-04T10:48:03Z','2026-08-04T10:48:59Z',NULL);
INSERT INTO asset_gen_tasks VALUES(62,'dfb0b3bab90740d08f1b9f910accdcda','completed','A shining steel dagger stabbing down into a red target mark, rogue ambush theme, game skill icon, centered composition, complete icon design with decorative emblem background, thick gold frame, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','white background, plain solid color background, transparency',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/c34440822320e9ba0aa980e3cfb584fb65d4b2b2e33eced26f49084b861ea32e.png','assets/art/skills\backstab_62.png','2026-08-04T10:48:12Z','2026-08-04T10:49:00Z',NULL);
INSERT INTO asset_gen_tasks VALUES(63,'50f98f0878a349399e51082b59c2eded','completed','An ice blue snowflake with a sword behind it, frost effect, game skill icon, centered composition, complete icon design with decorative emblem background, thick gold frame, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','white background, plain solid color background, transparency',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/c18e9de2467967c206d73e6f1946a9af44f01e6b08c6ded52d03dcc7e36d86d4.png','assets/art/skills\slowing_strike_63.png','2026-08-04T10:48:20Z','2026-08-04T10:49:00Z',NULL);
INSERT INTO asset_gen_tasks VALUES(64,'96431b58d0e44bfeb42810e8a6d5ab6d','completed','Four small adventurer head portraits together, an elf, a dwarf, a halfling and a dragonborn, colorful group emblem, game skill icon, centered composition, complete icon design with decorative emblem background, thick gold frame, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','white background, plain solid color background, transparency',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/821043a27ff7a06568380e677b17fd9cb4bd660511a816cdbcdfb635f41e92f8.png','assets/art/skills\motley_crew_64.png','2026-08-04T10:49:01Z','2026-08-04T10:49:57Z',NULL);
INSERT INTO asset_gen_tasks VALUES(65,'50683c1198b74b60b4fbe476edab77eb','completed','A blazing red and orange flame shaped like an angry face, rage energy burst, game skill icon, centered composition, complete icon design with decorative emblem background, thick gold frame, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','white background, plain solid color background, transparency',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/2dc6bba862f1ac37f01dd43bf5d5fdf295580a29ad1d4231b8360937556afa92.png','assets/art/skills\berserker_rage_65.png','2026-08-04T10:49:09Z','2026-08-04T10:49:57Z',NULL);
INSERT INTO asset_gen_tasks VALUES(66,'783048e9c94043cab1bfae47f9dcce11','completed','A golden holy shield with a glowing cross emblem, divine protection, game skill icon, centered composition, complete icon design with decorative emblem background, thick gold frame, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','white background, plain solid color background, transparency',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/2fc35b10d313289f89573131591a5bcc552b2b07cafc5e8119aeaea9be36f7f5.png','assets/art/skills\grace_shield_66.png','2026-08-04T10:49:18Z','2026-08-04T10:49:57Z',NULL);
INSERT INTO asset_gen_tasks VALUES(67,'31cd60c7c65947538f6b1809c4bbf994','completed','A golden hourglass radiating holy light rays, divine blessing, game skill icon, centered composition, complete icon design with decorative emblem background, thick gold frame, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','white background, plain solid color background, transparency',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/5d3b96ac06d4269a2ad81d13d640c80b5614d1257277e3e9d65b884ef068dff9.png','assets/art/skills\grace_cooldown_67.png','2026-08-04T10:49:58Z','2026-08-04T10:50:45Z',NULL);
INSERT INTO asset_gen_tasks VALUES(68,'f7ca9814593f4067902ae88d1207968c','completed','Holy white-gold flames burning on a sacred emblem, divine judgment, game skill icon, centered composition, complete icon design with decorative emblem background, thick gold frame, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors','white background, plain solid color background, transparency',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/952278849b6ffbafd17a3490fadfbb1b7835092e8de0ffbe4b0cae615594229e.png','assets/art/skills\grace_burn_68.png','2026-08-04T10:50:06Z','2026-08-04T10:50:45Z',NULL);
INSERT INTO asset_gen_tasks VALUES(69,'13a25ff75c1e484d8ce4e072fc36142c','completed','A medieval holy shield bearer unit, wearing heavy white and gold plate armor, holding a large tower shield and a mace, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/ebe154bbf97e6918f90c41d6440dca11d41acb1957a4bb49d17aba2d29055f98.png','assets/art/cards\HolyShieldBearer_69.png','2026-08-04T12:43:55Z','2026-08-04T12:44:51Z',NULL);
INSERT INTO asset_gen_tasks VALUES(70,'4aed851d1e424226b4f7e18a71886c46','completed','A medieval paladin unit, ornate white and gold plate armor with a sun holy emblem, holding a war hammer, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/6908ba58223ed865cba1012d3f1c30ab80430bcffa12b1d2e63f8f636846dc31.png','assets/art/cards\Paladin_70.png','2026-08-04T12:44:04Z','2026-08-04T12:44:52Z',NULL);
INSERT INTO asset_gen_tasks VALUES(71,'a4b9a509308043f38a6586f66e445fc9','completed','A medieval priest unit, white robe with gold trim, holding a holy staff topped with a glowing gem, gentle healing aura, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/f752f5a7fa0adb306304eb5492ae9c63e8e540627918adc8cd02ab6365b327df.png','assets/art/cards\Priest_71.png','2026-08-04T12:44:13Z','2026-08-04T12:44:52Z',NULL);
INSERT INTO asset_gen_tasks VALUES(72,'9865cde5a44a4d82973363b5709c11e8','completed','A medieval archbishop unit, elaborate white and gold ceremonial robes with a tall mitre hat, holding a grand holy staff with a radiant sun emblem, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/8034877c28c14c1992a234153d66515c66740377fcd89c2397654a5ee931ec6b.png','assets/art/cards\Archbishop_72.png','2026-08-04T12:44:52Z','2026-08-04T12:45:48Z',NULL);
INSERT INTO asset_gen_tasks VALUES(73,'1fba52c123e8456890a0742e36729c18','completed','A medieval inquisitor unit, dark robe over heavy armor with a red cross emblem, wielding a heavy crossbow, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/2a82cb9f3111aea5ea066af152b9cfb6f92ddea7d56397222093c90269b7a01b.png','assets/art/cards\Inquisitor_73.png','2026-08-04T12:45:01Z','2026-08-04T12:45:49Z',NULL);
INSERT INTO asset_gen_tasks VALUES(74,'e069aa7b204d4d3a983f66a69f72b051','completed','A medieval punisher unit, heavy dark armor with red holy markings, wielding a large crossbow, menacing zealot, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/60418ea9a9e88a93658bd565cded1c739ae603bff5cc375bc112178208abef03.png','assets/art/cards\Punisher_74.png','2026-08-04T12:45:10Z','2026-08-04T12:45:49Z',NULL);
INSERT INTO asset_gen_tasks VALUES(75,'8ecf7cd1ce6343a4b8f86edc57e10496','completed','A medieval orc warrior unit, holding a heavy spiked club and a rough wooden shield, wearing crude iron armor with shoulder plates, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/a0ba79b3d50e26322debb92c389dc28003628a7f52afe2a7a82710e5f90e677f.png','assets/art/cards\OrcWarrior_75.png','2026-08-04T17:50:11Z','2026-08-04T17:50:53Z',NULL);
INSERT INTO asset_gen_tasks VALUES(76,'1850c5f6d9e24576bbe216ca0f489619','completed','A medieval orc berserker unit, wielding two jagged hand axes, muscular and enraged, wearing fur and leather straps with war paint, facing right, full body character, flat 2D, anime style, thick bold outlines, no shading, no gradient, simple flat colors, game character icon','',512,512,1,1,30,4.0,-1,'bf085132c7134622895b783b520b39ff','75e0be0c93b34dd8baeec9c968013e0c',5,'https://liblibai-tmp-image.liblib.cloud/img/b19801b7615a458fae2de746d03c7ea4/bc0d7ac3d046ee955bda0d7bd7fb68f58fd646de415067d2e55cb597c45b8cc3.png','assets/art/cards\OrcBerserker_76.png','2026-08-04T17:50:17Z','2026-08-04T17:50:53Z',NULL);
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
, unit_cost INTEGER NOT NULL DEFAULT 2000, race TEXT NOT NULL DEFAULT '', armor_value INTEGER NOT NULL DEFAULT 0, can_detect_stealth INTEGER NOT NULL DEFAULT 0, heal_amount REAL NOT NULL DEFAULT 0, heal_range REAL NOT NULL DEFAULT 0, heal_interval REAL NOT NULL DEFAULT 0);
INSERT INTO unit_stats VALUES('Spearman',1,0,1,2,120,4,80.0,30.0,1.0,0,4289733792,250.0,2000,'Human',30,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('ShieldBearer',1,0,2,4,220,3,45.0,30.0,1.5,0,4286611584,200.0,2400,'Human',80,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('Archer',1,1,1,1,50,12,60.0,200.0,1.5,0,4286627968,300.0,2200,'Human',20,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('Knight',2,0,0,4,150,14,120.0,35.0,1.2,0,4291862624,280.0,2600,'Human',60,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('Scout',0,0,0,0,35,4,160.0,25.0,0.8,0,4286644095,400.0,1400,'Human',10,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('Swordsman',1,0,0,3,160,6,80.0,30.0,0.9,0,4285567184,260.0,2600,'Human',50,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('HeavyShield',2,0,2,4,330,4,40.0,35.0,1.5,0,4284515808,220.0,3000,'Human',120,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('Crossbowman',1,1,1,1,70,18,55.0,180.0,2.0,0,4283476000,280.0,2800,'Human',40,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('HeavyCavalry',2,0,0,4,200,22,110.0,35.0,1.3,0,4288712672,290.0,3400,'Human',90,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('LightCavalry',1,0,0,0,55,8,180.0,30.0,0.8,0,4286632191,450.0,1800,'Human',25,1,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('HolyShieldGuardian',2,0,2,4,700,8,45.0,35.0,1.5,0,4286616704,170.0,5000,'Human',250,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('DragonRiderCommander',2,1,1,3,300,40,120.0,180.0,0.8,0,4290526975,200.0,4600,'Human',120,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('ShadowLord',1,0,6,2,180,50,100.0,30.0,1.0,0,4284248320,220.0,4000,'Human',80,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('King',2,0,5,6,1200,20,60.0,35.0,1.2,0,4294956800,170.0,6000,'Human',300,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('RoyalGuard',1,0,0,4,220,8,80.0,30.0,0.9,0,4488440,270.0,3400,'Human',80,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('RoyalShieldGuard',2,0,2,4,480,5,40.0,35.0,1.5,0,12303291,230.0,3800,'Human',180,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('RoyalSharpshooter',1,1,1,2,90,28,55.0,200.0,2.0,0,5621589,300.0,3600,'Human',70,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('RoyalKnight',2,0,0,4,280,30,110.0,35.0,1.3,0,16754900,300.0,4200,'Human',140,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('RoyalScout',1,0,0,2,80,14,180.0,30.0,0.8,0,10040232,500.0,2400,'Human',50,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('ElfRanger',0,1,1,1,45,14,150.0,220.0,1.2,0,4294954080,300.0,2400,'Elf',20,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('ElfSharpshooter',0,1,1,1,60,22,150.0,280.0,1.0,0,4294954320,350.0,3400,'Elf',35,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('DwarfWarrior',1,0,0,4,100,16,90.0,90.0,1.5,0,4286611584,170.0,2400,'Dwarf',70,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('DwarfBerserker',1,0,0,3,130,28,110.0,90.0,1.2,0,4286740992,170.0,3400,'Dwarf',90,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('HalflingThief',0,0,1,0,35,10,200.0,70.0,0.7,0,4278241280,200.0,1800,'Halfling',10,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('HalflingAssassin',0,0,1,0,50,20,220.0,70.0,0.6,0,4278237952,220.0,3200,'Halfling',15,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('DragonbornMage',1,1,4,1,55,18,110.0,200.0,1.8,0,4278190336,250.0,2600,'Dragonborn',25,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('DragonbornArchmage',1,1,4,1,75,30,110.0,240.0,1.5,0,4278190080,280.0,3800,'Dragonborn',40,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('HolyShieldBearer',2,0,2,4,300,35,40.0,45.0,1.5,0,4294956800,200.0,2600,'Human',100,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('Paladin',2,0,2,4,450,55,45.0,45.0,1.5,0,4294954080,210.0,3600,'Human',150,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('Priest',1,0,5,4,90,0,60.0,0.0,10.0,0,4290526975,220.0,2600,'Human',40,0,30.0,150.0,2.0);
INSERT INTO unit_stats VALUES('Archbishop',1,0,5,4,120,0,60.0,0.0,10.0,0,4288712672,240.0,3600,'Human',60,0,40.0,200.0,1.5);
INSERT INTO unit_stats VALUES('Inquisitor',1,1,1,4,140,45,55.0,170.0,2.0,0,4286644095,300.0,2800,'Human',70,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('Punisher',1,1,1,4,180,65,60.0,190.0,1.8,0,4286632191,320.0,3800,'Human',100,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('OrcWarrior',2,0,2,4,360,40,45.0,45.0,1.6,0,4283337530,200.0,2800,'Orc',70,0,0.0,0.0,0.0);
INSERT INTO unit_stats VALUES('OrcBerserker',2,0,2,4,480,60,55.0,45.0,1.3,0,4288359712,210.0,3800,'Orc',90,0,0.0,0.0,0.0);
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
, faction_id TEXT NOT NULL DEFAULT 'human', production_rate REAL NOT NULL DEFAULT 0.0, texture_path TEXT NOT NULL DEFAULT 'res://assets/art/placeholders/test_image.png', sprite_modulate_r REAL NOT NULL DEFAULT 1.0, sprite_modulate_g REAL NOT NULL DEFAULT 1.0, sprite_modulate_b REAL NOT NULL DEFAULT 1.0, sprite_modulate_a REAL NOT NULL DEFAULT 1.0, material_path TEXT NOT NULL DEFAULT '', value INTEGER);
INSERT INTO building_defs VALUES('CastleHeart','城堡之心',500,NULL,0,0,0,0,NULL,0,0,1,'[[0,0],[1,0],[0,1],[1,1]]',180,180,'alliance',0.0,'res://assets/art/placeholders/test_image.png',1.0,0.82,0.35,1.0,'',0);
INSERT INTO building_defs VALUES('RangerPost','游骑哨',150,5.0,0,0,1,1,'LightCavalry',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.6,0.75,0.95,1.0,'',5);
INSERT INTO building_defs VALUES('RangerGuild','游侠公会',180,9.0,0,0,1,0,'ElfRanger',0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.35,0.7,0.4,1.0,'',3);
INSERT INTO building_defs VALUES('WindSanctum','风行圣殿',260,7.0,0,0,1,0,'ElfSharpshooter',0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.65,0.8,0.95,1.0,'',7);
INSERT INTO building_defs VALUES('WarriorGuild','战士公会',250,12.0,0,0,1,0,'DwarfWarrior',0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.7,0.35,0.25,1.0,'',4);
INSERT INTO building_defs VALUES('MountainFort','山丘要塞',360,10.0,0,0,1,0,'DwarfBerserker',0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.55,0.45,0.35,1.0,'',9);
INSERT INTO building_defs VALUES('ThiefGuild','盗贼行会',140,7.0,0,0,1,0,'HalflingThief',0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.35,0.35,0.4,1.0,'',2);
INSERT INTO building_defs VALUES('ShadowGuild','暗影公会',200,5.0,0,0,1,0,'HalflingAssassin',0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.3,0.2,0.4,1.0,'',5);
INSERT INTO building_defs VALUES('MageTower','术士高塔',160,14.0,0,0,1,0,'DragonbornMage',0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.5,0.3,0.8,1.0,'',5);
INSERT INTO building_defs VALUES('DragonfireTower','龙火塔',230,12.0,0,0,1,0,'DragonbornArchmage',0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',100.0,'res://assets/art/placeholders/test_image.png',0.9,0.3,0.15,1.0,'',10);
INSERT INTO building_defs VALUES('Inn','酒馆',120,NULL,0,0,1,0,NULL,0,0,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',0.0,'res://assets/art/placeholders/test_image.png',0.85,0.6,0.25,1.0,'',2);
INSERT INTO building_defs VALUES('AdventurerGuild','冒险者公会',200,NULL,0,0,1,0,NULL,0,1,0,'[[0,0],[0,1],[1,0],[1,1]]',200,200,'alliance',0.0,'res://assets/art/placeholders/test_image.png',0.8,0.5,0.2,1.0,'',6);
INSERT INTO building_defs VALUES('OrcCamp','兽人营地',260,9.0,0,0,1,1,'OrcWarrior',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'horde',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',3);
INSERT INTO building_defs VALUES('OrcFortress','兽人要塞',400,7.0,0,0,1,1,'OrcBerserker',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'horde',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',7);
INSERT INTO building_defs VALUES('Barracks','兵营',200,8.0,0,0,1,1,'Spearman',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',3);
INSERT INTO building_defs VALUES('Sanctum','圣所',280,12.0,0,0,1,1,'HolyShieldBearer',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',5);
INSERT INTO building_defs VALUES('ShieldCamp','盾营',250,12.0,0,0,1,1,'ShieldBearer',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',3);
INSERT INTO building_defs VALUES('HolySpring','圣泉',180,10.0,0,0,1,1,'Priest',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',5);
INSERT INTO building_defs VALUES('ArcheryRange','靶场',150,10.0,0,0,1,1,'Archer',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',4);
INSERT INTO building_defs VALUES('Tribunal','裁判所',200,11.0,0,0,1,1,'Inquisitor',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',6);
INSERT INTO building_defs VALUES('Stable','马厩',220,14.0,0,0,1,1,'Knight',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',5);
INSERT INTO building_defs VALUES('ScoutCamp','斥候营',100,6.0,0,0,1,1,'Scout',0,0,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',2);
INSERT INTO building_defs VALUES('Armory','军府',300,7.0,0,0,1,1,'Swordsman',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',6);
INSERT INTO building_defs VALUES('GreatTemple','圣殿',420,9.0,0,0,1,1,'Paladin',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',10);
INSERT INTO building_defs VALUES('Bulwark','壁垒',380,10.0,0,0,1,1,'HeavyShield',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',6);
INSERT INTO building_defs VALUES('HolyLightSanctum','圣光圣所',280,8.0,0,0,1,1,'Archbishop',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',10);
INSERT INTO building_defs VALUES('CrossbowTower','射楼',220,9.0,0,0,1,1,'Crossbowman',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',8);
INSERT INTO building_defs VALUES('JudgementHall','裁决厅',320,9.0,0,0,1,1,'Punisher',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',12);
INSERT INTO building_defs VALUES('Ranch','牧场',330,12.0,0,0,1,1,'HeavyCavalry',0,1,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',8);
INSERT INTO building_defs VALUES('RoyalArmory','皇家军营',300,7.0,0,0,1,1,'RoyalGuard',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',10);
INSERT INTO building_defs VALUES('RoyalBulwark','皇家壁垒',380,10.0,0,0,1,1,'RoyalShieldGuard',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',10);
INSERT INTO building_defs VALUES('RoyalArcheryRange','皇家射场',220,9.0,0,0,1,1,'RoyalSharpshooter',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',14);
INSERT INTO building_defs VALUES('RoyalStable','皇家马场',330,12.0,0,0,1,1,'RoyalKnight',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',12);
INSERT INTO building_defs VALUES('RoyalRanger','皇家游骑',150,5.0,0,0,1,1,'RoyalScout',0,2,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',10);
INSERT INTO building_defs VALUES('HolyWall','圣壁',500,14.0,0,0,1,1,'HolyShieldGuardian',0,3,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',24);
INSERT INTO building_defs VALUES('HeavenPunishmentTower','天罚塔',350,12.0,0,0,1,1,'DragonRiderCommander',0,3,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',30);
INSERT INTO building_defs VALUES('ShadowSanctum','暗影圣所',250,8.0,0,0,1,1,'ShadowLord',0,3,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',16);
INSERT INTO building_defs VALUES('RoyalCourt','王庭',800,30.0,0,0,1,1,'King',0,4,0,'[[0,0],[1,0],[0,1],[1,1]]',188,188,'alliance',100.0,'res://assets/art/placeholders/test_image.png',1.0,1.0,1.0,1.0,'',60);
CREATE TABLE damage_matrix (
    damage_type INTEGER NOT NULL,
    armor_type  INTEGER NOT NULL,
    multiplier  REAL    NOT NULL,
    PRIMARY KEY (damage_type, armor_type)
);
INSERT INTO damage_matrix VALUES(0,0,1.5);
INSERT INTO damage_matrix VALUES(0,1,1.5);
INSERT INTO damage_matrix VALUES(0,2,1.0);
INSERT INTO damage_matrix VALUES(0,3,1.0);
INSERT INTO damage_matrix VALUES(0,4,0.69999998807907105);
INSERT INTO damage_matrix VALUES(0,5,0.69999998807907105);
INSERT INTO damage_matrix VALUES(0,6,0.10000000149011612);
INSERT INTO damage_matrix VALUES(1,0,1.5);
INSERT INTO damage_matrix VALUES(1,1,1.0);
INSERT INTO damage_matrix VALUES(1,2,1.5);
INSERT INTO damage_matrix VALUES(1,3,1.0);
INSERT INTO damage_matrix VALUES(1,4,1.0);
INSERT INTO damage_matrix VALUES(1,5,0.69999998807907105);
INSERT INTO damage_matrix VALUES(1,6,0.10000000149011612);
INSERT INTO damage_matrix VALUES(2,0,1.5);
INSERT INTO damage_matrix VALUES(2,1,1.0);
INSERT INTO damage_matrix VALUES(2,2,0.69999998807907105);
INSERT INTO damage_matrix VALUES(2,3,1.0);
INSERT INTO damage_matrix VALUES(2,4,1.5);
INSERT INTO damage_matrix VALUES(2,5,0.69999998807907105);
INSERT INTO damage_matrix VALUES(2,6,0.10000000149011612);
INSERT INTO damage_matrix VALUES(3,0,1.5);
INSERT INTO damage_matrix VALUES(3,1,0.69999998807907105);
INSERT INTO damage_matrix VALUES(3,2,0.69999998807907105);
INSERT INTO damage_matrix VALUES(3,3,0.69999998807907105);
INSERT INTO damage_matrix VALUES(3,4,0.69999998807907105);
INSERT INTO damage_matrix VALUES(3,5,1.5);
INSERT INTO damage_matrix VALUES(3,6,0.10000000149011612);
INSERT INTO damage_matrix VALUES(4,0,1.5);
INSERT INTO damage_matrix VALUES(4,1,0.69999998807907105);
INSERT INTO damage_matrix VALUES(4,2,1.0);
INSERT INTO damage_matrix VALUES(4,3,1.0);
INSERT INTO damage_matrix VALUES(4,4,1.5);
INSERT INTO damage_matrix VALUES(4,5,0.69999998807907105);
INSERT INTO damage_matrix VALUES(4,6,0.10000000149011612);
INSERT INTO damage_matrix VALUES(5,0,1.0);
INSERT INTO damage_matrix VALUES(5,1,1.0);
INSERT INTO damage_matrix VALUES(5,2,1.0);
INSERT INTO damage_matrix VALUES(5,3,1.0);
INSERT INTO damage_matrix VALUES(5,4,1.0);
INSERT INTO damage_matrix VALUES(5,5,1.0);
INSERT INTO damage_matrix VALUES(5,6,1.0);
INSERT INTO damage_matrix VALUES(6,0,0.69999998807907105);
INSERT INTO damage_matrix VALUES(6,1,0.69999998807907105);
INSERT INTO damage_matrix VALUES(6,2,1.0);
INSERT INTO damage_matrix VALUES(6,3,1.0);
INSERT INTO damage_matrix VALUES(6,4,1.5);
INSERT INTO damage_matrix VALUES(6,5,1.5);
INSERT INTO damage_matrix VALUES(6,6,1.0);
CREATE TABLE shop_catalog (
    id            TEXT PRIMARY KEY,
    name          TEXT    NOT NULL,
    cost          INTEGER NOT NULL,
    building_type TEXT    NOT NULL,
    weight        INTEGER NOT NULL DEFAULT 1
);
INSERT INTO shop_catalog VALUES('RangerGuild','游侠公会',3,'RangerGuild',12);
INSERT INTO shop_catalog VALUES('WarriorGuild','战士公会',4,'WarriorGuild',10);
INSERT INTO shop_catalog VALUES('ThiefGuild','盗贼行会',2,'ThiefGuild',10);
INSERT INTO shop_catalog VALUES('MageTower','术士高塔',5,'MageTower',6);
INSERT INTO shop_catalog VALUES('Inn','酒馆',2,'Inn',14);
INSERT INTO shop_catalog VALUES('OrcCamp','兽人营地',3,'OrcCamp',12);
INSERT INTO shop_catalog VALUES('Barracks','兵营',3,'Barracks',14);
INSERT INTO shop_catalog VALUES('Sanctum','圣所',5,'Sanctum',6);
INSERT INTO shop_catalog VALUES('ShieldCamp','盾营',3,'ShieldCamp',10);
INSERT INTO shop_catalog VALUES('HolySpring','圣泉',5,'HolySpring',4);
INSERT INTO shop_catalog VALUES('ArcheryRange','靶场',4,'ArcheryRange',12);
INSERT INTO shop_catalog VALUES('Tribunal','裁判所',6,'Tribunal',2);
INSERT INTO shop_catalog VALUES('Stable','马厩',5,'Stable',10);
INSERT INTO shop_catalog VALUES('ScoutCamp','斥候营',2,'ScoutCamp',8);
INSERT INTO shop_catalog VALUES('Armory','军府',6,'Armory',8);
INSERT INTO shop_catalog VALUES('Bulwark','壁垒',6,'Bulwark',6);
INSERT INTO shop_catalog VALUES('Ranch','牧场',8,'Ranch',6);
INSERT INTO shop_catalog VALUES('RoyalArmory','皇家军营',10,'RoyalArmory',4);
INSERT INTO shop_catalog VALUES('RoyalBulwark','皇家壁垒',10,'RoyalBulwark',4);
INSERT INTO shop_catalog VALUES('RoyalStable','皇家马场',12,'RoyalStable',4);
CREATE TABLE IF NOT EXISTS "faction_defs" (
    id TEXT PRIMARY KEY,
    display_name TEXT NOT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0
);
INSERT INTO faction_defs VALUES('alliance','联盟',0);
INSERT INTO faction_defs VALUES('horde','部落',1);
INSERT INTO faction_defs VALUES('necromancer','死灵法师',2);
INSERT INTO faction_defs VALUES('dungeon','地下城',3);
CREATE TABLE tech_tree_nodes (
    type_id TEXT PRIMARY KEY,
    faction_id TEXT NOT NULL REFERENCES "faction_defs"(id),
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
, value INTEGER);
INSERT INTO tech_tree_nodes VALUES('OrcCamp','horde',1,0,1,3,12,0,'兽人营地','OrcWarrior',260,9.0,3);
INSERT INTO tech_tree_nodes VALUES('OrcFortress','horde',2,0,0,NULL,NULL,0,'兽人要塞','OrcBerserker',400,7.0,7);
INSERT INTO tech_tree_nodes VALUES('Barracks','alliance',1,0,1,3,14,0,'兵营','Spearman',200,8.0,3);
INSERT INTO tech_tree_nodes VALUES('Sanctum','alliance',1,0,1,5,6,0,'圣所','HolyShieldBearer',280,12.0,5);
INSERT INTO tech_tree_nodes VALUES('ShieldCamp','alliance',1,1,1,3,10,0,'盾营','ShieldBearer',250,12.0,3);
INSERT INTO tech_tree_nodes VALUES('HolySpring','alliance',1,1,1,5,4,0,'圣泉','Priest',180,10.0,5);
INSERT INTO tech_tree_nodes VALUES('ArcheryRange','alliance',1,2,1,4,12,0,'靶场','Archer',150,10.0,4);
INSERT INTO tech_tree_nodes VALUES('Tribunal','alliance',1,2,1,6,2,0,'裁判所','Inquisitor',200,11.0,6);
INSERT INTO tech_tree_nodes VALUES('Stable','alliance',1,3,1,5,10,0,'马厩','Knight',220,14.0,5);
INSERT INTO tech_tree_nodes VALUES('ScoutCamp','alliance',1,4,1,2,8,0,'斥候营','Scout',100,6.0,2);
INSERT INTO tech_tree_nodes VALUES('Armory','alliance',2,0,1,6,8,0,'军府','Swordsman',300,7.0,6);
INSERT INTO tech_tree_nodes VALUES('GreatTemple','alliance',2,0,0,NULL,NULL,0,'圣殿','Paladin',420,9.0,10);
INSERT INTO tech_tree_nodes VALUES('Bulwark','alliance',2,1,1,6,6,0,'壁垒','HeavyShield',380,10.0,6);
INSERT INTO tech_tree_nodes VALUES('HolyLightSanctum','alliance',2,1,0,NULL,NULL,0,'圣光圣所','Archbishop',280,8.0,10);
INSERT INTO tech_tree_nodes VALUES('CrossbowTower','alliance',2,2,0,NULL,NULL,0,'射楼','Crossbowman',220,9.0,8);
INSERT INTO tech_tree_nodes VALUES('JudgementHall','alliance',2,2,0,NULL,NULL,0,'裁决厅','Punisher',320,9.0,12);
INSERT INTO tech_tree_nodes VALUES('Ranch','alliance',2,3,1,8,6,0,'牧场','HeavyCavalry',330,12.0,8);
INSERT INTO tech_tree_nodes VALUES('RoyalArmory','alliance',3,0,1,10,4,0,'皇家军营','RoyalGuard',300,7.0,10);
INSERT INTO tech_tree_nodes VALUES('RoyalBulwark','alliance',3,1,1,10,4,0,'皇家壁垒','RoyalShieldGuard',380,10.0,10);
INSERT INTO tech_tree_nodes VALUES('RoyalArcheryRange','alliance',3,2,0,NULL,NULL,0,'皇家射场','RoyalSharpshooter',220,9.0,14);
INSERT INTO tech_tree_nodes VALUES('RoyalStable','alliance',3,3,1,12,4,0,'皇家马场','RoyalKnight',330,12.0,12);
INSERT INTO tech_tree_nodes VALUES('RoyalRanger','alliance',3,4,0,NULL,NULL,0,'皇家游骑','RoyalScout',150,5.0,10);
INSERT INTO tech_tree_nodes VALUES('HolyWall','alliance',4,0,0,NULL,NULL,0,'圣壁','HolyShieldGuardian',500,14.0,24);
INSERT INTO tech_tree_nodes VALUES('HeavenPunishmentTower','alliance',4,2,0,NULL,NULL,0,'天罚塔','DragonRiderCommander',350,12.0,30);
INSERT INTO tech_tree_nodes VALUES('ShadowSanctum','alliance',4,4,0,NULL,NULL,0,'暗影圣所','ShadowLord',250,8.0,16);
INSERT INTO tech_tree_nodes VALUES('RoyalCourt','alliance',5,2,0,NULL,NULL,0,'王庭','King',800,30.0,60);
CREATE TABLE IF NOT EXISTS "combine_recipes" (
                main_type_id TEXT NOT NULL,
                material_type_id TEXT NOT NULL,
                material_count INTEGER NOT NULL,
                result_type_id TEXT NOT NULL,
                PRIMARY KEY (main_type_id, material_type_id, result_type_id)
            );
INSERT INTO combine_recipes VALUES('RoyalBulwark','RoyalArmory',1,'HolyWall');
INSERT INTO combine_recipes VALUES('HolyWall','HeavenPunishmentTower',1,'RoyalCourt');
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
INSERT INTO unit_skills VALUES('HalflingAssassin','stealth');
INSERT INTO unit_skills VALUES('HolyShieldBearer','grace_shield');
INSERT INTO unit_skills VALUES('Paladin','grace_shield');
INSERT INTO unit_skills VALUES('Priest','grace_cooldown');
INSERT INTO unit_skills VALUES('Archbishop','grace_cooldown');
INSERT INTO unit_skills VALUES('Inquisitor','grace_burn_t1');
INSERT INTO unit_skills VALUES('Punisher','grace_burn_t2');
INSERT INTO unit_skills VALUES('OrcWarrior','orc_rage_t1');
INSERT INTO unit_skills VALUES('OrcBerserker','orc_rage_t2');
CREATE TABLE IF NOT EXISTS "skill_defs" (
    id TEXT PRIMARY KEY,
    display_name TEXT NOT NULL,
    skill_type TEXT NOT NULL CHECK(skill_type IN ('stat_modifier','aura','on_hit','on_heal','special')),
    config_json TEXT NOT NULL
, description TEXT);
INSERT INTO skill_defs VALUES('berserker_rage','狂战士之怒','stat_modifier','{"trigger":"low_health","trigger_param":0.5,"modifiers":{"attack_speed_mult":1.5}}','生命低于 50% 时攻击速度提升 50%');
INSERT INTO skill_defs VALUES('backstab','背刺','stat_modifier','{"trigger":"target_isolated","modifiers":{"attack_damage_mult":2.0}}','对孤立目标的攻击伤害提升 100%');
INSERT INTO skill_defs VALUES('slowing_strike','减速打击','on_hit','{"effects":[{"type":"slow","value":0.3,"duration":2.0}]}','攻击命中使目标减速 30%，持续 2 秒');
INSERT INTO skill_defs VALUES('motley_crew','鱼龙混杂','stat_modifier','{"trigger":"nearby_diverse","modifiers":{"dodge_chance":0.0}}','周围友军种族越多，闪避率越高');
INSERT INTO skill_defs VALUES('stealth','潜行','special','{"behavior": "stealth", "param": 180, "speed_mult": 0.85, "cooldown": 3.0}','脱离战斗后进入潜行，移动速度降低 15%，被 AOE 命中会现身');
INSERT INTO skill_defs VALUES('grace_shield','恩典·护盾','on_heal','{"effects":[{"type":"shield"}]}','受到治疗时获得护盾（治疗量×150%）');
INSERT INTO skill_defs VALUES('grace_cooldown','恩典·冷却祝福','on_heal','{"effects":[{"type":"cooldown_reduction","value":1.0}]}','受到治疗时技能冷却减少 1 秒');
INSERT INTO skill_defs VALUES('grace_burn_t1','恩典·灼烧','on_heal','{"effects":[{"type":"burn","value":15,"range":120}]}','受到治疗时灼烧周围 120 范围内敌人，造成 15 点伤害');
INSERT INTO skill_defs VALUES('grace_burn_t2','恩典·灼烧','on_heal','{"effects":[{"type":"burn","value":25,"range":120}]}','受到治疗时灼烧周围 120 范围内敌人，造成 25 点伤害');
INSERT INTO skill_defs VALUES('orc_rage_t1','狂怒','stat_modifier','{"trigger":"on_damaged","race":"Orc","modifiers":{"attack_damage_mult":1.3,"move_speed_mult":1.15}}','受到伤害后进入狂怒状态（永久）：攻击力 +30%，移动速度 +15%（仅兽人）');
INSERT INTO skill_defs VALUES('orc_rage_t2','狂暴','stat_modifier','{"trigger":"on_damaged","race":"Orc","modifiers":{"attack_damage_mult":1.15,"attack_speed_mult":1.3,"move_speed_mult":1.15}}','受到伤害后进入狂暴状态（永久）：攻击力 +15%，攻击速度 +30%，移动速度 +15%（仅兽人）');
CREATE TABLE upgrade_recipes (
    source_type_id TEXT NOT NULL,
    result_type_id TEXT NOT NULL,
    PRIMARY KEY (source_type_id, result_type_id)
);
INSERT INTO upgrade_recipes VALUES('OrcCamp','OrcFortress');
INSERT INTO upgrade_recipes VALUES('Sanctum','GreatTemple');
INSERT INTO upgrade_recipes VALUES('ShieldCamp','Bulwark');
INSERT INTO upgrade_recipes VALUES('HolySpring','HolyLightSanctum');
INSERT INTO upgrade_recipes VALUES('ArcheryRange','CrossbowTower');
INSERT INTO upgrade_recipes VALUES('Tribunal','JudgementHall');
INSERT INTO upgrade_recipes VALUES('Stable','Ranch');
INSERT INTO upgrade_recipes VALUES('Armory','RoyalBulwark');
INSERT INTO upgrade_recipes VALUES('Ranch','RoyalRanger');
INSERT INTO upgrade_recipes VALUES('Ranch','RoyalStable');
INSERT INTO upgrade_recipes VALUES('RoyalArcheryRange','HeavenPunishmentTower');
INSERT INTO upgrade_recipes VALUES('RoyalRanger','ShadowSanctum');
PRAGMA writable_schema=ON;
CREATE TABLE IF NOT EXISTS sqlite_sequence(name,seq);
DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('asset_gen_tasks',76);
PRAGMA writable_schema=OFF;
COMMIT;
