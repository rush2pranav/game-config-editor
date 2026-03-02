using Microsoft.EntityFrameworkCore;
using ConfigEditor.Shared.Models;

namespace ConfigEditor.Shared.Data
{
    public class ConfigDbContext : DbContext
    {
        public DbSet<WeaponConfig> Weapons => Set<WeaponConfig>();
        public DbSet<EnemyConfig> Enemies => Set<EnemyConfig>();
        public DbSet<ItemConfig> Items => Set<ItemConfig>();

        public string DbPath { get; }

        public ConfigDbContext()
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GameConfigEditor");
            Directory.CreateDirectory(appData);
            DbPath = Path.Combine(appData, "gameconfig.db");
        }

        public ConfigDbContext(DbContextOptions<ConfigDbContext> options) : base(options)
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GameConfigEditor");
            Directory.CreateDirectory(appData);
            DbPath = Path.Combine(appData, "gameconfig.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
                options.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeaponConfig>(e =>
            {
                e.HasKey(w => w.Id);
                e.HasIndex(w => w.Name);
                e.HasIndex(w => w.Category);
            });

            modelBuilder.Entity<EnemyConfig>(e =>
            {
                e.HasKey(en => en.Id);
                e.HasIndex(en => en.Name);
                e.HasIndex(en => en.EnemyType);
            });

            modelBuilder.Entity<ItemConfig>(e =>
            {
                e.HasKey(i => i.Id);
                e.HasIndex(i => i.Name);
                e.HasIndex(i => i.ItemType);
            });
        }

        // database seeds with sample game config data
        public async Task SeedAsync()
        {
            if (await Weapons.AnyAsync()) return;

            Weapons.AddRange(
                new WeaponConfig { Name = "Iron Sword", Category = "Melee", Damage = 45, FireRate = 1.2, MagazineSize = 1, ReloadTime = 0.5, Accuracy = 95, Range = 2, Cost = 100, Rarity = "Common", Description = "A basic iron sword for beginners" },
                new WeaponConfig { Name = "Assault Rifle", Category = "Rifle", Damage = 32, FireRate = 8.5, MagazineSize = 30, ReloadTime = 2.1, Accuracy = 75, Range = 50, Cost = 2800, Rarity = "Common", Description = "Standard issue assault rifle" },
                new WeaponConfig { Name = "Plasma Pistol", Category = "Pistol", Damage = 28, FireRate = 5.0, MagazineSize = 12, ReloadTime = 1.5, Accuracy = 82, Range = 25, Cost = 500, Rarity = "Uncommon", Description = "Energy based sidearm" },
                new WeaponConfig { Name = "Thunder Shotgun", Category = "Shotgun", Damage = 120, FireRate = 1.5, MagazineSize = 8, ReloadTime = 3.5, Accuracy = 40, Range = 10, Cost = 1800, Rarity = "Rare", Description = "Devastating at close range" },
                new WeaponConfig { Name = "Void Sniper", Category = "Sniper", Damage = 250, FireRate = 0.8, MagazineSize = 5, ReloadTime = 3.0, Accuracy = 98, Range = 150, Cost = 4500, Rarity = "Epic", Description = "Long range precision weapon" },
                new WeaponConfig { Name = "Dragon's Breath", Category = "Heavy", Damage = 65, FireRate = 12.0, MagazineSize = 100, ReloadTime = 5.0, Accuracy = 60, Range = 40, Cost = 5200, Rarity = "Legendary", Description = "Minigun with incendiary rounds" },
                new WeaponConfig { Name = "Shadow Dagger", Category = "Melee", Damage = 80, FireRate = 2.0, MagazineSize = 1, ReloadTime = 0.3, Accuracy = 90, Range = 1.5, Cost = 3200, Rarity = "Epic", Description = "Fast melee weapon with bleed effect" },
                new WeaponConfig { Name = "Burst SMG", Category = "SMG", Damage = 22, FireRate = 10.0, MagazineSize = 25, ReloadTime = 1.8, Accuracy = 65, Range = 20, Cost = 1200, Rarity = "Common", Description = "High fire rate, moderate damage" }
            );

            Enemies.AddRange(
                new EnemyConfig { Name = "Goblin Scout", EnemyType = "Standard", Health = 100, Damage = 15, MoveSpeed = 3.5, XpReward = 25, SpawnChance = 40, MinLevel = 1, LootTable = "common_loot", Description = "Fast but fragile" },
                new EnemyConfig { Name = "Orc Warrior", EnemyType = "Standard", Health = 300, Damage = 45, MoveSpeed = 2.0, XpReward = 75, SpawnChance = 25, MinLevel = 5, LootTable = "uncommon_loot", Description = "Heavy melee fighter" },
                new EnemyConfig { Name = "Shadow Assassin", EnemyType = "Elite", Health = 500, Damage = 80, MoveSpeed = 5.0, XpReward = 200, SpawnChance = 10, MinLevel = 15, LootTable = "rare_loot", Description = "Stealthy and deadly" },
                new EnemyConfig { Name = "Crystal Golem", EnemyType = "Elite", Health = 2000, Damage = 60, MoveSpeed = 1.0, XpReward = 350, SpawnChance = 8, MinLevel = 20, LootTable = "rare_loot", Description = "Slow but extremely durable" },
                new EnemyConfig { Name = "Dragon Lord", EnemyType = "Boss", Health = 50000, Damage = 500, MoveSpeed = 3.0, XpReward = 5000, SpawnChance = 1, MinLevel = 40, LootTable = "legendary_loot", Description = "End game raid boss" },
                new EnemyConfig { Name = "Skeleton Minion", EnemyType = "Minion", Health = 50, Damage = 10, MoveSpeed = 2.5, XpReward = 10, SpawnChance = 60, MinLevel = 1, LootTable = "common_loot", Description = "Summoned by necromancers" }
            );

            Items.AddRange(
                new ItemConfig { Name = "Health Potion", ItemType = "Consumable", Rarity = "Common", Value = 50, DropRate = 30, MaxStack = 99, LevelRequired = 1, Description = "Restores 100 HP" },
                new ItemConfig { Name = "Mana Crystal", ItemType = "Consumable", Rarity = "Uncommon", Value = 120, DropRate = 15, MaxStack = 50, LevelRequired = 5, Description = "Restores 80 MP" },
                new ItemConfig { Name = "Iron Ore", ItemType = "Material", Rarity = "Common", Value = 25, DropRate = 25, MaxStack = 200, LevelRequired = 1, Description = "Basic crafting material" },
                new ItemConfig { Name = "Dragon Scale", ItemType = "Material", Rarity = "Legendary", Value = 5000, DropRate = 0.5, MaxStack = 10, LevelRequired = 40, IsTradeable = false, Description = "Rare material from Dragon Lord" },
                new ItemConfig { Name = "Teleport Scroll", ItemType = "Consumable", Rarity = "Rare", Value = 500, DropRate = 5, MaxStack = 20, LevelRequired = 10, Description = "Teleport to nearest town" },
                new ItemConfig { Name = "Phoenix Feather", ItemType = "Quest", Rarity = "Epic", Value = 0, DropRate = 2, MaxStack = 1, LevelRequired = 30, IsTradeable = false, Description = "Required for the Rebirth questline" }
            );

            await SaveChangesAsync();
        }
    }
}