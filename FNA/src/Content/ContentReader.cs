#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2022 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	public sealed class ContentReader : BinaryReader
	{
		#region Public Properties

		public ContentManager ContentManager
		{
			get
			{
				return contentManager;
			}
		}

		public string AssetName
		{
			get
			{
				return assetName;
			}
		}

		#endregion

		#region Internal Properties

		internal ContentTypeReader[] TypeReaders
		{
			get
			{
				return typeReaders;
			}
		}

		#endregion

		#region Internal Variables

		internal int version;
		internal char platform;

		#endregion

		#region Private Variables

		private ContentManager contentManager;
		private Action<IDisposable> recordDisposableObject;
		private ContentTypeReaderManager typeReaderManager;
		private ContentTypeReader[] typeReaders;
		private string assetName;

		/* From what I can tell, shared resources work like this:
		 * A list of shared resources is stored at the end of the file,
		 * and while we're reading the whole object, the parts that ask
		 * for "shared" objects store the 1-based index of the shared
		 * resource in the list. For example, if there are two shared
		 * resources, a ReadSharedResource function will ask for either
		 * 1 or 2. For null references, the index will be 0.
		 */
		private int sharedResourceCount;
		private object[] sharedResources;
		private List<Action<object>>[] sharedResourceFixups;

		#endregion

		#region Internal Constructor

		internal ContentReader(
			ContentManager manager,
			Stream stream,
			string assetName,
			int version,
			char platform,
			Action<IDisposable> recordDisposableObject
		) : base(stream) {
			this.recordDisposableObject = recordDisposableObject;
			this.contentManager = manager;
			this.assetName = assetName;
			this.version = version;
			this.platform = platform;
		}

		#endregion

		#region Public Read Methods

		public T ReadExternalReference<T>()
		{
			string externalReference = ReadString();
			if (!String.IsNullOrEmpty(externalReference))
			{
				return contentManager.Load<T>(
					MonoGame.Utilities.FileHelpers.ResolveRelativePath(assetName, externalReference)
				);
			}
			return default(T);
		}

		public Matrix ReadMatrix()
		{
            Matrix result = new Matrix
            {
                M11 = ReadSingle(),
                M12 = ReadSingle(),
                M13 = ReadSingle(),
                M14 = ReadSingle(),
                M21 = ReadSingle(),
                M22 = ReadSingle(),
                M23 = ReadSingle(),
                M24 = ReadSingle(),
                M31 = ReadSingle(),
                M32 = ReadSingle(),
                M33 = ReadSingle(),
                M34 = ReadSingle(),
                M41 = ReadSingle(),
                M42 = ReadSingle(),
                M43 = ReadSingle(),
                M44 = ReadSingle()
            };
            return result;
		}

		public T ReadObject<T>()
		{
			return ReadObject(default(T));
		}

		public T ReadObject<T>(ContentTypeReader typeReader)
		{
			T result = (T) typeReader.Read(this, default(T));
			RecordDisposable(result);
			return result;
		}

		public T ReadObject<T>(T existingInstance)
		{
			return InnerReadObject(existingInstance);
		}

		public T ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
		{
			if (!typeReader.TargetType.IsValueType)
			{
				return ReadObject(existingInstance);
			}
			T result = (T) typeReader.Read(this, existingInstance);
			RecordDisposable(result);
			return result;
		}

		public Quaternion ReadQuaternion()
		{
            Quaternion result = new Quaternion
            {
                X = ReadSingle(),
                Y = ReadSingle(),
                Z = ReadSingle(),
                W = ReadSingle()
            };
            return result;
		}

		public T ReadRawObject<T>()
		{
			return ReadRawObject(default(T));
		}

		public T ReadRawObject<T>(ContentTypeReader typeReader)
		{
			return ReadRawObject(typeReader, default(T));
		}

		public T ReadRawObject<T>(T existingInstance)
		{
			Type objectType = typeof(T);
			foreach (ContentTypeReader typeReader in typeReaders)
			{
				if (typeReader.TargetType == objectType)
				{
					return ReadRawObject(typeReader, existingInstance);
				}
			}
			throw new NotSupportedException();
		}

		public T ReadRawObject<T>(ContentTypeReader typeReader, T existingInstance)
		{
			return (T) typeReader.Read(this, existingInstance);
		}

		public void ReadSharedResource<T>(Action<T> fixup)
		{
			int index = Read7BitEncodedInt();
			if (index > 0)
			{
				sharedResourceFixups[index - 1].Add(
					delegate(object v)
					{
						if (!(v is T))
						{
							throw new ContentLoadException(
								String.Format(
									"Error loading shared resource. Expected type {0}, received type {1}",
									typeof(T).Name, v.GetType().Name
								)
							);
						}
						fixup((T) v);
					}
				);
			}
		}

		public Vector2 ReadVector2()
		{
            Vector2 result = new Vector2
            {
                X = ReadSingle(),
                Y = ReadSingle()
            };
            return result;
		}

		public Vector3 ReadVector3()
		{
            Vector3 result = new Vector3
            {
                X = ReadSingle(),
                Y = ReadSingle(),
                Z = ReadSingle()
            };
            return result;
		}

		public Vector4 ReadVector4()
		{
            Vector4 result = new Vector4
            {
                X = ReadSingle(),
                Y = ReadSingle(),
                Z = ReadSingle(),
                W = ReadSingle()
            };
            return result;
		}

		public Color ReadColor()
		{
            Color result = new Color
            {
                R = ReadByte(),
                G = ReadByte(),
                B = ReadByte(),
                A = ReadByte()
            };
            return result;
		}

		#endregion

		#region Internal Methods

		internal object ReadAsset<T>()
		{
			InitializeTypeReaders();
			// Read primary object
			object result = ReadObject<T>();
			// Read shared resources
			ReadSharedResources();
			return result;
		}

		internal void InitializeTypeReaders()
		{
			typeReaderManager = new ContentTypeReaderManager();
			typeReaders = typeReaderManager.LoadAssetReaders(this);
			sharedResourceCount = Read7BitEncodedInt();
			sharedResources = new object[sharedResourceCount];
			sharedResourceFixups = new List<Action<object>>[sharedResourceCount];
			for (int i = 0; i < sharedResourceCount; i += 1)
			{
				sharedResourceFixups[i] = new List<Action<object>>();
			}
		}

		internal void ReadSharedResources()
		{
			// We have to read _all_ the objects first, BEFORE doing fixups!
			for (int i = 0; i < sharedResourceCount; i += 1)
			{
				sharedResources[i] = InnerReadObject<object>(null);
			}

			// ... okay, NOW we send them to each ReadSharedResource caller
			for (int i = 0; i < sharedResourceCount; i += 1)
			{
				object sharedResource = sharedResources[i];
				foreach (Action<object> fixup in sharedResourceFixups[i])
				{
					fixup(sharedResource);
				}
			}
		}

		internal new int Read7BitEncodedInt()
		{
			return base.Read7BitEncodedInt();
		}

		internal BoundingSphere ReadBoundingSphere()
		{
			Vector3 position = ReadVector3();
			float radius = ReadSingle();
			return new BoundingSphere(position, radius);
		}

		#endregion

		#region Private Methods

		private T InnerReadObject<T>(T existingInstance)
		{
			int typeReaderIndex = Read7BitEncodedInt();
			if (typeReaderIndex == 0)
			{
				return existingInstance;
			}
			if (typeReaderIndex > typeReaders.Length)
			{
				throw new ContentLoadException(
					"Incorrect type reader index found!"
				);
			}
			ContentTypeReader typeReader = typeReaders[typeReaderIndex - 1];
			T result = (T) typeReader.Read(this, default(T));
			RecordDisposable(result);
			return result;
		}

		private void RecordDisposable<T>(T result)
		{
			IDisposable disposable = result as IDisposable;
			if (disposable == null)
			{
				return;
			}
			if (recordDisposableObject != null)
			{
				recordDisposableObject(disposable);
			}
			else
			{
				contentManager.RecordDisposable(disposable);
			}
		}

		#endregion
	}
}
