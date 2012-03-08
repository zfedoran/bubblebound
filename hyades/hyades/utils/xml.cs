/*
 * Copyright (C) 2009-2012 - Zelimir Fedoran
 *
 * This file is part of Bubble Bound.
 *
 * Bubble Bound is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Bubble Bound is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Bubble Bound.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.IO;

namespace hyades.utils
{
    public struct XmlLevel
    {
        public string name;
        public int number;
        public List<XmlEntity> entities;

        public XmlLevel(string name, int number, List<XmlEntity> entities)
        { this.name = name; this.number = number; this.entities = entities; }

        public override string ToString()
        { return string.Format("{{name:{0} number:{1} entities:{2}}}", name, number, entities.Count); }
    }

    public struct XmlEntity
    {
        public string asset;
        public string type;
        public Vector3 position, rotation, size;
        public Color color;

        public XmlEntity(string asset, string type, Vector3 position, Vector3 rotation, Vector3 size)
        { this.asset = asset; this.type = type; this.position = position; this.rotation = rotation; this.size = size; this.color = Color.White; }

        public override string ToString()
        { return string.Format("{{asset:{0} type:{1} position:[{2}] rotation:[{3}] size:[{4}]}}", asset, type, position, rotation, size); }
    }

    public class XmlHelper
    {

        public static void Save(XmlLevel level)
        { Write(string.Format("Content/levels/{0}.xml", level.number), level); }

        public static XmlLevel Load(int level)
        { return Read(string.Format("Content/levels/{0}.xml", level)); }

        public static void Write(string path, XmlLevel level)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XmlLevel));
            using (StreamWriter writer = new StreamWriter(path))
                serializer.Serialize(writer, level);
        }

        public static XmlLevel Read(string path)
        {
            XmlLevel level;

            XmlSerializer serializer = new XmlSerializer(typeof(XmlLevel));
            using (Stream reader = TitleContainer.OpenStream(path))
                 level = (XmlLevel)serializer.Deserialize(reader);

            return level;
        }
    }
}
