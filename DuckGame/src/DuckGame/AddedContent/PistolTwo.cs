﻿// Decompiled with JetBrains decompiler
// Type: DuckGame.Pistol
//removed for regex reasons Culture=neutral, PublicKeyToken=null
// MVID: C907F20B-C12B-4773-9B1E-25290117C0E4
// Assembly location: D:\Program Files (x86)\Steam\steamapps\common\Duck Game\DuckGame.exe
// XML documentation location: D:\Program Files (x86)\Steam\steamapps\common\Duck Game\DuckGame.xml

namespace DuckGame
{
    [EditorGroup("Guns|Pistols")]
    [BaggedProperty("isInDemo", true)]
    [BaggedProperty("previewPriority", true)]
    [ClientOnly]
    public class PistolTwo : Gun
    {
        private SpriteMap _sprite;

        public PistolTwo(float xval, float yval)
          : base(xval, yval)
        {
            ammo = 9;
            _ammoType = new ATMissile();
            wideBarrel = true;
            barrelInsertOffset = new Vec2(0f, -1f);
            _type = "gun";
            _sprite = new SpriteMap("pistol", 18, 10);
            _sprite.AddAnimation("idle", 1f, true, new int[1]);
            _sprite.AddAnimation("fire", 0.8f, false, 1, 2, 2, 3, 3);
            _sprite.AddAnimation("empty", 1f, true, 2);
            graphic = _sprite;
            center = new Vec2(10f, 3f);
            collisionOffset = new Vec2(-8f, -3f);
            collisionSize = new Vec2(16f, 9f);
            _barrelOffsetTL = new Vec2(18f, 2f);
            _fireSound = "pistolFire";
            _kickForce = 3f;
            _fireRumble = RumbleIntensity.Kick;
            _holdOffset = new Vec2(0f, 0f);
            loseAccuracy = 0.1f;
            maxAccuracyLost = 0.6f;
            _bio = "Old faithful, the 9MM pistol.";
            _editorName = nameof(PistolTwo);
            editorTooltip = "Your average everyday pistol. Just workin' to keep its kids fed, never bothered nobody.";
            physicsMaterial = PhysicsMaterial.Metal;
        }

        public override void Update()
        {
            if (_sprite.currentAnimation == "fire" && _sprite.finished)
                _sprite.SetAnimation("idle");
            base.Update();
        }

        public override void OnPressAction()
        {
            if (ammo > 0)
            {
                _sprite.SetAnimation("fire");
                for (int index = 0; index < 3; ++index)
                {
                    Vec2 vec2 = Offset(new Vec2(-9f, 0f));
                    Vec2 hitAngle = barrelVector.Rotate(Rando.Float(1f), Vec2.Zero);
                    Level.Add(Spark.New(vec2.x, vec2.y, hitAngle, 0.1f));
                }
            }
            else
                _sprite.SetAnimation("empty");
            Fire();
        }
    }
}
