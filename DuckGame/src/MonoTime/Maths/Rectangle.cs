﻿// Decompiled with JetBrains decompiler
// Type: DuckGame.Rectangle
//removed for regex reasons Culture=neutral, PublicKeyToken=null
// MVID: C907F20B-C12B-4773-9B1E-25290117C0E4
// Assembly location: D:\Program Files (x86)\Steam\steamapps\common\Duck Game\DuckGame.exe
// XML documentation location: D:\Program Files (x86)\Steam\steamapps\common\Duck Game\DuckGame.xml

using System;

namespace DuckGame
{
    [Serializable]
    public struct Rectangle
    {
        public float height;
        public float width;
        public float x;
        public float y;

        public float Top
        {
            get => y;
            set => y = value;
        }

        public float Bottom
        {
            get => y + height;
            set => y = value - height;
        }

        public float Left
        {
            get => x;
            set => x = value;
        }

        public float Right
        {
            get => x + width;
            set => x = value - width;
        }

        /// <summary>Top-Left corner of the rectangle</summary>
        public Vec2 tl => new Vec2(x, y);

        /// <summary>Top-Right corner of the rectangle</summary>
        public Vec2 tr => new Vec2(x + width, y);

        /// <summary>Bottom-Left corner of the rectangle</summary>
        public Vec2 bl => new Vec2(x, y + height);

        /// <summary>Bottom-Right corner of the rectangle</summary>
        public Vec2 br => new Vec2(x + width, y + height);

        public Vec2 Center
        {
            get => new Vec2(x + width / 2f, y + height / 2f);
            set
            {
                x = value.x - width / 2f;
                y = value.y - height / 2f;
            }
        }

        /// <summary>The ratio of the width and height (w / h)</summary>
        public float aspect => width / height;

        public Rectangle(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rectangle(Vec2 tl, Vec2 br)
        {
            if (tl.x > br.x)
            {
                float x = br.x;
                br.x = tl.x;
                tl.x = x;
            }
            if (tl.y > br.y)
            {
                float y = br.y;
                br.y = tl.y;
                tl.y = y;
            }
            x = tl.x;
            y = tl.y;
            width = br.x - tl.x;
            height = br.y - tl.y;
        }

        public static implicit operator Microsoft.Xna.Framework.Rectangle(Rectangle r) => new Microsoft.Xna.Framework.Rectangle((int)r.x, (int)r.y, (int)r.width, (int)r.height);

        public static implicit operator Rectangle(Microsoft.Xna.Framework.Rectangle r) => new Rectangle(r.X, r.Y, r.Width, r.Height);

        public bool Contains(Vec2 position) => position.x >= x && position.y >= y && position.x <= x + width && position.y <= y + height;

        public Rectangle GetQuadrant(int pQuadrantStartingWithTLClockwise)
        {
            switch (pQuadrantStartingWithTLClockwise)
            {
                case 0:
                    return new Rectangle(x, y, width / 2f, height / 2f);
                case 1:
                    return new Rectangle(x + width / 2f, y, width / 2f, height / 2f);
                case 2:
                    return new Rectangle(x + width / 2f, y + height / 2f, width / 2f, height / 2f);
                default:
                    return new Rectangle(x, y + height / 2f, width / 2f, height / 2f);
            }
        }
    }
}
