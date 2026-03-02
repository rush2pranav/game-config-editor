using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ConfigEditor.Shared.Data;
using ConfigEditor.Shared.Models;
using ConfigEditor.Shared.Protos;

namespace ConfigEditor.Server.Services
{
    // this is for the gRPC service implementation that handles all the config CRUD operations
    public class GameConfigGrpcService : ConfigService.ConfigServiceBase
    {
        private readonly ConfigDbContext _db;
        private readonly ILogger<GameConfigGrpcService> _logger;

        public GameConfigGrpcService(ConfigDbContext db, ILogger<GameConfigGrpcService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // weapons section
        public override async Task<WeaponListReply> GetWeapons(GetAllRequest request, ServerCallContext context)
        {
            var weapons = await _db.Weapons.OrderBy(w => w.Name).ToListAsync();
            var reply = new WeaponListReply();
            reply.Weapons.AddRange(weapons.Select(MapWeaponToReply));
            _logger.LogInformation("GetWeapons: returned {Count} weapons", weapons.Count);
            return reply;
        }

        public override async Task<WeaponReply> GetWeapon(GetByIdRequest request, ServerCallContext context)
        {
            var weapon = await _db.Weapons.FindAsync(request.Id);
            if (weapon == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Weapon {request.Id} not found"));
            return MapWeaponToReply(weapon);
        }

        public override async Task<SaveReply> SaveWeapon(WeaponReply request, ServerCallContext context)
        {
            try
            {
                WeaponConfig weapon;
                if (request.Id > 0)
                {
                    weapon = await _db.Weapons.FindAsync(request.Id)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Weapon not found"));
                    MapReplyToWeapon(request, weapon);
                    weapon.Version++;
                    weapon.ModifiedAtUtc = DateTime.UtcNow;
                    weapon.LastModifiedBy = "editor";
                }
                else
                {
                    weapon = new WeaponConfig();
                    MapReplyToWeapon(request, weapon);
                    weapon.CreatedAtUtc = DateTime.UtcNow;
                    weapon.ModifiedAtUtc = DateTime.UtcNow;
                    weapon.LastModifiedBy = "editor";
                    _db.Weapons.Add(weapon);
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("SaveWeapon: saved {Name} (ID: {Id})", weapon.Name, weapon.Id);
                return new SaveReply { Success = true, Message = "Weapon saved", Id = weapon.Id };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveWeapon failed");
                return new SaveReply { Success = false, Message = ex.Message };
            }
        }

        public override async Task<SaveReply> DeleteWeapon(GetByIdRequest request, ServerCallContext context)
        {
            var weapon = await _db.Weapons.FindAsync(request.Id);
            if (weapon == null)
                return new SaveReply { Success = false, Message = "Weapon not found" };

            _db.Weapons.Remove(weapon);
            await _db.SaveChangesAsync();
            _logger.LogInformation("DeleteWeapon: deleted {Name}", weapon.Name);
            return new SaveReply { Success = true, Message = "Weapon deleted", Id = request.Id };
        }

        // enemies section
        public override async Task<EnemyListReply> GetEnemies(GetAllRequest request, ServerCallContext context)
        {
            var enemies = await _db.Enemies.OrderBy(e => e.Name).ToListAsync();
            var reply = new EnemyListReply();
            reply.Enemies.AddRange(enemies.Select(MapEnemyToReply));
            return reply;
        }

        public override async Task<EnemyReply> GetEnemy(GetByIdRequest request, ServerCallContext context)
        {
            var enemy = await _db.Enemies.FindAsync(request.Id);
            if (enemy == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Enemy {request.Id} not found"));
            return MapEnemyToReply(enemy);
        }

        public override async Task<SaveReply> SaveEnemy(EnemyReply request, ServerCallContext context)
        {
            try
            {
                EnemyConfig enemy;
                if (request.Id > 0)
                {
                    enemy = await _db.Enemies.FindAsync(request.Id)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Enemy not found"));
                    MapReplyToEnemy(request, enemy);
                    enemy.Version++;
                    enemy.ModifiedAtUtc = DateTime.UtcNow;
                }
                else
                {
                    enemy = new EnemyConfig();
                    MapReplyToEnemy(request, enemy);
                    _db.Enemies.Add(enemy);
                }
                await _db.SaveChangesAsync();
                return new SaveReply { Success = true, Message = "Enemy saved", Id = enemy.Id };
            }
            catch (Exception ex)
            {
                return new SaveReply { Success = false, Message = ex.Message };
            }
        }

        public override async Task<SaveReply> DeleteEnemy(GetByIdRequest request, ServerCallContext context)
        {
            var enemy = await _db.Enemies.FindAsync(request.Id);
            if (enemy == null)
                return new SaveReply { Success = false, Message = "Enemy not found" };
            _db.Enemies.Remove(enemy);
            await _db.SaveChangesAsync();
            return new SaveReply { Success = true, Message = "Enemy deleted", Id = request.Id };
        }

        // items section
        public override async Task<ItemListReply> GetItems(GetAllRequest request, ServerCallContext context)
        {
            var items = await _db.Items.OrderBy(i => i.Name).ToListAsync();
            var reply = new ItemListReply();
            reply.Items.AddRange(items.Select(MapItemToReply));
            return reply;
        }

        public override async Task<ItemReply> GetItem(GetByIdRequest request, ServerCallContext context)
        {
            var item = await _db.Items.FindAsync(request.Id);
            if (item == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Item {request.Id} not found"));
            return MapItemToReply(item);
        }

        public override async Task<SaveReply> SaveItem(ItemReply request, ServerCallContext context)
        {
            try
            {
                ItemConfig item;
                if (request.Id > 0)
                {
                    item = await _db.Items.FindAsync(request.Id)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Item not found"));
                    MapReplyToItem(request, item);
                    item.Version++;
                    item.ModifiedAtUtc = DateTime.UtcNow;
                }
                else
                {
                    item = new ItemConfig();
                    MapReplyToItem(request, item);
                    _db.Items.Add(item);
                }
                await _db.SaveChangesAsync();
                return new SaveReply { Success = true, Message = "Item saved", Id = item.Id };
            }
            catch (Exception ex)
            {
                return new SaveReply { Success = false, Message = ex.Message };
            }
        }

        public override async Task<SaveReply> DeleteItem(GetByIdRequest request, ServerCallContext context)
        {
            var item = await _db.Items.FindAsync(request.Id);
            if (item == null)
                return new SaveReply { Success = false, Message = "Item not found" };
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return new SaveReply { Success = true, Message = "Item deleted", Id = request.Id };
        }

        // mappers to convert betw the EF entities and protobuf msgs
        private static WeaponReply MapWeaponToReply(WeaponConfig w) => new()
        {
            Id = w.Id,
            Name = w.Name,
            Category = w.Category,
            Damage = w.Damage,
            FireRate = w.FireRate,
            MagazineSize = w.MagazineSize,
            ReloadTime = w.ReloadTime,
            Accuracy = w.Accuracy,
            Range = w.Range,
            Cost = w.Cost,
            Rarity = w.Rarity,
            IsEnabled = w.IsEnabled,
            Description = w.Description ?? "",
            Version = w.Version
        };

        private static void MapReplyToWeapon(WeaponReply r, WeaponConfig w)
        {
            w.Name = r.Name; w.Category = r.Category; w.Damage = r.Damage;
            w.FireRate = r.FireRate; w.MagazineSize = r.MagazineSize; w.ReloadTime = r.ReloadTime;
            w.Accuracy = r.Accuracy; w.Range = r.Range; w.Cost = r.Cost; w.Rarity = r.Rarity;
            w.IsEnabled = r.IsEnabled; w.Description = r.Description;
        }

        private static EnemyReply MapEnemyToReply(EnemyConfig e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            EnemyType = e.EnemyType,
            Health = e.Health,
            Damage = e.Damage,
            MoveSpeed = e.MoveSpeed,
            XpReward = e.XpReward,
            SpawnChance = e.SpawnChance,
            MinLevel = e.MinLevel,
            LootTable = e.LootTable ?? "",
            IsEnabled = e.IsEnabled,
            Description = e.Description ?? "",
            Version = e.Version
        };

        private static void MapReplyToEnemy(EnemyReply r, EnemyConfig e)
        {
            e.Name = r.Name; e.EnemyType = r.EnemyType; e.Health = r.Health;
            e.Damage = r.Damage; e.MoveSpeed = r.MoveSpeed; e.XpReward = r.XpReward;
            e.SpawnChance = r.SpawnChance; e.MinLevel = r.MinLevel; e.LootTable = r.LootTable;
            e.IsEnabled = r.IsEnabled; e.Description = r.Description;
        }

        private static ItemReply MapItemToReply(ItemConfig i) => new()
        {
            Id = i.Id,
            Name = i.Name,
            ItemType = i.ItemType,
            Rarity = i.Rarity,
            Value = i.Value,
            DropRate = i.DropRate,
            MaxStack = i.MaxStack,
            LevelRequired = i.LevelRequired,
            IsTradeable = i.IsTradeable,
            IsEnabled = i.IsEnabled,
            Description = i.Description ?? "",
            Version = i.Version
        };

        private static void MapReplyToItem(ItemReply r, ItemConfig i)
        {
            i.Name = r.Name; i.ItemType = r.ItemType; i.Rarity = r.Rarity;
            i.Value = r.Value; i.DropRate = r.DropRate; i.MaxStack = r.MaxStack;
            i.LevelRequired = r.LevelRequired; i.IsTradeable = r.IsTradeable;
            i.IsEnabled = r.IsEnabled; i.Description = r.Description;
        }
    }
}