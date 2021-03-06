﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.ViewModels
{
	using System;
	using Anamnesis;
	using ConceptMatrix;
	using ConceptMatrix.AppearanceModule.Items;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public abstract class EquipmentBaseViewModel : IDisposable
	{
		public static readonly DummyNoneItem NoneItem = new DummyNoneItem();
		public static readonly DummyNoneDye NoneDye = new DummyNoneDye();
		public static readonly NpcBodyItem NpcbodyItem = new NpcBodyItem();

		public readonly Actor Actor;

		protected IGameDataService gameData;

		protected Vector scale;
		protected ushort modelBase;
		protected ushort modelSet;
		protected ushort modelVariant;
		protected byte dyeId;

		private IItem item;
		private IDye dye;

		public EquipmentBaseViewModel(ItemSlots slot, Actor actor)
		{
			this.Actor = actor;
			this.gameData = Services.Get<IGameDataService>();
			this.Slot = slot;
		}

		public delegate void ChangedHandler();

		public event ChangedHandler Changed;

		public ItemSlots Slot
		{
			get;
			private set;
		}

		[AlsoNotifyFor(nameof(ModelSet), nameof(ModelBase), nameof(ModelVariant))]
		public IItem Item
		{
			get
			{
				return this.item;
			}

			set
			{
				IItem oldItem = this.item;
				this.item = value;

				if (value != null)
				{
					bool useSubModel = this.Slot == ItemSlots.OffHand && value.HasSubModel;

					this.modelSet = useSubModel ? value.SubWeaponSet : value.WeaponSet;
					this.modelBase = useSubModel ? value.SubModelBase : value.ModelBase;
					this.modelVariant = useSubModel ? value.ModelVariant : value.ModelVariant;
				}
				else
				{
					this.modelSet = 0;
					this.modelBase = 0;
					this.modelVariant = 0;
				}

				if (this.modelBase == 0)
					this.Dye = NoneDye;

				if (oldItem != null && oldItem != this.item)
				{
					this.Apply();
				}
			}
		}

		public IDye Dye
		{
			get
			{
				return this.dye;
			}

			set
			{
				IDye oldDye = this.dye;
				this.dye = value;
				this.dyeId = this.dye != null ? value.Id : (byte)0;

				if (oldDye != null && oldDye != this.dye)
				{
					this.Apply();
				}
			}
		}

		public byte DyeId
		{
			get
			{
				return this.dyeId;
			}
			set
			{
				this.dyeId = value;
				this.Dye = this.GetDye();
			}
		}

		public Vector Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				if (this.scale == value)
					return;

				this.scale = value;
				this.Apply();
			}
		}

		[DependsOn(nameof(EquipmentBaseViewModel.Item))]
		public int Key
		{
			get
			{
				if (this.item == null)
					return 0;

				return this.Item.Key;
			}
			set
			{
				IItem item = this.gameData.Items.Get(value);

				if (item != null && item.FitsInSlot(this.Slot))
				{
					this.Item = item;
				}
			}
		}

		[DependsOn(nameof(EquipmentBaseViewModel.Item))]
		public ushort ModelBase
		{
			get
			{
				return this.modelBase;
			}
			set
			{
				this.modelBase = value;
				this.Item = this.GetItem();
			}
		}

		[DependsOn(nameof(EquipmentBaseViewModel.Item))]
		public ushort ModelVariant
		{
			get
			{
				return this.modelVariant;
			}

			set
			{
				this.modelVariant = value;
				this.Item = this.GetItem();
			}
		}

		[DependsOn(nameof(EquipmentBaseViewModel.Item))]
		public ushort ModelSet
		{
			get
			{
				return this.modelSet;
			}
			set
			{
				this.modelSet = value;
				this.Item = this.GetItem();
			}
		}

		public abstract void Dispose();

		public abstract void Apply();

		protected IItem GetItem()
		{
			if (this.ModelBase == 0 && this.modelVariant == 0)
				return NoneItem;

			if (this.ModelBase == NpcbodyItem.ModelBase)
				return NpcbodyItem;

			foreach (IItem tItem in this.gameData.Items.All)
			{
				if (this.Slot == ItemSlots.MainHand || this.Slot == ItemSlots.OffHand)
				{
					if (!tItem.IsWeapon)
						continue;
				}
				else
				{
					if (!tItem.FitsInSlot(this.Slot))
						continue;
				}

				// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
				if (this.Slot == ItemSlots.Wrists && tItem.Name.StartsWith("Promise of"))
					continue;

				if (this.Slot == ItemSlots.MainHand || this.Slot == ItemSlots.OffHand)
				{
					if (tItem.HasSubModel)
					{
						if (tItem.SubWeaponSet != this.ModelSet && tItem.WeaponSet != this.ModelSet)
						{
							continue;
						}
					}
					else
					{
						if (tItem.WeaponSet != this.ModelSet)
						{
							continue;
						}
					}
				}

				if (tItem.ModelBase == this.ModelBase && tItem.ModelVariant == this.ModelVariant)
				{
					return tItem;
				}

				if (tItem.HasSubModel && tItem.SubModelBase == this.modelBase && tItem.SubModelVariant == this.modelVariant)
				{
					return tItem;
				}
			}

			return new DummyItem(this.ModelSet, this.ModelBase, this.ModelVariant);
		}

		protected IDye GetDye()
		{
			// None
			if (this.DyeId == 0)
				return NoneDye;

			foreach (IDye dye in this.gameData.Dyes.All)
			{
				if (dye.Id == this.DyeId)
				{
					return dye;
				}
			}

			return NoneDye;
		}
	}
}
