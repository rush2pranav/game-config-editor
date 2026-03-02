using System.ComponentModel.DataAnnotations;

namespace ConfigEditor.Shared.Models
{
    // weapon configuration for designers to edit and make changes to in editor
    public class WeaponConfig
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Category { get; set; } = "Rifle"; //rifle, pistol, shotgun, smg, sniper, heavy guns and melee

        [Range(0, 10000)]
        public int Damage { get; set; }

        [Range(0.1, 30.0)]
        public double FireRate { get; set; } // rounds/sec

        [Range(1, 500)]
        public int MagazineSize { get; set; }

        [Range(0.1, 10.0)]
        public double ReloadTime { get; set; } // in seconds

        [Range(0, 100)]
        public double Accuracy { get; set; } // in percentage

        [Range(0, 200)]
        public double Range { get; set; } // in meters

        [Range(0, 50000)]
        public int Cost { get; set; } // in game currency

        [MaxLength(20)]
        public string Rarity { get; set; } = "Common"; //common, uncommon, rare, epic and legendary

        public bool IsEnabled { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
        

        //versioning for tracking changes
        public int Version { get; set; } = 1;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAtUtc { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; }
    }

    // enemy configuration
    public class EnemyConfig
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string EnemyType { get; set; } = "Standard"; // standard, elit, boss or minion

        [Range(1, 100000)]
        public int Health { get; set; }

        [Range(0, 5000)]
        public int Damage { get; set; }

        [Range(0.1, 20.0)]
        public double MoveSpeed { get; set; }

        [Range(0, 100000)]
        public int XpReward { get; set; }

        [Range(0, 100)]
        public double SpawnChance { get; set; }

        [Range(1, 100)]
        public int MinLevel { get; set; } = 1;

        [MaxLength(50)]
        public string? LootTable { get; set; }

        public bool IsEnabled { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Version { get; set; } = 1;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAtUtc { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; }
    }
    
    // items or loot configurations
    public class ItemConfig
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ItemType { get; set; } = "Consumable"; // weapon,armor, consumable, material, quests

        [MaxLength(20)]
        public string Rarity { get; set; } = "Common";

        [Range(0, 999999)]
        public int Value { get; set; }

        [Range(0.01, 100.0)]
        public double DropRate { get; set; }

        [Range(1, 999)]
        public int MaxStack { get; set; } = 1;

        [Range(1, 100)]
        public int LevelRequired { get; set; } = 1;

        public bool IsTradeable { get; set; } = true;
        public bool IsEnabled { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
        public int Version { get; set; } = 1;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAtUtc { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; }
    }
}