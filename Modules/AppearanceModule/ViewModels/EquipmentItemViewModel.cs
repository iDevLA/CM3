﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.ViewModels
{
	using Anamnesis;
	using ConceptMatrix.GameData;

	public class EquipmentItemViewModel : EquipmentBaseViewModel
	{
		private readonly IMemory<Equipment> memory;

		public EquipmentItemViewModel(IMemory<Equipment> equipmentMemory, ItemSlots slot, Actor actor)
			: base(slot, actor)
		{
			this.memory = equipmentMemory;
			this.memory.ValueChanged += this.OnMemoryValueChanged;

			this.OnMemoryValueChanged(null, null);
		}

		public override void Dispose()
		{
			this.memory.Dispose();
		}

		public override void Apply()
		{
			Equipment eq = this.memory.Value;

			Equipment.Item i = eq.GetItem(this.Slot);

			if (i.Base == this.ModelBase &&
				i.Dye == this.DyeId &&
				i.Variant == this.ModelVariant)
				return;

			i.Base = this.ModelBase;
			i.Dye = this.DyeId;
			i.Variant = (byte)this.ModelVariant;

			this.memory.Value = eq;

			this.Actor.ActorRefresh();
		}

		[PropertyChanged.SuppressPropertyChangedWarnings]
		private void OnMemoryValueChanged(object sender, object value)
		{
			Equipment eq = this.memory.Value;

			Equipment.Item item = this.memory.Value.GetItem(this.Slot);

			if (item == null)
				return;

			this.modelBase = item.Base;
			this.modelVariant = item.Variant;
			this.dyeId = item.Dye;

			this.Item = this.GetItem();
			this.Dye = this.GetDye();
		}
	}
}
